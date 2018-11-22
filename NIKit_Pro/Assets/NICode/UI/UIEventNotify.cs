using System;
using System.Collections;
using System.Collections.Generic;

public sealed class UIEventNotify: Singleton<UIEventNotify>
{
    private Dictionary<string, Delegate> _eventDic = new Dictionary<string, Delegate>();

    #region 注册
    public void RegisterEvent(string eventType, Action callback)
    {
        RegisterEvent(eventType, (Delegate)callback);
    }

    public void RegisterEvent<T>(string eventType, Action<T> callback)
    {
        RegisterEvent(eventType, (Delegate)callback);
    }

    public void RegisterEvent<T,U>(string eventType, Action<T,U> callback)
    {
        RegisterEvent(eventType, (Delegate)callback);
    }

    public void RegisterEvent<T, U, V>(string eventType, Action<T, U, V> callback)
    {
        RegisterEvent(eventType, (Delegate)callback);
    }

    private void RegisterEvent(string eventType, Delegate del)
    {
        Delegate temp;
        if (_eventDic.TryGetValue(eventType, out temp))
        {
            Delegate.Combine(temp, del);
        }
        else
        {
            _eventDic.Add(eventType, del);
        }
    }
    #endregion

    #region 注销
    public void UnregisterEvent(string eventType, Action callback)
    {
        UnregisterEvent(eventType, (Delegate)callback);
    }

    public void UnregisterEvent<T>(string eventType, Action<T> callback)
    {
        UnregisterEvent(eventType, (Delegate)callback);
    }

    public void UnregisterEvent<T,U>(string eventType, Action<T,U> callback)
    {
        UnregisterEvent(eventType, (Delegate)callback);
    }

    public void UnregisterEvent<T,U,V>(string eventType, Action<T, U, V> callback)
    {
        UnregisterEvent(eventType, (Delegate)callback);
    }

    private void UnregisterEvent(string eventType, Delegate del)
    {
        Delegate temp;
        if (_eventDic.TryGetValue(eventType, out temp))
        {
            Delegate.Remove(temp, del);
        }
        else
        {
            NIDebug.Log("===============不存在这个委托，无需移除==================");
        }
    }
    #endregion

    #region Fire广播事件
    public void FireEvent(string eventType)
    {
        Delegate[] delList = GetMethods(eventType);
        if (delList != null)
        {
            for (int i = 0; i < delList.Length; i++)
            {
                try
                {
                    ((Action)delList[i])();
                }
                catch (Exception e)
                {
                    NIDebug.LogError(e);
                }
            }
        }
    }

    public void FireEvent<T>(string eventType, T arg)
    {
        Delegate[] delList = GetMethods(eventType);
        if (delList != null)
        {
            for (int i = 0; i < delList.Length; i++)
            {
                try
                {
                    ((Action<T>)delList[i])(arg);
                }
                catch (Exception e)
                {
                    NIDebug.LogError(e);
                }
            }
        }
    }

    public void FireEvent<T,U>(string eventType, T arg1, U arg2)
    {
        Delegate[] delList = GetMethods(eventType);
        if (delList != null)
        {
            for (int i = 0; i < delList.Length; i++)
            {
                try
                {
                    ((Action<T,U>)delList[i])(arg1, arg2);
                }
                catch (Exception e)
                {
                    NIDebug.LogError(e);
                }
            }
        }
    }

    public void FireEvent<T,U,V>(string eventType, T arg1, U arg2, V arg3)
    {
        Delegate[] delList = GetMethods(eventType);
        if (delList != null)
        {
            for (int i = 0; i < delList.Length; i++)
            {
                try
                {
                    ((Action<T,U,V>)delList[i])(arg1, arg2, arg3);
                }
                catch (Exception e)
                {
                    NIDebug.LogError(e);
                }
            }
        }
    }

    private Delegate[] GetMethods(string eventType)
    {
        Delegate temp;
        if (_eventDic.TryGetValue(eventType, out temp))
        {
            return temp.GetInvocationList();
        }

        return null;
    }
    #endregion
}
