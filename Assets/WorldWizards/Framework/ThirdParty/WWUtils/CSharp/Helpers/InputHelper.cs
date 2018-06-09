using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif //UNITY_EDITOR

public class InputHelper  {
    /// <summary>
    /// static initializer
    /// </summary>
    static InputHelper() {
        try {
            axisList = Resources.Load<AxisList>("UnityInputAxisList");
            if (axisList == null)
            {
                axisList = ScriptableObject.CreateInstance(typeof(AxisList)) as AxisList;
                Debug.Log("CReated axisLIst "+axisList.ToString());
                ReadAxes();
                Debug.Log("Read axis list");
#if UNITY_EDITOR
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateAsset(axisList, "Assets/Resources/UnityInputAxisList.asset");
                Debug.Log("CReateded asset");
                EditorUtility.SetDirty(axisList);
                AssetDatabase.SaveAssets();
                Debug.Log("saved asset");
#endif //UNITY_EDITOR
            }
            Debug.Log("AxisList = " + axisList);
        } catch (System.Exception e){
            Debug.Log("ERROR Initting Input");
            Debug.LogException(e);
        }
    }

    [System.Serializable]
    public enum InputType
    {
        KeyOrMouseButton,
        MouseMovement,
        JoystickAxis,
    };

    public static AxisList.AxisRec[] ListAxes()
    {
#if UNITY_EDITOR
        ReadAxes();
        EditorUtility.SetDirty(axisList);
        AssetDatabase.SaveAssets();
#endif
        return axisList.ListAxes();
    }

   

    private static AxisList axisList;

    /// <summary>
    /// This method ONLY works at edit time.  It reads the Project Settings to discover
    /// the current Inout axes.  It stores them in a SerializableObject for use at runtime.
    /// <Hack/>
    /// </summary>
    private static void ReadAxes()
    {
        Debug.Log("STarting axis read");
        axisList.Clear();

#if UNITY_EDITOR
        var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];

        Debug.Log("Loaded input manager " + inputManager);

        SerializedObject obj = new SerializedObject(inputManager);

        SerializedProperty axisArray = obj.FindProperty("m_Axes");

        if (axisArray.arraySize == 0)
            Debug.Log("No Axes");

        for (int i = 0; i < axisArray.arraySize; ++i)
        {
            var axis = axisArray.GetArrayElementAtIndex(i);

            var name = axis.FindPropertyRelative("m_Name").stringValue;
            Debug.Log("Read axis " + name);
            var axisVal = axis.FindPropertyRelative("axis").intValue;
            var inputType = (InputType)axis.FindPropertyRelative("type").intValue;
            var joynum = axis.FindPropertyRelative("joyNum").intValue;
            var descr = axis.FindPropertyRelative("descriptiveName").stringValue;
            axisList.Add(name,descr,axisVal,inputType,joynum);
        }
#endif //UNITY_EDITOR
    }




}

