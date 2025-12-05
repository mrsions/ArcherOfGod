using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    /// <summary>
    /// 플레이어 상태 표시. HP/실드 게이지랑 원형 파워 게이지, 이름 보여줌.
    /// 캐릭터 이벤트 구독해서 자동 갱신됨. 실드 0되면 실드바 숨김.
    /// m_PlayerId로 어떤 캐릭터인지 지정. 버프/디버프 아이콘 없음.
    /// </summary>
    public class UIPlayerStatus : MonoBehaviour
    {
        [SerializeField] private int m_PlayerId;
        [SerializeField] private UIGageBar m_Hp;
        [SerializeField] private UIGageBar m_Shield;
        [SerializeField] private TMP_Text m_PlayerName;

        [SerializeField] private Image m_PowerGage;
        [SerializeField] private Image m_PowerGage2;
        [SerializeField] private Transform m_PowerEnd;
        [SerializeField] private Transform m_PowerStart;

        private void Awake()
        {
            print($"[UIPlayerStatus] RegistOnChangedStatus");
            GameManager.main.OnChangedStatus += OnChangedStatus;
            OnChangedStatus(GameManager.main, GameManager.main.Status);
        }

        private void OnChangedStatus(GameManager manager, EGameStatus status)
        {
            if (status != EGameStatus.Ready) return;

            CharacterBehaviour cha = manager.GetCharacter(m_PlayerId);
            m_PlayerName.text = cha.PlayerStatus.name;

            cha.OnChangedHp += OnChangedHp;
            cha.OnChangedShield += OnChangedShield;
            cha.OnChangedPower += OnChangedPower;

            OnChangedHp(cha, cha.CurrentHp, cha.CurrentHp, cha.MaxHp);
            OnChangedShield(cha, cha.CurrentShield, cha.CurrentShield, cha.MaxShield);
        }

        private void OnChangedShield(ObjectBehaviour behaviour, float bef, float cur, float max)
        {
            m_Shield.SetValue((int)cur, cur / max);
            if (cur <= 0)
            {
                m_Shield.gameObject.SetActive(false);
                m_Hp.ShowText = true;
            }
            else
            {
                m_Shield.gameObject.SetActive(true);
                m_Hp.ShowText = false;
            }
        }

        private void OnChangedHp(ObjectBehaviour behaviour, float bef, float cur, float max)
        {
            m_Hp.SetValue((int)cur, cur / max);
        }

        private void OnChangedPower(CharacterBehaviour behaviour, float bef, float cur, float max)
        {
            float ratio = Mathf.Clamp01(cur / max);
            m_PowerGage.fillAmount = ratio;
            m_PowerGage2.fillAmount = ratio;

            DOTween.To(() => m_PowerGage.fillAmount, v => 
            {
                m_PowerGage.fillAmount = v;
                m_PowerGage2.fillAmount = v;
                m_PowerEnd.localEulerAngles = new Vector3(0, 0, -90 - (v * 360));
            }, 
            ratio, 0.2f);

            m_PowerStart.gameObject.SetActive(ratio > 0);
            m_PowerEnd.gameObject.SetActive(ratio > 0);
        }
    }
}