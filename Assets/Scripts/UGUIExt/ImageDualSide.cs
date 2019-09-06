using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Image))]
    [ExecuteInEditMode]
    public class ImageDualSide : MonoBehaviour
    {
        public bool useLocalRot;
        public Sprite frontSprite;
        public Sprite backSprite;

        private Image _image;
        private Transform _trans;

        void Start()
        {
            _image = this.GetComponent<Image>();
            _trans = this.transform;
        }

        void Update()
        {
            if (_image == null || frontSprite == null || backSprite == null) return;

            float angle = useLocalRot ? _trans.localEulerAngles.y : _trans.eulerAngles.y;
            float factor = angle / 90f;
            if (factor >= 0 && factor < 1)
            {
                _image.sprite = frontSprite;
            }
            else if (factor >= 1 && factor < 3)
            {
                _image.sprite = backSprite;
            }
            else
            {
                _image.sprite = frontSprite;
            }
        }
    }
}