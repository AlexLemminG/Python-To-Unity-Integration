using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting;

[System.Serializable]
public class PythonScript {
	[SerializeField]
	Object m_scriptAsset;

	int m_updateCount = 0;
	public int updateCount{
		get{
			return scriptInstance == null ? m_updateCount : scriptInstance.updateCount;
		}
	}

	PythonScriptInstance m_scriptInstance;
	PythonScriptInstance scriptInstance{
		get{
			bool currentValid = m_scriptInstance != null && m_scriptInstance.asset == m_scriptAsset || m_scriptInstance == null && m_scriptAsset == null;
			if (!currentValid) {
				m_updateCount++;
				Unsubscribe ();
				m_scriptInstance = PythonScriptsDatabase.GetScriptInstance(m_scriptAsset);
				Subscribe ();
			}
			return m_scriptInstance;
		}
	}

	bool m_subscribedToChanges = false;

	public void Subscribe(){
		if (m_subscribedToChanges || m_scriptAsset == null)
			return;

		//do subscription

		m_subscribedToChanges = true;
	}

	public void Unsubscribe(){
		if (!m_subscribedToChanges || m_scriptAsset == null)
			return;

		//do unsubscription

		m_subscribedToChanges = false;
	}

	public void Execute(ScriptScope scope){
		if (scriptInstance != null) {
			scriptInstance.compiled.Execute (scope);
		}
	}
}
