using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlugVirtualAxes))]
public class PlugVirtualAxesEditor : Editor
{
    
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
