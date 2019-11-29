using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
using Version = System.Version;

namespace ResourceModule
{
    public class GamePatcher
    {
        public delegate void OnMsgBox(string hint, string ok, Action onSuccess);

        private string _platformName;
        private string _remoteRoot;

        public string curVerStr;
        public bool needReload;
        private string _resVerPath;
        private Action<string> onMsg;
        private OnMsgBox onMsgBox;
        private Action<float> onProgress;
        private Action onFinish;

        public GamePatcher(string remoteRoot)
        {
            _platformName = ResManager.BuildPlatformName;
            _remoteRoot = remoteRoot + "/" + _platformName;
            _resVerPath = ResManager.DocumentDirPath + ResManager.BundlePathRoot + ResourceModuleConfig.ResVerFileName;

            if (ResManager.IsEdiotrMode)
            {
                var path = ResourceModuleConfig.BundleResoucesDir + "/" + ResManager.BuildPlatformName + "_" +
                           ResourceModuleConfig.ResVerFileName;
                this.curVerStr = FileExt.ReadAllText(path);
            }
            else
            {
                var path = ResManager.BundlePathRoot + ResourceModuleConfig.ResVerFileName;
                this.curVerStr = ResManager.ReadAllText(path);
            }

            Debug.Log($"GamePatcher Init:\nresVer:{curVerStr}\nresVerPath:{_resVerPath}\nremoteRoot:{remoteRoot}");
        }

        public void CheckResVer(Action<string> onMsg, Action<float> onProgress, Action onFinish, OnMsgBox onMsgBox = null, Action onPackageReinstall = null)
        {
            this.onMsg = onMsg;
            this.onProgress = onProgress;
            this.onFinish = onFinish;
            this.onMsgBox = onMsgBox;

            if (ResManager.IsEdiotrMode)
            {
                Debug.Log("GamePatcher ignore update");
                onFinish();
                return;
            }

            if (CheckPackageVer(onPackageReinstall))
            {
                FetchPatchInfo();
            }
        }

        private bool CheckPackageVer(Action onPackageReinstall)
        {
            var path = ResManager.BundlePathRoot + ResourceModuleConfig.ResVerFileName;
            var packageResVer = ResManager.LoadTextFromStreamingAssets(path);
            var resVerCode = new Version(this.curVerStr);
            var packageVerCode = new Version(packageResVer);
            Debug.Log($"__check_package_ver:{packageVerCode}, {resVerCode}");

            if (packageVerCode > resVerCode)
            {
                Debug.Log($"!!!package_resVer > resVer:{packageVerCode} {resVerCode}");
                FileExt.DeleteDirectory(ResManager.DocumentDirPath + this._platformName, true);
                if (onPackageReinstall != null)
                    onPackageReinstall();

                return false;
            }

            return true;
        }

        private void FetchPatchInfo()
        {
            var patchInfoUrl = $"{_remoteRoot}/patch_info_{curVerStr}.json?{DateTime.Now.Ticks}";

            WebRequestMgr.Get(patchInfoUrl, (request) =>
            {
                var str = request.downloadHandler.text;
                Debug.Log("patch_info:\n" + str);
                var patchInfo = JsonMapper.ToObject<PatchInfo>(str);
                if (patchInfo != null)
                {
                    //最新版本资源无需下载补丁包
                    if (patchInfo.version == patchInfo.nextVer)
                    {
                        onMsg("当前为最新版本资源，忽略更新");
                        UpdateResVer(null);
                        onFinish();
                    }
                    else
                    {
                        //开始下载补丁包
                        var networkType = Application.internetReachability;
                        if (networkType == NetworkReachability.ReachableViaLocalAreaNetwork)
                        {
                            //WIFI直接下载
                            _DownloadRes(patchInfo);
                        }
                        else
                        {
                            if (onMsgBox != null)
                            {
                                onMsgBox("正在使用非WiFi网络,\n下载补丁包将产生流量费用\n是否继续？", "下载",
                                    () => { _DownloadRes(patchInfo); });
                            }
                            else
                            {
                                _DownloadRes(patchInfo);
                            }
                        }
                    }
                }
                else
                {
                    OnPatchInfoError(null);
                }

            }, OnPatchInfoError, null);
        }

        private void OnPatchInfoError(UnityWebRequest request)
        {
            UpdateResVer(null);
            onMsg("请求资源版本信息出错，忽略更新");
            onProgress(1f);
            onFinish();
        }

        private void _DownloadRes(PatchInfo patchInfo)
        {
            var patchZipUrl = $"{this._remoteRoot}/{patchInfo.fileName}?{DateTime.Now.Ticks}";
            var savePath = ResManager.DocumentDirPath + patchInfo.fileName;
            DownloadHandlerFileRange downloadHandler = null;
            WebRequestMgr.DownloadFile(patchZipUrl, savePath, (e) =>
            {
                downloadHandler = e.downloadHandler as DownloadHandlerFileRange;
                onMsg("开始下载...");
                onProgress(0f);
            }, (request) =>
            {
                onMsg("解压补丁包...");
                onProgress(0.8f);
                ZipTool.UncompressFile(savePath, ResManager.DocumentBundlePath);
                FileExt.DeleteFile(savePath);
                UpdateResVer(patchInfo.nextVer);
                needReload = true;
                //更新完毕再重新拉取最新patchInfo
                FetchPatchInfo();
            }, (request) =>
            {
                UpdateResVer(null);
                onMsg("下载补丁包失败，忽略更新");
                onProgress(1f);
                onFinish();
            }, (asyncOp) =>
            {
                if (downloadHandler != null)
                {
                    long totalSize = downloadHandler.FileSize;
                    long curSize = downloadHandler.DownloadedSize;
                    onMsg($"下载补丁包中...({curSize}/{totalSize})({downloadHandler.DownloadProgress * 100}%)");
                }
                else
                {
                    onMsg($"下载补丁包中...({asyncOp.webRequest.downloadedBytes}/{patchInfo.fileSize})");
                }
                onProgress(asyncOp.progress * 0.8f);
            });
        }

        private void UpdateResVer(string newVer)
        {
            if (!string.IsNullOrEmpty(newVer))
            {
                this.curVerStr = newVer;
            }

            FileExt.WriteAllText(_resVerPath, curVerStr);
        }
    }
}