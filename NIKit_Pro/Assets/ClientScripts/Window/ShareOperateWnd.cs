/// <summary>
/// ShareOperateWnd.cs
/// Created by zhangwm 2018/07/06
/// 分享操作界面(1.隐藏圣域 2.单次召唤 3.十次召唤 4.装备强化+12 5.使魔升星6)
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;
using QCCommonSDK.Addition;
using QCCommonSDK;
using System;

public class ShareOperateWnd : WindowBase<ShareOperateWnd>
{
    #region 公有变量
    public UITexture mBg;
    public GameObject mSharePartGo;
    //按钮Grid
    public UIGrid mShareGrid;
    public UILabel mShareTitleLab;
    public GameObject mQRCodeGo;
    public UILabel mQRCodeLab;
    //按钮数组
    public GameObject[] mBtnArray;
    //玩家等级
    public UILabel mUserLevelLab;
    //玩家姓名
    public UILabel mUserNameLab;
    //玩家ID
    public UILabel mUserIDLab;

    public GameObject mEquipWndGo;
    public GameObject mSacredWndGo;
    public GameObject mPetUpStarWndGo;
    public GameObject mSummonWndGo;

    //关闭按钮
    public GameObject mCloseBtn;

    public TweenScale mTweenScale;
    #endregion

    #region 私有变量
    private ShareOperateType mCurShareOperateType = ShareOperateType.None;
    public enum ShareOperateType
    {
        None = 0,
        /// <summary>
        /// 隐藏圣域
        /// </summary>
        Sacred,
        /// <summary>
        /// 单次召唤
        /// </summary>
        SingleSummon,
        /// <summary>
        /// 十次召唤
        /// </summary>
        TenSummon,
        /// <summary>
        /// 装备强化 +15
        /// </summary>
        EquipIntensify,
        /// <summary>
        /// 使魔升星6
        /// </summary>
        PetUpStar,
    }

    private Dictionary<ShareOperateType, string> mShareTypeBgDic = new Dictionary<ShareOperateType, string>
    {
        { ShareOperateType.Sacred, "sacred_share_bg"},
        { ShareOperateType.SingleSummon, "summon_share_bg"},
        { ShareOperateType.TenSummon, "summon_share_bg"},
        { ShareOperateType.EquipIntensify, "equip_share_bg"},
        { ShareOperateType.PetUpStar, "equip_share_bg"},
    };

    private bool isClick = false;
    [HideInInspector]
    public Action mTweenFinishedCallback;
    private Property mShareProp;
    #endregion

