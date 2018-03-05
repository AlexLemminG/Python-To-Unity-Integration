using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalPythonBehaviour : PythonBehaviour {
	public string filePath = "testPython.py";
	protected override void Awake(){
		script = PythonScript.CreateFromFile (filePath);
		base.Awake ();
	}
}
