using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    [ExecuteInEditMode]
    public class UIAnchorProgress : UIBehaviour
    {
        [SerializeField]
        [Range(0f, 1f)]
        private float m_Progress;

        private RectTransform m_RectTransform;

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
            m_RectTransform = (RectTransform)transform;
            UpdateProgress();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Awake();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            m_RectTransform.anchorMax = m_RectTransform.anchorMax.SetX(m_Progress);
        }
    }
}