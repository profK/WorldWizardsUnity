using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.Xml;
using System.Text;

/// <summary>
/// This class implementst the mapping of physcial controls provided by the 
/// PlugInputProviderManaer to application defined virtual controls.  These mappings
/// are soft and may be changed in the Unity editor with the Virtual Axes Window or
/// at run-time through API calls provided  by this class.
/// 
/// This class is implemnted as a one-intsance sigleton.  It may not be 
/// instanced with a new command.  Instead, the instance is accessed via
/// the global property PlugVirtualAxes.Instance
///
/// </summary>
[Serializable]
public class PlugVirtualAxes
{
    /// <summary>
    /// Virtual Axis settings are stored in an XML file.  This property
    /// gives the path to the default faile.
    /// </summary>
    private static string XMLPATH = "/PlugVirtualAxes.xml";

    /// <summary>
    /// This private field is where the pointer to the singleton
    /// instance is stored
    /// </summary>
    private static PlugVirtualAxes _instance = null;
    /// <summary>
    /// This is a "smart property" that creats the singleton instance
    /// if it hasn't been created yet.  It cyrrently only loads and saces
    /// to the default file
    /// </summary>
    public static PlugVirtualAxes Instance {
        get
        {
            if (_instance == null)
            {
                // check to see if we need to init from  default
                if (!File.Exists(Application.persistentDataPath + XMLPATH))
                {
                    TextAsset defaultFile = 
                        Resources.Load("PlugVirtualAxes") as TextAsset;
                    File.WriteAllText(Application.persistentDataPath + XMLPATH,
                        defaultFile.text);
                }
                // now attemot to load
                Debug.Log("Checking VAxis file existance at " +
                Application.persistentDataPath + XMLPATH);
                if (File.Exists(Application.persistentDataPath + XMLPATH))
                {
                    Debug.Log("Loading VAxis File");
                    XmlSerializer ser = new XmlSerializer(typeof(PlugVirtualAxes));
                    FileStream fs = File.OpenRead(Application.persistentDataPath + XMLPATH);
                    _instance = ser.Deserialize(fs) as PlugVirtualAxes;
                    fs.Close();
                    // map 
                    foreach(PlugVirtualAxis va in _instance.VirtualAxes)
                    {
                        if (va.PhysicalAxis != null)
                        {
                            PlugControlRecord ctl = PlugInputProviderManager.GetControlByName(va.PhysicalAxis);
                            if (ctl != null)
                            {
                                _instance.MapVirtualAxis(va, ctl);
                            }
                        } 
                    }
                }
                else
                {
                    Debug.Log("Creating VAxis File");
                    _instance = new PlugVirtualAxes();
                }
            }
            return _instance;
        }

        internal set
        {
            _instance = value;
        }
    }

    /// <summary>
    /// This method finds and returns a vitual axis by its assigned name
    /// </summary>
    /// <param name="name">the name of the virtual axis</param>
    /// <returns>the virtual axis record, or null if there are no matches</returns>
    internal PlugVirtualAxis GetVirtualAxis(string name)
    {
        foreach(PlugVirtualAxis vaxis in VirtualAxes)
        {
            if (name == vaxis.Name)
            {
                return vaxis;
            }
        }
        return null;
    }

    /// <summary>
    /// This call binds a virtual axis to a physical control as provided by
    /// the PlugInputProvderManager.  Passing null for the physical control
    /// just removes any existing binding.
    /// </summary>
    /// <param name="va">the virtual axis to bind</param>
    /// <param name="xtrl">the physical control to bind it to</param>
    public void MapVirtualAxis(PlugVirtualAxis va, PlugControlRecord xtrl)
    {
        // remove from old list
        if (VaxisToCtrlMap.ContainsKey(va))
        {
            PlugControlRecord oldCtrl =
                VaxisToCtrlMap[va];
            VaxisToCtrlMap.Remove(va);
            if (CtrlToVaxesMap.ContainsKey(oldCtrl))
            {
                CtrlToVaxesMap[oldCtrl].Remove(va);
            }
        }
        if (xtrl != null)
        {
            // add va axis to ctrls list of vaxes
            if (!CtrlToVaxesMap.ContainsKey(xtrl))
            {
                CtrlToVaxesMap.Add(xtrl, new List<PlugVirtualAxis>());
            }
            List<PlugVirtualAxis> vaList = CtrlToVaxesMap[xtrl];
            vaList.Add(va);
            VaxisToCtrlMap.Add(va, xtrl);
            va.PhysicalAxis = xtrl.FQName;
        }
     }
    
