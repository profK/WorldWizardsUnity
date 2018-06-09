using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class PlugDumpProviders : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        PlugInputProviderManager.ScanProviders();
        IEnumerable<PlugInputProvider> providers = PlugInputProviderManager.EnabledProviders;
        foreach(PlugInputProvider provider in providers)
        {

            StringBuilder sb = new StringBuilder("Found provider " + provider.ToString()+"\n");
            if (provider.Devices != null)
            {
                foreach (PlugDeviceRecord dev in provider.Devices)
                {
                    sb.AppendLine("Device:" + dev.Name);
                    foreach (PlugControlRecord rec in dev.Controls)
                    {
                        RecurseControls(sb, rec, "    ");
                    }
                }
            }
            Debug.Log(sb);
        }
        
	}

    private void RecurseControls(StringBuilder sb, PlugControlRecord rec, string inset)
    {
        sb.AppendLine(inset + rec.Name);
        foreach(PlugControlRecord child in rec.Children)
        {
            RecurseControls(sb, child, inset + "    ");
        }
    }



    // Update is called once per frame
    void Update () {
		
	}
}
