using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using UnityEditor;


[ScriptedImporter(1, "py")]
public class PythonScriptsImporter : ScriptedImporter
{
	public override void OnImportAsset(AssetImportContext ctx)
	{
		var text = File.ReadAllText (ctx.assetPath);
		var psa = ScriptableObject.CreateInstance<PythonScriptAsset> ();
		psa.text = text;
		ctx.SetMainAsset ("code", psa);
	}
}
