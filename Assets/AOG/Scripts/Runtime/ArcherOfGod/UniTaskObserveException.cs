using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AOT
{
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