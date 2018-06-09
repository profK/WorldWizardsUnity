using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AxisControl : MonoBehaviour {
    private Dropdown _dropDown = null;
    private Dropdown dropDown
    {
        get
        {
            if (_dropDown == null)
            {
                _dropDown = GetComponentInChildren<Dropdown>();
            }
            return _dropDown;
        }
    }
    private PlugVirtualAxes.PlugVirtualAxis _vaxis = null;
    private PlugVirtualAxes.PlugVirtualAxis vaxis
    {
        get
        {
            if (_vaxis == null)
            {
                Text text = GetComponentInChildren<Text>();
                _vaxis = PlugVirtualAxes.Instance.GetVirtualAxis(text.text);
            }
            return _vaxis;
        }
    }
	// Use this for initialization
	void Start () {
        
       
	}
	
	public void ComboChanged(System.Int32 idx)
    {
        PlugControlRecord rec = PlugInputProviderManager.GetControlByName(
            dropDown.options[idx].text);
        PlugVirtualAxes.Instance.MapVirtualAxis(vaxis, rec);
    }
}
