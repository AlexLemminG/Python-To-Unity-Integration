using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class PythonScriptPostprocessor : AssetPostprocessor {
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		var allChangedAssets = movedAssets.Concat (movedFromAssetPaths).Concat (importedAssets);

		PythonScriptsDatabase.AssetsStartUpdating ();
		foreach (string assetPath in allChangedAssets)
		{
			bool isPythonScript = PythonUtils.IsPythonFile(assetPath);
			if (!isPythonScript) {
				continue;
			}
			PythonScriptsDatabase.UpdateScript (assetPath);
		}
		PythonScriptsDatabase.AssetsFinishedUpdating ();
	}
}
