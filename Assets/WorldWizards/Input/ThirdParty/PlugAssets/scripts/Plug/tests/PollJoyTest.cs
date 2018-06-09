using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PollJoyTest : MonoBehaviour {

    float last = float.MinValue;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float f = Input.GetAxis("Horizontal");
        if (last != f)
        {
            last = f;
            Debug.Log("H:" + f);
        }
	}
}
