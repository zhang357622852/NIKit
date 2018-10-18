/// <summary>
/// ResourceLoadingWnd.cs
/// Created by fengsc 2017/03/14
/// Changed by lic 2018/01/123
/// 资源更新加载界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ResourceLoadingWnd : WindowBase<ResourceLoadingWnd>
{
    // 描述
    public UILabel mDesc;

    // 主进度条
    public UISlider mainProgress;

    // 版权窗口
    public GameObject mCopyRight;

    // 游戏忠告
    public UILabel mTips;

    // 游戏版权
    public UILabel mCopyRightTips;

    // 版本号提示
    public UILabel mVersionTips;

    /// <summary>
    /// 版号提示背景
    /// </summary>
    public UISprite mCopyRightBg;

    #region 私有变量

    // 当前流程(默认开始阶段为解压缩阶段)
    private int mCurProcedure = ResourceLoadingConst.LOAD_TYPE_START_DECOMPRESS;

    // 当前状态(默认开始阶段为CHECK状态)
    private int mCurState = ResourceLoadingStateConst.LOAD_STATE_CHECK;

    // 记录上次下载的进度
    private int mLastDownloadBytes = 0;

    // 当前下载的实际值
    private float mCurProgressValue = 0f;

    private int mLastCheckTime = 0;

    private int mLastSpeend = 0;

    #endregion

    #region 内部函数

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 显示客户端版本
#if UNITY_ANDROID && ! UNITY_EDITOR

        if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_PUBLISH))
            mVersionTips.text = "「"+ QCCommonSDK.QCCommonSDK.FindNativeSetting("Client_Name") +"」"+ "Ver " + ConfigMgr.ClientVersion;
        else
            mVersionTips.text = "Ver " + ConfigMgr.ClientVersion;

#else

        mVersionTips.text = "Ver " + ConfigMgr.ClientVersion;

