using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldWizards.core.controller.builder;

public class GridControlHandlers : MonoBehaviour
{
    private GridController gridC;
	// Use this for initialization
	void Start ()
	{
	    gridC = FindObjectOfType<GridController>();
	}
	
	// Update is called once per frame
	void Update () {
		//nop
	}

    public void StepGridUp()
    {
        gridC.StepUp();
    }

    public void StepGridDown()
    {
        gridC.StepDown();
    }
}
