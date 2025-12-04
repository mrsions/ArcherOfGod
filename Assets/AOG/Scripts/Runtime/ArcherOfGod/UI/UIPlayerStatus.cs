using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    public class UIPlayerStatus : MonoBehaviour
    {
        [SerializeField] private int m_PlayerId;
        [SerializeField] private UIGageBar m_Hp;
        [SerializeField] private UIGageBar m_Shield;
        [SerializeField] private TMP_Text m_PlayerName;

        private void Awake()
        {
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
    }
}