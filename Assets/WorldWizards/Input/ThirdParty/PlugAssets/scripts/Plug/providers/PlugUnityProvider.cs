using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// This provider serves as an interface between the classic Unity input system and 
/// Plug
/// </summary>
public class PlugUnityProvider : PlugInputProvider {

    private PlugTouchDeviceRecord[] touchDevices = new PlugTouchDeviceRecord[5];

    /// <summary>
    /// Constructor, called by the PlugImputProviderManager when
    /// an instance is created
    /// </summary>
    public PlugUnityProvider():base("UNITY")
    {
        /// This maps a Unity control ID to its associated device's slot in
        /// the plug device list
        Dictionary<int, int> unityToOEIMdevIDMap = new Dictionary<int, int>();
        /// This maps a device record's device iD to the device record itself
        Dictionary<int, PlugDeviceRecord> deviceRecords =
            new Dictionary<int, PlugDeviceRecord>();
        /// This maps a plug device record to all the controls it owns
        Dictionary<PlugDeviceRecord, List<PlugControlRecord>> controlRecords =
           new Dictionary<PlugDeviceRecord, List<PlugControlRecord>>();
        //This gets the list of current defined Unity Axes
        AxisList.AxisRec[] unityAxes = InputHelper.ListAxes();
        int deviceid = 0;
        int controllid = 0;
        //Make Keyboard, devid 0
        deviceRecords.Add(deviceid, new PlugDeviceRecord(this, deviceid, "Keyboard", "The Keyboard device",
            new PlugKBControlRecord(ID,deviceid, controllid, "Keyboard Input",
            "Returns keys pressed on keyboard", PlugControlRecord.CTRL_TYPE.Ascii)));
        deviceid++;
        controllid++;
        //Make Mouse, devid 1
        if (Input.mousePresent)
        {
            deviceRecords.Add(deviceid, new PlugDeviceRecord(this, deviceid, "Mouse", "The mouse device",
               new PlugMouseControlRecord(ID,deviceid, controllid++, "X",
               "Returns mouse X position", PlugControlRecord.CTRL_TYPE.Analog),
               new PlugMouseControlRecord(ID,deviceid, controllid++, "Y",
               "Returns mouse Y position", PlugControlRecord.CTRL_TYPE.Analog))
            );
        }
        deviceid++;
        if (Input.touchSupported)
        {
            for (int i = 0; i <5; i++)
            {
                touchDevices[i] = new PlugTouchDeviceRecord(this, deviceid, ref controllid,
                            "Touch" + i, "The touch input device");
                deviceRecords.Add(deviceid, touchDevices[i]);
                deviceid++;
            }
            deviceRecords.Add(deviceid, new PlugTouchCountDeviceRecord(this,deviceid,ref controllid,"TouchCount",
                "A device that returns just the numeber of touches when that changes"));
            deviceid++;

        }
        
        // Make Joysticks,dev 2 and up
        int axiscntr = -1;
        foreach (AxisList.AxisRec unityAxis in unityAxes)
        {
            axiscntr++;
            if (unityAxis.inputType == InputHelper.InputType.JoystickAxis)
            {
                Debug.Log("Axis  " + unityAxis.name + " thinks its a joystick axis");
                if (!unityToOEIMdevIDMap.ContainsKey(unityAxis.joynum))
                {
                    unityToOEIMdevIDMap.Add(unityAxis.joynum, deviceid);
                    deviceRecords.Add(deviceid,new PlugDeviceRecord(this, deviceid,
                        "Joy" + deviceid, "Unity Joystick" + deviceid));
                    deviceid++;
                }
                PlugDeviceRecord devRec = deviceRecords[unityToOEIMdevIDMap[unityAxis.joynum]];
                PlugControlRecord crec = new PlugJoyControlRecord(ID,
                    devRec.ID, controllid++, unityAxis.name, unityAxis.description,
                    PlugControlRecord.CTRL_TYPE.Normalized);
                if (!controlRecords.ContainsKey(devRec))
                {
                    controlRecords.Add(devRec, new List<PlugControlRecord>());
                }
                controlRecords[devRec].Add(crec);
            } else if (unityAxis.inputType == InputHelper.InputType.KeyOrMouseButton)
            {
                Debug.Log("Axis  " + unityAxis.name + " thinks its a button axis");
                if (!unityToOEIMdevIDMap.ContainsKey(unityAxis.joynum))
                {
                    unityToOEIMdevIDMap.Add(unityAxis.joynum, deviceid);
                    deviceRecords.Add(deviceid, new PlugDeviceRecord(this, deviceid,
                        "ButtonDevice" + deviceid, "Unity Buttons" + deviceid));
                    deviceid++;
                }
                PlugDeviceRecord devRec = deviceRecords[unityToOEIMdevIDMap[unityAxis.joynum]];
                PlugControlRecord crec = new PlugButtonControlRecord(ID,
                    devRec.ID, controllid++, unityAxis.name, unityAxis.description,
                    PlugControlRecord.CTRL_TYPE.Digital);
                if (!controlRecords.ContainsKey(devRec))
                {
                    controlRecords.Add(devRec, new List<PlugControlRecord>());
                }
                controlRecords[devRec].Add(crec);

            }
        }
        // put control records on devices
        Devices = new PlugDeviceRecord[deviceRecords.Values.Count];
        int idx = 0;
        foreach (PlugDeviceRecord joy in deviceRecords.Values)
        {
            if (controlRecords.ContainsKey(joy))
            {
                joy.SetControls(controlRecords[joy].ToArray());
            }
            // put devices in list
            Devices[idx++] = joy;
        }
    }

