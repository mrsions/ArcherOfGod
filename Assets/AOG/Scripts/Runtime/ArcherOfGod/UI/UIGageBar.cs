using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    /// <summary>
    /// 게이지 바. 앞바는 바로 바뀌고 뒷바는 애니메이션으로 따라감.
    /// 데미지 받으면 앞바 먼저 줄고 뒷바가 스르륵 따라오는 연출.
    /// 숫자 텍스트도 표시됨. HP/실드 등에 쓰임. 급하게 바뀌면 애니 끊김.
    /// </summary>
    public class UIGageBar : MonoBehaviour
    {
        [SerializeField] private UIAnchorProgress m_ForwardBar;
        [SerializeField] private UIAnchorProgress m_BackwardBar;
        [SerializeField] private TMP_Text m_Text;
        [SerializeField] private AnimationCurve m_DecreaseAnimCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private float m_Duration = 0.5f;

        private int m_BeforeValue = -1;
        private float m_LastPercent;
        private int m_AsyncId;

        public bool ShowText { get => m_Text.gameObject.activeSelf; set => m_Text.gameObject.SetActive(value); }

        public void SetValue(int value, float percent)
        {
            if (value != m_BeforeValue)
            {
                m_Text.text = value.ToString("N0");
            }

            if (m_LastPercent != percent)
            {
                if (percent < m_LastPercent)
                {
                    m_ForwardBar.progress = percent;
                    AnimateAsync(m_BackwardBar, percent).Forget();
                }
                else
                {
                    m_BackwardBar.progress = percent;
                    AnimateAsync(m_ForwardBar, percent).Forget();
                }
                m_LastPercent = percent;
            }
        }

        private async UniTask AnimateAsync(UIAnchorProgress bar, float end)
        {
            int id = ++m_AsyncId;

            float t = 0;
            float duration = m_Duration;

            float start = bar.progress;

            while (t < duration)
            {
                t += Time.deltaTime;
                float ratio = t / duration;
                bar.progress = Mathf.Lerp(start, end, m_DecreaseAnimCurve.Evaluate(ratio));

                await UniTask.Yield(destroyCancellationToken);

                if (id != m_AsyncId) return;
            }

            bar.progress = end;
        }
    }
}