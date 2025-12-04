using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    [ExecuteInEditMode]
    public class LookAtTarget2D : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;
        
        private Quaternion m_CalcOffset;

        private void Start()
        {
            m_CalcOffset = Quaternion.Euler(offset);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector2 mPos = (Vector2)transform.position;
            Vector2 tPos = (Vector2)target.position;

#if UNITY_EDITOR
            transform.rotation = Quaternion.LookRotation(tPos - mPos) * Quaternion.Euler(offset);
#else
            transform.rotation = Quaternion.LookRotation(tPos - mPos) * m_CalcOffset;
#endif
        }
    }
}