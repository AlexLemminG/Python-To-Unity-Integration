using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrangeMono : MonoBehaviour {
	public Dictionary<string, object> objs = new Dictionary<string, object>();
	void Start () {
		objs.Add ("this", this);
		objs.Add ("number", 2321);
	}

	public string number;
	public string external;
	void Update(){
		number = objs ["number"].ToString ();

		if (objs.ContainsKey ("external")) {
			external = objs ["external"].ToString ();
		}

		if (Input.GetKey (KeyCode.F1)) {
			Debug.Log ("this is some strange text");
		}
		if (Input.GetKeyDown (KeyCode.F2)) {
			Debug.LogWarning ("this is some strange warning");
		}
		if (Input.GetKeyDown (KeyCode.F3)) {
			Debug.LogError ("this is some strange error");
		}
	}
	public PythonScript ase;
}
