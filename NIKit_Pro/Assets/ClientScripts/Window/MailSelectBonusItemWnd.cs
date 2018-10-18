/// <summary>
/// MailSelectBonusItemWnd.cs
/// Created by zhangwm 2018/08/15
/// 邮箱选择奖励界面: 套装选择
/// </summary>
using LPC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailSelectBonusItemWnd : MonoBehaviour
{
    public Transform mParent;

    public UITexture mIcon;

    public UILabel mName;

    public UILabel mSuitDesc;

    private CsvRow mRow;

    private CallBack mCallback;

    private void Start()
    {
        UIEventListener.Get(gameObject).onClick = OnClickSuit;
    }

    private void Redraw()
    {
        if (mRow == null)
            return;

        int suitId = mRow.Query<int>("suit_id");

        // 套装图标
        mIcon.mainTexture = EquipMgr.GetSuitTexture(suitId);

        // 套装名称
        mName.text = EquipMgr.GetSuitName(suitId);

        // 套装属性
        LPCArray props = mRow.Query<LPCArray>("props");

        if (props == null)
            return;

        string suitDesc = string.Empty;

        // 获取套装描述信息;
        foreach (LPCValue item in props.Values)
            suitDesc += PropMgr.GetPropDesc(item.AsArray, EquipConst.SUIT_PROP);

        mSuitDesc.text = string.Format("[ADFFA7]{0}{1}: [-]", mRow.Query<int>("sub_count"), LocalizationMgr.Get("EquipViewWnd_1")) + suitDesc;
    }

    private void OnClickSuit(GameObject go)
    {
        if (mRow == null)
            return;

        int suitId = mRow.Query<int>("suit_id");

        DialogMgr.ShowDailog(
            new CallBack(OnSureCallBack, suitId),
            string.Format(LocalizationMgr.Get("MailSelectBonusWnd_2"), EquipMgr.GetSuitName(suitId)),
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            mParent
            );
    }

    private void OnSureCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        int suitId = (int)para;

        if (mCallback != null)
            mCallback.Go(suitId);
    }

    #region 外部接口

    public void BindData(CsvRow row)
    {
        mRow = row;

        Redraw();
    }

    public void SetCallback(CallBack callback)
    {
        if (callback == null)
            return;

        mCallback = callback;
    }

    #endregion
}
