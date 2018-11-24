/// <summary>
/// Coroutine.cs
/// Copy from zhangyg 2014-10-22
/// 协程的实现
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

/// <summary>
/// 协程的实现
/// 和系统的协程区别是：这个协程有catch报错，而系统没有
///                  可以防止手机上闪退，所有的代码如果有使用协程应该使用本实现，而不能使用系统的协程
/// 另外，本协程调度可以不依赖于具体的gameobject（因为gameobject析构或者隐藏后，其挂载的协程就停止运行了）
/// </summary>
public class Coroutine : MonoBehaviour
{
    #region 变量

    // 协程对象
    private static Coroutine mInstance;

    // 协程实例列表
    private static Dictionary<string, Proxy> mCoroutionMap = new Dictionary<string, Proxy>();
    private static List<Proxy> mCoroutionList = new List<Proxy>();
    private static Dictionary<string, Proxy> mCoroutionFixedMap = new Dictionary<string, Proxy>();
    private static List<Proxy> mCoroutionFixedList = new List<Proxy>();

    // 协程开始时间
    public static float mStartTime;

    // 内部固定驱动模式
    static bool mFixedModeInternal = false;

    // 内部驱动时间间隔
    // 10毫秒，不得随意更改该数字!!!
    public const int mFixedDeltaTime = 10;

    // 时间缩放接受的精度，千分之一
    public const int mTimeScalePrecition = 1000;

    // 将0.001秒变成整数
    public const int mPrecision = 1000;
    public const float mAllRPrecision = 1e-6f;
    private static long mFixedTime;

    // 上一次驱动协程Proxy对象
    private static Proxy mLastProxy;

    #endregion

    #region 属性

    /// <summary>
    /// Coroutine对象
    /// </summary>
    public static Coroutine Instance
    {
        get
        {
            return mInstance;
        }

        set
        {
            mInstance = value;
        }
    }

    /// <summary>
    /// 内部固定驱动模式
    /// </summary>
    public static bool IsFixedModeInternal
    {
        get
        {
            return mFixedModeInternal;
        }
    }

    /// <summary>
    /// 修正后的驱动间隔
    /// </summary>
    public static int FixedDeltaTime
    {
        get
        {
            float timeScale = Time.timeScale <= 1f ? Time.timeScale : 1f;
            return (int) (mTimeScalePrecition * (timeScale + 1e-4f)) * mFixedDeltaTime;
        }
    }

    /// <summary>
    /// 获取FixedTime
    /// </summary>
    public static long FixedTime
    {
        get
        {
            return mFixedTime;
        }
    }

    /// <summary>
    /// 获取协程驱动时间间隔
    /// </summary>
    public static float DeltaTime
    {
        get
        {
            return (! mFixedModeInternal ? Time.deltaTime : mAllRPrecision * FixedDeltaTime);
        }
    }

    /// <summary>
    /// 获取协程时间
    /// </summary>
    public static float time
    {
        get
        {
            return (! mFixedModeInternal ? (Time.time - mStartTime) : mAllRPrecision * mFixedTime);
        }
    }

    #endregion

    /// <summary>
    /// Proxy.
    /// </summary>
    public class Proxy : IYieldObject
    {
        #region 变量

        public string mName;
        public IEnumerator mIEnumerator;
        public UnityEngine.Object mOwner;
        public bool mFixed;

        // 结束标识
        private bool mIsFinished = false;

        // 正在执行标识
        private bool mIsRunning = true;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Coroutine+Proxy"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="i">The index.</param>
        /// <param name="owner">Owner.</param>
        /// <param name="fixedMode">If set to <c>true</c> fixed coro.</param>
        public Proxy(string name, IEnumerator i, UnityEngine.Object owner, bool fixedMode)
        {
            mName = name;
            mIEnumerator = i;
            mOwner = owner;
            mFixed = fixedMode;
        }

