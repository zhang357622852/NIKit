/// <summary>
/// TaskItemWnd.cs
/// Created by lic 11/10/2016
/// 任务格子
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class TaskItemWnd : WindowBase<TaskItemWnd>
{
    #region 成员变量
    public UISprite mBg;

    public UILabel mTitle;

    // 奖励
    public GameObject mBonus;
    public UITexture[] mBonusTex;
    public UILabel[] mBonusLb;

    // 完成状态
    public GameObject mReceiveBtn;
    public UILabel mReceiveLb;
    public GameObject mComplete;
    public UISpriteAnimation mStarBtnAnima;
    public GameObject mWndItemMask;

    #endregion

    #region 私有变量

    public int TaskId { get; private set; }

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mReceiveBtn).onClick = OnReceiveBtn;
    }

    /// <summary>
    /// 领取奖励按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnReceiveBtn(GameObject ob)
    {
        if(TaskMgr.GetTaskInfo(TaskId) == null)
            return;

        // 已完成
        if(TaskMgr.IsCompleted(ME.user ,TaskId))
        {
            // 已领取
            if(TaskMgr.isBounsReceived(ME.user, TaskId))
                return;

            if(!TaskMgr.CheckCanReceiveBonus(ME.user, TaskId))
                return;

            // 服务器领取奖励
            Operation.CmdReceiveBonus.Go(TaskId);
        }
        else
        {
            GameObject wnd = WindowMgr.GetWindow(TaskInfoWnd.WndType);

            if (wnd == null)
                wnd = WindowMgr.CreateWindow(TaskInfoWnd.WndType, TaskInfoWnd.PrefebResource);

            if (wnd == null)
            {
                LogMgr.Trace("TaskInfoWnd打开失败");
                return;
            }

            // 显示邮件窗口
            WindowMgr.ShowWindow(wnd);

            wnd.GetComponent<TaskInfoWnd>().BindData(TaskId);
        }
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void Redraw()
    {
        if(TaskId <= 0)
            return;

        CsvRow item = TaskMgr.GetTaskInfo(TaskId);
        if(item == null)
            return;

        string title = LocalizationMgr.Get(item.Query<string>("title"));

        LPCArray bonusList = TaskMgr.GetBonus(ME.user, TaskId);

        mReceiveBtn.SetActive(true);
        mComplete.SetActive(false);
        mWndItemMask.SetActive(false);

        bool showBonus = true;
        mStarBtnAnima.gameObject.SetActive(false);

        //  任务已完成
        if(TaskMgr.IsCompleted(ME.user ,TaskId))
        {
            // 奖励还未领取
            if(!TaskMgr.isBounsReceived(ME.user ,TaskId))
            {
                title = string.Format("[D0FE6DFF]● {0}[-]", LocalizationMgr.Get(item.Query<string>("title")));
                mReceiveLb.text = LocalizationMgr.Get("TaskWnd_6");

                mStarBtnAnima.gameObject.SetActive(true);
                mStarBtnAnima.enabled = true;

                mBg.alpha = 1.0f;
            }
            else
            {
                mReceiveBtn.SetActive(false);
                mComplete.SetActive(true);
                mWndItemMask.SetActive(true);

                showBonus = false;

                title = string.Format("● {0}", title);

                mBg.alpha = 0.25f;
            }
        }
        else
        {
            mReceiveLb.text = LocalizationMgr.Get("TaskWnd_7");
            mBg.alpha = 0.5f;

            title = string.Format("◎ {0}", title);
        }

        mTitle.text = title;

        // 显示奖励
        if(showBonus)
        {
            mBonus.SetActive(true);
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

                    // 如果是道具
                    if(ItemMgr.IsItem(classId))
                    {
                        mBonusTex[i].mainTexture = ItemMgr.GetTexture(classId, true);
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

                mBonusTex[i].mainTexture = ItemMgr.GetTexture(FieldsMgr.GetFieldTexture(field));

                mBonusLb[i].text = string.Format("×{0}", bonusItem[field].AsInt);
            }
        }
        else
        {
            mBonus.SetActive(false);
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
        TaskId = task_id;

        Redraw();
    }

    #endregion
}
