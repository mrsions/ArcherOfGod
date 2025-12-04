#nullable enable

using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
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