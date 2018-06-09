using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

/// <summary>
/// This class finds and organizes the available PlugInputProviders.
/// It is implemented as a static class which is a class that has no
/// instances.  It acts as a form of singleton.
/// </summary>
public static class PlugInputProviderManager
{
    /// <summary>
    /// This is a static constructor that gets run when this
    /// class is loaded (which is the first time its referenced.)
    /// </summary>
    static PlugInputProviderManager()
    {
        ScanProviders();
    }

    /// <summary>
    /// This dictionary serves as an index and allows PlugInputProvider instances
    /// to be found based on their Type.  It also is used to ensure that
    /// only one instance of each available provider is created.
    /// </summary>
    static Dictionary<Type, PlugInputProvider> _providers = new Dictionary<Type, PlugInputProvider>();
    /// <summary>
    /// This is used to find provider instances based on a provider ID assigned
    /// when the instance is created
    /// </summary>
    static Dictionary<int, PlugInputProvider> _providersByID = new Dictionary<int, PlugInputProvider>();

    /// <summary>
    /// This property provides access to all the provider instances created
    /// </summary>
    static public IEnumerable<PlugInputProvider> EnabledProviders
    {
        get
        {
            return _providers.Values;
        }
    }

    /// <summary>
    /// This internal variuable is used to keep track of assigned provider IDs
    /// </summary>
    static private int providerid = 1;


    /// <summary>
    /// This proeprty returns a unique ID every time it is accessed
    /// </summary>
    static public int NewID
    {
        get
        {
            return providerid++;
        }
    }

    /// <summary>
    /// THis method is called to find PlugInputProviders and create instances
    /// of any that don't yet have them
    /// </summary>
    static public void ScanProviders()
    {
        // This is a method in TypeHelpers that gets all types that implement
        // a given parent type or interface
        Type[] allProviders = TypeHelper.GetAllDefinedSubTypes(typeof(PlugInputProvider));

        foreach (Type t in allProviders)
        {
            if (!t.IsInterface && !t.IsAbstract && !_providers.ContainsKey(t))
            {
                PlugInputProvider newProvider = (PlugInputProvider)Activator.CreateInstance(t);
                
                if (newProvider.IsEnabled())
                {
                    _providers.Add(t, newProvider);
                    _providersByID.Add(newProvider.ID, newProvider);
                    newProvider.AsciiInput += NewProvider_AsciiInput;
                    newProvider.AxisChange += NewProvider_AxisChange;
                    newProvider.ButtonDown += NewProvider_ButtonDown;
                    newProvider.ButtonUp += NewProvider_ButtonUp;
                    newProvider.NormalizedChanged += NewProvider_NormalizedChanged;
                    newProvider.DeviceAdded += NewProvider_DeviceAdded;
                    newProvider.DeviceRemoved += NewProvider_DeviceRemoved;
                    newProvider.ControlStateChange += NewProvider_ControlStateChange;
                    newProvider.BeginCallbacks();
                }
            }
        }
    }

    /// <summary>
    /// This returns the instance a PlugInputProvider base on its assigned id
    /// </summary>
    /// <param name="providerid">The ID to look up</param>
    /// <returns>the instance assigend that ID, or null if there is none</returns>
    internal static PlugInputProvider GetProviderByID(int providerid)
    {
        return _providersByID[providerid];
    }

    /// <summary>
    /// This function looks up and returns a specific control by its Fully
    /// Qualified Name.  Fully qualified names have the form 
    /// &lt;provider name&rt;.&lt;device name&rt;.&lt;control name&rt;
    /// 
    /// </summary>
    /// <param name="physicalAxis">The fully qualified name of the control</param>
    /// <returns>The named control or null if there is no cotrol with that FQN</returns>
    public static PlugControlRecord GetControlByName(string physicalAxis)
    {
        string[] nameArray = physicalAxis.Split('.');
        foreach(PlugInputProvider provider in _providers.Values)
        {
            if (provider.Name == nameArray[0])
            {
                foreach(PlugDeviceRecord drec in provider.Devices)
                {
                    if (drec.Name == nameArray[1])
                    {
                        int i = 2;
                        foreach (PlugControlRecord crec in drec.Controls) { 
                            PlugControlRecord rec =
                                RecurseGetControlByName(crec,nameArray, i);
                            if (rec != null)
                            {
                                return rec;
                            }
                        }
                    }
                   
                }
            }
        }
        return null;
    }

