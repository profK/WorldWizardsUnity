using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RadialMenuController : MonoBehaviour
{
    public enum MODE
    {
        CENTER,
        MENU
    }

    private bool _folded;

    private Dictionary<GameObject, Vector3> _storedPositions =
        new Dictionary<GameObject, Vector3>();

    private MODE _menuMode = MODE.MENU;

    public struct ButtonRec
    {
        public Sprite image;
        public string text;
        public List<ButtonRec> children;



        public ButtonRec(string text, Sprite image, params ButtonRec[] children)
        {
            this.text = text;
            this.image = image;
            this.children = new List<ButtonRec>();
            this.children.AddRange(children);
        }

        public void AddMenu(ButtonRec rec)
        {
            children.Add(rec);
        }

        public void RemoveMenu(ButtonRec rec)
        {
            children.Add(rec);
        }


    }

    #region Radial Methods

    public void Fold()
    {
        _storedPositions.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            RadialMenuController rmc = transform.GetChild(i).gameObject.GetComponent<RadialMenuController>();
            if (rmc != null)
            {
                if (!(rmc.transform.localPosition == transform.localPosition))
                {
                    _storedPositions.Add(rmc.gameObject, rmc.transform.localPosition);
                    rmc.transform.localPosition = transform.localPosition;
                    rmc.gameObject.SetActive(false);
                    //recurse
                }

                rmc.Fold();
            }
        }

        _folded = true;
    }

    public void Unfold()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            RadialMenuController rmc = transform.GetChild(i).gameObject.GetComponent<RadialMenuController>();
            if (rmc != null)
            {
                if (!(_storedPositions[rmc.gameObject] == transform.localPosition))
                {
                    rmc.transform.localPosition = _storedPositions[rmc.gameObject];
                    rmc.gameObject.SetActive(true);
                    //recurse

                }

                // unfoldign is NOT recursive unlike folding
            }
        }

        _folded = false;
    }

    public void SetCenter(bool b)
    {
        if (b)
        {
            _menuMode = MODE.CENTER;
        }
        else
        {
            _menuMode = MODE.MENU;
        }
    }

#endregion

    #region Event Callbacks

    public void OnDrag(BaseEventData data)
    {
        PointerEventData ped = data as PointerEventData;
        if (_menuMode == MODE.CENTER)
        {
            transform.position = ped.position;
        }
    }

    public void OnScroll(BaseEventData data)
    {
        PointerEventData ped = data as PointerEventData;
        if (_menuMode == MODE.CENTER)
        {
            Vector3 currScale = transform.localScale;
            transform.localScale = new Vector3(
                currScale.x+(ped.scrollDelta.y*0.2f),
                currScale.y + (ped.scrollDelta.y * 0.2f),
                currScale.z);
        }
    }

    public void OnClick()
    {
      
        if (_menuMode == MODE.MENU)
        {
            RadialMenuController parentRmc = GetComponentInParent<RadialMenuController>();
            if (parentRmc != null)
            {
                parentRmc.Fold();
            }

            Unfold();
        }
        else
        {
            // just fold or unfold
            if (_folded)
            {
                Unfold();
            }
            else
            {
                Fold();
            }

        }

    }

    #endregion
    #region Unity Callbacks
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
#endregion
}
