using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UI.Extensions
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ToolTips : UIBehaviour
    {
        public Animator animator;
        private string ShowTrigger = "Show";
        private string HideTrigger = "Hide";
        public float duration = 0.5f;

        private CanvasGroup _group;
        private bool _active;
        private bool _anyClickCheck;
        private GameObject _go;

        public void Toggle()
        {
            SetActive(!_active);
        }

        public void SetActive(bool active)
        {
            _active = active;
            StopAllCoroutines();
            if (duration > 0 && !active)
            {
                if (isActiveAndEnabled)
                    StartCoroutine(WaitForTween());
                else
                    _go.SetActive(false);
            }
            else
            {
                _go.SetActive(active);
                _anyClickCheck = false;
            }

            if (animator != null)
            {
                animator.SetTrigger(active ? ShowTrigger : HideTrigger);
            }
            else
            {
                _group.alpha = active ? 1 : 0;
            }
            //Debug.LogError("SetActive:" + active + "," + _anyClickCheck);
        }

        private IEnumerator WaitForTween()
        {
            yield return new WaitForSeconds(duration);
            _go.SetActive(false);
        }

        // Use this for initialization
        protected override void Awake()
        {
            _go = this.gameObject;
            _go.SetActive(false);
            _group = this.GetComponent<CanvasGroup>();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (_active && _anyClickCheck)
                {
                    SetActive(false);
                }

                _anyClickCheck = true;
            }
        }
    }
}
