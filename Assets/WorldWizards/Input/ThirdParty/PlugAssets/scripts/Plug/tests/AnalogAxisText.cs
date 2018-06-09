using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnalogAxisText : MonoBehaviour {

    Text text;
	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	
	public void SetText(float f)
    {
        text.text = f.ToString();
    }
}
