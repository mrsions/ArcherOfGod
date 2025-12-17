#nullable enable

using UnityEngine;

namespace AOT
{
    /// <summary>
    /// OnEnable될 때 posRange/rotRange 범위 내에서 랜덤하게 위치/회전 더함.
    /// 이펙트 같은거 똑같이 안보이게 할 때 씀. 매번 enable될 때마다 누적됨.
    /// </summary>
    public class RandomPositionOnEnable : MonoBehaviour
    {
        public Vector3 posRange;
        public Vector3 rotRange;

        private void OnEnable()
        {
            transform.localPosition = transform.localPosition + new Vector3(
                Random.Range(-posRange.x, posRange.x),
                Random.Range(-posRange.y, posRange.y),
                Random.Range(-posRange.z, posRange.z));

            transform.localEulerAngles = transform.localEulerAngles + new Vector3(
                Random.Range(-rotRange.x, rotRange.x),
                Random.Range(-rotRange.y, rotRange.y),
                Random.Range(-rotRange.z, rotRange.z));
        }
    }
}