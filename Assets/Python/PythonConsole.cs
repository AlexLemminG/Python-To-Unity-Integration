using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.Scripting.Hosting;
using UnityEngine.UI;
using System;
using System.Text;
using System.Reflection;

public class PythonConsole : MonoBehaviour {
	public TMP_InputField input;
	public RectTransform textsRoot;
	public TMP_Text textTemplate;
	public ScrollRect scroll;
	public RectTransform panel;

	const bool c_visibleByDefault = false;
	const float appearSpeed = 6f;

	private RectTransform lastTextTransform;
	List<string> m_previousCommands = new List<string>();
	int m_previousCommandSelected;
	ScriptScope m_scope;
	bool m_visible = true;
	string m_prevFrameInputText = "";
	bool m_commandExecutionInProgress;
	string m_log;
	bool m_suspendNextMessage;
	bool m_listeningToDevelopmentConsole = true;
	Coroutine m_toggleVisibilityCoroutine;

	public void Select(UnityEngine.Object o){
		#if UNITY_EDITOR
		UnityEditor.Selection.activeObject = o;
		#else
		m_scope.SetVariable ("selection", o);
		#endif
	}

	public void Clear(){
		var textTemplateTransform = textTemplate.transform;
		for(int i = textsRoot.childCount-1; i>=0; i--){
			var textTransform = textsRoot.GetChild(i);
			if(textTransform != textTemplateTransform){
				Destroy(textTransform.gameObject);
			}
		}
		lastTextTransform = null;
		m_suspendNextMessage = true;
	}

	public void ShowLog(){
		m_listeningToDevelopmentConsole = true;
		Application.logMessageReceived -= PrintLogMessageToConsole;
		Application.logMessageReceived += PrintLogMessageToConsole;
	}

	public void HideLog(){
		m_listeningToDevelopmentConsole = false;
		Application.logMessageReceived -= PrintLogMessageToConsole;
	}

	//used to write by python (definition is important)
	public void write(string s){
		if (string.IsNullOrEmpty (s) || s == "\n")
			return;
		var message = (string.IsNullOrEmpty(m_log) ? "" : "\n") +"<i>" + ">>>"+s + "</i> ";
		m_log += message;
	}

	bool ShouldToggleConsole(){
		return Input.GetKeyDown (KeyCode.BackQuote);
	}

	void OnDisable(){
		input.onSubmit.RemoveListener (ExecuteCommand);
		HideLog ();
	}

	void Start(){
		SetVisible (c_visibleByDefault, true);
	}

	void OnEnable(){
		input.onSubmit.AddListener (ExecuteCommand);

		if (m_scope == null) {
			RecreateScope ();
		}
		if (m_listeningToDevelopmentConsole)
			ShowLog ();
	}

	void RecreateScope(){
		m_scope = PythonUtils.GetEngine ().CreateScope ();
		m_scope.SetVariable ("console", this);
		var fullScript = PythonUtils.defaultPythonConsoleHeader + GlobalAssemblyImport ();
		PythonUtils.GetEngine ().Execute (fullScript, m_scope);
	}

	static string GlobalAssemblyImport(){
		var import = new StringBuilder();
		import.Append ("\nimport ");
		bool importedOne = false;
		var globalTypes = Assembly.GetAssembly (typeof(PythonConsole)).GetTypes ();
		foreach (var type in globalTypes) {
			if (type.IsPublic && type.Namespace == null) {
				if (importedOne) {
					import.Append (',');
				} else {
					importedOne = true;
				}
				import.Append (type.Name);
			}
		}
		return import.ToString ();
	}

