using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(VideoPlayer))]
public class UIVideoImage : MonoBehaviour
{
    public UnityEngine.Video.VideoClip videoClip;
    public RawImage rawImage;
    public VideoPlayer videoPlayer;
    public Texture errorTex;

    public bool autoPlay;

    [Serializable]
    public class VideoPlayerEvent : UnityEvent<VideoPlayer>
    {
    }

    [SerializeField]
    private VideoPlayerEvent m_OnPrepare = new VideoPlayerEvent();
    public VideoPlayerEvent onPrepare
    {
        get => this.m_OnPrepare;
        set => this.m_OnPrepare = value;
    }

    [SerializeField]
    private VideoPlayerEvent m_OnFinished = new VideoPlayerEvent();
    public VideoPlayerEvent onFinish
    {
        get => this.m_OnFinished;
        set => this.m_OnFinished = value;
    }

    [Serializable]
    public class VideoPlayerErrorEvent : UnityEvent<VideoPlayer, string>
    {
    }

    [SerializeField]
    private VideoPlayerErrorEvent m_OnError = new VideoPlayerErrorEvent();
    public VideoPlayerErrorEvent onError
    {
        get => this.m_OnError;
        set => this.m_OnError = value;
    }

    void Start()
    {
        videoPlayer.loopPointReached += OnLoopPointReached;
        videoPlayer.prepareCompleted += OnPrepare;
        videoPlayer.errorReceived += OnError;

        SetClip(this.videoClip);
    }

    private void OnError(VideoPlayer source, string message)
    {
        rawImage.texture = errorTex;
        onError.Invoke(source, message);
    }

    public void SetUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return;

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;
        videoPlayer.Prepare();
    }

    public void SetClip(VideoClip clip)
    {
        this.videoClip = clip;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;

        if (clip != null)
        {
            videoPlayer.Prepare();
        }
    }

    private void OnPrepare(VideoPlayer source)
    {
        rawImage.texture = videoPlayer.texture;
        if (autoPlay)
        {
            Play();
        }
        else
        {
            videoPlayer.StepForward();
        }

        onPrepare.Invoke(source);
    }

    private void OnLoopPointReached(VideoPlayer source)
    {
        onFinish.Invoke(source);
    }

    void Reset()
    {
        if (rawImage == null)
            rawImage = this.GetComponent<RawImage>();

        if (videoPlayer == null)
            videoPlayer = this.GetComponent<VideoPlayer>();

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.APIOnly;
        videoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.Direct;
    }

    [ContextMenu("ResetSize")]
    public void ResetSize()
    {
        if (videoClip != null)
        {
            var rectTrans = this.transform as RectTransform;
            if (rectTrans != null)
            {
                rectTrans.sizeDelta = new Vector2(videoClip.width, videoClip.height);
            }
        }
    }

    [ContextMenu("Play")]
    public void Play()
    {
        if (!videoPlayer.isPrepared) return;
        videoPlayer.Play();
    }
}