    /// <summary>
    /// This method is called by the input system to collect input events
    /// </summary>
    public override void UpdateInput()
    {
        if (IsEnabled())
        {
            foreach(PlugDeviceRecord dev in Devices)
            {
                if (dev.GetType() != typeof(PlugTouchDeviceRecord)) // Touch is a special case
                {  
                    foreach (PlugControlRecord ctrl in dev.Controls)
                    {
                        ctrl.UpdateInput(this);
                    }
                } else // handle touch
                {
                    for (int i=0;i<Input.touchCount;i++)
                    {
                        Touch touch = Input.GetTouch(i);
                        switch (touch.phase)
                        {
                            case TouchPhase.Began:
                                //find free slot and assign
                                foreach(PlugTouchDeviceRecord rec in touchDevices)
                                {
                                    if (rec.lastTouch.phase == TouchPhase.Ended)
                                    {
                                        rec.DoTouch(this,touch);
                                        break;
                                    }
                                }
                                break;
                            case TouchPhase.Moved:
                            case TouchPhase.Stationary:
                            case TouchPhase.Ended:
                            case TouchPhase.Canceled:
                                //find matching slot and process
                                foreach (PlugTouchDeviceRecord rec in touchDevices)
                                {
                                    if (rec.lastTouch.fingerId == touch.fingerId)
                                    {
                                        rec.DoTouch(this,touch);
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// If this method returns false then the instance of this class is never queried
    /// for input and discarded instead
    /// </summary>
    /// <returns></returns>
    public override Boolean IsEnabled()
    {
        // always available
        return true;
    }

    /// <summary>
    /// THis method tells the instance that the PlugInputProviderManager has registered
    /// all its callbacks and is ready for input
    /// </summary>
    internal override void BeginCallbacks()
    {
        // Throw device add callbacks for existing axes
		foreach (PlugDeviceRecord device in Devices) {
				SendDeviceAdded (device);
		}
    }

    /// <summary>
    /// A human readavble decription of this class
    /// </summary>
    public string Description
    {
        get
        {
            return "This class represents all of the stadnard Unity input mechanisms in the OEInput system.";
        }
    }

    //TODO Finish this device whct provides the touch count
    public class PlugTouchCountDeviceRecord : PlugDeviceRecord
    {
        

        public PlugTouchCountDeviceRecord(PlugUnityProvider plugUnityProvider, int deviceid, ref int controllid, string v1, string v2):
            base(plugUnityProvider,deviceid,v1,v2)
        {
            SetControls(new PlugControlRecord[] {
                    new PlugTouchCountControlRecord(plugUnityProvider.ID, deviceid, controllid++, "TouchCount",
                      "Returns change in number of touches", PlugControlRecord.CTRL_TYPE.Analog)
            });
        }

        private class PlugTouchCountControlRecord : PlugControlRecord
        {
            int lastTouches = 0;
            public PlugTouchCountControlRecord(int providerid, int deviceid, int controllid, string name, string description, CTRL_TYPE ctype) : base(providerid, deviceid, controllid, name, description, ctype)
            {
            }

            public override void UpdateInput(PlugInputProvider provider)
            {
                int touches = Input.touchCount;
                if (touches != lastTouches)
                {
                    lastTouches = touches;
                    provider.SendAxisChange(this, lastTouches);
                }
            }
        }
    }

    public class PlugTouchDeviceRecord : PlugDeviceRecord
    {
        internal Touch lastTouch = new Touch();
        

        public PlugTouchDeviceRecord(PlugInputProvider plugUnityProvider, int deviceid, ref int controllid, string name, string description, params PlugControlRecord[] plugControlRecord) :
            base(plugUnityProvider, deviceid, name, description, plugControlRecord)
        {
            SetControls(new PlugControlRecord[] {
                    new PlugTouchControlRecord(plugUnityProvider.ID, deviceid, controllid++, "X",
                      "Returns touch X position", PlugControlRecord.CTRL_TYPE.Analog),
                    new PlugTouchControlRecord(plugUnityProvider.ID, deviceid, controllid++, "Y",
                       "Returns touch Y position", PlugControlRecord.CTRL_TYPE.Analog),
                    new PlugTouchControlRecord(plugUnityProvider.ID, deviceid, controllid++, "Touched",
                       "Returns touch state", PlugControlRecord.CTRL_TYPE.Digital),
                    new PlugTouchControlRecord(plugUnityProvider.ID, deviceid, controllid++, "Pressure",
                       "Returns touch pressure", PlugControlRecord.CTRL_TYPE.Analog)
             });
            lastTouch.phase = TouchPhase.Ended;
        }

        public override void UpdateInput(PlugInputProvider provider)
        {
           
        }

        internal void DoTouch(PlugInputProvider provider,Touch touch)
        {
            foreach(PlugTouchControlRecord ctrl in Controls)
            {
                ctrl.DoTouch(provider,touch);
                lastTouch = touch;
            }
        }
    }
    

    #region Control Sub-classes
    /// <summary>
    /// This class represents the Unity Keyboard as an Ascii input device.
    /// It returns keystrokes sequentially
    /// </summary>
    private class PlugKBControlRecord: PlugControlRecord
    {
        
        /// <summary>
        /// This just passes the constructor params through to the PlugCOntrolRecord
        /// super-class
        /// <see cref="PlugControlRecord.PlugControlRecord( int, int, string, string, PlugControlRecord.CTRL_TYPE,int)"/>
        /// </summary>
       
        public PlugKBControlRecord(int providerid,int deviceid, int controllid, string v1, string v2, PlugControlRecord.CTRL_TYPE ctrltype):
            base(providerid, deviceid, controllid, v1, v2, ctrltype)
        {
           //nop
        }

        /// <summary>
        /// This is the method called by the device to poll thios control for input
        /// This control in return polls the Unity  keybpard and returns the current
        /// kb input as a series of callbacks for each letter
        /// </summary>
        /// <param name="provider"></param>
        public override void UpdateInput(PlugInputProvider provider)
        {
            string inp = Input.inputString;
            foreach(char c  in inp.ToCharArray())
            {
				provider.SendControlStateChange (this);
                provider.SendAsciiInput(this, c);
            }
        }
    }

    /// <summary>
    /// This class represents the Unity Mouse X and Y axes
    /// </summary>
    private class PlugMouseControlRecord:PlugControlRecord
    {
        /// <summary>
        /// This internal field is used to keep track of input so callbacks
        /// are only sent when a control value changes
        /// </summary>
        private Vector2 _pos = new Vector2(-1, -1); // impossible values
        public PlugMouseControlRecord(int providerid,int deviceid, int controllid, string name, string description, CTRL_TYPE ctype) : 
            base(providerid,deviceid, controllid, name, description, ctype)
        {
        }

        public override void UpdateInput(PlugInputProvider provider)
        {
			bool changed = false;
            switch (this.Name)
            {
                case "X":
                    if (Input.mousePosition.x != _pos.x)
                    {
                        _pos.x = Input.mousePosition.x;
						provider.SendControlStateChange(this);
                        provider.SendAxisChange(this, _pos.x);
                    }
                    break;
                case "Y":
                    if (Input.mousePosition.y != _pos.y)
                    {
                        _pos.y = Input.mousePosition.y;
						provider.SendControlStateChange (this);
                        provider.SendAxisChange(this, _pos.y);
                    }
                    break;
            }

        }
    }

    /// <summary>
    /// This class represents the Unity Mouse X and Y axes
    /// </summary>
    private class PlugTouchControlRecord : PlugControlRecord
    {
        /// <summary>
        /// This internal field is used to keep track of input so callbacks
        /// are only sent when a control value changes
        /// </summary>
        private Touch touch = new Touch(); // easy way to remember last state
        public PlugTouchControlRecord(int providerid, int deviceid, int controllid,string name, string description, CTRL_TYPE ctype) :
            base(providerid,deviceid, controllid, name, description, ctype)
        {
           
        }

        public override void UpdateInput(PlugInputProvider provider)
        {


        }

        internal void DoTouch(PlugInputProvider provider, Touch newTouch)
        {
            switch (this.Name)
            {
                case "X":
                    if (newTouch.position.x != touch.position.x)
                    {
                        touch = newTouch;
                        provider.SendControlStateChange(this);
                        provider.SendAxisChange(this, touch.position.x);
                    }
                    break;
                case "Y":
                    if (newTouch.position.y != touch.position.y)
                    {
                        touch = newTouch;
                        provider.SendControlStateChange(this);
                        provider.SendAxisChange(this, touch.position.y);
                    }
                    break;
                case "Touched":
                    if (newTouch.phase != touch.phase)
                    {
                        touch = newTouch;
                        provider.SendControlStateChange(this);
                        provider.SendButtonChange(this,
                            (touch.phase == TouchPhase.Began) && (touch.phase == TouchPhase.Moved));

                    }
                    break;
                case "Pressure":
                    if (newTouch.pressure != touch.pressure)
                    {
                        touch = newTouch;
                        provider.SendControlStateChange(this);
                        provider.SendAxisChange(this, touch.pressure);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// THis class represents a Unity defined Joystick Axis  
    /// </summary>
    private class PlugJoyControlRecord : PlugControlRecord
    {
        float _lastVal = -1;
        public PlugJoyControlRecord(int providerid, int deviceid, int controllid, string name, string description, CTRL_TYPE ctype) : 
            base(providerid,deviceid, controllid, name, description, ctype)
        {
        }

        public override void UpdateInput(PlugInputProvider provider)
        {
            float v = (Input.GetAxis(Name) + 1) / 2;
            if (v != _lastVal)
            {
                //Debug.Log("H:" + v);
                _lastVal = v;
				provider.SendControlStateChange (this);
                provider.SendNormalizedChange(this, _lastVal);
            }
        }
    }

    /// <summary>
    /// This class represents a Unity "Key or Button"
    /// </summary>
    private class PlugButtonControlRecord : PlugControlRecord
    {
        bool _lastVal = false;
        public PlugButtonControlRecord(int providerid, int deviceid, int controllid, string name, string description, CTRL_TYPE ctype) : 
            base(providerid,deviceid, controllid, name, description, ctype)
        {
        }

        public override void UpdateInput(PlugInputProvider provider)
        {
            bool v = Input.GetButton(Name);
            if (v != _lastVal)
            {
                _lastVal = v;
                provider.SendControlStateChange(this);
                provider.SendButtonChange(this, _lastVal);
            }
        }
    }
    #endregion
}

