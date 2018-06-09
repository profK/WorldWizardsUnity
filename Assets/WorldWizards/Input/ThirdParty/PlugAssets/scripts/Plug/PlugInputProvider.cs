using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This is an abstract class that provides the necessary interface hooks
/// and common behavior for an Input Provider.  It should be sub-classed to
/// create "drivers" that talk to low-level input APIs.  See the included
/// PlugUnityInputProvider for a practical, working example.
/// 
/// This class also defines the event types for all the physical input events
/// </summary>
public abstract class PlugInputProvider
{
   
    #region event definitions
    /// <summary>
    /// An Ascii event is thrown whenever a keypress is registered
    /// 
    /// </summary>
    /// <param name="ctrl"> The control that received the keypress</param>
    /// <param name="c">The keypress recieved</param>
    public delegate void AsciiEvent(PlugControlRecord ctrl, char c);
    /// <summary>
    /// AN axis event is thrown whenever a control that returns a 
    /// non-normalized floating point value changes its value.
    /// </summary>
    /// <param name="ctrl">The control that generated the event</param>
    /// <param name="axisValue">The control's new value</param>
    public delegate void AxisEvent(PlugControlRecord ctrl, float axisValue);
    /// <summary>
    /// A normalized event is thrown whenever a control that returns a 
    /// normalized floating point value changes its value.  Normalized 
    /// values are 0.0 to 1.0 inclusive.
    /// </summary>
    /// <param name="ctrl">The control whose value changed</param>
    /// <param name="normValue">The control's new normalized value</param>
    public delegate void NormalizedEvent(PlugControlRecord ctrl, float normValue);
    /// <summary>
    /// A button event is thrown when a digital control changes its value
    /// from 0 (up in a button, or false in this callback) to a 1
    /// (down/true) or the reverse.
    /// 
    /// There are seperate ButtonDown and ButtonUp event callbacks, which are
    /// both of type ButtonEvent
    /// </summary>
    /// <param name="ctrl">The digital control that changed state</param>
    public delegate void ButtonEvent(PlugControlRecord ctrl);
    /// <summary>
    /// When a device is added or removed, the associated DeviceEvent
    /// callback is thrown.
    /// </summary>
    /// <param name="device">the device added or removed</param>
    public delegate void DeviceEvent(PlugDeviceRecord device);
    /// <summary>
    /// When any control chnages its state, this event gets thrownas well
    /// as the more sepcific typed event.
    /// It is useful for thinsg like the "show me" mechanism in the
    /// user control mapping panel
    /// </summary>
    /// <param name="ctrl">The control whose state changed</param>
    public delegate void ControlChangeEvent(PlugControlRecord ctrl);
    #endregion

    #region PlugInputProvider class def
    /// <summary>
    /// The ID of this input provider.  It is unique across input providers,
    /// and stable for the life of a run of the application, but may change between
    /// runs.
    /// </summary>
    public int ID
    {
        get; internal set;
    }

    /// <summary>
    /// base constructor 
    /// </summary>
    /// <param name="name">A unique provider name</param>
    public PlugInputProvider(string name)
    {
        Name = name;
        // this gets an unused ID from the PlugInputProviderManager
        ID = PlugInputProviderManager.NewID;
    }
    /// <summary>
    /// Returns the name of this input provider
    /// </summary>
    public virtual string Name
    {
        get; internal set;
    }
    /// <summary>
    /// returns an array of the currrently connected devices provided
    /// by this input provider
    /// </summary>
    public virtual PlugDeviceRecord[] Devices
    {
        get; internal set;
    }

