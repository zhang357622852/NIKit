
/// <summary>
/// DebugClonePetWnd.cs
/// Created by fengsc 2017/05/26
/// GM 克隆宠物
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class DebugClonePetWnd : WindowBase<DebugClonePetWnd>
{
    // 宠物id输入框
    public UIInput mIDInput;

    // 星级输入框
    public UIInput mStarInput;

    // 等级输入框
    public UIInput mLevelInput;

    // 是否觉醒输入框
    public UIInput mRankInput;

    // 克隆数量输入框
    public UIInput mAmountInput;

    // 取消按钮
    public GameObject mCancelBtn;

    // 确认按钮
    public GameObject mConfirmBtn;

    void Start()
    {
        UIEventListener.Get(mCancelBtn).onClick = OnClickCancelBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;

        EventDelegate.Add(mIDInput.onChange, OnIdInputChange);
    }

    void OnIdInputChange()
    {
        int classId = -1;

        if (!int.TryParse(mIDInput.value, out classId))
        {
            mStarInput.value = string.Empty;

            mRankInput.value = string.Empty;

            mLevelInput.value = string.Empty;

            DialogMgr.Notify("classId格式不正确");

            return;
        }

        // 宠物配置数据
        CsvRow row = MonsterMgr.GetRow(classId);
        if (row == null)
            return;

        mStarInput.value = row.Query<int>("star").ToString();

        mRankInput.value = row.Query<int>("rank").ToString();

        mLevelInput.value = row.Query<int>("level").ToString();

        mAmountInput.value = 1.ToString();
    }

    /// <summary>
    /// 取消按钮点击回调
    /// </summary>
    void OnClickCancelBtn(GameObject go)
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        int classId = -1;

        if (!int.TryParse(mIDInput.value, out classId))
        {
            DialogMgr.Notify("classId格式不正确");
            return;
        }

        // 宠物配置数据
        CsvRow row = MonsterMgr.GetRow(classId);
        if (row == null)
        {
            DialogMgr.Notify(string.Format("没有{0}的配置数据", classId));
            return;
        }

        int star = 0;
        if (!int.TryParse(mStarInput.value, out star))
        {
            if (!string.IsNullOrEmpty(mStarInput.value))
            {
                DialogMgr.Notify("宠物星级数据类型错误");
                return;
            }
            else
                star = row.Query<int>("star");
        }

        int level = 1;
        if (!int.TryParse(mLevelInput.value, out level))
        {
            if (!string.IsNullOrEmpty(mLevelInput.value))
            {
                DialogMgr.Notify("宠物等级数据类型错误");
                return;
            }
        }

        int rank = 0;
        int defaultRank = row.Query<int>("rank");
        if (!int.TryParse(mRankInput.value, out rank))
        {
            if (!string.IsNullOrEmpty(mRankInput.value))
            {
                DialogMgr.Notify("是否觉醒数据类型错误");
                return;
            }
            else
            {
                rank = defaultRank;
            }
        }

        if (defaultRank == 0 && rank > 0)
        {
            DialogMgr.Notify("该宠物不能觉醒：" + classId);
            return;
        }

        if (defaultRank > 0 && rank < 1)
        {
            DialogMgr.Notify("该宠物rank不能为0：");
            return;
        }

        int amount = 1;
        if (!int.TryParse(mAmountInput.value, out amount))
        {
            if (!string.IsNullOrEmpty(mAmountInput.value))
            {
                DialogMgr.Notify("宠物数量数据类型错误");
                return;
            }
        }

        // 构建参数
        LPCMapping para = LPCMapping.Empty;
        para.Add("star", star);
        para.Add("level", level);
        para.Add("rank", rank);

        // 通知服务器克隆宠物
        Operation.CmdAdminClone.Go(classId.ToString(), amount, LPCValue.Create(para));
    }
}
