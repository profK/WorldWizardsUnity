using UnityEngine;
using System.Collections;

/// <summary>
/// This is an abstract superclass for managed singletons
/// Managed Singletons are Monobehaviours that are gauranteed to have one and only one
/// implementation per scene.  They cna be accessed and edited through the ManagedSingletons parent object
/// at the scene root
/// </summary>
/// <typeparam name="SubT"></typeparam>
public abstract class ManagedSingleton<SubT> : MonoBehaviour  where SubT : ManagedSingleton<SubT>{
    private const string PNAME = "ManagedSingletons";
    private static SubT _instance;
    
    public static SubT Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject parentObj = GameObject.Find(PNAME);
                if (parentObj == null)
                {
                    parentObj = new GameObject(PNAME);
                }
                _instance = parentObj.GetComponentInChildren<SubT>();
                if (_instance == null)
                {
                    GameObject iobj = new GameObject(typeof(SubT).Name);
                    _instance = iobj.AddComponent<SubT>();
                    iobj.transform.parent = parentObj.transform;
                }
            }
            return _instance;
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	
}
