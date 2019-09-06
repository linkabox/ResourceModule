using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class UILocalizeImage : MonoBehaviour
{
    [Serializable]
    public class SpriteLocalizeDictionary : SerializableDictionary<string, Sprite> { }

    [SerializeField]
    private SpriteLocalizeDictionary m_spriteMap;
    public SpriteLocalizeDictionary SpriteMap
    {
        get { return m_spriteMap; }
        set { m_spriteMap = value; }
    }

    private Image mImage;
    public Image image
    {
        get
        {
            if (mImage == null)
            {
                mImage = this.GetComponent<Image>();
            }
            return mImage;
        }
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        OnLocalize();
    }

    void OnLocalize()
    {
        Sprite sprite;
        if (m_spriteMap.TryGetValue(Localization.language, out sprite))
        {
            this.image.sprite = sprite;
        }
    }
}
