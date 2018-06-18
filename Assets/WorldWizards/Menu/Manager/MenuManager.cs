using System;
using System.Collections;
using System.Collections.Generic;
using Boo.Lang.Environments;
using UnityEngine;
using UnityEngine.EventSystems;
using WorldWizards.core.manager;
using ButtonRec = RadialMenuController.ButtonRec;
using UnityEngine.UI;


public class MenuManager : Manager {

    private Canvas uiCanvas;
    private Sprite rootMenuSprite;
    private Sprite defaultMenuSprite;
    private GameObject basicButtonPrefab;

    public MenuManager()
    {
        uiCanvas = Camera.main.transform.GetComponentInChildren<Canvas>();
        if (uiCanvas == null)
        {  //make a UI canvas
            GameObject obj = GameObject.Instantiate(
                Resources.Load<GameObject>("RadialMenuCanvas"));
            obj.transform.parent = Camera.main.transform;
            uiCanvas = obj.GetComponent<Canvas>();
        }

        rootMenuSprite = Resources.Load<Sprite>("defaultRootSprite");
        defaultMenuSprite = Resources.Load<Sprite>("defaultMenuSprite");
        basicButtonPrefab = Resources.Load<GameObject>("RadialButton");
        GameObject menu =
            MakeRadialMenu(MakeMenusFromTileset());
        menu.transform.parent = uiCanvas.transform;
        menu.transform.localPosition = Vector3.zero;

    }

    public GameObject MakeRadialMenu(ButtonRec recRoot)
    {
        GameObject root = MakeRadialButton(recRoot);
        root.GetComponent<RadialMenuController>() .SetCenter(true);
        AddChildren(root.transform, recRoot);
        
        return root;
    }

    private GameObject MakeRadialButton(ButtonRec recRoot)
    {
        GameObject newButton = GameObject.Instantiate(basicButtonPrefab);
        newButton.GetComponentInChildren<Text>().text = recRoot.text;
        newButton.GetComponentInChildren<Image>().sprite = recRoot.image;
       
        
        return newButton;
    }

    private void AddChildren(Transform parentXform, ButtonRec recRoot)
    {
        foreach (ButtonRec brec in recRoot.children)
        {
            GameObject newButton = MakeRadialButton(brec);
            newButton.transform.parent = parentXform;
           
        }
        RadialLayout(parentXform.gameObject);

    }

    private void RadialLayout(GameObject buttonsParent)
    {
        int buttonCount = buttonsParent.transform.childCount;
        double arc = (Math.PI * 2 / buttonCount);
        RectTransform centerTform =
            buttonsParent.transform as RectTransform;
        float centerRadius = Math.Max(
                                 centerTform.rect.width, centerTform.rect.height) / 2;
        for (int i = 0; i < buttonCount; i++)
        {
            double angle = arc * i;
            RectTransform menuTform =
                buttonsParent.transform.GetChild(i) as RectTransform;
            float menuRadius = Math.Max(
                                   menuTform.rect.width, menuTform.rect.height) / 2;

            Vector3 placementVec =
                new Vector3((float)Math.Sin(angle),
                    (float)Math.Cos(angle), 0) *
                (centerRadius + menuRadius);

            menuTform.localPosition = placementVec;
        }
    }

    private ButtonRec MakeMenusFromTileset()
    {
        ButtonRec root = new ButtonRec("Tiles", rootMenuSprite);
        root.AddMenu(new ButtonRec("Walls", defaultMenuSprite));
        root.AddMenu(new ButtonRec("Floors", defaultMenuSprite));
        root.AddMenu(new ButtonRec("Ceiling", defaultMenuSprite));
        root.AddMenu(new ButtonRec("Props", defaultMenuSprite));
        return root;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