        /// <summary>
        /// Determines whether this instance is done.
        /// </summary>
        /// <returns><c>true</c> if this instance is done; otherwise, <c>false</c>.</returns>
        public bool IsDone()
        {
            return IsFinished;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Coroutine+Proxy"/> is finished.
        /// </summary>
        /// <value><c>true</c> if is finished; otherwise, <c>false</c>.</value>
        public bool IsFinished
        {
            get
            {
                if (mIsFinished)
                    return true;

                return mOwner == null;
            }
            set
            {
                mIsFinished = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Coroutine+Proxy"/> is running.
        /// </summary>
        /// <value><c>true</c> if is running; otherwise, <c>false</c>.</value>
        public bool IsRunning
        {
            get
            {
                return mIsRunning;
            }
            set
            {
                mIsRunning = value;
            }
        }

        /// <summary>
        /// Finish this instance.
        /// </summary>
        public void Finish()
        {
            // 标识IsFinished为true
            IsFinished = true;

            Dictionary<string, Proxy> cm = (! mFixed ? mCoroutionMap : mCoroutionFixedMap);
            List<Proxy> cl = (! mFixed ? mCoroutionList : mCoroutionFixedList);

            Proxy p;
            if (cm.TryGetValue(mName, out p) && p == this)
            {
                cm.Remove(mName);
                cl.Remove(p);
            }
        }
    }

    /// <summary>
    /// Initializes the <see cref="Coroutine"/> class.
    /// </summary>
    static Coroutine()
    {
        //// 获取RootGameObject对象
        //if (GameRoot.RootGameObject == null)
        //    return;

        //// 添加Coroutine组件
        //mInstance = GameRoot.RootGameObject.AddComponent<Coroutine>();
    }

    /// <summary>
    /// 确定精度
    /// </summary>
    public static float FixedPrecision(float f)
    {
        return (long)(f * 1000) * 0.001f;
    }

    /// <summary>
    /// 将FixedTime和Time同步一次
    /// </summary>
    public static void SyncFixedTime()
    {
        mStartTime = FixedPrecision(Time.time);
        mFixedTime = 0;
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        SyncFixedTime();
    }

    // update只有在编辑器或者手机上才会生效
    // 如果该代码是运行在其他平台上则标识是验证客户端不需要Update驱动
    // 外面逻辑自己驱动
    #if UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        mFixedModeInternal = true;
        long uf = (long) FixedDeltaTime;
        bool steping = true;

        // 计算时间差
        long interval = (long) ((FixedPrecision(Time.time) - mStartTime) * mPrecision * mTimeScalePrecition);

        // 循环驱动固定协程
        while (uf > 0 && (mFixedTime + uf) <= interval)
        {
            mFixedTime += uf;

            if (steping && mCoroutionFixedList.Count > 0)
            {
                Proxy[] ps = new Proxy[mCoroutionFixedList.Count];
                mCoroutionFixedList.CopyTo(ps, 0);
                int i = 0;

                // 如果mLastProxy不为null，则接着上衣系驱动继续驱动
                if(mLastProxy != null)
                {
                    for (i = 0; i < ps.Length; i++)
                    {
                        if (ps[i] == mLastProxy)
                            break;
                    }

                    if (i == ps.Length)
                        i = 0;

                    mLastProxy = null;
                }

                // 接着上一次驱动继续StepFixed
                for (; i < ps.Length; i++)
                {
                    Proxy p = ps[i];

                    // StepFixed
                    StepFixed(p);

                    // 时间缩放没有改变
                    long newUF = (long) FixedDeltaTime;
                    if (uf == newUF)
                        continue;

                    // 时间缩放改变不能再继续驱动了，记录uf
                    uf = newUF;

                    // 标识steping为false
                    steping = false;

                    // 记录当前驱动位置
                    if (i < ps.Length - 1)
                        mLastProxy = ps[i + 1];

                    // 退出驱动
                    break;
                }
            }
        }

        // 标识mFixedModeInternal驱动结束
        mFixedModeInternal = false;

        // 刷新TimeScale
        TimeMgr.RefreshTimeScale();
    }

    #endif

    /// <summary>
    /// Steps the fixed.
    /// </summary>
    /// <param name="p">P.</param>
    static void StepFixed(Proxy p)
    {
        // 判断协程是否需要结束
        if (p.mIEnumerator == null)
        {
            p.Finish();
            return;
        }

        if (! p.IsFinished && p.IsRunning)
        {
            try
            {
                object yieldObj = p.mIEnumerator.Current;
                if (yieldObj != null && yieldObj is IYieldObject)
                {
                    // 我们自己的协程类
                    if ((yieldObj as IYieldObject).IsDone())
                    {
                        bool isFin = !p.mIEnumerator.MoveNext();
                        if (!p.IsFinished)
                            p.IsFinished = isFin;
                    }
                }
                else if (yieldObj is UnityEngine.WaitForSeconds ||
                    yieldObj is UnityEngine.WaitForEndOfFrame ||
                    yieldObj is UnityEngine.WaitForFixedUpdate)
                {
                    NIDebug.Log("固定协程驱动类型非法, IEnumerator = {0}, Type = {1}",
                        p.mIEnumerator, yieldObj.GetType().ToString());
                }
                else
                {
                    bool isFin = ! p.mIEnumerator.MoveNext();
                    if (! p.IsFinished)
                        p.IsFinished = isFin;
                }
            }
            catch (Exception e)
            {
                p.IsFinished = true;
                NIDebug.LogException(e);
            }
        }

        // 判断是否需要结束
        if (p.IsFinished)
        {
            p.Finish();
        }
    }

    // 固定协程驱动一次
    public static void StepFixedOnce()
    {
        mFixedModeInternal = true;

        if (mCoroutionFixedList.Count != 0)
        {
            long uf = (long) FixedDeltaTime;
            mFixedTime += uf;

            Proxy[] ps = new Proxy[mCoroutionFixedList.Count];
            mCoroutionFixedList.CopyTo(ps, 0);
            int i = 0;

            // 如果mLastProxy不为null，则接着上衣系驱动继续驱动
            if(mLastProxy != null)
            {
                for (i = 0; i < ps.Length; i++)
                {
                    if (ps[i] == mLastProxy)
                        break;
                }

                if (i == ps.Length)
                    i = 0;

                mLastProxy = null;
            }

            // 接着上一次驱动继续StepFixed
            for (; i < ps.Length; i++)
            {
                Proxy p = ps[i];

                // StepFixed
                StepFixed(p);

                // 时间缩放没有改变
                long newUF = (long) FixedDeltaTime;
                if (uf == newUF)
                    continue;

                // 时间缩放改变不能再继续驱动了，记录uf
                uf = newUF;

                // 记录当前驱动位置
                if (i < ps.Length - 1)
                    mLastProxy = ps[i + 1];

                // 退出驱动
                break;
            }
        }

        // 标识驱动结束
        mFixedModeInternal = false;
    }

    // 协程步进，这里主要是增加了try catch，以及处理从YieldObject继承的我们自己的协程类
    static IEnumerator Step(Proxy p)
    {
        while (true)
        {
            if (p.mIEnumerator == null)
            {
                p.Finish();
                yield break;
            }

            if (!p.IsFinished && p.IsRunning)
            {
                try
                {
                    object yieldObj = p.mIEnumerator.Current;

                    if (yieldObj != null && yieldObj is IYieldObject)
                    {
                        // 我们自己的协程类
                        if ((yieldObj as IYieldObject).IsDone())
                        {
                            bool isFin = ! p.mIEnumerator.MoveNext();

                            if (! p.IsFinished)
                                p.IsFinished = isFin;
                        }
                    }
                    else
                    {
                        bool isFin = ! p.mIEnumerator.MoveNext();
                        if (! p.IsFinished)
                            p.IsFinished = isFin;
                    }
                }
                catch (Exception e)
                {
                    p.IsFinished = true;
                    NIDebug.LogException(e);
                }
            }

            if (p.IsFinished)
            {
                p.Finish();
                yield break;
            }
            else
            {
                yield return p.mIEnumerator.Current;
            }
        }
    }

    /// <summary>
    /// Dispatchs the service.
    /// </summary>
    /// <returns>The service.</returns>
    /// <param name="i">The index.</param>
    public static Proxy DispatchService(IEnumerator i)
    {
        // mInstance对象不存在
        if (mInstance == null)
            return null;

        return DispatchService(i, mInstance.gameObject, null, false);
    }

    /// <summary>
    /// Dispatchs the service.
    /// </summary>
    /// <returns>The service.</returns>
    /// <param name="i">The index.</param>
    /// <param name="name">Name.</param>
    public static Proxy DispatchService(IEnumerator i, string name)
    {
        // mInstance对象不存在
        if (mInstance == null)
            return null;

        return DispatchService(i, mInstance.gameObject, name, false);
    }

    /// <summary>
    /// Dispatchs the service.
    /// </summary>
    /// <returns>The service.</returns>
    /// <param name="i">The index.</param>
    /// <param name="fixedMode">If set to <c>true</c> fixed mode.</param>
    public static Proxy DispatchService(IEnumerator i, bool fixedMode)
    {
        // mInstance对象不存在
        if (mInstance == null)
            return null;

        return DispatchService(i, mInstance.gameObject, null, fixedMode);
    }

    /// <summary>
    /// Dispatchs the service.
    /// </summary>
    /// <returns>The service.</returns>
    /// <param name="i">The index.</param>
    /// <param name="name">Name.</param>
    /// <param name="fixedMode">If set to <c>true</c> fixed mode.</param>
    public static Proxy DispatchService(IEnumerator i, string name, bool fixedMode)
    {
        // mInstance对象不存在
        if (mInstance == null)
            return null;

        return DispatchService(i, mInstance.gameObject, name, fixedMode);
    }

    /// <summary>
    /// Dispatchs the service.
    /// </summary>
    /// <returns>The service.</returns>
    /// <param name="i">The index.</param>
    /// <param name="owner">Owner.</param>
    public static Proxy DispatchService(IEnumerator i, UnityEngine.Object owner)
    {
        return DispatchService(i, owner, null, false);
    }

    /// <summary>
    /// Dispatchs the service.
    /// </summary>
    /// <returns>The service.</returns>
    /// <param name="i">The index.</param>
    /// <param name="owner">Owner.</param>
    /// <param name="name">Name.</param>
    public static Proxy DispatchService(IEnumerator i, UnityEngine.Object owner, string name)
    {
        return DispatchService(i, owner, name, false);
    }

    /// <summary>
    /// Dispatchs the service.
    /// </summary>
    /// <returns>The service.</returns>
    /// <param name="i">The index.</param>
    /// <param name="owner">Owner.</param>
    /// <param name="name">Name.</param>
    /// <param name="fixedMode">If set to <c>true</c> fixed coro.</param>
    public static Proxy DispatchService(IEnumerator i, UnityEngine.Object owner, string name, bool fixedMode)
    {
        // mInstance对象不存在
        if (mInstance == null)
            return null;

        // 没有owner对象
        if (owner == null)
        {
            NIDebug.LogWarning("owner 是空的，i = {0}, name = {1}", i, name);
            return null;
        }

        Dictionary<string, Proxy> cm = (! fixedMode ? mCoroutionMap : mCoroutionFixedMap);
        List<Proxy> cl = (! fixedMode ? mCoroutionList : mCoroutionFixedList);

#if UNITY_EDITOR
        if (cm.Count > 500)
            NIDebug.LogWarning("Coroutine数量太多!");
#endif

        // 没有名字，自动取一个
        if (string.IsNullOrEmpty(name))
            name = GameUtility.GetUniqueName("AnonymousCoroutine");

        // 如果已经包含了该协程名的协程
        if (cm.ContainsKey(name))
        {
            NIDebug.LogWarning(string.Format("重名Coroutine {0}, 失败!", name));
            return null;
        }

        Proxy p = new Proxy(name, i, owner, fixedMode);
        cm.Add(name, p);
        cl.Add(p);

        if (! fixedMode)
            mInstance.StartCoroutine(Step(p));
        else
            StepFixed(p);

        return p;
    }

    /// <summary>
    /// Stops the coroutine.
    /// </summary>
    /// <param name="name">Name.</param>
    public static new void StopCoroutine(string name)
    {
        Proxy p;
        if (mCoroutionMap.TryGetValue(name, out p))
        {
            p.IsFinished = true;
            mCoroutionMap.Remove(name);
            mCoroutionList.Remove(p);
        }

        if (mCoroutionFixedMap.TryGetValue(name, out p))
        {
            p.IsFinished = true;
            mCoroutionFixedMap.Remove(name);
            mCoroutionFixedList.Remove(p);
        }
    }

    /// <summary>
    /// Stops all coroutines.
    /// </summary>
    public static new void StopAllCoroutines()
    {
        foreach (Proxy p in mCoroutionList)
            p.IsFinished = true;

        mCoroutionMap.Clear();
        mCoroutionList.Clear();

        foreach (Proxy p in mCoroutionFixedList)
            p.IsFinished = true;

        mCoroutionFixedMap.Clear();
        mCoroutionFixedList.Clear();
    }

    /// <summary>
    /// 挂起协程
    /// </summary>
    /// <param name="name">Name.</param>
    public static void SuspendCoroutine(string name)
    {
        Proxy p;
        if (mCoroutionMap.TryGetValue(name, out p))
            p.IsFinished = false;

        if (mCoroutionFixedMap.TryGetValue(name, out p))
            p.IsFinished = false;
    }

    /// <summary>
    /// 检查是否有同名协程存在
    /// </summary>
    /// <returns><c>true</c> if has coroutine the specified name; otherwise, <c>false</c>.</returns>
    /// <param name="name">Name.</param>
    public static bool HasCoroutine(string name)
    {
        return mCoroutionMap.ContainsKey(name) || mCoroutionFixedMap.ContainsKey(name);
    }

    /// <summary>
    /// 秒转变为固定时间
    /// </summary>
    public static long SecondToFixedTime(float t)
    {
        return (long) (FixedPrecision(t) * mPrecision * mTimeScalePrecition);
    }

    /// <summary>
    /// 固定时间转变为秒
    /// </summary>
    public static float FixedTimeToSecond(long t)
    {
        return mAllRPrecision * t;
    }
}

public interface IYieldObject
{
    bool IsDone();
}

public abstract class YieldMonoObject : IYieldObject
{
    public abstract bool IsDone();
}

public class WaitForMSeconds
{
    public static WaitForSeconds New(int msec)
    {
        return new WaitForSeconds(0.001f * msec);
    }
}

// 重写UnityEngine的WaitForSeconds
public class WaitForSeconds : IYieldObject
{
    protected long mWaitTimes;

    public WaitForSeconds(float waitSeconds)
    {
        mWaitTimes = Coroutine.SecondToFixedTime(waitSeconds);
    }

    public bool IsDone()
    {
        if (Coroutine.IsFixedModeInternal)
            mWaitTimes -= Coroutine.FixedDeltaTime;
        else
            mWaitTimes -= Coroutine.SecondToFixedTime(Time.deltaTime);

        return mWaitTimes <= 0;
    }
}