    /// <summary>
    /// Returns a device based on its provider-assigned device ID
    /// </summary>
    /// <param name="providerid">The ID</param>
    /// <returns>The device record or null if there is no match</returns>
    public PlugDeviceRecord GetDeviceByID(int providerid) {
        foreach(PlugDeviceRecord drec in Devices)
        {
            if (drec.ID == providerid)
            {
                return drec;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns true if the provider can currently provide input
    /// </summary>
    /// <returns></returns>
    public abstract bool IsEnabled();

    /// <summary>
    /// This method is called  by the PlugInputProviderManager in order to 
    /// inform the provider that all callbacks have been registered with the appropriate events
    /// </summary>
    internal abstract void BeginCallbacks();

    /// <summary>
    /// This is called once per frame.  All input callbacks should happen
    /// within the context of this call to avoid race conditions.
    /// </summary>
    public abstract void UpdateInput();

    /// <summary>
    /// Add a listener to this event to receieve keystroke callbacks
    /// </summary>
    public event AsciiEvent AsciiInput;
    /// <summary>
    /// Add a listener to this event to recieve analog axis callbacks
    /// </summary>
    public event AxisEvent AxisChange;
    /// <summary>
    /// Add a listener to this event to receieve button down callbacks on
    /// digital axes
    /// </summary>
    public event ButtonEvent ButtonDown;
    /// <summary>
    /// Add a listener to this event to receieve button up callbacks on
    /// digital axes
    /// </summary>
    public event ButtonEvent ButtonUp;
    /// <summary>
    /// And a listener to this event to receieve normalized axis callbacks 
    /// </summary>
    public event NormalizedEvent NormalizedChanged;
    /// <summary>
    /// Add a listener to this event tor eceieve a callback when a new device
    /// becomes available
    /// </summary>
    public event DeviceEvent DeviceAdded;
    /// <summary>
    /// Add a listener to this event tor eceieve a callback when a device
    /// is no longer available
    /// </summary>
    public event DeviceEvent DeviceRemoved;
    /// <summary>
    /// Add a listener to this event to recieve a callback when any axis has
    /// a value change
    /// </summary>
    public event ControlChangeEvent ControlStateChange;
    #endregion

    #region Event send methods
    /// <summary>
    /// Call this method to dispatch callbacks to the AsciiInput event
    /// </summary>
    /// <param name="ctrl">The ctrl the event ocurred on</param>
    /// <param name="c">The character read on the control</param>
    internal void SendAsciiInput(PlugControlRecord ctrl, char c)
    {
        if (AsciiInput != null)
        {
            AsciiInput(ctrl, c);
        }
    }

    /// <summary>
    /// Call this method to dispatch callbacks to the AxisChange event
    /// </summary>
    /// <param name="ctrl">The ctrl the event ocurred on</param>
    /// <param name="v">The new value of the control</param>
    internal void SendAxisChange(PlugControlRecord ctrl, float v)
    {
        if (AxisChange != null)
        {
            AxisChange(ctrl, v);
        }
    }

    /// <summary>
    /// Call this method to dispatch callbacks to the NormlizedChanged event
    /// </summary>
    /// <param name="ctrl">The ctrl the event ocurred on</param>
    /// <param name="v">The new value of the control</param>
    internal void SendNormalizedChange(PlugControlRecord ctrl, float v)
    {
        if (NormalizedChanged != null)
        {
            NormalizedChanged(ctrl, v);
        }
    }

    /// <summary>
    /// Call this method to dispatch callbacks to the DeviceAdded event
    /// </summary>
    /// <param name="device">The device that was just added</param>
    internal void SendDeviceAdded(PlugDeviceRecord device)
    {
        if (DeviceAdded != null)
        {
            DeviceAdded(device);
        }
    }

    /// <summary>
    /// Call this method to dispatch callbacks to the DeviceRemoved event
    /// </summary>
    /// <param name="device">The device that was just added</param>
    internal void SendDeviceRemoved(PlugDeviceRecord device)
    {
        if (DeviceRemoved != null)
        {
            DeviceRemoved(device);
        }
    }

    /// <summary>
    /// Call this method to dispatch callbacks to the ButtonDown and ButtonUp
    /// events
    /// </summary>
    /// <param name="ctrl">The ctrl the event ocurred on</param>
    /// <param name="v">The new value of the control, ButtonDown is true,
    /// ButtonUp is false</param>
    internal void SendButtonChange(PlugControlRecord ctrl, bool v)
    {
        if (v) { // button down
            if (ButtonDown != null)
            {
                ButtonDown(ctrl);
            }
        } else {  // button up
            if (ButtonUp != null)
            {
                ButtonUp(ctrl);
            }
        }
        
    }

    /// <summary>
    /// Call this method to dispatch callbacks to the ControlStateChange event
    /// </summary>
    /// <param name="ctrl">The ctrl the event ocurred on</param>
    /// <param name="v">The new value of the control</param>
    internal void SendControlStateChange(PlugControlRecord sliderAxis)
    {
        if (ControlStateChange != null)
        {
            ControlStateChange(sliderAxis);
        }
    }
    #endregion
}
