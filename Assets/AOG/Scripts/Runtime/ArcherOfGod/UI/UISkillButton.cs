using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    public class UISkillButton : OnScreenButton
    {
        //-- Serializable
        [SerializeField]
        private int m_SkillIndex;

        [SerializeField]
        private TMP_Text m_DurationTxt;

        [SerializeField]
        private Image m_DurationImg;

        //-- Private
        private BaseSkillBehaviour m_Skill;


        //------------------------------------------------------------------------------

        private void Start()
        {
            GameManager.main.OnChangedStatus += OnChangedStatus;
        }

        private void OnValidate()
        {
            Assert.IsTrue(0 <= m_SkillIndex);
        }

        private void OnChangedStatus(GameManager manager, EGameStatus status)
        {
            if (status != EGameStatus.Start) return;

            PlayerBehaviour player = manager.player;
            Assert.IsNotNull(player);
            Assert.IsNotNull(player.Skills);
            Assert.IsTrue(m_SkillIndex < player.Skills.Length);

            BaseSkillBehaviour skill = manager.player.Skills[m_SkillIndex];
            Assert.IsNotNull(skill);

            m_Skill = skill;
            m_Skill.SetDelay(GameSettings.main.skill_delay_onAwake);
        }
    }
}