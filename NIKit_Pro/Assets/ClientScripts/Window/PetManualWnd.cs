/// <summary>
/// PetManualWnd.cs
/// Created by fengsc 2017/12/28
/// 使魔图鉴窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LPC;

public class PetManualWnd : WindowBase<PetManualWnd>
{
    // 标题
    public UILabel mTitle;

    // 标题元素类型图标
    public UISprite mElementTitle;

    public UILabel mLeftTitle;

    public UILabel mTips;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public UIToggle[] mToggles;

    public UILabel[] mToggleTips;

    public ShowManualPetWnd mShowManualPetWnd;

    public TweenScale mTweenScale;

    List<LPCMapping> mManualLIst = new List<LPCMapping>();

    int mSelectElement = -1;

    // Use this for initialization
    void Start ()
    {
        if (mTweenScale != null)
        {
            float scale = Game.CalcWndScale();
            mTweenScale.to = new Vector3(scale, scale, scale);

            EventDelegate.Add(mTweenScale.onFinished, OnFinish);
        }

        // 注册事件
        RegisterEvent();

        Redraw();
    }

    void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 移除消息关注
        MsgMgr.RemoveDoneHook("MSG_RECEIVE_MANUAL_BONUS", "PetManualWnd");
    }

    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mToggles[0].gameObject).onClick = OnClickFireToggleBtn;
        UIEventListener.Get(mToggles[1].gameObject).onClick = OnClickStormToggleBtn;
        UIEventListener.Get(mToggles[2].gameObject).onClick = OnClickWaterToggleBtn;
        UIEventListener.Get(mToggles[3].gameObject).onClick = OnClickLightToggleBtn;
        UIEventListener.Get(mToggles[4].gameObject).onClick = OnClickDarkToggleBtn;

        MsgMgr.RegisterDoneHook("MSG_RECEIVE_MANUAL_BONUS", "PetManualWnd", OnMsgReceiveManulaBonus);
    }

    void OnMsgReceiveManulaBonus(string cmd, LPCValue para)
    {
        // 显示图鉴提示
        ShowNewTips();

        ShowBonusTips(mManualLIst[0]);
    }

    /// <summary>
    /// 火元素选择按钮点击回调
    /// </summary>
    void OnClickFireToggleBtn(GameObject go)
    {
        if (mSelectElement == MonsterConst.ELEMENT_FIRE)
            return;

        mSelectElement = MonsterConst.ELEMENT_FIRE;

        GetManualList();

        mShowManualPetWnd.ResetPosition();

        // 刷新图鉴面板数据
        mShowManualPetWnd.Bind(mSelectElement, mManualLIst);

        // 元素图标
        mElementTitle.spriteName = PetMgr.GetElementIconName(mSelectElement);

        mLeftTitle.text =
            MonsterConst.MonsterElementTypeMap[mSelectElement] + " "
            + string.Format(LocalizationMgr.Get("PetManualWnd_3"), ManualMgr.GetHoldManualAmountByElement(mSelectElement), mManualLIst.Count);
    }

    /// <summary>
    /// 风元素选择按钮点击回调
    /// </summary>
    void OnClickStormToggleBtn(GameObject go)
    {
        if (mSelectElement == MonsterConst.ELEMENT_STORM)
            return;

        mSelectElement = MonsterConst.ELEMENT_STORM;

        GetManualList();

        mShowManualPetWnd.ResetPosition();

        // 刷新图鉴面板数据
        mShowManualPetWnd.Bind(mSelectElement, mManualLIst);

        // 元素图标
        mElementTitle.spriteName = PetMgr.GetElementIconName(mSelectElement);

        mLeftTitle.text =
            MonsterConst.MonsterElementTypeMap[mSelectElement] + " "
            + string.Format(LocalizationMgr.Get("PetManualWnd_3"), ManualMgr.GetHoldManualAmountByElement(mSelectElement), mManualLIst.Count);
    }

    /// <summary>
    /// 水元素选择按钮点击回调
    /// </summary>
    void OnClickWaterToggleBtn(GameObject go)
    {
        if (mSelectElement == MonsterConst.ELEMENT_WATER)
            return;

        mSelectElement = MonsterConst.ELEMENT_WATER;

        mShowManualPetWnd.ResetPosition();

        GetManualList();

        // 刷新图鉴面板数据
        mShowManualPetWnd.Bind(mSelectElement, mManualLIst);

        // 元素图标
        mElementTitle.spriteName = PetMgr.GetElementIconName(mSelectElement);

        mLeftTitle.text =
            MonsterConst.MonsterElementTypeMap[mSelectElement] + " "
            + string.Format(LocalizationMgr.Get("PetManualWnd_3"), ManualMgr.GetHoldManualAmountByElement(mSelectElement), mManualLIst.Count);
    }

    /// <summary>
    /// 光元素选择按钮点击回调
    /// </summary>
    void OnClickLightToggleBtn(GameObject go)
    {
        if (mSelectElement == MonsterConst.ELEMENT_LIGHT)
            return;

        mShowManualPetWnd.ResetPosition();

        mSelectElement = MonsterConst.ELEMENT_LIGHT;

        GetManualList();

        // 刷新图鉴面板数据
        mShowManualPetWnd.Bind(mSelectElement, mManualLIst);

        // 元素图标
        mElementTitle.spriteName = PetMgr.GetElementIconName(mSelectElement);

        mLeftTitle.text =
            MonsterConst.MonsterElementTypeMap[mSelectElement] + " "
            + string.Format(LocalizationMgr.Get("PetManualWnd_3"), ManualMgr.GetHoldManualAmountByElement(mSelectElement), mManualLIst.Count);
    }

    /// <summary>
    /// 暗元素选择按钮点击回调
    /// </summary>
    void OnClickDarkToggleBtn(GameObject go)
    {
        if (mSelectElement == MonsterConst.ELEMENT_DARK)
            return;

        mShowManualPetWnd.ResetPosition();

        mSelectElement = MonsterConst.ELEMENT_DARK;

        GetManualList();

        // 刷新图鉴面板数据
        mShowManualPetWnd.Bind(mSelectElement, mManualLIst);

        // 元素图标
        mElementTitle.spriteName = PetMgr.GetElementIconName(mSelectElement);

        mLeftTitle.text =
            MonsterConst.MonsterElementTypeMap[mSelectElement] + " "
            + string.Format(LocalizationMgr.Get("PetManualWnd_3"), ManualMgr.GetHoldManualAmountByElement(mSelectElement), mManualLIst.Count);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnCloseBtn(GameObject go)
    {
        // 关闭窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mSelectElement = MonsterConst.ELEMENT_FIRE;

        // 获取图鉴列表
        GetManualList();

        // 刷新图鉴面板数据
        mShowManualPetWnd.Bind(mSelectElement, mManualLIst);

        mLeftTitle.text =
            MonsterConst.MonsterElementTypeMap[mSelectElement] + " "
            + string.Format(LocalizationMgr.Get("PetManualWnd_3"), ManualMgr.GetHoldManualAmountByElement(mSelectElement), mManualLIst.Count);

        // 窗口标题
        mTitle.text = LocalizationMgr.Get("PetManualWnd_2") + " "
            + string.Format(LocalizationMgr.Get("PetManualWnd_3"), ManualMgr.GetHoldManualAmount(), ManualMgr.GetManualAmount());

        // 元素图标
        mElementTitle.spriteName = PetMgr.GetElementIconName(mSelectElement);

        ShowBonusTips(mManualLIst[0]);

        ShowNewTips();
    }

    void ShowNewTips()
    {
        for (int i = 0; i < mToggleTips.Length; i++)
            mToggleTips[i].gameObject.SetActive(true);

        int amount = ManualMgr.GetNewTipsByElement(ME.user, MonsterConst.ELEMENT_FIRE);
        if (amount > 0)
            mToggleTips[0].text = amount.ToString();
        else
            mToggleTips[0].gameObject.SetActive(false);

        amount = ManualMgr.GetNewTipsByElement(ME.user, MonsterConst.ELEMENT_STORM);
        if (amount > 0)
            mToggleTips[1].text = amount.ToString();
        else
            mToggleTips[1].gameObject.SetActive(false);

        amount = ManualMgr.GetNewTipsByElement(ME.user, MonsterConst.ELEMENT_WATER);
        if (amount > 0)
            mToggleTips[2].text = amount.ToString();
        else
            mToggleTips[2].gameObject.SetActive(false);

        amount = ManualMgr.GetNewTipsByElement(ME.user, MonsterConst.ELEMENT_LIGHT);
        if (amount > 0)
            mToggleTips[3].text = amount.ToString();
        else
            mToggleTips[3].gameObject.SetActive(false);

        amount = ManualMgr.GetNewTipsByElement(ME.user, MonsterConst.ELEMENT_DARK);
        if (amount > 0)
            mToggleTips[4].text = amount.ToString();
        else
            mToggleTips[4].gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示提示信息
    /// </summary>
    void ShowBonusTips(LPCMapping petData)
    {
        CsvRow row = MonsterMgr.GetRow(petData.GetValue<int>("class_id"));
        if (row == null)
            return;

        LPCMapping manual_bonus = row.Query<LPCMapping>("manual_bonus");

        LPCMapping bonus = manual_bonus.GetValue<LPCMapping>(petData.GetValue<int>("rank"));

        string fields = FieldsMgr.GetFieldInMapping(bonus);

        mTips.text = string.Format(LocalizationMgr.Get("PetManualWnd_9"), bonus.GetValue<int>(fields));
    }

    /// <summary>
    /// 获取图鉴列表
    /// </summary>
    void GetManualList()
    {
        List<LPCMapping> manualList = new List<LPCMapping>();

        // 配置表中该元素类型的所有图鉴使魔
        List<int> monsterList = ManualMgr.GetManualListByElement(mSelectElement);

        for (int i = 0; i < monsterList.Count; i++)
        {
            int classId = monsterList[i];

            CsvRow row = MonsterMgr.GetRow(classId);
            if (row == null)
                continue;

            int star = row.Query<int>("star");

            if (row.Query<int>("rank") == MonsterConst.RANK_UNABLEAWAKE)
            {
                // 创建宠物对象
                LPCMapping unableAwakepara = LPCMapping.Empty;

                unableAwakepara.Add("rid", Rid.New());
                unableAwakepara.Add("class_id", classId);
                unableAwakepara.Add("star", star);
                unableAwakepara.Add("rank", MonsterConst.RANK_UNABLEAWAKE);
                unableAwakepara.Add("level", MonsterMgr.GetMaxLevel(star));

                // 创建宠物对象
                manualList.Add(unableAwakepara);
            }
            else
            {
                // 创建未觉醒的宠物对象
                LPCMapping unAwakepara = LPCMapping.Empty;

                unAwakepara.Add("rid", Rid.New());
                unAwakepara.Add("class_id", classId);
                unAwakepara.Add("star", star);
                unAwakepara.Add("rank", MonsterConst.RANK_UNAWAKE);
                unAwakepara.Add("level", MonsterMgr.GetMaxLevel(star));

                manualList.Add(unAwakepara);

                // 创建觉醒的宠物对象
                LPCMapping awakePara = LPCMapping.Empty;

                awakePara.Add("rid", Rid.New());
                awakePara.Add("class_id", classId);
                awakePara.Add("rank", MonsterConst.RANK_AWAKED);
                awakePara.Add("star", star + 1);
                awakePara.Add("level", MonsterMgr.GetMaxLevel(star + 1));

                manualList.Add(awakePara);
            }
        }

        for (int i = 0; i < manualList.Count; i++)
        {
            if (!ManualMgr.IsCompleted(ME.user, manualList[i].GetValue<int>("class_id"), manualList[i].GetValue<int>("rank")))
                continue;

            manualList[i].Add("is_user", 1);
        }

        IEnumerable<LPCMapping> sortList = from data in manualList orderby SortRule(data) ascending select data;

        mManualLIst.Clear();

        foreach (LPCMapping item in sortList)
            mManualLIst.Add(item);
    }

    /// <summary>
    /// 排序规则, 觉醒标识，星级，class_id
    /// </summary>
    string SortRule(LPCMapping data)
    {
        return string.Format("@{0:D2}{1:D2}{2:D4}", data.GetValue<int>("rank"), data.GetValue<int>("star"), data.GetValue<int>("class_id"));
    }
}
