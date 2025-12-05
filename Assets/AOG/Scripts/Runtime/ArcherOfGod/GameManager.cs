using System;
using System.Collections.Generic;
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
        [SerializeField] private List<CharacterBehaviour> m_Characters;
        public Transform effectContainer;

        //-- Events
        public event Action<GameManager, EGameStatus> OnChangedStatus;

        //-- Private 
        private EGameStatus m_Status;

        //-- Properties
        public List<CharacterBehaviour> Characters { get => m_Characters; private set => m_Characters = value; }
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

        private void OnEnable()
        {
            s_Main = this;
        }

        private void OnDisable()
        {
            if (s_Main == this) s_Main = null;
        }

        private void Start()
        {
            StartProcessAsync().Forget();
        }

        private async UniTask StartProcessAsync()
        {
            Debug.Log("[GameManager] StartProcessAsync()");

            Status = EGameStatus.Loading;

            //TODO : Sync Data
            await UniTask.WaitForSeconds(0.5f, cancellationToken: destroyCancellationToken);

            //TODO : Sync Ping
            await UniTask.WaitForSeconds(0.5f, cancellationToken: destroyCancellationToken);

            Status = EGameStatus.Ready;

            //TODO : Ready Animation
            await UniTask.WaitForSeconds(3, cancellationToken: destroyCancellationToken);

            Status = EGameStatus.Start;

            //TODO : something settings.

            Status = EGameStatus.Battle;

            // wait for game end
            float endTime = Time.time + GameSettings.main.gameTime;
            while (Time.time < endTime && Winner == null)
            {
                for (int i = 0; i < Characters.Count; i++)
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
                    for (int i = 0; i < Characters.Count; i++)
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

            //Status = EGameStatus.End;
        }

        public CharacterBehaviour GetCharacter(int id)
        {
            return Characters[id];
        }

        public CharacterBehaviour GetTargetCharacter(int id)
        {
            return id == 0 ? Characters[1] : Characters[0];
        }

        internal void AttachCharacter(CharacterBehaviour cha)
        {
            int idx = m_Characters.IndexOf(cha);
            if (idx == null)
            {
                cha.Id = m_Characters.Count;
                m_Characters.Add(cha);
            }
            else
            {
                cha.Id = idx;
            }
        }
    }
}