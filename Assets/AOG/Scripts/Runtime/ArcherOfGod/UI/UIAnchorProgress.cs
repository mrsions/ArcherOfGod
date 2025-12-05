using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AOT
{
    [ExecuteInEditMode]
    public class UIAnchorProgress : UIBehaviour
    {
        [SerializeField]
        [Range(0f, 1f)]
        private float m_Progress;

        private RectTransform m_RectTransform;
        private RectTransform RectTransform => m_RectTransform ??= (RectTransform)transform;

        public float progress
        {
            get
            {
                return m_Progress;
            }
            set
            {
                m_Progress = value;
                UpdateProgress();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            UpdateProgress();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Awake();
        }
#endif

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            RectTransform.anchorMax = RectTransform.anchorMax.SetX(m_Progress);
        }
    }
}