using UnityEngine;
using System.Collections;
using LPC;

public class GotoWnd : WindowBase<GotoWnd>
{
    public UISprite mBg;

    public UILabel mDescLb;

    public UISprite mGotoBtn;
    public UILabel mGotoBtnLb;

    // 控件宽度的偏移量
    int mOffsetWidth = 30;

    int mOffsetHeight = 12;

    LPCMapping mData = LPCMapping.Empty;

    GameObject mTarget;

    CallBack cb = null;

    void Start()
    {
        UIEventListener.Get(mGotoBtn.gameObject).onClick = OnClickGotoBtn;

        mGotoBtnLb.text = LocalizationMgr.Get("GotoWnd_1");
    }

    /// <summary>
    /// 立即前往按钮点击事件
    /// </summary>
    void OnClickGotoBtn(GameObject go)
    {
        CancelInvoke("HideWnd");

        if (cb != null)
            cb.Go();

        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        string desc = string.Empty;

        if (mData.ContainsKey("task_id"))
        {
            desc = TaskMgr.GetTaskDesc(ME.user, mData.GetValue<int>("task_id"));
        }
        else
        {
            desc = LocalizationMgr.Get(mData.GetValue<string>("desc"));
        }

        mDescLb.text = desc;

        mDescLb.UpdateNGUIText();

        // 计算背景图片的大小
        mBg.width = mOffsetWidth + Mathf.RoundToInt(NGUIText.CalculatePrintedSize(desc).x) + mGotoBtn.width + 32;
        mBg.height = mOffsetHeight + mDescLb.height;

        // 设置当前对象transform的相对位置
        UITexture bg = mTarget.GetComponent<UITexture>();
        if (bg == null)
            return;

        if (SystemFunctionConst.SCREEN_LEFT.Equals(mData.GetValue<int>("show_pos")))
        {
            float xOffset = 8.0f;

            Vector3 worldTarget = transform.parent.InverseTransformPoint(mTarget.transform.position);
            this.transform.localPosition = new Vector3(
                worldTarget.x + bg.width * 0.5f + xOffset,
                worldTarget.y - bg.height * 0.5f,
                worldTarget.z);
        }
        else
        {
//            Vector3 worldTarget = transform.parent.InverseTransformPoint(mTarget.transform.position);
//            this.transform.localPosition = new Vector3(
//                worldTarget.x - bg.width * 0.5f,
//                worldTarget.y + bg.height * 0.35f,
//                worldTarget.z);
        }
    }

    public void HideWnd()
    {
        CancelInvoke("HideWnd");

        this.gameObject.SetActive(false);

        cb = null;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void ShowView(LPCMapping data, GameObject target, CallBack cb)
    {
        mData = data;

        if (mData == null)
            return;

        mTarget = target;

        this.cb = cb;

        // 绘制窗口
        Redraw();

        CancelInvoke("HideWnd");

        Invoke("HideWnd", 3.0f);
    }
}
