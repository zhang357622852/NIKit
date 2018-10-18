/// <summary>
/// SummonPetListWnd.cs
/// Created by fengsc 2016/10/31
/// 刻印召唤石可召唤的宠物列表
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SummonPetListWnd : WindowBase<SummonPetListWnd>
{
    #region 成员变量

    public UILabel mTitle;

    // 剩余刷新天数
    public UILabel mRemainDays;

    public UILabel mTitleLb;

    // 召唤说明
    public UILabel mSummonCaptionBtn;

    // 宠物基础格子
    public GameObject mItemWnd;

    // 排序组件
    public UIGrid mGrid;

    public List<Property> mPetData = new List<Property>();

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化本地文本
        InitLocalText();

        // 绘制窗口
        Redraw();

        // 刷新显示时间
        RedrawTime();

        // 注册按钮点击事件
        UIEventListener.Get(mSummonCaptionBtn.gameObject).onClick = OnClickCaptionBtn;

        // 计算当前时间到晚上零点的时间
        Invoke("RedrawTime", (int)Game.GetZeroClock(1));
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 监听chatroom字段变化
        ME.user.dbase.RegisterTriggerField("SummonPetListWnd",
            new string[]{ "special_summon" },
            new CallBack(OnFieldsChange));
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除字段变化监听
        ME.user.dbase.RemoveTriggerField("SummonPetListWnd");
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 析构掉临时创建的宠物对象
        for (int i = 0; i < mPetData.Count; i++)
        {
            if (mPetData[i] == null)
                continue;

            // 析构物件对象
            mPetData[i].Destroy();
        }
    }

    /// <summary>
    /// 字段变化监听回调
    /// </summary>
    void OnFieldsChange(object para, params object[] param)
    {
        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 初始化本地化列表
    /// </summary>
    void InitLocalText()
    {
        mTitle.text = LocalizationMgr.Get("SummonPetListWnd_1");
        mTitleLb.text = LocalizationMgr.Get("SummonPetListWnd_2");
        mSummonCaptionBtn.text = LocalizationMgr.Get("SummonPetListWnd_3");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void RedrawTime()
    {
        int weekDay = Game.GetWeekDay(TimeMgr.GetServerTime());
        // 显示剩余刷新的天数
        mRemainDays.text = (7 - (weekDay == 0 ? 7 : weekDay) + 1).ToString();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 清除旧数据
        mGrid.transform.DestroyChildren();

        // 获取所有的抽取列表
        LPCMapping specialSummon = ME.user.Query<LPCMapping>("special_summon");

        // 没有获取到抽取列表
        if (specialSummon == null)
            return;

        // 获取特殊召唤信息
        // 没有获取到召唤信息
        LPCArray summonList = specialSummon.GetValue<LPCArray>("batch_summon_list");
        if (summonList == null || summonList.Count == 0)
            return;

        mItemWnd.SetActive(false);

        // 遍历宠物召唤列表
        foreach (LPCValue classId in summonList[0].AsArray.Values)
        {
            GameObject go = Instantiate(mItemWnd);

            if (go == null)
                continue;

            go.transform.SetParent(mGrid.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            go.name = classId.AsString;

            // 获取宠物配置表数据
            CsvRow row = MonsterMgr.GetRow(classId.AsInt);

            if (row == null)
                continue;

            // 构造参数
            LPCMapping data = new LPCMapping();
            data.Add("rid", Rid.New());
            data.Add("class_id", classId);

            // 创建物件
            Property ob = PropertyMgr.CreateProperty(data);

            mPetData.Add(ob);

            PetItemWnd itemWnd = go.GetComponent<PetItemWnd>();

            if (itemWnd == null)
                continue;

            // 不显示队长技能
            itemWnd.ShowLeaderSkill(false);

            // 不显示宠物等级
            itemWnd.ShowLevel(false);

            // 绑定数据
            itemWnd.SetBind(ob);

            // 添加格子点击事件
            UIEventListener.Get(go).onClick = OnClickItem;

            go.SetActive(true);
        }

        mGrid.repositionNow = true;
    }

    /// <summary>
    /// 宠物格子点击事件
    /// </summary>
    void OnClickItem(GameObject go)
    {
        // 获取宠物对象
        Property ob = go.GetComponent<PetItemWnd>().item_ob;

        if (ob == null)
            return;

        // 创建窗口
        GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 创建窗口失败
        if (wnd == null)
            return;

        PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

        if (script == null)
            return;

        script.Bind(ob);
        script.ShowBtn(true);
    }

    /// <summary>
    /// 说明按钮点击事件
    /// </summary>
    void OnClickCaptionBtn(GameObject go)
    {
        GameObject wnd = WindowMgr.GetWindow(SummonPetCaptionWnd.WndType);

        if (wnd == null)
            wnd = WindowMgr.CreateWindow(SummonPetCaptionWnd.WndType, SummonPetCaptionWnd.PrefebResource);

        if (wnd == null)
        {
            LogMgr.Trace("SummonPetCaptionWnd窗口创建失败");
            return;
        }

        WindowMgr.ShowWindow(wnd);
    }
}