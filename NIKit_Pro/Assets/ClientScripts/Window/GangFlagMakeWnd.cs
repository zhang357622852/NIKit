/// <summary>
/// GangFlagMakeWnd.cs
/// Created by fengsc 2018/01/25
/// 公会旗帜制作界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangFlagMakeWnd : WindowBase<GangFlagMakeWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 旗帜提示
    public UILabel mFlagTips;

    // 旗帜基础格子
    public FlagItemWnd mFlagItemWnd;

    // 随机制作旗帜按钮
    public GameObject mRandomFlagBtn;
    public UILabel mRandomFlagBtnLb;

    // 取消制作按钮
    public GameObject mCancelBtn;
    public UILabel mCancelBtnLb;

    // 旗帜底图
    public UITexture mBaseIcon;

    // 底图切换按钮
    public GameObject mBaseLeftBtn;
    public GameObject mBaseRightBtn;

    // 底图颜色改变滑动条
    public UISlider mBaseColorSlider;
    public UILabel mBaseColorTips;

    // 底图明暗改变滑动条
    public UISlider mBaseLightSlider;
    public UILabel mBaseLightTips;

    // 旗帜样式图标
    public UITexture mStyleIcon;

    // 样式图标切换按钮
    public GameObject mStyleLeftBtn;
    public GameObject mStyleRightBtn;

    // 样式图标明暗改变滑动条
    public UISlider mStyleLightSlider;
    public UILabel mStyleLightTips;

    // 旗帜图标
    public UITexture mIcon;

    // 图标切换按钮
    public GameObject mIconLeftBtn;
    public GameObject mIconRightBtn;

    // 随机图标按钮
    public GameObject mRandomIconBtn;
    public UILabel mRandomIconBtnLb;

    // 制作完成按钮
    public GameObject mMakeFinishBtn;
    public UILabel mMakeFinishBtnLb;

    LPCArray mFlagData = LPCArray.Empty;

    CallBack mCallBack;

    List<string> mBaseIconList = new List<string>();

    List<string> mIconList = new List<string>();

    List<string> mStyleIconList = new List<string>();

    int mBaseIconIndex = 0;

    int mStyleIconIndex = 0;

    int mIconIndex = 0;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化本地化文本
        InitText();

        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("GangFlagMakeWnd_1");
        mFlagTips.text = LocalizationMgr.Get("GangFlagMakeWnd_2");
        mRandomFlagBtnLb.text = LocalizationMgr.Get("GangFlagMakeWnd_3");
        mCancelBtnLb.text = LocalizationMgr.Get("GangFlagMakeWnd_4");
        mBaseColorTips.text = LocalizationMgr.Get("GangFlagMakeWnd_5");
        mBaseLightTips.text = LocalizationMgr.Get("GangFlagMakeWnd_9");
        mStyleLightTips.text = LocalizationMgr.Get("GangFlagMakeWnd_6");
        mRandomIconBtnLb.text = LocalizationMgr.Get("GangFlagMakeWnd_7");
        mMakeFinishBtnLb.text = LocalizationMgr.Get("GangFlagMakeWnd_8");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mRandomFlagBtn).onClick = OnClickRandomFlagBtn;
        UIEventListener.Get(mCancelBtn).onClick = OnClickCancelBtn;
        UIEventListener.Get(mBaseLeftBtn).onClick = OnClickBaseLeftBtn;
        UIEventListener.Get(mBaseRightBtn).onClick = OnClickBaseRightBtn;
        UIEventListener.Get(mStyleLeftBtn).onClick = OnClickStyleLeftBtn;
        UIEventListener.Get(mStyleRightBtn).onClick = OnClickStyleRightBtn;
        UIEventListener.Get(mIconLeftBtn).onClick = OnClickIconLeftBtn;
        UIEventListener.Get(mIconRightBtn).onClick = OnClickIconRightBtn;
        UIEventListener.Get(mRandomIconBtn).onClick = OnClickRandomIconBtn;
        UIEventListener.Get(mMakeFinishBtn).onClick = OnClickMakeFinishBtn;

        // 注册滑动条变化事件
        EventDelegate.Add(mBaseColorSlider.onChange, OnBaseColorChange);
        EventDelegate.Add(mBaseLightSlider.onChange, OnBaseColorChange);
        EventDelegate.Add(mStyleLightSlider.onChange, OnStyleLightChange);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mBaseIconList = GangMgr.BaseIconList;
        mIconList = GangMgr.IconList;
        mStyleIconList = GangMgr.StyleIconList;

        // 图标路径
        string path = "Assets/Art/UI/Icon/gang/{0}.png";

        // 底图
        LPCArray baseData = mFlagData[0].AsArray;

        float baseH = baseData[1].AsFloat / 255f;

        float baseS = baseData[2].AsFloat / 255f;

        mBaseIcon.mainTexture = ResourceMgr.LoadTexture(string.Format(path, baseData[0].AsString));

        // HSV
        mBaseIcon.color = Color.HSVToRGB(baseH, baseS, 1);

        // 图标
        LPCArray iconData = mFlagData[1].AsArray;

        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format(path, iconData[0].AsString));

        // 样式
        LPCArray styleData = mFlagData[2].AsArray;

        float styleV = styleData[1].AsFloat / 255f;

        mStyleIcon.mainTexture = ResourceMgr.LoadTexture(string.Format(path, styleData[0].AsString));

        // 设置图片的明暗
        mStyleIcon.color = Color.HSVToRGB(0, 0, styleV);

        // 初始化滑动条
        mBaseColorSlider.Set(baseH);
        mBaseLightSlider.Set(baseS);

        mStyleLightSlider.Set(styleV);
    }

    /// <summary>
    /// 底图颜色滑动条值变化回调
    /// </summary>
    void OnBaseColorChange()
    {
        mBaseIcon.color = Color.HSVToRGB(mBaseColorSlider.value, mBaseLightSlider.value, 1);

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 底图明暗滑动条变化回调
    /// </summary>
    void OnBaseLightChange()
    {
        // 明暗度调节
        mBaseIcon.color = Color.HSVToRGB(mBaseColorSlider.value, mBaseLightSlider.value, 1);

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 样式滑动条变化回调
    /// </summary>
    void OnStyleLightChange()
    {
        // 明暗度调节
        mStyleIcon.color = Color.HSVToRGB(0, 0, mStyleLightSlider.value);

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 窗口关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 随机制作旗帜按钮点击回调
    /// </summary>
    void OnClickRandomFlagBtn(GameObject go)
    {
        RandomFlag();

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 取消制作按钮点击回调
    /// </summary>
    void OnClickCancelBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 底图切换按钮点击回调
    /// </summary>
    void OnClickBaseLeftBtn(GameObject go)
    {
        if (mBaseIconIndex == 0)
            mBaseIconIndex = mBaseIconList.Count;

        mBaseIconIndex--;

        mBaseIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/gang/{0}.png", mBaseIconList[mBaseIconIndex]));

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 底图切换按钮点击回调
    /// </summary>
    void OnClickBaseRightBtn(GameObject go)
    {
        if (mBaseIconIndex + 1 == mBaseIconList.Count)
            mBaseIconIndex = 0;

        mBaseIconIndex++;

        mBaseIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/gang/{0}.png", mBaseIconList[mBaseIconIndex]));

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 样式切换按钮点击回调
    /// </summary>
    void OnClickStyleLeftBtn(GameObject go)
    {
        if (mStyleIconIndex == 0)
            mStyleIconIndex = mStyleIconList.Count;

        mStyleIconIndex--;

        mStyleIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/gang/{0}.png", mStyleIconList[mStyleIconIndex]));

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 样式切换按钮点击回调
    /// </summary>
    void OnClickStyleRightBtn(GameObject go)
    {
        if (mStyleIconIndex + 1 == mStyleIconList.Count)
            mStyleIconIndex = 0;

        mStyleIconIndex++;

        mStyleIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/gang/{0}.png", mStyleIconList[mStyleIconIndex]));

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 图标切换按钮点击回调
    /// </summary>
    void OnClickIconLeftBtn(GameObject go)
    {
        if (mIconIndex == 0)
            mIconIndex = mIconList.Count;

        mIconIndex--;

        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/gang/{0}.png", mIconList[mIconIndex]));

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 图标切换按钮点击回调
    /// </summary>
    void OnClickIconRightBtn(GameObject go)
    {
        if (mIconIndex + 1 == mIconList.Count)
            mIconIndex = 0;

        mIconIndex++;

        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/gang/{0}.png", mIconList[mIconIndex]));

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 随机图标按钮点击回调
    /// </summary>
    void OnClickRandomIconBtn(GameObject go)
    {
        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/gang/{0}.png", mIconList[Random.Range(0, mIconList.Count)]));

        // 刷新旗帜
        RefreshFlag();
    }

    /// <summary>
    /// 制作完成按钮点击回调
    /// </summary>
    void OnClickMakeFinishBtn(GameObject go)
    {
        if (mCallBack != null)
            mCallBack.Go(mFlagData);

        WindowMgr.DestroyWindow(gameObject.name);
    }

    void RefreshFlag()
    {
        mFlagData = LPCArray.Empty;

        LPCArray baseData = LPCArray.Empty;
        baseData.Add(mBaseIcon.mainTexture.name);
        baseData.Add(Mathf.RoundToInt(mBaseColorSlider.value * 255));
        baseData.Add(Mathf.RoundToInt(mBaseLightSlider.value * 255));

        mFlagData.Add(baseData);

        LPCArray iconData = LPCArray.Empty;
        iconData.Add(mIcon.mainTexture.name);

        mFlagData.Add(iconData);

        LPCArray styleData = LPCArray.Empty;
        styleData.Add(mStyleIcon.mainTexture.name);
        styleData.Add(Mathf.RoundToInt(mStyleLightSlider.value * 255));

        mFlagData.Add(styleData);

        // 绑定数据
        mFlagItemWnd.Bind(mFlagData);
    }

    /// <summary>
    /// 随机旗帜
    /// </summary>
    void RandomFlag()
    {
        mFlagData = LPCArray.Empty;

        string baseIcon = mBaseIconList[Random.Range(0, mBaseIconList.Count)];

        LPCArray baseData = LPCArray.Empty;
        baseData.Add(baseIcon);
        baseData.Add(Random.Range(0, 256));
        baseData.Add(Random.Range(0, 256));

        mFlagData.Add(baseData);

        string icon = mIconList[Random.Range(0, mIconList.Count)];
        LPCArray iconData = LPCArray.Empty;
        iconData.Add(icon);

        mFlagData.Add(iconData);

        string styleIcon = mStyleIconList[Random.Range(0, mStyleIconList.Count)];

        LPCArray styleData = LPCArray.Empty;
        styleData.Add(styleIcon);
        styleData.Add(Random.Range(0, 256));

        mFlagData.Add(styleData);

        // 绑定数据
        mFlagItemWnd.GetComponent<FlagItemWnd>().Bind(mFlagData);
    }

    public void Bind(LPCArray flag, CallBack cb)
    {
        mFlagData = flag;

        mCallBack = cb;

        // 绘制窗口
        Redraw();
    }
}