    #region 内部接口
    private void Awake()
    {
        for (int i = 0; i < mShareGrid.transform.childCount; i++)
        {
            Transform tran = mShareGrid.transform.GetChild(i);
            tran.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
        mTweenScale.AddOnFinished(OnTweenFinished);

        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();
    }

    private void OnTweenFinished()
    {
        if (mTweenFinishedCallback != null)
            mTweenFinishedCallback();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    private void OnDestroy()
    {
        ShareSupport.OnShareResult -= ShareHook;
        // 重置打开窗口列表中移除
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mShareTitleLab.text = LocalizationMgr.Get("ShareOperateWnd_1");
        mQRCodeLab.text = LocalizationMgr.Get("ShareOperateWnd_2");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        UIEventListener.Get(mBtnArray[0]).onClick = OnClickFacebook;
        UIEventListener.Get(mBtnArray[1]).onClick = OnClickTwitter;
        UIEventListener.Get(mBtnArray[2]).onClick = OnClickLine;
        UIEventListener.Get(mBtnArray[3]).onClick = OnClickWechat;
        UIEventListener.Get(mBtnArray[4]).onClick = OnClickWechatMoments;
        UIEventListener.Get(mBtnArray[5]).onClick = OnClickWeibo;

        ShareSupport.OnShareResult += ShareHook;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        //背景
        string resPath = string.Format("Assets/Art/UI/Window/Background/{0}.png", mShareTypeBgDic[mCurShareOperateType]);
        mBg.mainTexture = ResourceMgr.LoadTexture(resPath);
        mBg.MakePixelPerfect();

        //分享平台按钮
        mSharePartGo.SetActive(true);
        mQRCodeGo.SetActive(false);
        List<string> sharePlatformList = ShareMgr.GetSharePlatformList();
        for (int i = 0; i < sharePlatformList.Count; i++)
        {
            Transform tran = mShareGrid.transform.Find(sharePlatformList[i]);
            if (tran != null)
                tran.gameObject.SetActive(true);
        }
        mShareGrid.Reposition();

        if (ME.user == null)
            return;

        //玩家等级
        mUserLevelLab.text = string.Format(LocalizationMgr.Get("MainWnd_13") ,ME.user.GetLevel().ToString());
        //玩家姓名
        mUserNameLab.text = ME.user.GetName();
        //玩家ID
        mUserIDLab.text = ME.user.GetRid();
    }

    /// <summary>
    /// 各个页面的开关
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    private void SwitchWnd(ShareOperateType type, params object[] args)
    {
        switch (type)
        {
            case ShareOperateType.Sacred:
                Property petProp = args[0] as Property;
                //设置背景图片颜色-元素颜色
                if (petProp != null)
                {
                    string elementColor = MonsterConst.MonsterElementColorMap[MonsterMgr.GetElement(petProp.GetClassID())];
                    mBg.color = ColorConfig.ParseToColor(ColorConfig.ExtractColorHex(elementColor));
                }

                ShareOperateSacredWnd sacredCtrl = null;
                if (mSacredWndGo.transform.childCount > 0)
                    sacredCtrl = mSacredWndGo.GetComponentInChildren<ShareOperateSacredWnd>();
                else
                {
                    GameObject wndGo = WindowMgr.OpenWnd(ShareOperateSacredWnd.WndType, mSacredWndGo.transform);
                    if (wndGo != null)
                        sacredCtrl = mSacredWndGo.GetComponentInChildren<ShareOperateSacredWnd>();
                }

                if (sacredCtrl != null)
                    sacredCtrl.BindData(petProp, this);

                break;

            case ShareOperateType.SingleSummon:

            case ShareOperateType.TenSummon:
                ShareOperateSummonWnd summonCtrl = null;
                if (mSummonWndGo.transform.childCount > 0)
                    summonCtrl = mSummonWndGo.GetComponentInChildren<ShareOperateSummonWnd>();
                else
                {
                    GameObject wndGo = WindowMgr.OpenWnd(ShareOperateSummonWnd.WndType, mSummonWndGo.transform);
                    if (wndGo != null)
                        summonCtrl = mSummonWndGo.GetComponentInChildren<ShareOperateSummonWnd>();
                }

                if (summonCtrl != null)
                    summonCtrl.BindData(type, args[0] as Texture, args[1] as CsvRow, args[2] as Property);
                break;

            case ShareOperateType.EquipIntensify:
                ShareOperateEquipIntensifyWnd ctrl = null;
                if (mEquipWndGo.transform.childCount > 0)
                    ctrl = mEquipWndGo.GetComponentInChildren<ShareOperateEquipIntensifyWnd>();
                else
                {
                    GameObject wndGo = WindowMgr.OpenWnd(ShareOperateEquipIntensifyWnd.WndType, mEquipWndGo.transform);
                    if (wndGo != null)
                        ctrl = mEquipWndGo.GetComponentInChildren<ShareOperateEquipIntensifyWnd>();
                }

                if (ctrl != null)
                    ctrl.BindData(args[0] as Property, args[1] as Dictionary<int, int>);

                break;

            case ShareOperateType.PetUpStar:
                ShareOperatePetUpStarWnd petUpStarCtrl = null;
                if (mPetUpStarWndGo.transform.childCount > 0)
                    petUpStarCtrl = mPetUpStarWndGo.GetComponentInChildren<ShareOperatePetUpStarWnd>();
                else
                {
                    GameObject wndGo = WindowMgr.OpenWnd(ShareOperatePetUpStarWnd.WndType, mPetUpStarWndGo.transform);
                    if (wndGo != null)
                        petUpStarCtrl = mPetUpStarWndGo.GetComponentInChildren<ShareOperatePetUpStarWnd>();
                }

                if (petUpStarCtrl != null)
                    petUpStarCtrl.BindData(args[0] as Property, this);

                break;
        }
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    private void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// Facebook
    /// </summary>
    private void OnClickFacebook(GameObject go)
    {
        if (isClick)
            return;

        isClick = true;

        Coroutine.DispatchService(CaptureScreenshot(SharePlatformType.Facebook));
    }

    /// <summary>
    /// Twitter
    /// </summary>
    private void OnClickTwitter(GameObject go)
    {
        if (isClick)
            return;

        isClick = true;

        Coroutine.DispatchService(CaptureScreenshot(SharePlatformType.Twitter));
    }

    /// <summary>
    /// Line
    /// </summary>
    private void OnClickLine(GameObject go)
    {
        if (isClick)
            return;

        isClick = true;

        Coroutine.DispatchService(CaptureScreenshot(SharePlatformType.Line));
    }

    /// <summary>
    /// 微信好友
    /// </summary>
    private void OnClickWechat(GameObject go)
    {
        if (isClick)
            return;

        isClick = true;

        Coroutine.DispatchService(CaptureScreenshot(SharePlatformType.Wechat));
    }

    /// <summary>
    /// WeiXin 分享朋友圈
    /// </summary>
    private void OnClickWechatMoments(GameObject go)
    {
        if (isClick)
            return;

        isClick = true;

        Coroutine.DispatchService(CaptureScreenshot(SharePlatformType.WechatMoments));
    }

    /// <summary>
    /// Weibo
    /// </summary>
    private void OnClickWeibo(GameObject go)
    {
        if (isClick)
            return;

        isClick = true;

        Coroutine.DispatchService(CaptureScreenshot(SharePlatformType.Sina));
    }

    /// <summary>
    /// 得到分享描述
    /// </summary>
    /// <returns></returns>
    private string GetShareDes()
    {
        switch (mCurShareOperateType)
        {
            case ShareOperateType.Sacred:
                return LocalizationMgr.Get("ShareOperateWnd_17");

            case ShareOperateType.SingleSummon:
                if (mShareProp != null)
                {
                    if (mShareProp.GetStar() >= 5)
                        return string.Format(LocalizationMgr.Get("ShareOperateWnd_22"), LocalizationMgr.Get(mShareProp.Query<string>("share_desc")));
                    else
                        return LocalizationMgr.Get("ShareOperateWnd_20");
                }
                else
                    return LocalizationMgr.Get("ShareOperateWnd_20");

            case ShareOperateType.TenSummon:
                return LocalizationMgr.Get("ShareOperateWnd_21");

            case ShareOperateType.EquipIntensify:
                return LocalizationMgr.Get("ShareOperateWnd_18");

            case ShareOperateType.PetUpStar:
                return LocalizationMgr.Get("ShareOperateWnd_19");
        }

        return LocalizationMgr.Get("ShareWnd_3");
    }

    private IEnumerator CaptureScreenshot(string sharePlatforType)
    {
        mSharePartGo.SetActive(false);
        mQRCodeGo.SetActive(true);
        mCloseBtn.SetActive(false);
        yield return new WaitForEndOfFrame();

        if (SceneMgr.UiCamera == null)
        {
            ShareHook(null);
            yield return null;
        }

        //计算实际图片大小，但改变窗口屏幕大小时，图片的实际像素是不一样的。
        float captureWeight = 1f * Screen.width * mBg.width / 1280;
        float captureHeight = 1f * Screen.height * mBg.height / 720;

        Vector3 screenPos = SceneMgr.UiCamera.WorldToScreenPoint(mBg.transform.position);
        string filename = string.Format("{0}/Screenshot/{1}.png", ConfigMgr.LOCAL_ROOT_PATH, "screenshot");
        Game.CustomCaptureScreenshot(filename, new Rect(screenPos.x - captureWeight / 2,
            Screen.height - (screenPos.y + captureHeight / 2), captureWeight, captureHeight));

#if UNITY_EDITOR
        ShareHook(null);
#else
            ShareSupport.Share(sharePlatforType, string.Empty, string.Empty, GetShareDes(), filename);
#endif
    }

    /// <summary>
    /// 分享成功的回调
    /// </summary>
    /// <param name="result"></param>
    private void ShareHook(QCEventResult result)
    {
        mSharePartGo.SetActive(true);
        mQRCodeGo.SetActive(false);
        mCloseBtn.SetActive(true);
        isClick = false;
    }
    #endregion

    #region 外部接口
    public void BindData(ShareOperateType type, params object[] args)
    {
        if (type == ShareOperateType.None)
            return;

        mCurShareOperateType = type;

        Redraw();
        SwitchWnd(mCurShareOperateType, args);

        if (mCurShareOperateType == ShareOperateType.SingleSummon)
            mShareProp = args[2] as Property;
    }
    #endregion
}
