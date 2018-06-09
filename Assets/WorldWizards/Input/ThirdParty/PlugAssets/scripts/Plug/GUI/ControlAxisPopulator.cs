using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ControlAxisPopulator : MonoBehaviour {
    public GameObject axisLinePrefab;
    List<string> digitalAxes = new List<string>();
    List<string> normalizedAxes = new List<string>();
    List<string> analogAxes = new List<string>();
    List<string> asciiAxes = new List<string>();
    
    void Start() {

        PlugInputProviderManager.DeviceAdded += PlugInputProviderManager_DeviceAdded;
        PlugInputProviderManager.DeviceRemoved += PlugInputProviderManager_DeviceRemoved;
        PlugInputProviderManager.ScanProviders();
        DrawAxes();
    }

    private void PlugInputProviderManager_DeviceAdded(PlugDeviceRecord device)
    {
        foreach(PlugControlRecord ctrl in device.Controls)
        {
            switch (ctrl.CtrlType)
            {
                case PlugControlRecord.CTRL_TYPE.Digital:
                    digitalAxes.Add(ctrl.FQName);
                    break;
                case PlugControlRecord.CTRL_TYPE.Analog:
                    analogAxes.Add(ctrl.FQName);
                    break;
                case PlugControlRecord.CTRL_TYPE.Normalized:
                    normalizedAxes.Add(ctrl.FQName);
                    break;
                case PlugControlRecord.CTRL_TYPE.Ascii:
                    asciiAxes.Add(ctrl.FQName);
                    break;
            }
        }
        DrawAxes();
    }

    private void PlugInputProviderManager_DeviceRemoved(PlugDeviceRecord device)
    {
        foreach (PlugControlRecord ctrl in device.Controls)
        {
            switch (ctrl.CtrlType)
            {
                case PlugControlRecord.CTRL_TYPE.Digital:
                    digitalAxes.Remove(ctrl.FQName);
                    break;
                case PlugControlRecord.CTRL_TYPE.Analog:
                    analogAxes.Remove(ctrl.FQName);
                    break;
                case PlugControlRecord.CTRL_TYPE.Normalized:
                    normalizedAxes.Remove(ctrl.FQName);
                    break;
                case PlugControlRecord.CTRL_TYPE.Ascii:
                    asciiAxes.Remove(ctrl.FQName);
                    break;
            }
        }
        DrawAxes();
    }

    public void DrawAxes() {
        Debug.Log("Drawing axes");
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (PlugVirtualAxes.PlugVirtualAxis axis in PlugVirtualAxes.Instance.VirtualAxes)
        {
            GameObject go = Instantiate(axisLinePrefab);
            go.GetComponentInChildren<Text>().text = axis.Name;
            List<string> choices = null;
            switch (axis.CtrlType)
            {
                case PlugControlRecord.CTRL_TYPE.Digital:
                    choices = digitalAxes;
                    break;
                case PlugControlRecord.CTRL_TYPE.Analog:
                    choices = analogAxes;
                    break;
                case PlugControlRecord.CTRL_TYPE.Normalized:
                    choices= normalizedAxes;
                    break;
                case PlugControlRecord.CTRL_TYPE.Ascii:
                    choices=  asciiAxes;
                    break;
            }
            Dropdown dd = go.GetComponentInChildren<Dropdown>();
            dd.ClearOptions();
            dd.AddOptions(choices);
            dd.value = FindDropDownValue(dd, axis.PhysicalAxis);
            Button button = go.GetComponentInChildren<Button>();
            button.onClick.AddListener(delegate
            {
                DoShowMe(axis,go);
            });
            go.transform.SetParent(transform);
        }
        Canvas.ForceUpdateCanvases();
	}


    private GameObject _showmeGO = null;
    private PlugVirtualAxes.PlugVirtualAxis _showMeVAxis = null;
    
    private void DoShowMe(PlugVirtualAxes.PlugVirtualAxis axis,GameObject go)
    {
        _showmeGO = go;
        _showMeVAxis = axis;
        foreach (Transform child in transform)
        {
            child.GetComponentInChildren<Button>().interactable = false;
        }
        PlugInputProviderManager.ControlStateChange += PlugInputProviderManager_ControlStateChange;
    }

    private void PlugInputProviderManager_ControlStateChange(PlugControlRecord ctrl)
    {
        if (ctrl.CtrlType == _showMeVAxis.CtrlType)
        {
            Dropdown dd = _showmeGO.GetComponentInChildren<Dropdown>();
            dd.value = FindDropDownValue(dd, ctrl.FQName);
            foreach (Transform child in transform)
            {
                child.GetComponentInChildren<Button>().interactable = true;
            }
            PlugInputProviderManager.ControlStateChange -= PlugInputProviderManager_ControlStateChange;
        }
    }

    private int FindDropDownValue(Dropdown dd, string name)
    {
  
        for(int i = 0; i < dd.options.Count; i++)
        {
            if (dd.options[i].text == name)
            {
                return i;
            }
        }
        return 0;
    }

    // Update is called once per frame
    void Update () {
        PlugInputProviderManager.UpdateInput();
	}
}
