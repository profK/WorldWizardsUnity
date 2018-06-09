using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NormalizedAxisSliderTest : MonoBehaviour {

    Slider slider;
	// Use this for initialization
	void Start () {
        slider = GetComponent<Slider>();
       
        slider.minValue = 0;
        slider.maxValue = 1.0f;
	}
	
	public void SetSlider(float v)
    {
        slider.value = v;
    }
}
