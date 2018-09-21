using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NewEditorMenuItems{
    [MenuItem("WWTools/Get Mesh Bounds")]
    private static void NewMenuOption()
    {
        GameObject obj = Selection.activeGameObject;
        MeshFilter mf = obj.GetComponentInChildren<MeshFilter>();
        if (mf == null)
        {
            Debug.Log("Mesh bounds error: no mesh selected");
        }
        else
        {
            Bounds mb = mf.sharedMesh.bounds;
            Debug.Log("Mesh Extents: (" + mb.extents.x + "," + mb.extents.y + "," + mb.extents.z + ")");
            Debug.Log("Mesh center: ("+mb.center.x+","+mb.center.y+","+mb.center.z+")");
        }
    }
	
}
