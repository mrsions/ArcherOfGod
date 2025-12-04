#nullable enable

using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    public class AutoReturnPool : MonoBehaviour
    {
        [SerializeField] private float m_Duration;

        private void OnEnable()
        {
            ReturnAsync().Forget();
        }

        private async UniTask ReturnAsync()
        {
            await UniTask.WaitForSeconds(m_Duration);

            GameObjectPool.main.Return(gameObject);
        }
    }
}