using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ResourceModule
{
    public class WebRequestMgr : MonoBehaviour
    {
        public delegate void OnRequestSuccess(UnityWebRequest request);
        public delegate void OnRequestError(UnityWebRequest request);
        public delegate void OnRequestProgress(UnityWebRequestAsyncOperation asyncOp);

        private static WebRequestMgr _instance;

        public static WebRequestMgr Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject mgr = GameObject.Find("_WebRequestMgr_");
                    if (mgr == null)
                    {
                        mgr = new GameObject("_WebRequestMgr_");
                        GameObject.DontDestroyOnLoad(mgr);
                    }

                    _instance = mgr.AddComponent<WebRequestMgr>();
                }
                return _instance;
            }
        }

        public static void Get(string url, OnRequestSuccess onFinish, OnRequestError onError, OnRequestProgress onProgress, int maxRetry = 3, float retryDelay = 1.0f)
        {
            Instance.StartCoroutine(TrySendRequest(url, UnityWebRequest.Get, onFinish, onError, onProgress, maxRetry, retryDelay));
        }

        #region Download
        public static void Download(string url, string savePath, OnRequestSuccess onFinish, OnRequestError onError,
            OnRequestProgress onProgress, int maxRetry = 3, float retryDelay = 1.0f)
        {
            string dir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Instance.StartCoroutine(TrySendRequest(url, (uri) =>
            {
                var uwr = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbGET)
                {
                    downloadHandler = new DownloadHandlerFile(savePath)
                };
                return uwr;
            }, onFinish, onError, onProgress, maxRetry, retryDelay));
        }

        public static void DownloadFile(string url, string savePath,
            OnRequestSuccess onDownloadStart,
            OnRequestSuccess onFinish,
            OnRequestError onError,
            OnRequestProgress onProgress)
        {
            string dir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Instance.StartCoroutine(_DownloadFile(url, savePath, onDownloadStart, onFinish, onError, onProgress));
        }

        private static IEnumerator _DownloadFile(string url, string savePath,
            OnRequestSuccess onDownloadStart,
            OnRequestSuccess onFinish,
            OnRequestError onError,
            OnRequestProgress onProgress)
        {
            var headRequest = UnityWebRequest.Head(url);
            yield return headRequest.SendWebRequest();

            var totalSize = long.Parse(headRequest.GetResponseHeader("Content-Length"));
            var fileInfo = new FileInfo(savePath);
            if (fileInfo.Exists && fileInfo.Length == totalSize)
            {
                onFinish?.Invoke(headRequest);
            }
            else
            {
                using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
                {
                    var handler = new DownloadHandlerFileRange(savePath, request);
                    request.downloadHandler = handler;
                    onDownloadStart?.Invoke(request);

                    var asyncOp = request.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        yield return null;
                        onProgress?.Invoke(asyncOp);
                    }

                    if (request.isNetworkError || request.isHttpError)
                    {
                        handler.Close();
                        if (onError != null)
                        {
                            Debug.LogWarningFormat("SendRequest Failed:\nresponseCode :{0}\nerror :{1}\nurl:{2}",
                                request.responseCode, request.error, request.url);
                            onError(request);
                        }
                        else
                        {
                            Debug.LogErrorFormat("SendRequest Failed:\nresponseCode :{0}\nerror :{1}\nurl:{2}",
                                request.responseCode, request.error, request.url);
                        }
                    }
                    else
                    {
                        Debug.LogFormat("Finish UnityWebRequest: {0}\nresponseCode :{1}", request.url,
                            request.responseCode);
                        onFinish?.Invoke(request);
                    }
                }
            }
        }
        #endregion

        #region POST

        public static void PostRaw(string url, string postData, OnRequestSuccess onFinish,
            OnRequestError onError, OnRequestProgress onProgress, int maxRetry = 3, float retryDelay = 1.0f)
        {
            Instance.StartCoroutine(TrySendRequest(url, (uri) => UnityWebRequest.Post(uri, postData), onFinish, onError, onProgress, maxRetry, retryDelay));
        }

        public static void Post(string url, string postData, string contentType, OnRequestSuccess onFinish,
            OnRequestError onError, OnRequestProgress onProgress, int maxRetry = 3, float retryDelay = 1.0f)
        {
            Post(url, Encoding.UTF8.GetBytes(postData), contentType, onFinish, onError, onProgress, maxRetry, retryDelay);
        }

        public static void Post(string url, byte[] postBytes, string contentType, OnRequestSuccess onFinish, OnRequestError onError, OnRequestProgress onProgress, int maxRetry = 3, float retryDelay = 1.0f)
        {
            Instance.StartCoroutine(TrySendRequest(url, (uri) =>
            {
                UnityWebRequest request = new UnityWebRequest(uri, "POST")
                {
                    uploadHandler = new UploadHandlerRaw(postBytes) { contentType = contentType },
                    downloadHandler = new DownloadHandlerBuffer()
                };
                return request;
            }, onFinish, onError, onProgress, maxRetry, retryDelay));
        }

        #endregion

        private static IEnumerator TrySendRequest(string url, Func<string, UnityWebRequest> requestCreator, OnRequestSuccess onFinish, OnRequestError onError, OnRequestProgress onProgress, int maxRetry, float retryDelay)
        {
            Debug.LogFormat("Start Send UnityWebRequest: " + url);
            int retry = 0;
            while (retry++ < maxRetry)
            {
                using (var request = requestCreator(url))
                {
                    var asyncOp = request.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        if (onProgress != null) onProgress(asyncOp);
                        yield return null;
                    }

                    if (request.isNetworkError || request.isHttpError)
                    {
                        if (retry >= maxRetry)
                        {
                            if (onError != null)
                            {
                                Debug.LogWarningFormat("SendRequest Failed:\nresponseCode :{0}\nerror :{1}\nurl:{2}", request.responseCode, request.error, request.url);
                                onError(request);
                            }
                            else
                            {
                                Debug.LogErrorFormat("SendRequest Failed:\nresponseCode :{0}\nerror :{1}\nurl:{2}", request.responseCode, request.error, request.url);
                            }
                        }
                        else
                        {
                            Debug.LogFormat("Try again url :{0}\ntime :{1}\nresponseCode :{2}\nerror :{3}",
                                request.url, retry, request.responseCode, request.error);
                            yield return new WaitForSeconds(retryDelay);
                        }
                    }
                    else
                    {
                        Debug.LogFormat("Finish UnityWebRequest: {0}\nresponseCode :{1}", request.url, request.responseCode);
                        if (onFinish != null)
                            onFinish(request);

                        break; //success,break retry loop
                    }
                }
            }
        }

    }
}