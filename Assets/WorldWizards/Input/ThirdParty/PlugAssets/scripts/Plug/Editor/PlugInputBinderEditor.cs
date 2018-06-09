using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

[CustomEditor(typeof(PlugInputBinder))]
public class PlugInputBinderEditor : Editor
{
    private static bool showButtonBindings = false;
    private static bool showFloatBindings = false;
    private static bool showNormalizedBindings = false;
    private static bool showAsciiBindings = false;
    private static HashSet<string> openFoldouts = new HashSet<string>();



    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        //PlugInputBinder binder = (PlugInputBinder)target;
        //binder.UpdateVAxes();
        SerializedProperty bindings =
            serializedObject.FindProperty("boolBindings");
        showButtonBindings = 
           EditorGUILayout.Foldout(showButtonBindings, "Button Bindings");
        if (showButtonBindings)
        {
            DoBindings(ref bindings, bindings.arraySize);
        }

        bindings =
            serializedObject.FindProperty("floatBindings");
        showFloatBindings =
            EditorGUILayout.Foldout(showFloatBindings, "Analog Bindings");
        if (showFloatBindings)
        {
            DoBindings(ref bindings, bindings.arraySize);
        }

        bindings =
            serializedObject.FindProperty("normalizedBindings");
        showNormalizedBindings =
            EditorGUILayout.Foldout(showNormalizedBindings, "Normalized Bindings");
        if (showNormalizedBindings)
        {
            DoBindings(ref bindings, bindings.arraySize);
        }
        bindings =
            serializedObject.FindProperty("intBindings");
        showAsciiBindings =
            EditorGUILayout.Foldout(showAsciiBindings, "Ascii Bindings");
        if (showAsciiBindings)
        {
            DoBindings(ref bindings, bindings.arraySize);
        }
       
        serializedObject.ApplyModifiedProperties();
        
       
    }

  
    private void DoBindings(ref SerializedProperty bindings,int blen)
    {
        EditorGUI.indentLevel++;

        for (int i = 0; i < blen; i++)
        {
            SerializedProperty serNode = bindings.GetArrayElementAtIndex(i);
            SerializedProperty serName = serNode.FindPropertyRelative("vaxisName");
            if (DoFoldout(serName.stringValue))
            {
                //EditorGUI.indentLevel++;
                //EditorGUILayout.PropertyField(serName, true);
                SerializedProperty serEvt = serNode.FindPropertyRelative("Callbacks");
                EditorGUILayout.PropertyField(serEvt,true);
               
                //EditorGUI.indentLevel--;
            }
        }
        EditorGUI.indentLevel--;
        
    }

    private bool DoFoldout(string key)
    {
        bool fstate = EditorGUILayout.Foldout(
            openFoldouts.Contains(key),key);
        if ((fstate == true) && (!openFoldouts.Contains(key))){
            openFoldouts.Add(key);
        } else if ((fstate == false) && (openFoldouts.Contains(key)))
        {
            openFoldouts.Remove(key);
        }
        return fstate;
       
    }
}





