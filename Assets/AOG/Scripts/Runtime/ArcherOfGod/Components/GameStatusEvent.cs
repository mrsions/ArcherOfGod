using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    [RequireComponent(typeof(Button))]
    public class GameStatusEvent : MonoBehaviour
    {
        //-- Serializable
        public EGameStatus eventStatus;
        public UnityEvent onEvent;

        //-- Private

        //------------------------------------------------------------------------------

        private void Awake()
        {
            GameManager.main.OnChangedStatus += OnChangedStatus;
            OnChangedStatus(GameManager.main, GameManager.main.Status);
        }

        private void OnChangedStatus(GameManager manager, EGameStatus status)
        {
            if (status == eventStatus)
            {
                onEvent?.Invoke();
            }
        }
    }
}