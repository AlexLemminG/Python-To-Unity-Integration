using UnityEngine;
using System.IO;

public class PythonScriptAsset:ScriptableObject{
	[SerializeField]
	string m_text;
	public string text {
		get {
			return m_text;
		}
		set {
			m_text = value;
		}
	}

}