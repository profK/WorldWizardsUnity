using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Environments;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.EventSystems;
using WorldWizards.core.manager;
using ButtonRec = RadialMenuController.ButtonRec;
using UnityEngine.UI;
using WorldWizards.core.controller.resources;
using WorldWizards.core.entity.gameObject.resource.metaData;
//temporary
using UnityEditor;
using UnityEditorInternal;


public class MenuManager : Manager {

    private Canvas uiCanvas;
    private Sprite rootMenuSprite;
    private Sprite defaultMenuSprite;
    private GameObject basicButtonPrefab;

    public float spacing = 20f;
    public int maxMenus = 8;

    public MenuManager()
    {
        uiCanvas = Camera.main.transform.GetComponentInChildren<Canvas>();
        if (uiCanvas == null)
        {  //make a UI canvas
            GameObject obj = GameObject.Instantiate(
                Resources.Load<GameObject>("DesktopEditorCanvas"));
            //obj.transform.parent = Camera.main.transform;
            uiCanvas = obj.GetComponent<Canvas>();
           
        }

        GameObject tileSelector = uiCanvas.transform.Find("TileSelector").gameObject;
        GameObject content =
            tileSelector.GetComponent<ScrollRect>().viewport.Find("Content").gameObject;
        AddTextureButtons(content);
    }

    private void AddTextureButtons(GameObject viewport)
    {
        GameObject buttonPrefab = Resources.Load<GameObject>("TileButton");
        Dictionary<string, AssetBundle> bundles = WWAssetBundleController.GetAllAssetBundles();
        Debug.Log("bundles count= " + bundles.Count);
        AssetBundle bundle = bundles.First().Value; // just get a random bundle for now
        GameObject[] goa = bundle.LoadAllAssets<GameObject>();
        Debug.Log("Game object count = " + goa.Length);
        foreach (GameObject go in goa)
        {
            WWResourceMetadata md = go.GetComponent<WWResourceMetadata>();
            if (md != null)
            {
                WWWallMetadata wmd = md.wwTileMetadata.wwWallMetadata;
                if (wmd.east || wmd.west || wmd.north || wmd.south)
                {
                    //TODO:  Move this to meta data because wont work in build!
                    Texture2D img = AssetPreview.GetAssetPreview(go);
                    Sprite thumbNail =
                        Sprite.Create(img,new Rect(0,0,img.width,img.height), 
                            new Vector2((float)img.width/2, (float)img.height/2));
                    GameObject newButton = GameObject.Instantiate(buttonPrefab);
                    newButton.GetComponent<Image>().sprite = thumbNail;
                    RectTransform t2 = newButton.transform as RectTransform;
                    //t2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,img.width/5);
                    //t2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, img.height/5);
                    newButton.transform.parent = viewport.transform;
                }
            }
        }
    }

    public GameObject MakeRadialMenu(ButtonRec recRoot)
    {
        GameObject root = MakeRadialButton(recRoot);
        RadialMenuController rmc = root.GetComponent<RadialMenuController>();
        rmc.SetCenter(true);
        AddChildren(root.transform, recRoot);
        rmc.Fold();
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
            AddChildren(newButton.transform,brec);
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
                (centerRadius + menuRadius+spacing);

            menuTform.localPosition = placementVec;
        }
    }

    private ButtonRec MakeMenusFromTileset()
    {
        ButtonRec root = new ButtonRec("Tiles", rootMenuSprite);
        ButtonRec wallsB = new ButtonRec("Walls", defaultMenuSprite);
        root.AddMenu(wallsB);
        ButtonRec floorsB = new ButtonRec("Floors", defaultMenuSprite);
        root.AddMenu(floorsB);
        ButtonRec ceilingB = new ButtonRec("Ceilings", defaultMenuSprite);
        root.AddMenu(ceilingB);
        ButtonRec propsB = new ButtonRec("Props", defaultMenuSprite);
        root.AddMenu(propsB);
        
        // add tiles
        // temporary code to get first tileset
        Dictionary<string, AssetBundle> bundles = WWAssetBundleController.GetAllAssetBundles();
        Debug.Log("bundles count= "+bundles.Count);
        AssetBundle bundle = bundles.First().Value; // just get a random bundle for now
        GameObject[] goa = bundle.LoadAllAssets<GameObject>();
        Debug.Log("Game objct count = "+goa.Length);
        foreach (GameObject go in goa)
        {
            WWResourceMetadata md = go.GetComponent<WWResourceMetadata>();
            if (md != null)
            {
                WWWallMetadata wmd = md.wwTileMetadata.wwWallMetadata;
                if (wmd.east || wmd.west || wmd.north || wmd.south)
                {
                    //TODO:  Move this to meta data because wont work in build!
                    Sprite thumbNail = 
                        Sprite.Create(AssetPreview.GetMiniThumbnail(go),
                            new Rect(0,0,40,40), new Vector2(20,20));
                    wallsB.AddMenu(new ButtonRec("",thumbNail));
                }
            }
        }

        return LimitMenusPerLevel(root, maxMenus);

    }

    private ButtonRec LimitMenusPerLevel(ButtonRec oldRoot, int maxMenus)
    {
        return RecursiveRebuild(oldRoot,maxMenus);
    }

    private ButtonRec RecursiveRebuild(ButtonRec oldRoot, int maxCount)
    {
        // if we have no children, return a clone of ourselves
        if (oldRoot.children.Count == 0)
        {
           return new ButtonRec(oldRoot);
        } else {  // not at bottom yet
            ButtonRec newRoot = new ButtonRec(oldRoot);
            newRoot.children.Clear(); // remove shallow copied references
            if (oldRoot.children.Count < maxMenus)
            { // return clone with processed children
                foreach (ButtonRec cb in oldRoot.children)
                {
                    newRoot.AddMenu(RecursiveRebuild(cb,maxCount));
                }

                return newRoot;
            } else {  //need to subdevide children
                for (int bn = 0;
                    bn < Math.Ceiling(((float) oldRoot.children.Count) / maxMenus);
                    bn++)
                {
                    ButtonRec numberedRoot = new ButtonRec(bn.ToString(), null);
                    for (int cn = bn * maxCount;
                        cn < Math.Min((bn + 1) * maxCount, oldRoot.children.Count);
                        cn++)
                    {
                        numberedRoot.AddMenu(RecursiveRebuild(oldRoot.children.ElementAt(cn), maxCount));
                    }
                    newRoot.AddMenu(numberedRoot);
                }

                return newRoot;
            }
        }
        
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
