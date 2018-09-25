using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using WorldWizards.core.entity.common;
using WorldWizards.core.manager;

namespace WorldWizards.core.entity.gameObject
{
   
    [RequireComponent(typeof(EventTrigger))]
    public class Interactable : WWObject
    {
        private SceneGraphManager _sgc;
       

        private SceneGraphManager SGC
        {
            get
            {
                if (_sgc == null)
                {
                    _sgc = ManagerRegistry.Instance.GetAnInstance<SceneGraphManager>();
                }

                return _sgc;
            }
        }
        //dinosaur code
        private InteractionType interactionType;

       

        /*
        public void OnPointerClick(BaseEventData data)
        {
            PointerEventData ped = data as PointerEventData;
            if (ped.button == PointerEventData.InputButton.Right)
            {
                Debug.Log("Right click on interactable");
            }
            else if (SGC.GridController!=null)// pass off to grid if it exists
            {
                //Ray cast from collision poitnto grid
                RaycastHit hit;
                Ray ray = new Ray(ped.pointerPressRaycast.worldPosition,
                    ped.pressEventCamera.transform.forward);
                if (Physics.Raycast(ray,out hit,float.MaxValue,
                    1<<LayerMask.NameToLayer("TransparentFX"))){ 
                    
                    SGC.GridController.GetComponentInChildren<GridClickHandler>().
                        ClickedAt(hit.point);
                }

            }
        }*/
    }
}