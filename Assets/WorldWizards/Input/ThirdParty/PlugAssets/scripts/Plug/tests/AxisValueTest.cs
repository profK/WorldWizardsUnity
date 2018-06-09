using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AxisValueTest : MonoBehaviour {

    Text valueText;
	// Use this for initialization
	void Start () {
        valueText = GetComponent<Text>();
	}
	
	public void SetValue(float f)
    {
        valueText.text = f.ToString();
    }
}