    /// <summary>
    /// This is used internally to match FQNs.  Its recursive to allow
    /// for future controls that may have sub-controls
    /// </summary>
    /// <param name="crec">the current record being compared</param>
    /// <param name="nameArray">the list of names in the FQN</param>
    /// <param name="i">the current level being comnpared</param>
    /// <returns>the control or  null if none match</returns>
    private static PlugControlRecord RecurseGetControlByName(
        PlugControlRecord crec, string[] nameArray, int i)
    {
        if (crec.Name != nameArray[i])
        {
            return null;
        } else if (i==nameArray.Length-1) {
            return crec;
        } else
        {
            return RecurseGetControlByName(crec, nameArray, i++);
        }
    }

    #region event callback handlers
    /// <summary>
    /// This receives ControlStateChange events from PlugInputProvider
    /// instances
    /// </summary>
    /// <param name="ctrl">the control whose value has changed</param>
    private static void NewProvider_ControlStateChange(PlugControlRecord ctrl)
    {
        if (ControlStateChange != null)
        {
            ControlStateChange(ctrl);
        }
    }

    /// <summary>
    /// This receives DeviceRemove events from PlugInputProvider
    /// instances
    /// </summary>
    /// <param name="device">the device no longer present</param>
    private static void NewProvider_DeviceRemoved(PlugDeviceRecord device)
    {
        if (DeviceRemoved != null)
        {
            DeviceRemoved(device);
        }
    }

    /// <summary>
    /// This receives DeviceAdded events from PlugInputProvider
    /// instances
    /// </summary>
    /// <param name="device">the device added</param>
    private static void NewProvider_DeviceAdded(PlugDeviceRecord device)
    {
        if (_DeviceAdded != null)
        {
            _DeviceAdded(device);
        }
    }

    #endregion

    #region events
    /// <summary>
    /// client code adds a listener to this event to get keyboard input
    /// </summary>
    public static event PlugInputProvider.AsciiEvent AsciiInput;
    /// <summary>
    /// client code adds a listener to this event to get Analog axis events
    /// </summary>
    public static event PlugInputProvider.AxisEvent AxisChange;
    /// <summary>
    /// client code adds a listener to this event to get button down events from
    /// Digital axes
    /// </summary>
    public static event PlugInputProvider.ButtonEvent ButtonDown;
    /// <summary>
    /// client code adds a listener to this event to get button up events from
    /// Digital axes
    /// </summary>
    public static event PlugInputProvider.ButtonEvent ButtonUp;
    /// <summary>
    /// client code adds a listener to this event to get Normalized axis events
    /// </summary>
    public static event PlugInputProvider.NormalizedEvent NormalizedChanged;
    /// <summary>
    /// This is a private event that holds the actual callbacks for device
    /// added events
    /// </summary>
    private static event PlugInputProvider.DeviceEvent _DeviceAdded;
    /// <summary>
    /// This property is used to trigger code on the addition of a
    /// DeviceAdded listener.  The triggered code responds by sending
    /// callbacks for each of the already added devices tothe newly
    /// registered listener
    /// </summary>
    public static event PlugInputProvider.DeviceEvent DeviceAdded
    {
        add
        {
            _DeviceAdded += value;
            foreach (PlugInputProvider provider in _providers.Values)
            {
                foreach(PlugDeviceRecord device in provider.Devices)
                {
                    value(device);
                }
            }
        }

        remove
        {
            _DeviceAdded -= value;
        }
    }
    /// <summary>
    /// client code adds a listener to this event to get DeviceRemoved events
    /// </summary
    public static event PlugInputProvider.DeviceEvent DeviceRemoved;
    /// <summary>
    /// client code adds a listener to this event to get an event callback
    /// when any control changes its value
    /// </summary
    public static event PlugInputProvider.ControlChangeEvent ControlStateChange;
    #endregion

