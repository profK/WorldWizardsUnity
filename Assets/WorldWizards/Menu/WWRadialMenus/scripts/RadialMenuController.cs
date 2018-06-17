using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuController : MonoBehaviour
{
    public Sprite rootMenuSprite;
    public Sprite defaultMenuSprite;
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
    #region Unity Callbacks
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    #endregion

    

}
