using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SysWin  {
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    public static System.IntPtr GetWindowHandle()
    {
        return GetActiveWindow();
    }

}