#endif

        // 重置进度条
        ResetProgress();

        // 设置描述
        SetStartDesc();

        // 游戏版号版权提示
        string copyRightTip = LocalizationMgr.Get("ResourceLoadingWnd_10", LocalizationConst.START);

        // 提示防沉迷提示
        string preventAddictionTip = LocalizationMgr.Get("ResourceLoadingWnd_100", LocalizationConst.START);

        // 没有防沉迷提示和游戏版号版权提示
        if (string.IsNullOrEmpty(copyRightTip) &&
            string.IsNullOrEmpty(preventAddictionTip))
        {
            mCopyRight.SetActive(false);
            return;
        }

        // 设置文本信息
        mCopyRightTips.text = copyRightTip;
        mTips.text = preventAddictionTip;

        // 设置背景高度
        if (string.IsNullOrEmpty(copyRightTip))
            mCopyRightBg.height = 40;
        else
            mCopyRightBg.height = 145;

        // 显示节点
        mCopyRight.SetActive(true);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        LoadingMgr.eventStateChange += OnChangeState;

        LoadingMgr.eventProgressChange += OnChangeProgress;

        // 注册tween播放完毕回调
        EventDelegate.Add(GetComponent<TweenAlpha>().onFinished, OnTweenFinish);
    }

    /// <summary>
    /// 状态变化回调
    /// </summary>
    /// <param name="Procedure">Procedure.</param>
    /// <param name="state">State.</param>
    private void OnChangeState(int Procedure, int state)
    {
        mCurProcedure = Procedure;

        mCurState = state;

        // 设置进度条
        if(mCurState == ResourceLoadingStateConst.LOAD_STATE_CHECK)
        {
            CancelInvoke("RefreshProgress");

            // 重置进度
            ResetProgress();

            // 设置描述
            SetStartDesc();

            mCurProgressValue = 0f;
        }
        else if(mCurState == ResourceLoadingStateConst.LOAD_STATE_UPDATE)
        {
            InvokeRepeating("RefreshProgress", 0.0f, 0.001f);
        }
    }

    /// <summary>
    /// Raises the change state event.
    /// </summary>
    /// <param name="progress">Progress.</param>
    private void OnChangeProgress(float progress)
    {
        mCurProgressValue = progress;
    }

    /// <summary>
    /// 刷新进度条
    /// </summary>
    private void RefreshProgress()
    {
        if(mainProgress.value < 1f)
        {

            float value = Mathf.Min(mainProgress.value + 0.002f, mCurProgressValue);

            if(value < mainProgress.value)
                return;

            // 修正进度值
            value = value > 1.0f ? 1.0f : value;

            mainProgress.value = value;

            // 更新描述信息
            UpdateDesc(value);

            return;
        }

        // 完成
        LoadingMgr.DoLoadingEnd(LoadingType.LOAD_TYPE_UPDATE_RES, mCurProcedure);

        if(mCurProcedure == ResourceLoadingConst.LOAD_TYPE_INIT)
        {
            // 播放动画
            GetComponent<TweenAlpha>().PlayForward();
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
    {
        LoadingMgr.eventStateChange -= OnChangeState;

        LoadingMgr.eventProgressChange -= OnChangeProgress;

        CancelInvoke("RefreshProgress");
    }

    /// <summary>
    /// 设置进度
    /// </summary>
    private void ResetProgress()
    {
        // 重置进度条进度
        mainProgress.value = 0f;
    }

    /// <summary>
    /// 计算下载速度
    /// </summary>
    /// <returns>The down speed.</returns>
    private int CalcDownSpeed()
    {
        int nowTime = TimeMgr.GetTime();

        // 1s内不做检测
        if(nowTime - mLastCheckTime < 1)
            return mLastSpeend;

        mLastCheckTime = nowTime;

        // 已经下载完的资源
        int downloadBytes = ResourceMgr.DownloadedBytes;

        // 计算1s内的下载速度
        int downloadSpeed = (int)((downloadBytes - mLastDownloadBytes) / (1024.0f * 1f));

        // 重新赋值上次已下载
        mLastDownloadBytes = downloadBytes;

        mLastSpeend = downloadSpeed;

        return downloadSpeed;
    }

    /// <summary>
    /// 设置描述
    /// </summary>
    /// <param name="progress">Progress.</param>
    private void UpdateDesc(float progress)
    {

        // 设置提示信息
        string desc = string.Empty;

        switch(mCurProcedure)
        {
            case ResourceLoadingConst.LOAD_TYPE_START_DECOMPRESS:
                desc = string.Format(LocalizationMgr.Get("ResourceLoadingWnd_2", LocalizationConst.START), progress);
                break;

            case ResourceLoadingConst.LOAD_TYPE_UPDATE:
                desc = string.Format(LocalizationMgr.Get("ResourceLoadingWnd_3", LocalizationConst.START), CalcDownSpeed(), progress);
                break;

            case ResourceLoadingConst.LOAD_TYPE_DECOMPRESS:
                desc = string.Format(LocalizationMgr.Get("ResourceLoadingWnd_4", LocalizationConst.START), progress);
                break;

            case ResourceLoadingConst.LOAD_TYPE_INIT:
                desc = string.Format(LocalizationMgr.Get("ResourceLoadingWnd_5", LocalizationConst.START), progress);
                break;

            default :
                break;
        }

        mDesc.text = desc;
    }

    /// <summary>
    /// 设置起始描述
    /// </summary>
    /// <param name="progress">Progress.</param>
    private void SetStartDesc()
    {
        // 设置提示信息
        string desc = string.Empty;

        switch(mCurProcedure)
        {
            case ResourceLoadingConst.LOAD_TYPE_START_DECOMPRESS:
                desc = LocalizationMgr.Get("ResourceLoadingWnd_1", LocalizationConst.START);
                break;

            case ResourceLoadingConst.LOAD_TYPE_UPDATE:
                desc = string.Format(LocalizationMgr.Get("ResourceLoadingWnd_9", LocalizationConst.START));
                break;

            default :
                break;
        }

        mDesc.text = desc;
    }

    /// <summary>
    /// 播放Tween完成
    /// </summary>
    void OnTweenFinish()
    {
        LPCMapping ePara = new LPCMapping();
        ePara.Add("type", LoadingType.LOAD_TYPE_UPDATE_RES);

        // 抛出进度条结束事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_LOADING_END, MixedValue.NewMixedValue<LPCMapping>(ePara));

        // 销毁窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    #endregion

}
