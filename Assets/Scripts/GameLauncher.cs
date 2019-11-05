using System.Collections;
using System.Collections.Generic;
using ResourceModule;
using UnityEngine;

/// <summary>
/// 用于启动游戏初始化场景
/// </summary>
public class GameLauncher : MonoBehaviour
{
    public string PrefabName = "GameMain.prefab";
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GameLauncher Load MainScene...");
        var loader = ResManager.LoadAsset(PrefabName);
        var prefab = loader.ResultObject as GameObject;
        var go = Instantiate(prefab);
        go.name = prefab.name;

        Destroy(this.gameObject);
    }
}
