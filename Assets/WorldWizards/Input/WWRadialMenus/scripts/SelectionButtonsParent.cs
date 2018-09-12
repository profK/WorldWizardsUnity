using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionButtonsParent : MonoBehaviour
{
    public Button centerButton;
    
	// Use this for initialization
	void Awake () {
	    if (centerButton == null)
	    {
	        centerButton = transform.parent.GetComponentInChildren<Button>();
	    }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