	void UpdateSubmitButtonReaction ()
	{
		TMP_InputField.LineType preferedLineType;
		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
			preferedLineType = TMP_InputField.LineType.MultiLineNewline;
		}
		else {
			preferedLineType = TMP_InputField.LineType.MultiLineSubmit;
		}
		//setting lineType every frame makes garbage
		if (input.lineType != preferedLineType) {
			input.lineType = preferedLineType;
		}
	}

	void UpdateSelection ()
	{
		#if UNITY_EDITOR
		m_scope.SetVariable ("selection", UnityEditor.Selection.activeObject);
		#endif
	}

	void Update(){
		UpdateSelection ();

		if (ShouldToggleConsole()) {
			input.text = m_prevFrameInputText;
			ToggleVisibility ();
		}

		if (!input.isFocused)
			return;
		
		HandleSelectPreviousCommand ();

		UpdateSubmitButtonReaction ();

		
		input.GetComponent<LayoutElement> ().preferredHeight = input.textComponent.preferredHeight + 8;

		m_prevFrameInputText = input.text;
	}

	void ToggleVisibility (bool immediately = false){
		SetVisible (!m_visible, immediately);
	}

	void SetVisible(bool value, bool immediately){
		m_visible = value;
		if (m_toggleVisibilityCoroutine != null) {
			StopCoroutine (m_toggleVisibilityCoroutine);
		}
		m_toggleVisibilityCoroutine = StartCoroutine (ToggleVisibilityCoroutine(m_visible, immediately));
	}

	IEnumerator ToggleVisibilityCoroutine(bool makeVisible, bool immediately){

		if (makeVisible) {
			panel.GetComponentInParent<Canvas>().enabled = true;
		} else {
			input.interactable = false;
		}
		float t = immediately ? 1f : 0f;
		while (t <= 1f) {
			t += Time.unscaledDeltaTime * appearSpeed;
			panel.GetComponent<Animator>().SetFloat("Appeared", makeVisible ? t : 1f - t);
			if (t <= 1f) {
				yield return null;
			}
		}
		if (!makeVisible) {
			panel.GetComponentInParent<Canvas>().enabled = false;
		} else {
			input.interactable = true;
			input.ActivateInputField ();
		}

		yield break;
	}

	void HandleSelectPreviousCommand(){
		if (m_previousCommands.Count == 0 || m_previousCommandSelected == -1 && input.textComponent.textInfo.lineCount > 1)
			return;
		bool commandSet = false;
		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			m_previousCommandSelected++;
			commandSet = true;
		}
		if (Input.GetKeyDown (KeyCode.DownArrow)) {
			m_previousCommandSelected--;
			commandSet = true;
		}
		if (commandSet) {
			bool erase = m_previousCommandSelected < 0;
			m_previousCommandSelected = Mathf.Clamp(m_previousCommandSelected, 0, m_previousCommands.Count-1);
			var previousCommand = m_previousCommands [m_previousCommandSelected];
			if (erase)
				m_previousCommandSelected = -1;

			input.text = erase ? "": previousCommand;
			input.textComponent.ForceMeshUpdate ();
			input.caretPosition = input.text.Length;
		}
	}

	void ExecuteCommand (string command){
		if (input.wasCanceled) {
			input.ActivateInputField ();
			return;
		}
		input.text = "";
		input.ActivateInputField ();
		if (command.Length != 0) {
			m_previousCommands.Insert (0, command);
		}
		m_previousCommandSelected = -1;

		m_commandExecutionInProgress = true;
		bool exception = false;
		try{
			PythonUtils.GetEngine ().Execute (command, m_scope);
		}catch(Exception e){
			exception = true;
			write (e.Message);
		}
		m_commandExecutionInProgress = false;

		var commandLog = "<b>" + (exception ? "<color=#d22>" : "") + command + (exception ? "</color=#f66>" : "") + "</b>";
		if(string.IsNullOrEmpty(m_log)){
			m_log = commandLog;
		}else{
			m_log =  commandLog + "\n" + m_log;
		}

		FlushLog ();
		scroll.verticalNormalizedPosition = 0f;
	}

	void FlushLog(){
		if (!m_suspendNextMessage) {
			var text = Instantiate(textTemplate, textsRoot);
			text.text = m_log + " ";
			Vector2 pos;
			if(lastTextTransform != null){
				pos = lastTextTransform.anchoredPosition - Vector2.up * text.preferredHeight;
			}else{
				pos = textTemplate.rectTransform.anchoredPosition - Vector2.up * text.preferredHeight;
			}
			lastTextTransform = text.rectTransform;
			lastTextTransform.anchoredPosition = pos;
			var rootSize = textsRoot.sizeDelta;
			rootSize.y = pos.y - 6;
			textsRoot.sizeDelta = rootSize;
		}
		m_suspendNextMessage = false;
		m_log = "";

		UpdateScrollPositionAfterMove ();
	}

	void UpdateScrollPositionAfterMove(){
		var viewportHeight = scroll.GetComponent<RectTransform> ().sizeDelta.y;
		var oldHeight = scroll.content.sizeDelta.y - viewportHeight;
		float pos = scroll.verticalNormalizedPosition;

		Canvas.ForceUpdateCanvases ();

		var newHeight = scroll.content.sizeDelta.y - viewportHeight;
		if (pos * oldHeight < 20f)
			scroll.verticalNormalizedPosition = 0f;
		else
			scroll.verticalNormalizedPosition = (pos*oldHeight + (newHeight - oldHeight)) / newHeight;
	}

	void PrintLogMessageToConsole (string condition, string stackTrace, LogType type){
		Color color = Color.black;
		bool printStackTrace = false;
		switch (type) {
		case LogType.Assert:
		case LogType.Error:
		case LogType.Exception:
			color = Color.red;
			printStackTrace = true;
			break;
		case LogType.Warning:
			color = Color.yellow;
			break;
		}
		var colorHex = "#" + ColorUtility.ToHtmlStringRGBA(color);
		var message = "[" + type + "] " + condition + (printStackTrace ? "\n" + stackTrace : "");
		message = "<color="+colorHex+">" + message + "</color="+colorHex+">";
		if(string.IsNullOrEmpty(m_log)){
			m_log = "<i>" + message + "</i> ";
		}else{
			m_log += "\n<i>" + message + "</i> ";
		}

		if (!m_commandExecutionInProgress) {
			FlushLog ();
		}
	}
}