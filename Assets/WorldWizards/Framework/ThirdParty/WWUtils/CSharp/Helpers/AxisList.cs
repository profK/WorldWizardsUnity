using UnityEngine;
using System.Collections.Generic;
using InputType = InputHelper.InputType;

public class AxisList : ScriptableObject
{
    #region Axis List Support
    

    [System.Serializable]
    public struct AxisRec
    {
        public string name;
        public string description;
        public int axis;
        public InputType inputType;
        public int joynum;

        public AxisRec(string name, string descr, int axis, InputType inputType, int joynum)
        {
            this.name = name;
            this.axis = axis;
            this.inputType = inputType;
            this.joynum = joynum;
            this.description = descr;
        }
    }



    public List<AxisRec> axesList = new List<AxisRec>();

    public void Add(string name, string descr, int axis, InputType type, int joynum)
    {
         axesList.Add(new AxisRec(name, descr, axis, type,joynum));
    }

    public void Clear()
    {
        axesList.Clear();
    }

    public AxisRec[] ListAxes()
    {
        return axesList.ToArray();
        //return new AxisRec[0];
    }

}
#endregion