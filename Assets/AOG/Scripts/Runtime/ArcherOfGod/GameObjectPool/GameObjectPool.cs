using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using UObject = UnityEngine.Object;

namespace AOT
{
    public interface IPoolable
    {
        void OnReleased();
    }

    public class GameObjectPool : MonoBehaviour
    {
        private static GameObjectPool s_Main;
        public static GameObjectPool main => s_Main
            ??= Resources.FindObjectsOfTypeAll<GameObjectPool>().FirstOrDefault()
            ?? new GameObject("GameObjectPool").AddComponent<GameObjectPool>();

        class PoolInfo : MonoBehaviour
        {
            public UObject Prefab;
            public UObject Instance;
            internal bool InPool;

#if UNITY_EDITOR
            private void OnDestroy()
            {
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new SystemException("Don't deestroy rented object.");
                }
            }
#endif

            internal void Setup(UObject prefab, UObject item)
            {
                Prefab = prefab;
                Instance = item;
                hideFlags = HideFlags.HideAndDontSave;
            }
        }

        class Pool
        {
            public UObject Prefab;
            public bool hasInterface;
            public Stack<PoolInfo> stack = new();

            public Pool(UObject prefab)
            {
                Prefab = prefab;
                if (prefab is GameObject go) hasInterface = go.GetComponentInChildren<IPoolable>(true) != null;
                else if (prefab is Component comp) hasInterface = comp.GetComponentInChildren<IPoolable>(true) != null;
            }
        }


        //-- Serializable

        //-- Events

        //-- Private 
        private Dictionary<UObject, Pool> pools = new();

        //-- Properties


        //------------------------------------------------------------------------------

        private void Awake()
        {
            s_Main = this;

            m_TempTransform = new GameObject("Temp").transform;
            m_TempTransform.SetParent(transform);
            m_TempTransform.gameObject.SetActive(false);
        }

        public GameObject Rent(GameObject prefab, Transform parent = null)
            => Rent(prefab, Vector3.zero, Quaternion.identity, parent);

        public GameObject Rent(GameObject prefab, Vector3 pos, Transform parent = null)
            => Rent(prefab, pos, Quaternion.identity, parent);

        public GameObject Rent(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            if (!pools.TryGetValue(prefab, out var pool))
            {
                pools.Add(prefab, pool = new(prefab));
            }

            do
            {
                if (pool.stack.Count == 0)
                {
                    GameObject go = Instantiate(prefab, pos, rot, m_TempTransform);
                    go.AddComponent<PoolInfo>().Setup(prefab, go);
                    go.transform.SetParent(parent);
                    return go;
                }
                else
                {
                    PoolInfo info = pool.stack.Pop();
                    if (!info.InPool)
                    {
                        Debug.LogError("It has not in pool. but it in the pool.", info.Instance);
                        continue;
                    }

                    GameObject go = (GameObject)info.Instance;
                    go.transform.SetPositionAndRotation(pos, rot);
                    go.transform.SetParent(parent);

                    info.InPool = false;
                    return go;
                }
            }
            while (true);
        }


        public T Rent<T>(T prefab, Transform parent = null)
            where T : Component
            => Rent(prefab, Vector3.zero, Quaternion.identity, parent);

        public T Rent<T>(T prefab, Vector3 pos, Transform parent = null)
            where T : Component
            => Rent(prefab, pos, Quaternion.identity, parent);

        public T Rent<T>(T prefab, Vector3 pos, Quaternion rot, Transform parent = null)
            where T : Component
        {
            if (!pools.TryGetValue(prefab, out var pool))
            {
                pools.Add(prefab, pool = new(prefab));
            }

            do
            {
                if (pool.stack.Count == 0)
                {
                    T comp = Instantiate(prefab, pos, rot, m_TempTransform);
                    comp.gameObject.AddComponent<PoolInfo>().Setup(prefab, comp);
                    comp.transform.SetParent(parent);
                    return comp;
                }
                else
                {
                    PoolInfo info = pool.stack.Pop();
                    if (!info.InPool)
                    {
                        Debug.LogError("It has not in pool. but it in the pool.", info.Instance);
                        continue;
                    }

                    T comp = (T)info.Instance;
                    comp.transform.SetPositionAndRotation(pos, rot);
                    comp.transform.SetParent(parent);

                    info.InPool = false;
                    return comp;
                }
            }
            while (true);
        }

        private static List<IPoolable> s_StackInterfaces = new();
        private Transform m_TempTransform;

        public void Return<T>(T obj) where T : Component => Return(obj.gameObject);
        public void Return(GameObject obj)
        {
            Assert.IsNotNull(obj);

            var info = obj.GetComponent<PoolInfo>();
            if (info == null) throw new InvalidCastException("It's not rented object.");

            if (info.InPool) throw new InvalidOperationException("It has already object in pool.");

            if (!pools.TryGetValue(info.Prefab, out var pool))
            {
                throw new InvalidOperationException("It is abnormal rented object.");
            }

            if (pool.hasInterface)
            {
                obj.GetComponentsInChildren<IPoolable>(true, s_StackInterfaces);
                for (int i = 0; i < s_StackInterfaces.Count; i++)
                {
                    IPoolable o = s_StackInterfaces[i];
                    o.OnReleased();
                }
            }

            obj.transform.SetParent(m_TempTransform, false);

            info.InPool = true;

            pool.stack.Push(info);
        }
    }

    public static class __PoolableExtension
    {
        public static void ReturnPool(this Component obj)
        {
            GameObjectPool.main.Return(obj);
        }
        public static void ReturnPool(this GameObject obj)
        {
            GameObjectPool.main.Return(obj);
        }
    }
}