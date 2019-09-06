using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Text))]
public class UILocalizeText : MonoBehaviour
{
    [SerializeField]
    private string _key = "";
    public string key
    {
        get { return _key; }
        set
        {
            if (value == _key) return;
            _key = value;

            string val;
            if (Localization.TryGet(value, out val))
            {
                this.val = val;
            }
        }
    }

    private string _val;
    public string val
    {
        get { return _val; }
        private set
        {
            _val = value;

            this.text.text = value;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this.text);
#endif
        }
    }

    private Text mText;

    public Text text
    {
        get
        {
            if (mText == null)
            {
                mText = this.GetComponent<Text>();
            }
            return mText;
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
        this.val = Localization.Get(key);
    }
}
