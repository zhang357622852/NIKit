using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NIKit.Tools
{
    /// <summary>
    /// Singleton pattern
    /// </summary>
    public abstract class Singleton<T> where T : class, new()
    {
        protected static T mInstance;
        private static readonly object padlock = new object();

        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    lock (padlock)
                    {
                        if (mInstance == null)
                        {
                            mInstance = new T();
                        }
                    }
                }

                return mInstance;
            }
        }
    }
}

