using UnityEngine;

[System.Serializable]
public class ServerInfo
{
    public string patch_url;

    public string name;
    public string ip;
    public int port;

    public bool show_range;
    public bool draw_grid;
    public bool test_nav;
    public bool test_nav_conn;
}

[CreateAssetMenu(menuName = "LaunchCfg")]
public class LaunchCfg : ScriptableObject
{
    public string launchCfgUrl;     //登陆配置拉取地址
    public string tag;              //游戏包类型标识，根据这个tag来读取对应版本信息，如：审核包，内开发包，公测包
    public string vendor;           //平台标识
    public bool debug;              //debug开启时直接读取dev_config
    public ServerInfo dev_config;   //仅用于编辑器下开发调试登陆配置
}