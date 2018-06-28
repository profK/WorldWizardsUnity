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

    private GameObject _currentlySelected = null;
    private GameObject _selectionCursor;

    public GameObject SelectedObject
    {
        get { return _currentlySelected; }
        set
        {
            if (_currentlySelected == value)
            {
                return;
            }
            if (value == null)
            {
                _selectionCursor.transform.parent = null;
                _selectionCursor.SetActive(false);
            }
            else
            {

                Renderer rend = value.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    _selectionCursor.SetActive(true);
                    _selectionCursor.transform.parent = null;
                    _selectionCursor.transform.localScale =
                        rend.bounds.extents * 2.1f;
                    _selectionCursor.transform.parent = value.transform;
                    _selectionCursor.transform.localPosition = new Vector3(0, 0, -4.5f);
                }
            }
            _currentlySelected = value;
        }

    }

    // Use this for initialization
    void Start ()
    {
        gridC = GetComponentInParent<GridController>();
        _selectionCursor = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ObjectCursor"));
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
        else
        {
            // throw a ray looking for game objects
            //Ray cast from collision back to camera
           
            Ray ray = ped.pressEventCamera.ScreenPointToRay(ped.position);
            RaycastHit hit;
           
            if (Physics.Raycast(ray, out hit, float.MaxValue,
                1 << LayerMask.NameToLayer("Default")))
            {
                SelectedObject = hit.collider.gameObject;
            }
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
