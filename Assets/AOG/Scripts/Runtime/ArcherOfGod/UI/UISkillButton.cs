using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    [RequireComponent(typeof(Button))]
    public class UISkillButton : OnScreenButton
    {

        //-- Serializable
        [SerializeField]
        private int m_PlayerId;
        [SerializeField]
        private int m_SkillIndex;

        [Header("Components")]
        [SerializeField]
        private Image m_Icon;
        [SerializeField]
        private TMP_Text m_DurationTxt;
        [SerializeField]
        private Image m_DurationImg;

        [Header("Animation")]
        [SerializeField]
        private Animator m_Animator;
        [SerializeField]
        private string m_AnimVar_Use = "use";
        [SerializeField]
        private string m_AnimVar_Ready = "ready";

        //-- Private
        private Button m_Button;
        private BaseSkillBehaviour m_Skill;

        public CharacterBehaviour Player
        {
            get
            {
                CharacterBehaviour player = GameManager.main.GetCharacter(m_PlayerId);
                Assert.IsNotNull(player);
                Assert.IsNotNull(player.Skills);
                Assert.IsTrue(m_SkillIndex < player.Skills.Count);
                return player;
            }
        }
        public BaseSkillBehaviour Skill
        {
            get
            {
                if (m_Skill == null)
                {
                    CharacterBehaviour player = GameManager.main.GetCharacter(m_PlayerId);
                    Assert.IsNotNull(player);
                    Assert.IsNotNull(player.Skills);
                    Assert.IsTrue(m_SkillIndex < player.Skills.Count);

                    m_Skill = player.Skills[m_SkillIndex];
                    Assert.IsNotNull(m_Skill);
                }
                return m_Skill;
            }
        }


        //------------------------------------------------------------------------------

        private void Awake()
        {
            m_Button = GetComponent<Button>();
            m_DurationTxt.text = "";
            m_DurationImg.fillAmount = 1;

            m_Button.onClick.AddListener(OnClick);

            print($"[OnChangedStatus] RegistOnChangedStatus");
            GameManager.main.OnChangedStatus += OnChangedStatus;
            OnChangedStatus(GameManager.main, GameManager.main.Status);

            GameSettings.main.GetPlayerSkillAction(m_SkillIndex).performed += OnClick;
        }

#if UNITY_EDITOR|| NOPT
        private void OnValidate()
        {
            Assert.IsTrue(0 <= m_SkillIndex);
        }
#endif

        private void OnClick(InputAction.CallbackContext context)
        {
            OnClick();
        }

        private void OnClick()
        {
            CharacterBehaviour cha = GameManager.main.GetCharacter(m_PlayerId);
            BaseSkillBehaviour skill = cha.Skills[m_SkillIndex];
            if (!cha.StartSkill(skill)) return;

            m_Animator.SetTrigger(m_AnimVar_Use);
        }

        private void OnChangedStatus(GameManager manager, EGameStatus status)
        {
            try
            {
                if (status != EGameStatus.Ready) return;

                BaseSkillBehaviour skill = Skill;
                m_Icon.sprite = skill.Icon;
                skill.SetForceDelay(GameSettings.main.skill_delay_onAwake);
                skill.OnChangedStatus.AddListener(OnSkillChangedStatus);

                OnSkillChangedStatus(skill, ESkillStatus.None, skill.Status);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnSkillChangedStatus(BaseSkillBehaviour sender, ESkillStatus bef, ESkillStatus cur)
        {
            switch (cur)
            {
                case ESkillStatus.Ready:
                    m_Animator.SetTrigger(m_AnimVar_Ready);
                    m_Button.interactable = true;
                    m_DurationTxt.enabled = false;
                    m_DurationImg.enabled = false;
                    break;
                case ESkillStatus.Cooldown:
                    m_Button.interactable = false;
                    m_DurationTxt.enabled = true;
                    m_DurationImg.enabled = true;
                    m_DurationImg.fillAmount = 0;
                    StartCountdownAsync().Forget();
                    break;
                case ESkillStatus.Lock:
                    m_Button.interactable = false;
                    m_DurationTxt.enabled = true;
                    m_DurationTxt.text = "LOCK";
                    m_DurationImg.enabled = true;
                    m_DurationImg.fillAmount = 0;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private async UniTask StartCountdownAsync()
        {
            int befRemainDelay = -1;

            BaseSkillBehaviour skill = Skill;

            while (skill.Status == ESkillStatus.Cooldown)
            {
                int remainDelay = (int)skill.RemainDelay;
                if (remainDelay != befRemainDelay)
                {
                    m_DurationTxt.text = remainDelay.ToString();
                    befRemainDelay = remainDelay;
                }

                m_DurationImg.fillAmount = 1f - skill.RemainPercent;

                await UniTask.Yield(destroyCancellationToken);
            }
        }
    }
}