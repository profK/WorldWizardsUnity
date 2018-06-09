using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

/// <summary>
/// This component handles receiving virtualk axis callbacks and dispatch calls to
/// connected UnityEvent listeners
/// </summary>
[ExecuteInEditMode]
public class PlugInputBinder : ManagedSingleton<PlugInputBinder> {
    /// <summary>
    /// A UnityEvent that takes a single boolean parameter
    /// </summary>
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    /// <summary>
    /// A  UnityEvent that takes a single float parameter
    /// </summary>
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    /// <summary>
    /// A UntiyEvent that takes a single  integer parameter
    /// </summary>
    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }

    /// <summary>
    /// A data record containing the binding of a Digital
    /// virtual control to a UnityEVent that takes a boolean parameter
    /// </summary>
    [System.Serializable]
    public class BoolBindingNode  {
        public string vaxisName;
        public BoolEvent Callbacks;

        public BoolBindingNode(string n)  {
            vaxisName = n;
            Callbacks = new BoolEvent();
        }

        /// <summary>
        /// This makes two nodes with the same virtual axis name logically equal
        /// to each other
        /// </summary>
        /// <param name="obj">The other node</param>
        /// <returns>true if the names are the same, otehrwise false</returns>
        public override bool Equals(object obj)
        {
            BoolBindingNode other = (BoolBindingNode)obj;
            return vaxisName==other.vaxisName;
        }
        
    }

    /// <summary>
    /// A data record containing the binding of an Analog or Normalized
    /// virtual control to a UnityEVent that takes a float parameter
    /// </summary>
    [System.Serializable]
    public class FloatBindingNode 
    {
        public string vaxisName;
        public FloatEvent Callbacks;

        public FloatBindingNode(string n)
        {
            vaxisName = n;
            Callbacks = new FloatEvent();
        }

        /// <summary>
        /// This makes two nodes with the same virtual axis name logically equal
        /// to each other
        /// </summary>
        /// <param name="obj">The other node</param>
        /// <returns>true if the names are the same, otehrwise false</returns>
        public override bool Equals(object obj)
        {
            FloatBindingNode other = (FloatBindingNode)obj;
            return vaxisName == other.vaxisName;
        }

    }

    /// <summary>
    /// A data record containing the binding of an Ascii
    /// virtual control to a UnityEvent that takes an integer parameter
    /// </summary>
    [System.Serializable]
    public class IntBindingNode
    {
        public string vaxisName;
        public IntEvent Callbacks;
        public IntBindingNode(string n)
        {
            vaxisName = n;
            Callbacks = new IntEvent();
        }

        /// <summary>
        /// This makes two nodes with the same virtual axis name logically equal
        /// to each other
        /// </summary>
        /// <param name="obj">The other node</param>
        /// <returns>true if the names are the same, otehrwise false</returns>
        public override bool Equals(object obj)
        {
            IntBindingNode other = (IntBindingNode)obj;
            return vaxisName == other.vaxisName;
        }

    }

    
    /// <summary>
    /// An array of all defined digital control bindings
    /// </summary>
    public BoolBindingNode[] boolBindings;
    /// <summary>
    /// An array of all defined analog control bindings
    /// </summary>
    public FloatBindingNode[] floatBindings;
    /// <summary>
    ///  An array of all defined normalized control bindings
    /// </summary>
    public FloatBindingNode[] normalizedBindings;
    /// <summary>
    ///  An array of all defined ascii control bindings
    /// </summary>
    public IntBindingNode[] intBindings;


    // Use this for initialization
    void Start () {
        //attach vertual axes
        PlugVirtualAxes.Instance.AsciiInput += PlugVirtualAxes_AsciiInput;
        PlugVirtualAxes.Instance.NormalizedChanged += PlugVirtualAxes_NormalizedChanged;
        PlugVirtualAxes.Instance.AxisChange += PlugVirtualAxes_AxisChange;
        PlugVirtualAxes.Instance.ButtonDown += PlugVirtualAxes_ButtonDown;
        PlugVirtualAxes.Instance.ButtonUp += PlugVirtualAxes_ButtonUp;
        UpdateVAxes();
    }

    #region control event handlers
    /// <summary>
    /// A callback for Normalized Virtual controls changing events
    /// </summary>
    /// <param name="axis">the control whose value changed</param>
    /// <param name="normValue">the new value</param>
    private void PlugVirtualAxes_NormalizedChanged(PlugVirtualAxes.PlugVirtualAxis axis, float normValue)
    {
        foreach (FloatBindingNode node in normalizedBindings)
        {
            if (axis.Name == node.vaxisName)
            {
                if (node.Callbacks != null)
                {
                    node.Callbacks.Invoke(normValue);
                }
            }
        }
    }

    /// <summary>
    /// A callback for Digital Virtual controls transitioning to up state
    /// </summary>
    /// <param name="axis">the control whose value changed</param>
    private void PlugVirtualAxes_ButtonUp(PlugVirtualAxes.PlugVirtualAxis axis)
    {
        foreach(BoolBindingNode node in boolBindings)
        {
            if (axis.Name == node.vaxisName)
            {
                if (node.Callbacks != null)
                {
                    node.Callbacks.Invoke(false);
                }
            }
        }
    }

    /// <summary>
    /// A callback for Digital Virtual control transitioning to the down state
    /// </summary>
    /// <param name="axis">the control whose value changed</param>
    private void PlugVirtualAxes_ButtonDown(PlugVirtualAxes.PlugVirtualAxis axis)
    {
        foreach (BoolBindingNode node in boolBindings)
        {
            if (axis.Name == node.vaxisName)
            {
                if (node.Callbacks != null)
                {
                    node.Callbacks.Invoke(true);
                }
            }
        }
    }

    /// <summary>
    /// A callback for Analog Virtual controls changing events
    /// </summary>
    /// <param name="axis">the control whose value changed</param>
    /// <param name="axisValue">the new value</param>
    private void PlugVirtualAxes_AxisChange(PlugVirtualAxes.PlugVirtualAxis axis, float axisValue)
    {
        foreach (FloatBindingNode node in floatBindings)
        {
            if (axis.Name == node.vaxisName)
            {
                if (node.Callbacks != null)
                {
                    node.Callbacks.Invoke(axisValue);
                }
            }
        }
    }

    /// <summary>
    /// A callback for Ascii Virtual controls reporting keystrokes
    /// </summary>
    /// <param name="axis">the control reporting </param>
    /// <param name="c">the reported key</param>
    private void PlugVirtualAxes_AsciiInput(PlugVirtualAxes.PlugVirtualAxis axis, char c)
    {
        foreach (IntBindingNode node in intBindings)
        {
            if (axis.Name == node.vaxisName)
            {
                if (node.Callbacks != null)
                {
                    node.Callbacks.Invoke(c);
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// This method scans the virtual controls and makes matching node for each
    /// one.  If there already exists a node for a control then a new node is not
    /// created.
    /// </summary>
    public void UpdateVAxes()
    {
        List<BoolBindingNode> bnodes = new List<BoolBindingNode>();
        List<FloatBindingNode> fnodes = new List<FloatBindingNode>();
        List<FloatBindingNode> nnodes = new List<FloatBindingNode>();
        List<IntBindingNode> inodes = new List<IntBindingNode>();
        foreach (PlugVirtualAxes.PlugVirtualAxis vaxis in
            PlugVirtualAxes.Instance.VirtualAxes)
        {
            switch (vaxis.CtrlType)
            {
                case PlugControlRecord.CTRL_TYPE.Analog:
                    FloatBindingNode fnode = new FloatBindingNode(vaxis.Name);
                    fnodes.Add(fnode);
                    break;
                case PlugControlRecord.CTRL_TYPE.Digital:
                    BoolBindingNode bnode = new BoolBindingNode(vaxis.Name);
                    bnodes.Add(bnode);
                    break;
                case PlugControlRecord.CTRL_TYPE.Normalized:
                    fnode = new FloatBindingNode(vaxis.Name);
                    nnodes.Add(fnode);
                    break;
                case PlugControlRecord.CTRL_TYPE.Ascii:
                    IntBindingNode inode =new IntBindingNode(vaxis.Name);
                    inodes.Add(inode);
                    break;
            }
        }
        List<BoolBindingNode> newBool = new List<BoolBindingNode>();
        newBool.AddRange(
            bnodes.Where(node => !boolBindings.Contains(node)));
        newBool.AddRange(boolBindings);
        boolBindings = newBool.ToArray();
        List<IntBindingNode> newInt = new List<IntBindingNode>();
        newInt.AddRange(inodes.Where(node => !intBindings.Contains(node)));
        newInt.AddRange(intBindings);
        intBindings = newInt.ToArray();
        List<FloatBindingNode> newFloat = new List<FloatBindingNode>();
        newFloat.AddRange(fnodes.Where(node => !floatBindings.Contains(node)));
        newFloat.AddRange(floatBindings);
        floatBindings = newFloat.ToArray();
        List<FloatBindingNode> newNorm = new List<FloatBindingNode>();
        newNorm.AddRange(
            nnodes.Where(node => !normalizedBindings.Contains(node)));
        newNorm.AddRange(normalizedBindings);
        normalizedBindings = newNorm.ToArray();
    }

    private void OnApplicationQuit()
    {
        PlugVirtualAxes.Instance.AsciiInput -= PlugVirtualAxes_AsciiInput;
        PlugVirtualAxes.Instance.NormalizedChanged -= PlugVirtualAxes_NormalizedChanged;
        PlugVirtualAxes.Instance.AxisChange -= PlugVirtualAxes_AxisChange;
        PlugVirtualAxes.Instance.ButtonDown -= PlugVirtualAxes_ButtonDown;
        PlugVirtualAxes.Instance.ButtonUp -= PlugVirtualAxes_ButtonUp;
    }


    // Update is called once per frame

    void Update () {
		if (Application.isPlaying)
        {
            PlugInputProviderManager.UpdateInput();
        }
	}
}
