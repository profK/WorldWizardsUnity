using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyTest : MonoBehaviour {

    Text text;
	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	
	public void SetText(int c)
    {
        text.text = "Key Stroke: " + (char)c;
    }
}
