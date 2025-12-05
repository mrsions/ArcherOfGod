using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    public class PlayerController : MonoBehaviour
    {
        //-- Serializable
        [SerializeField] private CharacterBehaviour m_Cha;

        //-- Private

        //-- Properties

        //------------------------------------------------------------------------------

        private void Start()
        {
            RunAsync().Forget();
        }

        private async UniTask RunAsync()
        {
            while(m_Cha.IsLive)
            {
                m_Cha.InputAxis = GameSettings.main.GetPlayerMoveAction().ReadValue<Vector2>();

                await UniTask.Yield(PlayerLoopTiming.PreUpdate, destroyCancellationToken);
            }
        }
    }
}