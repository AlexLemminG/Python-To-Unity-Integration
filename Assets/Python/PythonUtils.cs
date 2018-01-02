﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Reflection;

public static class PythonUtils {
	public static bool IsPythonFile(string path){
		return System.IO.Path.GetExtension (path) == ".py";
	}

	public static bool IsPythonFile(Object asset){
		return PythonScriptsDatabase.GetScriptInstance (asset) != null;
	}

	static ScriptEngine m_engine;
	public static ScriptEngine GetEngine(){
		if (m_engine == null) {
			m_engine = Python.CreateEngine ();
			m_engine.Runtime.LoadAssembly (Assembly.GetAssembly (typeof(GameObject)));
			m_engine.Runtime.LoadAssembly (Assembly.GetAssembly (typeof(Rigidbody)));
			m_engine.Runtime.LoadAssembly (Assembly.GetAssembly (typeof(PythonUtils)));
		}
		return m_engine;
	}

	public const string defaultPythonConsoleHeader=
@"import clr
clr.AddReference('UnityEngine','System', 'Assembly-CSharp')
from UnityEngine import *
import System.Single
def float(x):
	return clr.Convert(x, System.Single)
";

	public const string defaultPythonScriptHeader=
@"
import clr
clr.AddReference('UnityEngine','System', 'Assembly-CSharp')
import System.Single
def float(x):
	return clr.Convert(x, System.Single)

Update = None
Awake = None
Start = None
OnEnable = None
OnDisable = None
OnDestroy = None
OnCollisionEnter = None
OnCollisionStay = None
OnCollisionExit = None
";
	
}
