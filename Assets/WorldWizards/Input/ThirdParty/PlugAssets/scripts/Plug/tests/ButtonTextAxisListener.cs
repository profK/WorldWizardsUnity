using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTextAxisListener : MonoBehaviour {

    // Use this for initialization
    Text textComponent;
	void Start () {
        textComponent = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ButtonEvent(bool state)
    {
        if (state)
        {
            textComponent.text = "BUTTON DOWN";
            textComponent.color = Color.green;
        } else
        {
            textComponent.text = "BUTTON UP";
            textComponent.color = Color.red;
        }
    }


}
