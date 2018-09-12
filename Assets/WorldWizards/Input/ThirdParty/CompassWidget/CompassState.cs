using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WWUtils;

namespace WorldWizards.Input.ThirdParty.CompassWidget
{
    public class CompassState: MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
    
    {
        public Sprite offState;
        public Sprite northImage;
        public Sprite southImage;
        public Sprite eastImage;
        public Sprite westImage;

        private DataTypes.CARDINAL state = DataTypes.CARDINAL.NORTH;
        private Image myImage;

        [System.Serializable]
        public class ClickEvent : UnityEvent<DataTypes.CARDINAL>
        {
        }

        public ClickEvent  clickEvent = new ClickEvent();

        public DataTypes.CARDINAL Direction
        {
            get { return state; }
        }

        private Boolean inWidget = false;
        // Use this for initialization
        void Start ()
        {
            myImage = gameObject.GetComponent<Image>();
        }
	
        // Update is called once per frame
        void Update () {
            if (inWidget)
            {
                Vector2 normalizedPt;
                /// Note currently only works ina  screen space canvas!
                /// This code is purported to generally work but not implemnetd yet
                ///   RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out pos);
                ///   transform.position = myCanvas.transform.TransformPoint(pos);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    myImage.rectTransform,UnityEngine.Input.mousePosition , null, out normalizedPt);
                if (Math.Abs(normalizedPt.x) > Math.Abs(normalizedPt.y))
                {
                    // horizontal
                    if (normalizedPt.x < 0)
                    {
                        myImage.sprite = westImage;
                        state = DataTypes.CARDINAL.WEST;
                    }
                    else
                    {
                        myImage.sprite = eastImage;
                        state = DataTypes.CARDINAL.EAST;
                    }
                }
                else
                {
                    if (normalizedPt.y > 0)
                    {
                        state = DataTypes.CARDINAL.NORTH;
                    }
                    else
                    {
                        state = DataTypes.CARDINAL.SOUTH;
                    }
                }

            }
        }

        /// <summary>
    
        /// </summary>
        /// <param name="eventData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void OnPointerEnter(PointerEventData eventData)
        {
            inWidget = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inWidget = false;
            myImage.sprite = offState;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            clickEvent.Invoke(state);
        }
    }
}
