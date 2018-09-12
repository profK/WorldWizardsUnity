using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        public ButtonRec(ButtonRec brec) :
            this(brec.text, brec.image, brec.children.ToArray())
        {
            //nop
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
        if (_folded)
        {
            return;
        }
        _storedPositions.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            RadialMenuController rmc = transform.GetChild(i).gameObject.GetComponent<RadialMenuController>();
            if (rmc != null)
            {
                if (rmc.transform.localPosition != transform.localPosition)
                {
                    _storedPositions.Add(rmc.gameObject, rmc.transform.localPosition);
                    rmc.transform.localPosition = transform.localPosition;
                   
                    //recurse
                }
                rmc.HideMenu(true);
                rmc.Fold();
            }
        }

        _folded = true;
    }

    public void Unfold()
    {
        if (!_folded)
        {
            return;
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            RadialMenuController rmc = transform.GetChild(i).gameObject.GetComponent<RadialMenuController>();
            if (rmc != null)
            {
                if (_storedPositions[rmc.gameObject] != transform.localPosition){
                    rmc.transform.localPosition = _storedPositions[rmc.gameObject];
                   
                    //recurse
                }
                rmc.HideMenu(false);

                // unfoldign is NOT recursive unlike folding
            }
        }

        _folded = false;
    }

    private void HideMenu(bool state)
    {
        Button b = gameObject.GetComponent<Button>();
        if (b != null)
        {
            b.enabled = !state;        }
        Image i = gameObject.GetComponent<Image>();
        if (i != null)
        {
            i.enabled = !state;
        }

        Text t = gameObject.GetComponentInChildren<Text>();
        if (t != null)
        {
            t.enabled = !state;
        }
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
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        PointerEventData ped = data as PointerEventData;
        if (_menuMode == MODE.CENTER)
        {
            transform.position = ped.position;
        }
    }



    public void OnScroll(BaseEventData data)
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
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

    public void OnPointerClick(BaseEventData eventData)
    {
        
        if ((!EventSystem.current.IsPointerOverGameObject())||
            (!GetComponent<Button>().enabled))

        {
            return;
        }
        Debug.Log("Click on button "+GetComponentInChildren<Text>().text);
        PointerEventData ped = eventData as PointerEventData;
        
        if (_menuMode == MODE.MENU)
        {
            if (ped.button == PointerEventData.InputButton.Left)
            {
                if (transform.parent != null)
                {
                    RadialMenuController parentRmc =
                        transform.parent.GetComponentInParent<RadialMenuController>();
                    if (parentRmc != null)
                    {
                        parentRmc.Fold();
                        parentRmc.HideMenu(true);
                        HideMenu(false);
                        _menuMode = MODE.CENTER;
                    }
                }

                Unfold();
            } 
        }
        else
        {
            if (ped.button == PointerEventData.InputButton.Left)
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
            else if (ped.button == PointerEventData.InputButton.Right)
            {
                Fold();
                if (transform.parent != null)
                {
                    RadialMenuController parentRmc =
                        transform.parent.GetComponentInParent<RadialMenuController>();
                    if (parentRmc != null)
                    {
                        parentRmc.Unfold();
                        _menuMode = MODE.MENU;
                    }
                }

                ;
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
