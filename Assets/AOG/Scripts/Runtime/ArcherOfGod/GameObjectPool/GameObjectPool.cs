using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UObject = UnityEngine.Object;

namespace AOT
{
    public interface IPoolable
    {
        void OnReleased();
    }

    /// <summary>
    /// 오브젝트 풀링 싱글톤. Rent로 빌리고 Return으로 반납. GameObject랑 Component 둘 다 됨.
    /// IPoolable 인터페이스 있으면 반납할때 콜백 호출됨. 에디터에서 잘못 Destroy하면 경고 뜸.
    /// usePooling 끄면 풀링 안하고 매번 생성/파괴함. 프리웜 없고 사이즈 제한 없음.
    /// </summary>
    public class GameObjectPool : MonoBehaviour
    {
        private static GameObjectPool s_Main;
        public static GameObjectPool main => s_Main
            ??= Resources.FindObjectsOfTypeAll<GameObjectPool>().FirstOrDefault()
            ?? new GameObject("GameObjectPool").AddComponent<GameObjectPool>();

        class PoolLink : MonoBehaviour
        {
            public UObject Prefab;
            public UObject Instance;
            internal int Reference;

#if UNITY_EDITOR|| NOPT
            private void OnDestroy()
            {
                if (gameObject.scene.isLoaded && Reference != 0)
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

                AddHistory("Create");
            }

#if USE_GAMEOBJECTPOOL_HISTORY
            [SerializeField]
            private List<string> History = new();
            internal void AddHistory(string v)
            {
                History.Add(v + "\r\n" + Environment.StackTrace);
            }
#else
            [System.Diagnostics.Conditional("USE_GAMEOBJECTPOOL_HISTORY")]
            internal void AddHistory(string v) { }
#endif


            internal void ValidateRent()
            {
                if (Reference != 0)
                {
                    Debug.LogError("Use already rented object.", gameObject);
                    throw new InvalidOperationException("Use already rented object. " + name);
                }
            }

            internal void ValidateReturn()
            {
                if (Reference != 1)
                {
                    Debug.LogError("Return already returned object.", gameObject);
                    throw new InvalidOperationException("Return already returned object : " + name);
                }
            }
        }

        class Pool
        {
            public UObject Prefab;
            public bool hasInterface;
            public Stack<PoolLink> stack = new();

            public Pool(UObject prefab)
            {
                Prefab = prefab;
                if (prefab is GameObject go) hasInterface = go.GetComponentInChildren<IPoolable>(true) != null;
                else if (prefab is Component comp) hasInterface = comp.GetComponentInChildren<IPoolable>(true) != null;
            }
        }


        //-- Serializable
        public bool usePooling = true;

        //-- Events

        //-- Private 
        private Dictionary<UObject, Pool> pools = new();

        //-- Properties


        //------------------------------------------------------------------------------

        private void Awake()
        {
            m_TempTransform = new GameObject("Temp").transform;
            m_TempTransform.SetParent(transform);
            m_TempTransform.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            s_Main = this;
        }

        private void OnDisable()
        {
            if (s_Main == this) s_Main = null;
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
                    if (usePooling)
                    {
                        PoolLink link = go.AddComponent<PoolLink>();
                        link.Setup(prefab, go);
                        link.Reference++;
                    }
                    go.transform.SetParent(parent);
                    return go;
                }
                else
                {
                    PoolLink link = pool.stack.Pop();
                    link.ValidateRent();

                    GameObject go = (GameObject)link.Instance;
                    go.transform.SetPositionAndRotation(pos, rot);
                    link.AddHistory("Rent");
                    link.Reference++;
                    go.transform.SetParent(parent);
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
            if (parent == null)
            {
                parent = GameManager.main.effectContainer;
            }

            if (!pools.TryGetValue(prefab, out var pool))
            {
                pools.Add(prefab, pool = new(prefab));
            }

            do
            {
                if (pool.stack.Count == 0)
                {
                    T comp = Instantiate(prefab, pos, rot, m_TempTransform);
                    if (usePooling)
                    {
                        PoolLink link = comp.gameObject.AddComponent<PoolLink>();
                        link.Setup(prefab, comp);
                        link.Reference++;
                    }
                    comp.transform.SetParent(parent);
                    return comp;
                }
                else
                {
                    PoolLink link = pool.stack.Pop();
                    link.ValidateRent();

                    T comp = (T)link.Instance;
                    comp.transform.SetPositionAndRotation(pos, rot);
                    link.AddHistory("Rent");
                    link.Reference++;
                    comp.transform.SetParent(parent);
                    return comp;
                }
            }
            while (true);
        }

        private static List<IPoolable> s_StackInterfaces = new();
        private Transform m_TempTransform;

        public void Return<T>(T obj) where T : Component
        {
            if (!obj) return;

            Return(obj.gameObject);
        }
        public void Return(GameObject go)
        {
            if (!go) return;

            if (!usePooling)
            {
                Destroy(go);
                return;
            }

            var link = go.GetComponent<PoolLink>()
                ?? throw new InvalidCastException("It's not rented object.");

            link.ValidateReturn();

            if (!pools.TryGetValue(link.Prefab, out var pool))
            {
                throw new InvalidOperationException("It is abnormal rented object.");
            }

            if (pool.hasInterface)
            {
                go.GetComponentsInChildren<IPoolable>(true, s_StackInterfaces);
                for (int i = 0; i < s_StackInterfaces.Count; i++)
                {
                    IPoolable o = s_StackInterfaces[i];
                    o.OnReleased();
                }
            }

            go.transform.SetParent(m_TempTransform, false);

            link.Reference--;
            link.AddHistory("Return");
            pool.stack.Push(link);
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