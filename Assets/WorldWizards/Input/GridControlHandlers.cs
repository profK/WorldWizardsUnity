using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using WorldWizards.core.controller.builder;
using WorldWizards.core.entity.coordinate.utils;
using WorldWizards.core.entity.gameObject;
using WorldWizards.core.manager;

public class GridControlHandlers : MonoBehaviour
{
    private GridController gridC;
    private GridClickHandler gridClkHandler;

    private SceneGraphManager smgr;
	// Use this for initialization
	void Start ()
	{
	    gridC = FindObjectOfType<GridController>();
	    gridClkHandler = FindObjectOfType<GridClickHandler>();
	    smgr = ManagerRegistry.Instance.GetAnInstance<SceneGraphManager>();
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

    public void RotateTileCCW()
    {
        RotateWall(false);
    }
    
    private void RotateWall(bool clockwise=true){

    Debug.Log("in rotateCCW");
        Vector3 worldPos = gridClkHandler.SelectedObject.transform.position;
        List<WWObject> objects = smgr.GetObjectsInCoordinateIndex(
            CoordinateHelper.UnityCoordToWWCoord(worldPos));
        foreach (WWObject  wwobject in objects)
        {
            if (wwobject is Tile)
            {
                Tile tile = wwobject as Tile;
                if (tile.gameObject == gridClkHandler.SelectedObject)
                {
                    int newRot;
                    if (clockwise==false)
                    {
                        newRot =(tile.GetRotation()-90+360)%360;
                    }
                    else
                    {
                        newRot = (tile.GetRotation()+90)%360;
                    }
                    tile.SetRotation(newRot);
                    gridC.RefreshGrid();
                }
            }
            
        }
        
    }
    
    public void RotateTileCW()
    {
        RotateWall(true);
    }
}
