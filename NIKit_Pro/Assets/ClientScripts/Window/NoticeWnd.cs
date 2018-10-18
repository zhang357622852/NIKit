using UnityEngine;
using System.Collections;
using LPC;

public class NoticeWnd : WindowBase<NoticeWnd>
{
    // 确认按钮
    public UILabel mConfirmBtn;

    public RichTextContent mRichContent;

    // 关闭回调
    private CallBack mCallBack;

    void Start()
    {
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mConfirmBtn.text = LocalizationMgr.Get("NoticeWnd_1");

        UIEventListener.Get(mConfirmBtn.gameObject).onClick = OnClickConfirmBtn;

        // 服务器维护公告
        mRichContent.ParseValue(ConfigMgr.Get<string>("content"));
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        if (mCallBack != null)
            mCallBack.Go();

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 设置关闭回调
    /// </summary>
    /// <param name="callBack">Call back.</param>
    public void SetCallBack(CallBack callBack)
    {
        mCallBack = callBack;
    }
}
