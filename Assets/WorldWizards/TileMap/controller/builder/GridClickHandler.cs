using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WorldWizards.core.controller.builder;

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
        currentGridPos = new Vector2(
            Mathf.FloorToInt(collisionPos.x/(float)gridC.TileWidth),
            Mathf.FloorToInt(collisionPos.z/(float)gridC.TileDepth));
        gridC.Cursor.transform.position = new Vector3(
            currentGridPos.x*(float)gridC.TileWidth,
            collisionPos.y,
            currentGridPos.y*(float)gridC.TileDepth
            );
    }
}
