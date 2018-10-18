/// <summary>
/// ShareWnd.cs
/// Created by zhangwm 2018/06/26
/// 分享
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;
using QCCommonSDK.Addition;
using QCCommonSDK;

public class ShareWnd : WindowBase<ShareWnd>
{
    #region 成员变量
    //带有颜色背景图
    public UISprite mColorBg;

    // ● 游戏下载 ●
    public UILabel mCodeTip;

    //使魔立绘
    public UITexture mheroTex;

    //关闭按钮
    public GameObject mCloseBtn;

    //按钮Grid
    public UIGrid mShareGrid;

    //按钮数组
    public GameObject[] mBtnArray;

    //玩家ID
    public UILabel mMyIdLab;

    //复制我的ID
    public UILabel mCopyIdLab;

    public TweenScale mTweenScale;

    private bool isClick = false;
    #endregion

    private void Start()
    {
        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);

        for (int i = 0; i < mShareGrid.transform.childCount; i++)
        {
            Transform tran = mShareGrid.transform.GetChild(i);
            tran.gameObject.SetActive(false);
        }

        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();
    }

    private void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
        ShareSupport.OnShareResult -= ShareHook;
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        if (ME.user == null)
            return;

        mMyIdLab.text = string.Format(LocalizationMgr.Get("ShareWnd_2"), ME.user.GetRid());
        mCopyIdLab.text = LocalizationMgr.Get("ShareWnd_1");
        mCodeTip.text = LocalizationMgr.Get("ShareWnd_4");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mCopyIdLab.gameObject).onClick = OnClickCopyID;

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
        //随机背景与使魔立绘
        CsvRow config = ShareMgr.FetchShareRule();
        if (config != null)
        {
            mColorBg.color = ColorConfig.ParseToColor(config.Query<string>("color"));
            string resPath = string.Format("Assets/Art/UI/Window/Background/{0}.png", config.Query<string>("image"));
            mheroTex.mainTexture = ResourceMgr.LoadTexture(resPath);
            mheroTex.MakePixelPerfect();
        }

        //分享平台按钮
        List<string> sharePlatformList = ShareMgr.GetSharePlatformList();
        for (int i = 0; i < sharePlatformList.Count; i++)
        {
            Transform tran = mShareGrid.transform.Find(sharePlatformList[i]);
            if (tran != null)
                tran.gameObject.SetActive(true);
        }
        mShareGrid.Reposition();
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    private void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 复制我的ID
    /// </summary>
    private void OnClickCopyID(GameObject go)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 给出提示信息
        DialogMgr.Notify(string.Format(LocalizationMgr.Get("SystemWnd_42")));

        // 调用平台接口复制名称至剪切板
        QCCommonSDK.Native.NativeCall.CopyToNativeClipboard(ME.user.GetRid());
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
    /// 微信分享好友
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

    private IEnumerator CaptureScreenshot(string sharePlatforType)
    {
        mShareGrid.gameObject.SetActive(false);
        mCopyIdLab.gameObject.SetActive(false);
        mCloseBtn.SetActive(false);
        yield return new WaitForEndOfFrame();

        if (SceneMgr.UiCamera == null)
        {
            ShareHook(null);
            yield return null;
        }

        //计算实际图片大小，但改变窗口屏幕大小时，图片的实际像素是不一样的。
        float captureWeight = 1f * Screen.width * mColorBg.width / 1280;
        float captureHeight = 1f * Screen.height * mColorBg.height / 720;

        Vector3 screenPos = SceneMgr.UiCamera.WorldToScreenPoint(mColorBg.transform.position);
        string filename = string.Format("{0}/Screenshot/{1}.png", ConfigMgr.LOCAL_ROOT_PATH, "screenshot");
        Game.CustomCaptureScreenshot(filename, new Rect(screenPos.x - captureWeight / 2,
            Screen.height - (screenPos.y + captureHeight / 2), captureWeight, captureHeight));

#if UNITY_EDITOR
        ShareHook(null);
#else
            ShareSupport.Share(sharePlatforType, string.Empty, string.Empty, LocalizationMgr.Get("ShareWnd_3"), filename);
#endif
    }

    /// <summary>
    /// 分享成功的回调
    /// </summary>
    /// <param name="result"></param>
    private  void ShareHook(QCEventResult result)
    {
        mShareGrid.gameObject.SetActive(true);
        mCopyIdLab.gameObject.SetActive(true);
        mCloseBtn.SetActive(true);
        isClick = false;
    }

}
