using UnityEngine;
using System.Collections;
using System;

public class dummyGameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        KeyboardInput blah = GetComponent<KeyboardInput>();
        blah.green_strum += LogAThing;
        blah.red_strum += LogAThing;
	}

    void LogAThing(object sender, EventArgs e)
    {
        
		Debug.Log((e as ButtonEventArgs).button.ToString());
    }


	// Update is called once per frame
	void Update () {
	
	}
}
