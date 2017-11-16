using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PythonScript))]
public class PythonScriptPropertyDrawer : PropertyDrawer {
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label){
		var scriptAssetProperty = property.FindPropertyRelative ("m_scriptAsset");

		var scriptAsset = scriptAssetProperty.objectReferenceValue;

		var newAsset = EditorGUI.ObjectField (position, "script", scriptAsset, typeof(Object), false);

		if (newAsset != null && !PythonUtils.IsPythonFile (newAsset)) {
			newAsset = scriptAsset;
		}

		if (newAsset != scriptAsset) {
			scriptAssetProperty.objectReferenceValue = newAsset;
		}
	}
}
