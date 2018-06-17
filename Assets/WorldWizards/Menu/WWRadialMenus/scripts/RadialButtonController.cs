using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialButtonController : MonoBehaviour
{
    private SelectionButtonsParent selectionParent;

    public GameObject ButtonPrototype;
	// Use this for initialization
	void Start ()
	{
	    selectionParent = GetComponentInChildren<SelectionButtonsParent>();
	    RadialLayout();
	}

    public void AddButton(Sprite image)
    {
        GameObject newButton = Instantiate(ButtonPrototype);
        newButton.transform.parent = selectionParent.transform;
        newButton.GetComponent<Image>().sprite = image;
    }

    private void RadialLayout()
    {
        int buttonCount = selectionParent.transform.childCount;
        double arc = (Math.PI * 2 / buttonCount);
        RectTransform centerTform =
            selectionParent.centerButton.transform as RectTransform;
        float centerRadius = Math.Max(
                                 centerTform.rect.width, centerTform.rect.height) / 2;
        for (int i = 0; i < buttonCount; i++)
        {
            double angle = arc * i;
            RectTransform menuTform =
                selectionParent.transform.GetChild(i) as RectTransform;
            float menuRadius = Math.Max(
                                   menuTform.rect.width, menuTform.rect.height) / 2;

            Vector3 placementVec =
                new Vector3((float) Math.Sin(angle),
                    (float) Math.Cos(angle), 0) *
                (centerRadius + menuRadius);

            menuTform.localPosition = placementVec;
        }
    }

    // Update is called once per frame
	void Update () {
		
	}
}
