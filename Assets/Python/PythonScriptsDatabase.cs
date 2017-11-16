using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PythonScriptsDatabase : ScriptableObject {
	static string databaseLocation = System.IO.Path.Combine("pythonScriptsDatabase");
	static PythonScriptsDatabase s_instance;
	static PythonScriptsDatabase instance{
		get{
			if (s_instance == null) {
				s_instance = Resources.Load<PythonScriptsDatabase> (databaseLocation);
				if (s_instance == null) {
					#if UNITY_EDITOR
					s_instance = ScriptableObject.CreateInstance<PythonScriptsDatabase>();
					s_instance.FindAllPythonScripts();
					UnityEditor.AssetDatabase.CreateAsset(s_instance, databaseLocation);
					UnityEditor.AssetDatabase.SaveAssets();
					#else
					throw new UnityException("can't find PythonScriptsDatabase");
					#endif
				}
			}
			return s_instance;
		}
	}

	void FindAllPythonScripts(){
		//find all python scripts to initialize database
		//and add then to m_assetCodePairs
	}



	[SerializeField]
	List<PythonScriptInstance> m_uniquePythonScripts = new List<PythonScriptInstance>();

	#if UNITY_EDITOR
	public static void AssetsStartUpdating(){
		//do nothing
	}

	public static void UpdateScript(string assetPath){
		//load asset and code
		var asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath (assetPath);
		var code = System.IO.File.ReadAllText (assetPath);

		//put asset code pair to database
		var assetCodePair = instance.m_uniquePythonScripts.Find (x => x != null && x.asset == asset);
		if (assetCodePair == null) {
			assetCodePair = PythonScriptInstance.Create (asset, code);
			Debug.Log (assetCodePair);
			Debug.Log (instance);
			UnityEditor.AssetDatabase.AddObjectToAsset (assetCodePair, instance); 

			instance.m_uniquePythonScripts.Add (assetCodePair);
		} else {
			assetCodePair.code = code;
		}

		//mark dirty to save later
		UnityEditor.EditorUtility.SetDirty (instance);
		//inform associated with asset script about update 
		try{
			
		}catch(System.Exception e){
			Debug.LogException (e);
		}
	}

	public static void AssetsFinishedUpdating(){
		//remove deleted scripts from database

		//flush changes
		UnityEditor.AssetDatabase.SaveAssets ();
	}
	#endif

	public static PythonScriptInstance GetScriptInstance (Object asset)	{
		if (asset == null)
			return null;
		return instance.m_uniquePythonScripts.Find (x => x.asset == asset);
	}
}