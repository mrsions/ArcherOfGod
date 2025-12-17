using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// UniTask 취소 예외 때문에 콘솔 스팸되는거 막아주는 핸들러.
    /// OperationCanceledException이랑 TaskCanceledException은 무시하고 나머지만 로그 찍음.
    /// 한번 등록되면 끌 수 없음.
    /// </summary>
    public class UniTaskObserveException : MonoBehaviour
    {
        static bool hasObserve;

        private void Awake()
        {
            if (hasObserve) return;

            UniTaskScheduler.UnobservedExceptionWriteLogType = LogType.Warning;
            UniTaskScheduler.UnobservedTaskException += OnException;
        }

        private void OnException(Exception exception)
        {
            if (exception is System.OperationCanceledException
                || exception is TaskCanceledException)
            {
                return;
            }
            else
            {
                Debug.LogException(exception);
            }
        }
    }
}