    #region EventHandlers
    /// <summary>
    /// This method is registered as a NormalizedChanged callback with every
    /// provider instance
    /// </summary>
    /// <param name="ctrl">the ctrl whose value changed</param>
    /// <param name="normValue">the new value</param>
    private static void NewProvider_NormalizedChanged(PlugControlRecord ctrl, float normValue)
    {
        if (NormalizedChanged != null)
        {
            NormalizedChanged(ctrl, normValue);
        }
    }

    /// <summary>
    /// This method is registered as a ButtonUp callback with every
    /// provider instance
    /// </summary>
    /// <param name="ctrl">the ctrl whose value changed</param>
    
    private static void NewProvider_ButtonUp(PlugControlRecord ctrl)
    {
        if (ButtonDown != null)
        {
            ButtonUp(ctrl);
        }
    }

    /// <summary>
    /// This method is registered as a ButtonDown callback with every
    /// provider instance
    /// </summary>
    /// <param name="ctrl">the ctrl whose value changed</param>

    private static void NewProvider_ButtonDown(PlugControlRecord ctrl)
    {
        if (ButtonUp != null)
        {
            ButtonDown(ctrl);
        }
    }

    /// <summary>
    /// This method is registered as a AxisChange callback with every
    /// provider instance
    /// </summary>
    /// <param name="ctrl">the ctrl whose value changed</param>
    /// <param name="axisValue">the new value</param>
    private static void NewProvider_AxisChange(PlugControlRecord ctrl, float axisValue)
    {
        if (AxisChange != null)
        {
            AxisChange(ctrl, axisValue);
        }
    }

    /// <summary>
    /// This method is registered as a AsciiInput callback with every
    /// provider instance
    /// </summary>
    /// <param name="ctrl">the ctrl which reported a keystroke</param>
    /// <param name="c">the key stroke</param>
    private static void NewProvider_AsciiInput(PlugControlRecord ctrl, char c)
    {
        if (AsciiInput != null)
        {
            AsciiInput(ctrl, c);
        }
    }
    #endregion

    #region ctrl names

    /// <summary>
    /// This function returns the FQN of all controls currently available that
    /// are of the given control type.
    /// </summary>
    /// <param name="ctrlType">The type of control to return</param>
    /// <returns>an array of the FQNs of the matching controls</returns>
    public static string[] GetControlNamesOfType(PlugControlRecord.CTRL_TYPE ctrlType)
    {
        List<string> axesNames = new List<string>();
        foreach (PlugInputProvider p in EnabledProviders)
        {
            if (p.Devices != null)
            {
                foreach (PlugDeviceRecord d in p.Devices)
                {
                    axesNames.AddRange(RecursiveControlNames(
                        p.Name,d, ctrlType));
                }
            }
        }
        return axesNames.ToArray();
    }

    /// <summary>
    /// Returns the FQNs of all currently available controls
    /// </summary>
    /// <returns>an array of the  FQNs of all available controls</returns>
    public static string[] GetControlNames()
    {
        List<string> axesNames = new List<string>();
        foreach (PlugInputProvider p in EnabledProviders)
        {
            if (p.Devices != null)
            {
                foreach (PlugDeviceRecord d in p.Devices)
                {
                    axesNames.AddRange(RecursiveControlNames(p.Name,d));
                }
            }
        }
        return axesNames.ToArray();
    }

