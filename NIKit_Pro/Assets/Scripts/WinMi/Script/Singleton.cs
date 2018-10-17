using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace WinMi
{
    /// <summary>
    /// 单例模式
    /// 双重锁
    /// </summary>
    public abstract class Singleton<T> where T : new()
    {
        private static T _instance;
        private static readonly object locker = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                            _instance = new T();
                    }
                }

                return _instance;
            }
        }
    }

}
