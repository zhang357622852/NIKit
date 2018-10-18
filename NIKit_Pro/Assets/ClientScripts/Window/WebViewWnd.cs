using UnityEngine;
using System.Collections;

public class WebViewWnd  : WindowBase<StoreWnd>
{
    #region 成员变量

    public GameObject mForwardBtn;

    public GameObject mBackBtn;

    public GameObject mRefreshBtn;

    public GameObject mCloseBtn;

    public GameObject mWebView;

    #endregion

    #region 私有变量

    private UniWebView webView;

    #endregion

#if UNITY_IOS || UNITY_ANDROID

    #region 内部函数

    void Awake()
    {
        webView = mWebView.GetComponent<UniWebView>();
    }

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 本地化文字
        mForwardBtn.GetComponent<UILabel>().text = LocalizationMgr.Get("WebViewWnd_1");
        mBackBtn.GetComponent<UILabel>().text = LocalizationMgr.Get("WebViewWnd_2");
        mRefreshBtn.GetComponent<UILabel>().text = LocalizationMgr.Get("WebViewWnd_3");
        mCloseBtn.GetComponent<UILabel>().text = LocalizationMgr.Get("WebViewWnd_4");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mForwardBtn).onClick = OnForwardBtn;
        UIEventListener.Get(mBackBtn).onClick = OnBackBtn;
        UIEventListener.Get(mRefreshBtn).onClick = OnRefreshBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
    }

    /// <summary>
    /// 前进按钮
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnForwardBtn(GameObject ob)
    {
        webView.GoForward();
    }

    /// <summary>
    /// 后退按钮
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnBackBtn(GameObject ob)
    {
        webView.GoBack();
    }

    /// <summary>
    /// 刷新按钮
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnRefreshBtn(GameObject ob)
    {
        // 重新载入当前页面
        webView.Reload();

        webView.Show();
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        // 清理缓存
        if (webView != null)
        {
            webView.CleanCache();

            Destroy(webView);
            webView = null;
        }

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 显示webview
    /// </summary>
    /// <returns>The web view.</returns>
    /// <param name="url">URL.</param>
    IEnumerator ShowWebView(string url)
    {
        // 在这等已帧是因为屏幕适配取位置要在start()执行后的下一帧
        yield return null;

        // 取得webview的四个点
        Vector3[] corners = mWebView.GetComponent<UIWidget>().worldCorners;

        // 取得左下角点的屏幕坐标
        Vector2 screenPos = SceneMgr.UiCamera.WorldToScreenPoint(corners[0]);

        // 计算webview到屏幕顶部的相像素值
        float pixel =  Screen.height - screenPos.y;

        // 设定不显示loading条
        webView.SetShowSpinnerWhenLoading(false);

        // 不响应系统自带的返回按钮
        webView.backButtonEnable = false;

        /// 此处四个值（上，左，下，右）在ios与其他平台（android等）有区别，分别代表“边距”和像素。需要进行转换
        webView.insets = new UniWebViewEdgeInsets(UniWebViewHelper.ConvertPixelToPoint(pixel, false), 0, 0, 0);

        // 设定加载地址
        webView.url = url;

        // 加载
        webView.Load();

        // 直接显示
        webView.Show();
    }

    #endregion

    #region 外部接口

    // 绑定数据
    public void BindData(string url)
    {
        if (webView == null)
            return;

        Coroutine.DispatchService(ShowWebView(url));
    }

    #endregion

#else

    #region 外部接口

    // 绑定数据
    public void BindData(string url)
    {
    }

    #endregion

#endif
}