    /// <summary>
    /// This is used to recurse a device's list of controls and collect
    /// all the FQNs
    /// </summary>
    /// <param name="providerName">the name of the provider being parsed</param>
    /// <param name="d">the device to qiery</param>
    /// <returns>an enumerable list of the FQNs for the indicated device's
    /// controls</returns>
    private static IEnumerable<string> RecursiveControlNames(
        string providerName, PlugDeviceRecord d)
    {
        List<string> controlNames = new List<string>();
        foreach (PlugControlRecord ctrl in d.Controls)
        {
            IEnumerable<string> names =
                RecurseControlsForNames(
                    providerName+"."+d.Name, ctrl);
            controlNames.AddRange(names);
        }
        return controlNames;
    }

    /// <summary>
    /// This recrses the tree of controls and sub controls.
    /// It exists for future expansion as sub-controls are not yet
    /// supported
    /// </summary>
    /// <param name="parentName">name of parents (could be more then one seperated by periods)</param>
    /// <param name="ctrl">current control to parse</param>
    /// <returns>an enumerable list of FQNs</returns>
    private static IEnumerable<string> RecurseControlsForNames(string parentName, PlugControlRecord ctrl)
    {
        if (ctrl.CtrlType == PlugControlRecord.CTRL_TYPE.Control)
        {
            List<string> ctrls = new List<string>();
            foreach (PlugControlRecord child in ctrl.Children)
            {
                ctrls.AddRange(RecurseControlsForNames(
                    parentName + "." + ctrl.Name, child));

            }
            return ctrls;
        }
        else
        {
            return new string[] { parentName + "." + ctrl.Name };
        }

    }

    /// <summary>
    /// This is used to recurse a device's list of controls and collect
    /// all the FQNs of controls that match the passed in control type
    /// </summary>
    /// <param name="providerName">the name of the provider being parsed</param>
    /// <param name="d">the device to qiery</param>
    /// <param name="ctrlType">the type to match</param>
    /// <returns>an enumerable list of the FQNs for the indicated device's
    /// controls</returns>
    private static IEnumerable<string> RecursiveControlNames(
        string parentName, PlugDeviceRecord d, PlugControlRecord.CTRL_TYPE ctrlType)
    {
        List<string> controlNames = new List<string>();
        foreach (PlugControlRecord ctrl in d.Controls)
        {
            IEnumerable<string> names =
                RecurseControlsForNames(parentName+"."+d.Name, ctrl, ctrlType);
            controlNames.AddRange(names);
        }
        return controlNames;
    }

    /// <summary>
    /// This recrses the tree of controls and sub controls returning only those
    /// that match the passed in control type.
    /// It exists for future expansion as sub-controls are not yet
    /// supported
    /// </summary>
    /// <param name="parentName">name of parents (could be more then one seperated by periods)</param>
    /// <param name="ctrl">current control to parse</paramName>
    /// <param name="filter">the type to match</param>
    /// <return>an enumerable list of FQNs of the controls that match the
    /// passed iin control type</return>
    private static IEnumerable<string> RecurseControlsForNames(string parentName, PlugControlRecord ctrl, PlugControlRecord.CTRL_TYPE filter)
    {
        if (ctrl.CtrlType == PlugControlRecord.CTRL_TYPE.Control)
        {
            List<string> ctrls = new List<string>();
            foreach (PlugControlRecord child in ctrl.Children)
            {
                ctrls.AddRange(RecurseControlsForNames(
                    parentName + "." + ctrl.Name, child, filter));

            }
            return ctrls;
        }
        else if (filter == ctrl.CtrlType)
        {
            return new string[] { parentName + "." + ctrl.Name };
        }
        else
        {
            return new string[0];
        }
    }
    #endregion

    /// <summary>
    /// THis method is called by client code once per frame in order to test for 
    /// change in the controls.  All event callbacks are sent from within the 
    /// context of this call.
    /// </summary>
    public static void UpdateInput()
    {
        lock (_providers)
        {
            foreach (PlugInputProvider prov in _providers.Values)
            {
                Profiler.BeginSample("Update input " + prov.Name);
                prov.UpdateInput();
                Profiler.EndSample();
            }
        }
    }
}