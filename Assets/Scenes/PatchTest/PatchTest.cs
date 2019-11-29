using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ResourceModule;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PatchTest : MonoBehaviour
{
    public string url;
    public string remoteRoot;

    public Text downloadInfo;

    public Slider slider;

    private UnityWebRequest uwr;
    private GamePatcher patcher;

    // Start is called before the first frame update
    void Start()
    {
        patcher = new GamePatcher(remoteRoot);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (GUILayout.Button("Download"))
        {
            WebRequestMgr.DownloadFile(url, Application.dataPath + "/../new_pack.zip", (e) => { uwr = e; }, (e) =>
            {
                downloadInfo.text = "Finish";
                slider.value = 1f;
            }, (msg) => { downloadInfo.text = "Error"; }, (asyncOp) =>
            {
                var downloadHandler = asyncOp.webRequest.downloadHandler as DownloadHandlerFileRange;
                //var hint = $"Progress:{asyncOp.progress}\nDownload:{handler.DownloadProgress}\nSpeed:{handler.Speed}";
                long totalSize = downloadHandler.FileSize;
                long curSize = downloadHandler.DownloadedSize;
                var hint = $"下载补丁包中...({curSize}/{totalSize})({downloadHandler.DownloadProgress * 100}%)";
                downloadInfo.text = hint;
                slider.value = asyncOp.progress;
                Debug.Log(hint);
            });
        }

        if (GUILayout.Button("Abort"))
        {
            if (uwr != null)
            {
                uwr.Abort();
            }
        }

        if (GUILayout.Button("GamePatch Check"))
        {
            patcher.CheckResVer((str) => { downloadInfo.text = str; }, (val) => { slider.value = val; }, () =>
            {
                downloadInfo.text = "GamePatch Finish";
                slider.value = 1f;
            });
        }
    }
}
