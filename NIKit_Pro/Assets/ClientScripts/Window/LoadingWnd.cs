using UnityEngine;
using System.Collections;
using LPC;
using UnityEngine.SceneManagement;

/// <summary>
/// 简单的loading界面
/// </summary>
public class LoadingWnd : WindowBase<LoadingWnd>
{
    #region 变量

    // 背景图Texture
    public UITexture mLoadingBg;

    // 进度条Slider
    public UISlider mMainProgress;

    // 随机关于游戏小技巧描述Label
    public UILabel mLbdesc;

    // 进度条类型
    private int mType;

    // 副本数据
    private LPCMapping mPara = LPCMapping.Empty;

    #endregion

    #region 内部函数

    void Start()
    {
        // 注册进入副本失败回调
        EventMgr.RegisterEvent("LoadingWnd", EventMgrEventType.EVENT_ENTER_INSTANCE_FAIL, OnEnterInstanceFail);
    }

    /// <summary>
    /// 销毁事件
    /// </summary>
    void OnDestroy()
    {
        EventMgr.UnregisterEvent("LoadingWnd");

        // 释放资源
        if (mLoadingBg != null)
            mLoadingBg.mainTexture = null;
    }

    /// <summary>
    /// 界面心跳
    /// </summary>
    void UpdateProgress()
    {
        // 获取当期资源加载进度
        float progress = PreloadMgr.GetProgress("Combat");

        // 设置进度条位置
        if (mMainProgress.value > progress)
            mMainProgress.value = Mathf.Min(mMainProgress.value + 0.002f, 0.99f);
        else
            mMainProgress.value = Mathf.Min(mMainProgress.value + 0.002f, progress);;

        // 进度条还没有满，副本资源加载还没有完成
        if (mMainProgress.value < 1f)
            return;

        // 取消定时器
        CancelInvoke("UpdateProgress");

        // 抛出loading结束事件
        LPCMapping ePara = new LPCMapping();
        ePara.Add("type", mType);
        ePara.Append(mPara);
        EventMgr.FireEvent(EventMgrEventType.EVENT_LOADING_END, MixedValue.NewMixedValue<LPCMapping>(ePara));
    }

    /// <summary>
    /// 重绘界面
    /// </summary>
    private void RedrawWnd()
    {
        string bgName = string.Empty;

        // 根据不同的类型绘制窗口
        switch (mType)
        {
            case LoadingType.LOAD_TYPE_INSTANCE:

                // 获取副本id
                string instanceId = mPara.GetValue<string>("instance_id");

                // 副本类型的loading
                LPCMapping info = InstanceMgr.GetInstanceInfo(instanceId);
                CsvRow mapInfo = MapMgr.GetMapConfig(info.GetValue<int>("map_id"));
                bgName = mapInfo.Query<string>("loading");

                // 获取提示脚本类型
                int scriptNo = mapInfo.Query<int>("tip_type_script");
                string tipType = (string) ScriptMgr.Call(scriptNo, instanceId,
                    mType, mPara, mapInfo.Query<LPCValue>("tip_type_args"));

                // 设置文本信息
                mLbdesc.text = SystemTipsMgr.FetchTip(tipType);

                break;

            case LoadingType.LOAD_TYPE_LOGIN:
                // 登录类型的loading
                break;

            case LoadingType.LOAD_TYPE_PVP:
                // 竞技场类型的loading
                break;

            case LoadingType.LOAD_TYPE_SPE_INSTANCE:
                // 特殊副本类型的loading
                break;
        }

        mLoadingBg.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/Loading/{0}.png", bgName));
    }

    /// <summary>
    /// 进入副本失败回调
    /// </summary>
    private void OnEnterInstanceFail(int eventId, MixedValue para)
    {        
        WindowMgr.DestroyWindow(gameObject.name);
    }

    #endregion 

    #region 外部函数

    /// <summary>
    /// 设置Loading界面相关数据
    /// </summary>
    public void SetLoading(int type, LPCMapping para)
    {
        // 缓存数据
        mType = type;
        mPara = para;

        // 设置进度
        mMainProgress.value = 0f;

        // 注册定时刷新显示窗口事件
        InvokeRepeating("UpdateProgress", 0.0f, 0.01f);

        // 重绘窗口
        RedrawWnd();
    }

    #endregion
}
