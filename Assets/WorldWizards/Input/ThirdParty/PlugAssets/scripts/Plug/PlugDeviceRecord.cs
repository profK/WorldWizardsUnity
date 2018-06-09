using UnityEngine;
using System.Collections;
using System;


/// <summary>
/// This class is used to expose Plug controls to client code.
/// Its is an abstract class that is implemented by PlugInputProvider sub-classes
/// with their own logic for handling the gathering of input
/// </summary>
public class PlugDeviceRecord {
    // <summary>
    /// The ID assigned to this device.
    /// It is unique to its associated PlugInputProvider but may be duplicated
    /// across providers.
    /// This ID is fixed for the length of a run but can change between runs.
    /// </summary>
    private int deviceid;
    /// <summary>
    /// This array contains all the controls provided  by this device
    /// </summary>
    private PlugControlRecord[] plugControlRecords;
    /// <summary>
    /// This is a back-reference to the PlugUnitInputyProvider that provided
    /// this device
    /// </summary>
    private PlugInputProvider plugUnityProvider;
    /// <summary>
    /// A human readable name for this device.  It MUST be unique among
    /// devices provided by its associated provider but may be duplicated
    /// across providers.
    /// </summary>
    private string deviceName;
    /// <summary>
    /// A human readable description of this device
    /// </summary>
    private string description;

    /// <summary>
    /// The constructor to create a PlugControlRecord.  sub classes should
    /// call this constructor from their own constructors using the base(...)
    /// mechanism
    /// </summary>
    /// <param name="plugUnityProvider"> A reference to the provider sub-class
    /// instance that is providing this device</param>
    /// <param name="deviceid"> An id that is unique to the associated provider.
    /// It may be duplicated acrosss multiple distinct providers.  This ID
    /// ids fixed for any given run but may change between runs</param>
    /// <param name="name"> A human readable name for this device that is
    /// unique to the associated provider, but may be duplicated across 
    /// multiple distinct providers</param>
    /// <param name="description">A human readable description of this dvice</param>
    /// <param name="plugControlRecord">A optional list of controls provided
    /// by this device</param>
    public PlugDeviceRecord(PlugInputProvider plugUnityProvider, int deviceid, string name, string description, params PlugControlRecord[] plugControlRecord)
    {
        this.plugUnityProvider = plugUnityProvider;
        this.deviceid = deviceid;
        this.deviceName = name;
        this.description = description;
        this.plugControlRecords = plugControlRecord;
    }

   
    /// <summary>
    /// A list of all controls this device provides
    /// </summary>
    public PlugControlRecord[] Controls
    {
        get { return plugControlRecords; }
    }

    /// <summary>
    /// The non-qualified name of this device
    /// </summary>
    public string Name {
        get
        {
            return deviceName;
        }
    }

    /// <summary>
    /// The provider that provided this device
    /// </summary>
    public PlugInputProvider Provider {
        get
        {
            return plugUnityProvider;
        }
    }

    /// <summary>
    /// An id for this device that is unique to the associated provider.
    /// It may be duplicated acrosss multiple distinct providers.  This ID
    /// ids fixed for any given run but may change between runs
    /// </summary>
    public int ID {
        get
        {
            return deviceid;
        }
    }



    // Update is called once per frame
    /// <summary>
    /// This is called once per frame.  It has the responsability of
    /// calling UpdateInput on its provided controls, or otherwise
    /// updating all device state
    /// </summary>
    /// <param name="provider">A back pointer to the PlugInputProvider that
    /// provided this device</param>
    public virtual void UpdateInput (PlugInputProvider provider) {
	    //do controls
        foreach(PlugControlRecord ctrl in plugControlRecords)
        {
            ctrl.UpdateInput(provider);
        }
	}

    /// <summary>
    /// This method sets the list of controls provided by this
    /// device
    /// </summary>
    /// <param name="plugControlRecords"></param>
    internal void SetControls(PlugControlRecord[] plugControlRecords)
    {
        this.plugControlRecords = plugControlRecords;
    }
}
