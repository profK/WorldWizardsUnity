using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

/// <summary>
/// This class is used to expose Plug controls to client code.
/// Its is an abstract class that is implemented by PlugInputProvider sub-classes
/// with their own logic for handling the gathering of input
/// </summary>
public abstract class PlugControlRecord  {
    /// <summary>
    /// The type of this control:  Digitial, Analog, Normalized or Ascii
    /// </summary>     
    private CTRL_TYPE ctype;
    /// <summary>
    /// The ID assigned to the PlugInputProvider who provided this control.
    /// This ID is fixed for the length of a run but can change between runs.
    /// </summary>
    protected int providerid;
    /// <summary>
    /// The ID assigned to the device that provided this control. 
    /// It is unique to its associated PlugInputProvider but may be duplicated
    /// across providers.
    /// This ID is fixed for the length of a run but can change between runs.
    /// </summary>
    protected int deviceid;
    /// <summary>
    /// The ID assigned to this control. It is unique to a Device but may
    /// be duplicated across devices.
    /// This ID is fixed for the length of a run but can change between runs.
    /// </summary>
    protected int controllid;
    
    /// <summary>
    /// A human readable name for this control.  It is unique to its
    /// associated Device but may be duplicated across devices
    /// </summary>
    private string name;
    /// <summary>
    /// A human readable description for this control
    /// </summary>
    private string description;
    /// <summary>
    /// CURRENTLY UNUSED: Here for future expansion
    /// </summary>
    private List<PlugControlRecord> children = new List<PlugControlRecord>();

    /// <summary>
    /// The constructor to create a PlugControlRecord.  sub classes should
    /// call this constructor from their own constructors using the base(...)
    /// mechanism
    /// </summary>
    /// <param name="deviceid">The assigned id of this control's associated device</param>
    /// <param name="controllid">The assigned id of this control</param>
    /// <param name="name">A device-unique name for this control</param>
    /// <param name="description">A human readable description of this control</param>
    /// <param name="ctype">The type of this control.  Can be Digital,Analog,
    /// Normalized, or Ascii.  </param>
    /// <param name="providerid">The assigned id of this control's associated provider</param>
    public PlugControlRecord(int providerid,int deviceid, int controllid, string name, string description, CTRL_TYPE ctype )
    {
        this.deviceid = deviceid;
        this.providerid = providerid;
        this.controllid = controllid;
        this.name = name;
        this.description = description;
        this.ctype = ctype;
    }

    /// <summary>
    /// Define record equality as being the same name and ids
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>True if name, device id, and provider id all match</returns>
    public override bool Equals(object obj)
    {
        PlugControlRecord other = obj as PlugControlRecord;
        return (Name == other.name)&&(DeviceID == other.DeviceID)
            &&(ProviderID==other.ProviderID);
    }

    /// <summary>
    /// The ID of the device providing this control. It is unique
    /// to the PlugInputProvider that provided thge device but may be
    /// duplicated across providers.  It is fixed
    /// for a given run but may change across runs.
    /// </summary>
    public int DeviceID {
        get
        {
            return deviceid;
        }
    }

    /// <summary>
    /// The ID of the PlugInputProvider providing this control. It is fixed
    /// for a given run but may change across runs.
    /// </summary>
    public int ProviderID
    {
        get
        {
            return providerid;
        }
    }

    /// <summary>
    /// The type of this control: Digital, Analog, Normalized or ASCII
    /// </summary>
    public CTRL_TYPE CtrlType
    {
        get
        {
            return ctype;
        }
    }

    /// <summary>
    /// The unqualified name of this control (see FQName below for
    /// the definition of a fully qualified name)
    /// </summary>
    public string Name
    {
        get
        {
            return name;
        }
    }

    /// <summary>
    /// A human readable description of this control
    /// </summary>
    public string Description
    {
        get
        {
            return description;
        }
    }

    /// <summary>
    /// CURRENTLY UNUSED: for future expansion
    /// </summary>
    public PlugControlRecord[] Children
    {
        get
        {
            return children.ToArray();
        }
    }

    /// <summary>
    /// Returns the Fully Qualified name of this control in the form
    /// &lt;provider name&rt;.&lt;device name&rt;.&lt;control name&rt;
    /// </summary>
    public string FQName {
        get
        {
            PlugInputProvider provider =
                PlugInputProviderManager.GetProviderByID(providerid);
            PlugDeviceRecord device = provider.GetDeviceByID(deviceid);
            return provider.Name + "." + device.Name + "." + Name;
        }
    }

    
    /// <summary>
    /// Type of a control or virtual axis.  Type "Control" is for future
    /// expansion and is currently unused.
    /// </summary>
    public enum CTRL_TYPE {Digital, Analog, Ascii, Normalized, Control}

    /// <summary>
    /// Called once per frame by the PlugInputProvider to allow
    /// this control to update its status.  All control callback events
    /// MUST be called from inside this method to avoid race conditions.
    /// </summary>
    /// <param name="provider">The PlugInputProvider associated eith
    /// this control</param>
    public abstract void UpdateInput(PlugInputProvider provider);
    
}
