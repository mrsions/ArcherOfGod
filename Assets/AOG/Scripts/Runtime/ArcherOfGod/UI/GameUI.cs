using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 게임 UI 컨트롤러. 게임 상태 바뀌면 해당 UI 보여줌. Loading/Ready/Start/Finish 등.
    /// 타이머 카운트다운 표시하고 승리/패배 화면 구분함.
    /// 플레이어 인덱스 0이 플레이어라고 가정함. 일시정지 메뉴 없음.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        //-- Serializable
        [SerializeField] private GameObject m_HUD;
        [SerializeField] private TMP_Text m_TimerText;

        [Header("Animations")]
        [SerializeField] private GameObject[] m_None;
        [SerializeField] private GameObject[] m_Loading;
        [SerializeField] private GameObject[] m_Ready;
        [SerializeField] private GameObject[] m_Start;
        [SerializeField] private GameObject[] m_BattleLimit;
        [SerializeField] private GameObject[] m_Finish;
        [SerializeField] private GameObject[] m_FinishVictory;
        [SerializeField] private GameObject[] m_FinishFailed;

        //-- Private
        private GameObject[] m_Before;
        private GameManager m_GameManager;


        //------------------------------------------------------------------------------
        private void Awake()
        {
            Debug.Log($"[GameUI] Awake");
            m_HUD.SetActive(false);

            SetActive(m_None, false);
            SetActive(m_Loading, false);
            SetActive(m_Ready, false);
            SetActive(m_Start, false);
            SetActive(m_BattleLimit, false);
            SetActive(m_Finish, false);
            SetActive(m_FinishVictory, false);
            SetActive(m_FinishFailed, false);

            Active(m_None);

            Debug.Log($"[GameUI] RegistOnChangedStatus");
            m_GameManager = GameManager.main;
            m_GameManager.OnChangedStatus += OnChangeStatus;

            m_TimerText.text = GameSettings.main.gameTime.ToString();
        }

        private void OnDestroy()
        {
            if (m_GameManager != null)
                m_GameManager.OnChangedStatus -= OnChangeStatus;
        }

        private void Active(GameObject[] gameObject)
        {
            if (m_Before != null)
            {
                SetActive(m_Before, false);
            }
            if (gameObject != null)
            {
                SetActive(gameObject, true);
            }
            m_Before = gameObject;
        }

        private void SetActive(GameObject[] objects, bool active)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].SetActive(active);
            }
        }

        private void OnChangeStatus(GameManager manager, EGameStatus status)
        {
            switch (status)
            {
                case EGameStatus.Loading:
                    Active(m_Loading);
                    break;
                case EGameStatus.Ready:
                    m_HUD.SetActive(true);
                    Active(m_Ready);
                    break;
                case EGameStatus.Start:
                    Active(m_Start);
                    UpdateTimerAsync().Forget();
                    break;
                case EGameStatus.Battle_LimitOver:
                    Active(m_BattleLimit);
                    m_TimerText.text = "VS";
                    break;
                case EGameStatus.Finish:
                    var characters = GameManager.main.Characters;
                    if (characters == null || characters.Count == 0)
                    {
                        Active(m_Finish);
                        break;
                    }
                    // pc vs pc 대결이라면
                    if (characters.All(p => p.IsPlayer))
                    {
                        Active(m_Finish);
                    }
                    // ai 대결이라면
                    else if (characters[0].IsDead)
                    {
                        Active(m_FinishFailed);
                    }
                    else
                    {
                        Active(m_FinishVictory);
                    }
                    break;
            }
        }

        private async UniTask UpdateTimerAsync()
        {
            int duration = GameSettings.main.gameTime;
            float startTime = Time.time;
            for (int i = 0; i < duration; i++)
            {
                int t = duration - i;
                m_TimerText.text = t.ToString();

                float nextTime = startTime + i + 1;
                await UniTask.WaitForSeconds(nextTime - Time.time, cancellationToken: destroyCancellationToken);
            }
        }
    }
}