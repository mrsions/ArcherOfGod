using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    [RequireComponent(typeof(Button))]
    public class UIGameStatus : MonoBehaviour
    {
        //-- Serializable

        //-- Private


        //------------------------------------------------------------------------------

        private void Awake()
        {
            //GameManager.main.OnChangedStatus
        }

    }
}