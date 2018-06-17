using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragMove : MonoBehaviour
{
   

    public float scrollZoomSpeed = 0.01f;
    void Start()
    {

    }

   

    public void OnMove(BaseEventData ev)
    {
        
       SetDraggedPosition((PointerEventData) ev);
    }

    public void OnScroll(BaseEventData ev)
    {
        SetZoom((PointerEventData)ev);

    }

    private void SetZoom(PointerEventData ev)
    {
        RectTransform tf = transform as RectTransform;
        Vector2 zoom = ev.scrollDelta;
        tf.localScale = new Vector3(
            tf.localScale.x + (zoom.y*scrollZoomSpeed),
            tf.localScale.y + (zoom.y*scrollZoomSpeed),
            tf.localScale.z);

    }

    private void SetDraggedPosition(PointerEventData data)
    {
       
        RectTransform m_DraggingPlane = data.pointerEnter.transform as RectTransform;
        RectTransform rt = GetComponent<RectTransform>();
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = m_DraggingPlane.rotation;
        }
    }
}