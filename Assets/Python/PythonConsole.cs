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
	public TMP_InputField input;
	public TMP_Text text;
	public ScrollRect scroll;

	List<string> m_previousCommands = new List<string>();
	int m_previousCommandSelected = 0;

	ScriptScope m_scope;

	void OnDisable(){
		input.onSubmit.RemoveListener (ExecuteCommand);
	}

	void OnEnable(){
		input.onSubmit.AddListener (ExecuteCommand);

		if (m_scope == null) {
			m_scope = PythonUtils.GetEngine ().CreateScope ();
			//		m_scope.SetVariable ("_pythonConsole", this);
			PythonUtils.GetEngine ().Execute (PythonUtils.defaultPythonConsoleHeader + GlobalAssemblyImport () +
			@"
import sys
sys.stdout=PythonConsole
Select = PythonConsole.Select

import UnityEngine
Destroy = UnityEngine.Object.Destroy
FindObjectOfType = UnityEngine.Object.FindObjectOfType
FindObjectsOfType = UnityEngine.Object.FindObjectsOfType
Instantiate = UnityEngine.Object.Instantiate

"
, m_scope);
		}
	}

	string GlobalAssemblyImport(){
		StringBuilder import = new StringBuilder();
		import.Append ("\nimport ");
		bool importedOne = false;
		var globalTypes = Assembly.GetExecutingAssembly ().GetTypes ();
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
			m_scope.SetVariable ("selected", UnityEditor.Selection.activeGameObject);
		}
		#endif

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

		bool exception = false;
		try{
			PythonUtils.GetEngine ().Execute (command, m_scope);
		}catch(Exception e){
			exception = true;
//			Debug.LogException (e);
			write (e.Message);
		}

		text.text += "\n<b>" + (exception ? "<color=#d22>" : "") +command + (exception ?"</color=#f66>" : "") + "</b> ";
		text.text += log;
		log = "";

		scroll.verticalNormalizedPosition = 0f;
	}


	static string log;
	public static void write(string s){
		if (string.IsNullOrEmpty (s) || s == "\n")
			return;
		log += "\n<i>" + ">>>"+s + "</i> ";
	}

	public static void Select(UnityEngine.Object o){
		#if UNITY_EDITOR
		UnityEditor.Selection.activeObject = o;
		#endif
	}
}
