using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public static class TypeHelper  {
    public static Type[] GetAllDefinedSubTypes(Type parentType)
    {
        List<Type> types = new List<Type>();
        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type t in a.GetTypes())
            {
                if (parentType.IsAssignableFrom(t))
                {
                    types.Add(t);
                }
            }
        }
        return types.ToArray();
    }

    public static void ForAllTypes<T>(System.Action<System.Type> action)
    {

        foreach (Type t in GetAllDefinedSubTypes(typeof(T)))
        {

            action(t);
        }

    }

}
