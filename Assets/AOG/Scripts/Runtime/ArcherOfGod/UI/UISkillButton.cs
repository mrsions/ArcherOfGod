using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    /// <summary>
    /// 스킬 버튼. 누르면 스킬 발동하고 쿨타임 보여줌. 아이콘이랑 남은시간 표시됨.
    /// 키보드 단축키도 됨. Ready/Use 애니메이션 있고 쿨타임이면 버튼 비활성화.
    /// 드래그 조준이나 차지 없음. 스킬 인덱스 하드코딩되어있음.
    /// </summary>
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
        private CharacterBehaviour m_Player;
        private BaseSkillBehaviour m_Skill;
        private InputAction m_InputAction;
        private GameManager m_GameManager;

        private CharacterBehaviour Player => m_Player ??= GetValidatedPlayer();
        private BaseSkillBehaviour Skill => m_Skill ??= GetValidatedSkill();

        private CharacterBehaviour GetValidatedPlayer()
        {
            CharacterBehaviour player = GameManager.main.GetCharacter(m_PlayerId);
            Assert.IsNotNull(player);
            Assert.IsNotNull(player.Skills);
            Assert.IsTrue(m_SkillIndex < player.Skills.Count);
            return player;
        }

        private BaseSkillBehaviour GetValidatedSkill()
        {
            BaseSkillBehaviour skill = Player.Skills[m_SkillIndex];
            Assert.IsNotNull(skill);
            return skill;
        }


        //------------------------------------------------------------------------------

        private void Awake()
        {
            m_Button = GetComponent<Button>();
            m_DurationTxt.text = "";
            m_DurationImg.fillAmount = 1;

            m_Button.onClick.AddListener(OnClick);

            Debug.Log($"[OnChangedStatus] RegistOnChangedStatus");
            m_GameManager = GameManager.main;
            m_GameManager.OnChangedStatus += OnChangedStatus;
            OnChangedStatus(m_GameManager, m_GameManager.Status);

            m_InputAction = GameSettings.main.GetPlayerSkillAction(m_SkillIndex);
            m_InputAction.performed += OnClick;
        }

        private void OnDestroy()
        {
            m_InputAction.performed -= OnClick;

            if (m_GameManager != null)
                m_GameManager.OnChangedStatus -= OnChangedStatus;

            if (m_Button != null)
                m_Button.onClick.RemoveListener(OnClick);

            if (m_Skill != null)
                m_Skill.OnChangedStatus.RemoveListener(OnSkillChangedStatus);
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
            EGameStatus status = GameManager.main.Status;
            if (status != EGameStatus.Battle && status != EGameStatus.Battle_LimitOver)
            {
                return;
            }

            if (!Player.StartSkill(Skill)) return;

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