using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AOT
{
    /// <summary>
    /// progress 값(0~1)에 따라 anchorMax.x 조절해서 늘어나는 프로그레스 바.
    /// 에디터에서도 미리보기 됨. UIGageBar의 fill 레이어로 쓰임.
    /// 가로 방향만 됨. 세로는 안됨.
    /// </summary>
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