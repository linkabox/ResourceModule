using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AndroidBuildHelper
{
    static AndroidBuildHelper()
    {
        PlayerSettings.Android.keystoreName = Application.dataPath.Replace("/Assets", "/xxxgame.keystore");
        PlayerSettings.Android.keystorePass = "xxxgame";
        PlayerSettings.Android.keyaliasName = "key0";
        PlayerSettings.Android.keyaliasPass = "xxxgame";
    }
}
