using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MultiHitPhysicsRaycaster : PhysicsRaycaster
{
    private RaycastHit[] _hits;

    public RaycastHit[] LastHits
    {
        get { return _hits; }
    }
    protected MultiHitPhysicsRaycaster():base()
    {
    }

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        
        base.Raycast(eventData, resultAppendList);

        
    }
}
