using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.Scripting.Hosting;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Text;
using System.Reflection;

public class PythonConsole : MonoBehaviour {
	public bool visibleByDefault = true;

	public TMP_InputField input;
	public TMP_Text text;
	public ScrollRect scroll;
	public RectTransform panel;

	List<string> m_previousCommands = new List<string>();
	int m_previousCommandSelected = 0;

	ScriptScope m_scope;

	void OnDisable(){
		input.onSubmit.RemoveListener (ExecuteCommand);
		HideLog ();
	}

	void Start(){
		SetVisible (visibleByDefault, true);
	}


	void OnEnable(){
		input.onSubmit.AddListener (ExecuteCommand);

		if (m_scope == null) {
			RecreateScope ();
		}
		if (listeningToDevelopmentConsole)
			ShowLog ();
	}

	void RecreateScope(){
		m_scope = PythonUtils.GetEngine ().CreateScope ();
		m_scope.SetVariable ("console", this);
		var fullScript = PythonUtils.defaultPythonConsoleHeader + GlobalAssemblyImport () +
			@"
import sys
sys.stdout=console
Select = console.Select
import UnityEngine
Destroy = UnityEngine.Object.Destroy
FindObjectOfType = UnityEngine.Object.FindObjectOfType
FindObjectsOfType = UnityEngine.Object.FindObjectsOfType
DontDestroyOnLoad = UnityEngine.Object.DontDestroyOnLoad
DestroyImmediate = UnityEngine.Object.DestroyImmediate
Instantiate = UnityEngine.Object.Instantiate
Clear = console.Clear
";
		PythonUtils.GetEngine ().Execute (fullScript, m_scope);
	}

	string GlobalAssemblyImport(){
		StringBuilder import = new StringBuilder();
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

	void Update(){
		#if UNITY_EDITOR
		if (Application.isEditor) {
			m_scope.SetVariable ("selection", UnityEditor.Selection.activeObject);
		}
		#endif

		if (Input.GetKeyDown (KeyCode.BackQuote)) {
			ToggleVisibility ();
		}

		if (!input.isFocused)
			return;
		HandleSelectPreviousCommand ();

		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
			input.lineType = TMP_InputField.LineType.MultiLineNewline;
		} else {
			input.lineType = TMP_InputField.LineType.MultiLineSubmit;
		}
		input.GetComponent<LayoutElement> ().preferredHeight = input.textComponent.preferredHeight + 8;

	}

	bool visible = true;
	void ToggleVisibility (bool immediately = false){
		SetVisible (!visible, immediately);
	}

	void SetVisible(bool value, bool immediately){
		visible = value;
		StopAllCoroutines ();
		StartCoroutine (ToggleVIsibilityCoroutine(visible, immediately));
	}

	IEnumerator ToggleVIsibilityCoroutine(bool makeVisible, bool immediately){

		if (makeVisible) {
			panel.gameObject.SetActive (true);
		} else {
			input.interactable = false;
		}

		Vector2 targetMaxAnchor = makeVisible ? new Vector2 (1, 1) : new Vector2 (1, 2);
		Vector2 targetMinAnchor = makeVisible ? new Vector2 (0, 0) : new Vector2 (0, 1);

		Vector2 minAnchorAtStart = panel.anchorMin;
		Vector2 maxAnchorAtStart = panel.anchorMax;

		float appearSpeed = 6f;
		float t = immediately ? 1f : 0f;
		while (t <= 1f) {
			t += Time.unscaledDeltaTime * appearSpeed;
			panel.anchorMin = Vector2.Lerp (minAnchorAtStart, targetMinAnchor, t);
			panel.anchorMax = Vector2.Lerp (maxAnchorAtStart, targetMaxAnchor, t);
			if (t <= 1f) {
				yield return null;
			}
		}
		if (!makeVisible) {
			panel.gameObject.SetActive (false);
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

	bool m_commandExecutionInProgress = false;
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
			//			Debug.LogException (e);
			write (e.Message);
		}
		m_commandExecutionInProgress = false;

		var commandLog = "\n<b>" + (exception ? "<color=#d22>" : "") + command + (exception ? "</color=#f66>" : "") + "</b> ";
		log = commandLog + log;

		FlushLog ();
	}

	void FlushLog(){
		if (!m_suspendNextMessage) {
			text.text += log;
			log = "";
		}{
			m_suspendNextMessage = false;
		}

		scroll.verticalNormalizedPosition = 0f;
	}

	string log;
	public void write(string s){
		if (string.IsNullOrEmpty (s) || s == "\n")
			return;
		log += "\n<i>" + ">>>"+s + "</i> ";
	}

	public void Select(UnityEngine.Object o){
		#if UNITY_EDITOR
		UnityEditor.Selection.activeObject = o;
		#else
		m_scope.SetVariable ("selection", o);
		#endif
	}

	bool m_suspendNextMessage = false;
	public void Clear(){
		text.text = "";
		m_suspendNextMessage = true;
	}

	bool listeningToDevelopmentConsole = false;

	public void ShowLog(){
		listeningToDevelopmentConsole = true;
		Application.logMessageReceived -= PrintLogMessageToConsole;
		Application.logMessageReceived += PrintLogMessageToConsole;
	}

	void PrintLogMessageToConsole (string condition, string stackTrace, LogType type){
		string colorHex = "#000";
		bool printStackTrace = false;
		switch (type) {
		case LogType.Assert:
		case LogType.Error:
		case LogType.Exception:
			colorHex = "#f00";
			printStackTrace = true;
			break;
		case LogType.Warning:
			colorHex = "#ff0";
			break;
		}
		var message = "[" + type.ToString() + "] " + condition + (printStackTrace ? "\n" + stackTrace : "");
		message = "<color="+colorHex+">" + message + "</color="+colorHex+">";
//		write (message);
		log += "\n<i>" + message + "</i> ";

		if (!m_commandExecutionInProgress) {
			FlushLog ();
		}
	}

	public void HideLog(){
		listeningToDevelopmentConsole = false;
		Application.logMessageReceived -= PrintLogMessageToConsole;
	}
}
