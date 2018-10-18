/// <summary>
/// TaskInfoWnd.cs
/// Created by lic 11/10/2016
/// 任务信息窗口
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class TaskInfoWnd : WindowBase<TaskInfoWnd>
{

    #region 成员变量

    public UILabel mTitle;

    public UILabel mDesc;

    // 奖励
    public UITexture[] mBonusTex;
    public UILabel[] mBonusLb;
    public UILabel mBonusTitle;

    // 完成状态
    public GameObject mReceiveBtn;
    public UILabel mReceiveLb;

    public GameObject mCloseBtn;

    public GameObject mMask;

    #endregion

    #region 私有变量

    private int mTaskId = -1;

    private Property mPropOb;

    #endregion

    #region 内部函数

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
        // 销毁物件对象
        if (mPropOb != null)
            mPropOb.Destroy();

        // 解注册按钮点击事件
        for (int i = 0; i < mBonusTex.Length; i++)
            UIEventListener.Get(mBonusTex[i].gameObject).onClick -= OnClickItem;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mReceiveBtn).onClick = OnReceiveBtn;

        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;

        UIEventListener.Get(mMask).onClick = OnCloseBtn;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        mBonusTitle.text = LocalizationMgr.Get("TaskWnd_8");
    }

    /// <summary>
    /// 领取奖励按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnReceiveBtn(GameObject ob)
    {
        WindowMgr.HideWindow(gameObject);

        CsvRow item = TaskMgr.GetTaskInfo(mTaskId);

        if(item == null)
            return;

        // 已完成
        if(TaskMgr.IsCompleted(ME.user ,mTaskId))
        {
            // 已领取
            if(TaskMgr.isBounsReceived(ME.user, mTaskId))
                return;

            if(!TaskMgr.CheckCanReceiveBonus(ME.user, mTaskId))
                return;

            // 服务器领取奖励
            Operation.CmdReceiveBonus.Go(mTaskId);
        }
        else
        {
            int script = item.Query<int>("leave_for_script");

            if(script <= 0)
                return;

            // 立即前往
            bool isClose = (bool) ScriptMgr.Call(script, ME.user, item.Query<LPCValue>("leave_for_arg"));

            if(isClose)
                WindowMgr.DestroyWindow(TaskWnd.WndType);
        }
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.HideWindow(gameObject);
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    void Redraw()
    {
        if(mTaskId < 0)
            return;

        CsvRow item = TaskMgr.GetTaskInfo(mTaskId);
        if(item == null)
            return;

        mTitle.text = LocalizationMgr.Get(item.Query<string>("title"));

        mDesc.text = TaskMgr.GetTaskDesc(ME.user, mTaskId);

        //  任务已完成
        if(TaskMgr.IsCompleted(ME.user ,mTaskId))
        {
            // 奖励还未领取
            if(!TaskMgr.isBounsReceived(ME.user ,mTaskId))
            {
                mReceiveLb.text = LocalizationMgr.Get("TaskWnd_6");
            }
            else
            {
                mReceiveLb.text = LocalizationMgr.Get("TaskWnd_10");
            }
        }
        else
        {
            mReceiveLb.text = LocalizationMgr.Get("TaskWnd_9");
        }


        LPCArray bonusList = TaskMgr.GetBonus(ME.user, mTaskId);

        for(int i = 0; i < mBonusTex.Length; i++)
        {
            if(i >= bonusList.Count)
            {
                mBonusTex[i].gameObject.SetActive(false);
                mBonusLb[i].gameObject.SetActive(false);
                continue;
            }

            mBonusTex[i].gameObject.SetActive(true);
            mBonusLb[i].gameObject.SetActive(true);
            mBonusTex[i].width = 35;
            mBonusTex[i].height = 35;

            LPCMapping bonusItem = bonusList[i].AsMapping;

            if(bonusItem.Count == 0)
                continue;

            // 非属性奖励
            if(bonusItem.ContainsKey("class_id"))
            {
                // 非属性奖励必须要有class_id和amount两个参数
                int classId = bonusItem.GetValue<int>("class_id");
                int amount = bonusItem.GetValue<int>("amount");

                UIEventListener.Get(mBonusTex[i].gameObject).onClick = OnClickItem;

                // 绑定数据
                mBonusTex[i].gameObject.GetComponent<UIEventListener>().parameter = bonusItem;

                // 如果是道具
                if(ItemMgr.IsItem(classId))
                {
                    mBonusTex[i].mainTexture = ItemMgr.GetTexture(classId);
                    mBonusLb[i].text = string.Format("×{0}", amount);
                }
                else if(EquipMgr.IsEquipment(classId))
                {
                    int rarity = EquipConst.RARITY_WHITE;

                    if(bonusItem.ContainsKey("prop"))
                    {
                        LPCMapping prop = bonusItem.GetValue<LPCMapping>("prop");

                        if(prop.GetValue<LPCArray>(EquipConst.MINOR_PROP) != null)
                            rarity = prop.GetValue<LPCArray>(EquipConst.MINOR_PROP).Count;
                    }

                    // 获取装备稀有度
                    rarity = bonusItem.ContainsKey("prop") ?
                        bonusItem.GetValue<int>("rarity"): EquipConst.RARITY_WHITE;

                    mBonusTex[i].mainTexture = EquipMgr.GetTexture(classId, rarity);
                    mBonusLb[i].text = string.Format("×{0}", amount);
                }
                else if(MonsterMgr.IsMonster(classId))
                {
                    // 没有配置表示未觉醒
                    int rank = bonusItem.ContainsKey("rank") ?
                        bonusItem.GetValue<int>("rank"):1;

                    mBonusTex[i].mainTexture = MonsterMgr.GetTexture(classId, rank);
                    mBonusLb[i].text = string.Format("×{0}", amount);
                }

                continue;
            }

            // 显示属性奖励
            string field = FieldsMgr.GetFieldInMapping(bonusItem);

            // 对于exp作特殊处理
            if(field.Equals("exp"))
            {
                mBonusTex[i].width = 50;
                mBonusTex[i].height = 26;
            }

            mBonusTex[i].mainTexture =  ItemMgr.GetTexture(FieldsMgr.GetFieldTexture(field));
            mBonusLb[i].text = string.Format("×{0}", bonusItem[field].AsInt);
        }

    }

    void OnClickItem(GameObject go)
    {
        LPCMapping bonusItem = go.GetComponent<UIEventListener>().parameter as LPCMapping;

        int classId = bonusItem.GetValue<int>("class_id");

        // 构造参数
        LPCMapping dbase = LPCMapping.Empty;

        dbase.Append(bonusItem);
        dbase.Add("rid", Rid.New());

        if (mPropOb != null)
            mPropOb.Destroy();

        // 克隆物件对象
        mPropOb = PropertyMgr.CreateProperty(dbase);

        if(ItemMgr.IsItem(classId))
        {
            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            if (wnd == null)
                return;

            RewardItemInfoWnd itemOb = wnd.GetComponent<RewardItemInfoWnd>();

            if (itemOb == null)
                return;

            itemOb.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));
            itemOb.SetMask(true);
        }
        else if(EquipMgr.IsEquipment(classId))
        {
            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

            script.SetEquipData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

            script.SetMask(true);
        }
        else if(MonsterMgr.IsMonster(classId))
        {
            GameObject petWnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (petWnd == null)
                return;

            PetSimpleInfoWnd petSimpleInfoWnd = petWnd.GetComponent<PetSimpleInfoWnd>();
            if (petSimpleInfoWnd == null)
                return;

            petSimpleInfoWnd.Bind(mPropOb, true);
            petSimpleInfoWnd.ShowBtn(true);
        }
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void BindData(int task_id)
    {
        mTaskId = task_id;
        Redraw();
    }

    #endregion
}
