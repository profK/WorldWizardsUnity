using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class PlugVirtualAxesWindow : EditorWindow
{
    // Add menu named "My Window" to the Window menu

    const float updatePeriodSec = 0.1f;
    private bool coroutineRunning;

    [MenuItem("Window/Virtual Axes")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        PlugVirtualAxesWindow window = (PlugVirtualAxesWindow)
            EditorWindow.GetWindow(typeof(PlugVirtualAxesWindow));
       
    }

    private Vector2 scrollPos = Vector2.zero;
   

    public void Awake()
    {
        PlugInputProviderManager.DeviceAdded += PlugInputProviderManager_DeviceAdded;
        PlugInputProviderManager.DeviceRemoved += PlugInputProviderManager_DeviceRemoved;
        PlugInputProviderManager.ScanProviders();
        Repaint();
    }

    public void OnDestroy()
    {
        PlugInputProviderManager.DeviceAdded -= PlugInputProviderManager_DeviceAdded;
        PlugInputProviderManager.DeviceRemoved -= PlugInputProviderManager_DeviceRemoved;
    }

    private void PlugInputProviderManager_DeviceRemoved(PlugDeviceRecord device)
    {
        Repaint();
    }

    private void PlugInputProviderManager_DeviceAdded(PlugDeviceRecord device)
    {
        Repaint();
    }

    void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUI.enabled = false;
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();

        List<PlugVirtualAxes.PlugVirtualAxis> deleteList = 
            new List<PlugVirtualAxes.PlugVirtualAxis>();
        foreach (PlugVirtualAxes.PlugVirtualAxis va in PlugVirtualAxes.Instance.VirtualAxes)
        {
            EditorGUILayout.BeginHorizontal();
            if (va.Name == null) va.Name = "";
            va.Name = EditorGUILayout.TextField(va.Name);
            PlugControlRecord.CTRL_TYPE newType;
            newType = (PlugControlRecord.CTRL_TYPE)EditorGUILayout.EnumPopup(va.CtrlType);
            if (newType != va.CtrlType)
            {
                va.CtrlType = newType;
                va.PhysicalAxis = null;
                GUI.changed = true;
            }
            if (GUILayout.Button("X"))
            {
                deleteList.Add(va);

            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Physical Axis:");
            string[] axesAvailable = PlugInputProviderManager.GetControlNamesOfType(va.CtrlType);
            if (axesAvailable.Length > 0)
            {  // skip of there are none to assign it to

                //if new set default
                if (va.PhysicalAxis == null)
                {
                    va.PhysicalAxis = axesAvailable[0];
                }
                // now look for axis index
                int selectedAxis = 0;
                for (int i = 0; i < axesAvailable.Length; i++)
                {
                    if (axesAvailable[i] == va.PhysicalAxis)
                    {
                        selectedAxis = i;
                    }
                }
                int newAxis = EditorGUILayout.Popup(selectedAxis, axesAvailable);
                if (newAxis != selectedAxis)
                {
                    selectedAxis = newAxis;
                    va.PhysicalAxis = axesAvailable[selectedAxis];
                    if (va.PhysicalAxis != null)
                    {
                        PlugControlRecord ctrl = PlugInputProviderManager.GetControlByName(va.PhysicalAxis);
                        if (ctrl != null)
                        {
                            if (GUI.enabled)
                            {
                                PlugVirtualAxes.Instance.MapVirtualAxis(va, ctrl);
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Add Virtual Axis"))
        {
            //Debug.Log("aDD AXIS");
            PlugVirtualAxes.Instance.AddAxis();
            GUI.changed = true;
            //Debug.Log("Number of aXES=" + PlugVirtualAxes.Instance.VirtualAxes.Count);
        }
        foreach(PlugVirtualAxes.PlugVirtualAxis va in deleteList)
        {
            PlugVirtualAxes.Instance.RemoveVirtualAxis(va);
        }
        if (GUILayout.Button("Reload Providers"))
        {
           
            PlugInputProviderManager.ScanProviders();
            Repaint();
        }
        if (EditorGUI.EndChangeCheck())
        {
            // save axes as XML file
            PlugVirtualAxes.Instance.Save();
        }
        if (Application.isPlaying)
        {
            GUI.enabled = true;
        }
    }

    //coroutine for polling device additions
    public void Update() 
    {
        PlugInputProviderManager.UpdateInput();
    }

}