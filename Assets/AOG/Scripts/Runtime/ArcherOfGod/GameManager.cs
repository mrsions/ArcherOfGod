using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AOT
{
    public enum EGameStatus
    {
        None = 0,
        Loading,
        Ready,
        Start,
        Battle,
        Battle_LimitOver,
        Finish,
        End,
        MAX
    }


    public class GameManager : MonoBehaviour
    {
        private static GameManager s_Main;

        public static GameManager main => s_Main ??= Resources.FindObjectsOfTypeAll<GameManager>().FirstOrDefault();

        //-- Serializable
        [SerializeField] private CharacterBehaviour[] m_Characters;

        //-- Events
        public event Action<GameManager, EGameStatus> OnChangedStatus;

        //-- Private 
        private EGameStatus m_Status;

        //-- Properties
        public CharacterBehaviour[] Characters { get => m_Characters; private set => m_Characters = value; }
        public CharacterBehaviour Winner { get; private set; }
        public EGameStatus Status
        {
            get => m_Status;
            private set
            {
                if (m_Status == value) return;
                Debug.Log($"[GameManager] SetStatus({value})");
                m_Status = value;
                OnChangedStatus?.Invoke(this, m_Status);
            }
        }


        //------------------------------------------------------------------------------

        private void Awake()
        {
            s_Main = this;
        }

        private void Start()
        {
            StartProcessAsync().Forget();
        }

        private async UniTask StartProcessAsync()
        {
            Debug.Log("[GameManager] StartProcessAsync()");
            try
            {

                Status = EGameStatus.Loading;

                //TODO : Sync Data
                await UniTask.WaitForSeconds(0.5f);

                //TODO : Sync Ping
                await UniTask.WaitForSeconds(0.5f);

                Status = EGameStatus.Ready;

                //TODO : Ready Animation
                await UniTask.WaitForSeconds(3);

                Status = EGameStatus.Start;

                //TODO : something settings.

                Status = EGameStatus.Battle;

                // wait for game end
                float endTime = Time.time + GameSettings.main.gameTime;
                while (Time.time < endTime && Winner == null)
                {
                    for (int i = 0; i < Characters.Length; i++)
                    {
                        CharacterBehaviour cha = Characters[i];
                        if (cha.IsDead)
                        {
                            Debug.Log($"[GameManager] Character[{i}] is dead.");
                            Winner = GetTargetCharacter(i);
                            Debug.Log($"[GameManager] Character[{Winner.name}] has win.");
                            break;
                        }
                    }
                    await UniTask.Yield(destroyCancellationToken);
                }

                if (Winner == null)
                {
                    Status = EGameStatus.Battle_LimitOver;
                    while (Winner == null)
                    {
                        for (int i = 0; i < Characters.Length; i++)
                        {
                            CharacterBehaviour cha = Characters[i];
                            cha.CurrentHp--;
                            if (cha.IsDead)
                            {
                                Debug.Log($"[GameManager] Character[{i}] is dead.");
                                Winner = GetTargetCharacter(i);
                                Debug.Log($"[GameManager] Character[{Winner.name}] has win.");
                                break;
                            }
                        }
                        await UniTask.WaitForSeconds(0.1f, cancellationToken: destroyCancellationToken);
                    }
                }

                Status = EGameStatus.Finish;

                //TODO : Finish Animation
                //TODO : Send game result to server
                //await UniTask.WaitForSeconds(1);

                //Status = EGameStatus.End;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public CharacterBehaviour GetCharacter(int id)
        {
            return Characters[id];
        }

        public CharacterBehaviour GetTargetCharacter(int id)
        {
            return id == 0 ? Characters[1] : Characters[0];
        }
    }
}