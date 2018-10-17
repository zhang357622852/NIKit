using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WinMi
{
    /// <summary>
    /// 单例模式
    /// 继承于MonoBehaviour
    /// </summary>
    public abstract class SingletonMB<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _instance = this as T;
        }
    }
}