    /// <summary>
    /// This proeprty gives accesss to the list of virtual axes
    /// </summary>
    public List<PlugVirtualAxis> VirtualAxes = new List<PlugVirtualAxis>();

    /// <summary>
    /// This defines a record that describes a virtualaxis
    /// </summary>
    [Serializable]
    public class PlugVirtualAxis
    {
        public string Name { get; set; }
        public PlugControlRecord.CTRL_TYPE CtrlType { get; set; }
        public string PhysicalAxis { get; set; }
    }

    #region event definitions
    public delegate void AsciiEvent(PlugVirtualAxis axis, char c);
    public delegate void AxisEvent(PlugVirtualAxis axis, float axisValue);
    public delegate void NormalizedEvent(PlugVirtualAxis axis, float normValue);
    public delegate void ButtonEvent(PlugVirtualAxis axis);
    public delegate void DeviceEvent(PlugDeviceRecord device);
    public delegate void ControlChangeEvent(PlugVirtualAxis axis);
    #endregion
    #region events
    /// <summary>
    /// Add a listener to this event to recieve key stroke events
    /// </summary>
    public event AsciiEvent AsciiInput;
    /// <summary>
    /// Add a listener here to recieve floating point control changes
    /// </summary>
    public  event AxisEvent AxisChange;
    /// <summary>
    /// Add a listener here to recieve digital control transitions to down state
    /// </summary>
    public  event ButtonEvent ButtonDown;
    /// <summary>
    /// Add a listener here to recieve digital control transitions to up state
    /// </summary>
    public event ButtonEvent ButtonUp;
    /// Add a listener here to recieve normalized control changes
    public event NormalizedEvent NormalizedChanged;
    /// <summary>
    /// Add a listener here to be notified of new devices neing connected
    /// </summary>
    public  event DeviceEvent DeviceAdded;
    /// <summary>
    /// Add a listener here to be notified of devices being disconnected
    /// </summary>
    public  event DeviceEvent DeviceRemoved;
    /// <summary>
    /// Add a listener here to be notified of a change of state on any control
    /// </summary>
    public  event ControlChangeEvent ControlStateChange;
    
    #endregion

    #region translation maps
    [NonSerialized]
    private Dictionary<PlugControlRecord, List<PlugVirtualAxis>> CtrlToVaxesMap = 
        new Dictionary<PlugControlRecord, List<PlugVirtualAxis>>();
    [NonSerialized]
    private Dictionary<PlugVirtualAxis,PlugControlRecord> VaxisToCtrlMap =
        new Dictionary<PlugVirtualAxis, PlugControlRecord>();
    #endregion

    /// <summary>
    /// This method wires up event reception from the PlugInputProvider
    /// </summary>
    public PlugVirtualAxes()
    {
        PlugInputProviderManager.AsciiInput += 
            PlugInputProviderManager_AsciiInput;
        PlugInputProviderManager.AxisChange += 
            PlugInputProviderManager_AxisChange;
        PlugInputProviderManager.ButtonDown += 
            PlugInputProviderManager_ButtonDown;
        PlugInputProviderManager.ButtonUp += 
            PlugInputProviderManager_ButtonUp;
        PlugInputProviderManager.ControlStateChange += 
            PlugInputProviderManager_ControlStateChange;
        PlugInputProviderManager.DeviceAdded += 
            PlugInputProviderManager_DeviceAdded;
        PlugInputProviderManager.DeviceRemoved += 
            PlugInputProviderManager_DeviceRemoved;
        PlugInputProviderManager.NormalizedChanged += 
            PlugInputProviderManager_NormalizedChanged;
    }

