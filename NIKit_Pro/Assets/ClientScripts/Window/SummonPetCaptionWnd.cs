/// <summary>
/// SummonPetCaptionWnd.cs
/// Created by fengsc 2016/11/01
/// 宠物召唤说明窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SummonPetCaptionWnd : WindowBase<SummonPetCaptionWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public GameObject mPetItem;

    public GameObject mItem;

    public UIGrid mItemGrid;

    // 缓存创建的物件对象
    List<Property> mCacheCreateOb = new List<Property>();

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();

        TweenScale mTweenScale = transform.GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 监听chatroom字段变化
        ME.user.dbase.RegisterTriggerField("SummonPetCaptionWnd",
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
        ME.user.dbase.RemoveTriggerField("SummonPetCaptionWnd");
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 析构掉临时创建的宠物对象
        for (int i = 0; i < mCacheCreateOb.Count; i++)
        {
            if (mCacheCreateOb[i] == null)
                continue;

            // 析构物件对象
            mCacheCreateOb[i].Destroy();
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

    // 绘制窗口
    void Redraw()
    {
        // 清除旧数据
        mItemGrid.transform.DestroyChildren();

        // 显示title
        mTitle.text = LocalizationMgr.Get("SummonPetCaptionWnd_1");

        // 获取所有的抽取列表
        LPCMapping specialSummon = ME.user.Query<LPCMapping>("special_summon");

        // 没有获取到抽取列表
        if (specialSummon == null)
            return;

        int batch = GameSettingMgr.GetSettingInt("summon_max_show_batch");

        // 没有获取到召唤信息
        LPCArray summonList = specialSummon.GetValue<LPCArray>("batch_summon_list");
        if (summonList == null || summonList.Count == 0)
            return;

        for (int i = 0; i < mItemGrid.transform.childCount; i++)
            Destroy(mItemGrid.transform.GetChild(i).gameObject);

        // 上次刷新时间
        int refreshTime = specialSummon.GetValue<int>("refresh_time");

        mItem.SetActive(false);
        mPetItem.SetActive(false);
        for (int i = 0; i < summonList.Count + 1; i++)
        {
            GameObject itemClone = Instantiate(mItem);

            itemClone.transform.SetParent(mItemGrid.transform);

            itemClone.transform.localPosition = Vector3.zero;

            itemClone.transform.localScale = Vector3.one;

            itemClone.name = "week_" + i;

            itemClone.SetActive(true);

            UILabel mItemTitle = itemClone.transform.Find("title").GetComponent<UILabel>();

            UILabel mDesc = itemClone.transform.Find("desc").GetComponent<UILabel>();

            if (mDesc.gameObject.activeSelf)
                mDesc.gameObject.SetActive(false);

            if (i == 0)
                mItemTitle.text = LocalizationMgr.Get("SummonPetCaptionWnd_3");
            else if (i == batch)
            {
                mItemTitle.text = LocalizationMgr.Get("SummonPetCaptionWnd_5");

                // 获得召唤石的途径
                mDesc.text = LocalizationMgr.Get("SummonPetCaptionWnd_6");

                mDesc.gameObject.SetActive(true);

                break;
            }
            else
            {
                int weekDay = Game.GetWeekDay(refreshTime);
                mItemTitle.text = string.Format(LocalizationMgr.Get("SummonPetCaptionWnd_4"), i * 7 - (weekDay == 0 ? 7 : weekDay) + 1);
            }

            UIGrid grid = itemClone.transform.Find("grid").GetComponent<UIGrid>();

            // 遍历召唤列表
            foreach (LPCValue classId in summonList[i].AsArray.Values)
            {
                GameObject go = Instantiate(mPetItem);

                if (go == null)
                    continue;

                go.transform.SetParent(grid.transform);
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
                Property mOb = PropertyMgr.CreateProperty(data);

                PetItemWnd itemWnd = go.GetComponent<PetItemWnd>();

                if (itemWnd == null)
                    continue;

                mCacheCreateOb.Add(mOb);

                // 不显示队长技能
                itemWnd.ShowLeaderSkill(false);

                // 不显示宠物等级
                itemWnd.ShowLevel(false);

                // 绑定数据
                itemWnd.SetBind(mOb);

                // 添加格子点击事件
                UIEventListener.Get(go).onClick = OnClickPetItem;

                go.SetActive(true);
            }

            grid.repositionNow = true;
        }

        // 激活排序组件
        mItemGrid.repositionNow = true;
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 销毁窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 宠物格子点击事件
    /// </summary>
    void OnClickPetItem(GameObject go)
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
}
