#nullable enable

using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 일정 시간 후 자동으로 풀에 반납됨. OnEnable에서 타이머 시작.
    /// 파티클이나 이펙트처럼 시간 지나면 사라져야 하는거에 붙임.
    /// </summary>
    public class AutoReturnPool : MonoBehaviour
    {
        [SerializeField] private float m_Duration;

        private void OnEnable()
        {
            ReturnAsync().Forget();
        }

        private async UniTask ReturnAsync()
        {
            await UniTask.WaitForSeconds(m_Duration, cancellationToken: destroyCancellationToken);

            GameObjectPool.main.Return(gameObject);
        }
    }
}