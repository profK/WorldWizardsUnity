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
        PointerEventData ped = data as PointerEventData;
        if (ped.button == PointerEventData.InputButton.Left)
        {
            ClickedAt(ped.pointerCurrentRaycast.worldPosition);

        }
    }

    public void ClickedAt(Vector3 worldPos)
    {
        worldPos.y = transform.position.y + 0.1f;
        Coordinate c = CoordinateHelper.UnityCoordToWWCoord(worldPos);
        c.SetOffset(new Vector3(0f, 0f, 0));
        gridC.CursorLocation = c;
    }

}
