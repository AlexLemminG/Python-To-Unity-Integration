using UnityEngine;
using Microsoft.Scripting.Hosting;

[System.Serializable]
public class PythonScriptInstance : ScriptableObject{
	[SerializeField]
	Object m_asset;
	public Object asset{
		get{
			return m_asset;
		}
	}
	[SerializeField]
	[TextArea]
	string m_code;
	public string code{
		get{
			return m_code;
		}
		set{
			m_code = value;
			name = m_asset.name;
			m_dirty = true;
		}
	}

	[SerializeField]
	bool m_dirty = true;

	int m_updateCount;
	public int updateCount {
		get {
			return m_updateCount + (m_dirty ? 1 : 0);
		}
		private set {
			m_updateCount = value;
		}
	}



	public static PythonScriptInstance Create(Object asset, string code){
		var instance = ScriptableObject.CreateInstance<PythonScriptInstance> ();

		instance.m_asset = asset;
		instance.m_code = code;
		instance.m_dirty = true;

		instance.name = asset.name;
			
		return instance;
	}

	CompiledCode m_compiled;
	public CompiledCode compiled{
		get{
			if (m_compiled == null || m_dirty) {
				var engine = PythonUtils.GetEngine ();
				var source = engine.CreateScriptSourceFromString (PythonUtils.defaultPythonScriptHeader + code);
				m_compiled = source.Compile ();
				m_updateCount++;
				m_dirty = false;
			}
			return m_compiled;
		}
	}
}