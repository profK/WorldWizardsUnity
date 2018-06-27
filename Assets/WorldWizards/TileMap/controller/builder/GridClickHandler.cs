using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WorldWizards.core.controller.builder;
using WorldWizards.core.entity.coordinate;
using WorldWizards.core.entity.coordinate.utils;

public class GridClickHandler : MonoBehaviour
{
    private GridController gridC;
    private Vector2 currentGridPos;

    // Use this for initialization
    void Start ()
    {
        gridC = GetComponentInParent<GridController>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPointerClick(BaseEventData data)
    {
        PointerEventData ped  = data as PointerEventData;
        Vector3 collisionPos = ped.pointerCurrentRaycast.worldPosition;
        Coordinate c = CoordinateHelper.UnityCoordToWWCoord(collisionPos);
        c.SetOffset(Vector3.zero/**new Vector3(0.5f,0.5f,0.5f)**/);
        gridC.CursorLocation = c;
    }
}
