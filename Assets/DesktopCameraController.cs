using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopCameraController : MonoBehaviour
{
    private Camera cam;
	// Use this for initialization
	void Start ()
	{
	    cam = GetComponentInChildren<Camera>();
	    
	}

    public void DollyZ(float amnt)
    {
        gameObject.transform.Translate(new Vector3(0,0,amnt));
    }
}
