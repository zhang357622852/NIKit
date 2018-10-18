/// <summary>
/// ShareSelectWnd.cs
/// Created by fengsc 2018/08/20
/// 分享平台选择窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QCCommonSDK.Addition;
using QCCommonSDK;

public class ShareSelectWnd : WindowBase<ShareSelectWnd>
{
    // 窗口标题
    public UILabel mTitle;

    // 分享按钮
    public GameObject[] mShareBtns;

    public UIPanel mPanel;

    public UISprite mBg;

    public UIGrid mGrid;

    private bool isClick;

    // 分享标题
    private string mShareTitle;

    // 分享描述
    private string mDesc;

    // 截屏大小
    private Vector2 mScreenshotSize = Vector2.zero;

    // 截屏位置
    private Vector3 mScreenPositon = Vector3.zero;

    // Use this for initialization
    void Start ()
    {
        mTitle.text = LocalizationMgr.Get("ShareSelectWnd_1");

        List<string> sharePlatformList = ShareMgr.GetSharePlatformList();
        for (int i = 0; i < sharePlatformList.Count; i++)
        {
            Transform tran = mGrid.transform.Find(sharePlatformList[i]);
            if (tran == null)
                continue;

            tran.gameObject.SetActive(true);
        }

        mGrid.Reposition();

        // 注册按钮点击事件
        RegisterEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mShareBtns[0]).onClick = OnClickFacebook;
        UIEventListener.Get(mShareBtns[1]).onClick = OnClickTwitter;
        UIEventListener.Get(mShareBtns[2]).onClick = OnClickLine;
        UIEventListener.Get(mShareBtns[3]).onClick = OnClickWechat;
        UIEventListener.Get(mShareBtns[4]).onClick = OnClickWechatMoments;
        UIEventListener.Get(mShareBtns[5]).onClick = OnClickWeibo;

        ShareSupport.OnShareResult += ShareHook;
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
        mPanel.alpha = 0.01f;

        yield return new WaitForEndOfFrame();

        if (SceneMgr.UiCamera == null)
        {
            ShareHook(null);
            yield return null;
        }

        mShareTitle += "";

        mDesc += "";

        Vector3 screenPos = SceneMgr.UiCamera.WorldToScreenPoint(mScreenPositon);
        string path = string.Format("{0}/Screenshot/{1}.png", ConfigMgr.LOCAL_ROOT_PATH, "screenshot");
        Game.CustomCaptureScreenshot(path, new Rect(screenPos.x - mScreenshotSize.x / 2,
            Screen.height - (screenPos.y + mScreenshotSize.y / 2), mScreenshotSize.x, mScreenshotSize.y));
//
#if UNITY_EDITOR
        ShareHook(null);
#else
        ShareSupport.Share(sharePlatforType, string.Empty, mShareTitle, mDesc, path);
#endif
    }

    /// <summary>
    /// 分享成功的回调
    /// </summary>
    /// <param name="result"></param>
    private void ShareHook(QCEventResult result)
    {
        isClick = false;

        mShareTitle = string.Empty;

        mDesc = string.Empty;

        mPanel.alpha = 1.0f;

        EventMgr.FireEvent(EventMgrEventType.EVENT_SHARE_SUCCESS, null);

        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 设置背景透明度
    /// </summary>
    public void SetBgAlpha(float alpha)
    {
        mBg.alpha = alpha;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(Vector2 ScreenshotSize, Vector3 ScreenPositon, string title, string desc)
    {
        mScreenshotSize = ScreenshotSize;

        mScreenPositon = ScreenPositon;

        mShareTitle = title;

        mDesc = desc;
    }
}