    #region event handlers
    /// <summary>
    /// Callback for when normalized controls change their value
    /// </summary>
    /// <param name="ctrl">the ctrl that changed value</param>
    /// <param name="normValue">the new value</param>
    private void PlugInputProviderManager_NormalizedChanged(PlugControlRecord ctrl, float normValue)
    {
        List<PlugVirtualAxis> vaxes = CtrlToVaxes(ctrl);
        if (vaxes != null)
        {
            foreach (PlugVirtualAxis vaxis in vaxes)
            {
                if (vaxis != null)
                {
                    if (NormalizedChanged != null)
                    {
                        NormalizedChanged(vaxis, normValue);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Callback for when a device is removed
    /// </summary>
    /// <param name="device">the device that was removed</param>

    private void PlugInputProviderManager_DeviceRemoved(PlugDeviceRecord device)
    {
        if (DeviceRemoved != null)
        {
            DeviceRemoved(device);
        }
    }

    private void PlugInputProviderManager_DeviceAdded(PlugDeviceRecord device)
    {
        if (DeviceAdded != null)
        {
            DeviceAdded(device);
        }
    }

    /// <summary>
    /// Callback for when any control changes its value
    /// </summary>
    /// <param name="ctrl">the ctrl that changed value</param>
   
    private void PlugInputProviderManager_ControlStateChange(PlugControlRecord ctrl)
    {
        List<PlugVirtualAxis> vaxes = CtrlToVaxes(ctrl);
        if (vaxes != null)
        {
            foreach (PlugVirtualAxis vaxis in vaxes)
            {
                if (vaxis != null)
                {
                    if (ControlStateChange != null)
                    {
                        ControlStateChange(vaxis);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Callback for when a digital control changes to button up state
    /// </summary>
    /// <param name="ctrl">the ctrl that changed value</param>
  
    private void PlugInputProviderManager_ButtonUp(PlugControlRecord ctrl)
    {
        List<PlugVirtualAxis> vaxes = CtrlToVaxes(ctrl);
        if (vaxes != null)
        {
            foreach (PlugVirtualAxis vaxis in vaxes)
            {
                if (vaxis != null)
                {
                    if (ButtonUp != null)
                    {
                        ButtonUp(vaxis);
                    }
                }
            }
        }
    }

    
    /// <summary>
    /// Callback for when a digital control changes to button up state
    /// </summary>
    /// <param name="ctrl">the ctrl that changed value</param>

    private void PlugInputProviderManager_ButtonDown(PlugControlRecord ctrl)
    {
        List<PlugVirtualAxis> vaxes = CtrlToVaxes(ctrl);
        if (vaxes != null)
        {
            foreach (PlugVirtualAxis vaxis in vaxes)
            {
                if (vaxis != null)
                {
                    if (ButtonDown != null)
                    {
                        ButtonDown(vaxis);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Callback for when  floting point control changs it value
    /// </summary>
    /// <param name="ctrl">the ctrl that changed value</param>
    /// <param name="axisValue">The new value</param>
    /// 

    private void PlugInputProviderManager_AxisChange(PlugControlRecord ctrl, float axisValue)
    {
        List<PlugVirtualAxis> vaxes = CtrlToVaxes(ctrl);
        if (vaxes != null)
        {
            foreach (PlugVirtualAxis vaxis in vaxes)
            {
                if (vaxis != null)
                {
                    if (AxisChange != null)
                    {
                        AxisChange(vaxis, axisValue);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Callback for when an ascii control return a keystroke
    /// </summary>
    /// <param name="ctrl">the ctrl that detected the key</param>
    /// <param name="c">The character sent</param>
    /// 

    private void PlugInputProviderManager_AsciiInput(PlugControlRecord ctrl, char c)
    {
        List<PlugVirtualAxis> vaxes = CtrlToVaxes(ctrl);
        if (vaxes != null) {
            foreach (PlugVirtualAxis vaxis in vaxes)
            {
                if (vaxis != null)
                {
                    if (AsciiInput != null)
                    {
                        AsciiInput(vaxis, c);
                    }
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// This method returns the list of all virtual controls mapped to a given
    /// physical control
    /// </summary>
    /// <param name="ctrl">The physical controls</param>
    /// <returns>A list of the virtual controls mapped to the ctrl</returns>
    private List<PlugVirtualAxis> CtrlToVaxes(PlugControlRecord ctrl)
    {
        if (CtrlToVaxesMap.ContainsKey(ctrl))
        {
            return CtrlToVaxesMap[ctrl];
        }
        return null;
    }

    /// <summary>
    /// Remove a virtual control from the list of available axes
    /// </summary>
    /// <param name="va">the control to remove </param>
    public void RemoveVirtualAxis(PlugVirtualAxis va)
    {
        VirtualAxes.Remove(va);
        //TODO cleanup maps
    }

    /// <summary>
    /// Adds a new virtual control
    /// </summary>
    /// <returns>the newl created virtual control</returns>
    public PlugVirtualAxis AddAxis()
    {
        PlugVirtualAxis va = new PlugVirtualAxis();
        VirtualAxes.Add(va);
        return va;
    }

    /// <summary>
    /// Write all the virtual axis info out to the currently associated
    /// XML file
    /// </summary>
    public void Save()
    {
        XmlSerializer ser = new XmlSerializer(typeof(PlugVirtualAxes));
        FileStream fs = File.Create(Application.persistentDataPath + XMLPATH);
        using (XmlTextWriter tw = new XmlTextWriter(fs, Encoding.UTF8))
        {
            tw.Formatting = Formatting.Indented;
            ser.Serialize(tw, this);
            fs.Flush();
            fs.Close();
        }
    }
}