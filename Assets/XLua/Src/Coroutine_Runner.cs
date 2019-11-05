using UnityEngine;
using XLua;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Networking;

public class Coroutine_Runner : MonoBehaviour
{
}


public static class CoroutineConfig
{
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp
    {
        get
        {
            return new List<Type>()
            {
                typeof(WaitForSeconds),
                typeof(UnityWebRequest)
            };
        }
    }
}
