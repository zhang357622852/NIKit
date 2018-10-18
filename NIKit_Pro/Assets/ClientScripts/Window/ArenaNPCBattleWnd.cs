/// <summary>
/// NPCBattleWnd.cs
/// Created fengsc 2016/10/10
/// 竞技场npc对战窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ArenaNPCBattleWnd : WindowBase<ArenaNPCBattleWnd>
{
    public GameObject mItem;

    public UIGrid mGrid;

    public UIScrollView mSrollView;

    List<GameObject> mItems = new List<GameObject>();

    void Awake()
    {
        // 创建一批基础格子
        CreatedObjects();

        // 关注字段变化
        if (ME.user != null)
        {
            ME.user.dbase.RegisterTriggerField("ArenaNPCBattleWnd",
                new string[] { "level" }, new CallBack(OnUserLevelChange));
        }
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 整理位置
        mSrollView.ResetPosition();

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        if (ME.user == null)
            return;

        // 移除字段关注
        ME.user.dbase.RemoveTriggerField("ArenaNPCBattleWnd");
    }

    /// <summary>
    /// 等级变化回调
    /// </summary>
    void OnUserLevelChange(object para, params object[] _param)
    {
        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mItems == null || mItems.Count == 0)
            return;

        LPCValue arenaBattleNpcData = ME.user.Query<LPCValue>("instance_cooldown");

        LPCMapping npcData = new LPCMapping();

        // 没有对战数据
        if (arenaBattleNpcData != null && arenaBattleNpcData.IsMapping)
            npcData = arenaBattleNpcData.AsMapping;

        List<LPCMapping> unlockInstance = InstanceMgr.GetNpcList();

        for (int i = 0; i < unlockInstance.Count; i++)
        {
            LPCMapping data = unlockInstance[i];
            if (data == null)
                continue;

            string instanceId = data.GetValue<string>("instance_id");

            bool isUnLocked = CACL_ARENA_NPC_IS_UNLOCKED.CALL(ME.user.Query<int>("level"), data.GetValue<int>("level"));

            data.Add("is_unlocked", (isUnLocked == true ? 1 : 0));

            LPCMapping bonus = InstanceMgr.GetInstanceClearanceBonus(instanceId);
            if (bonus == null)
                bonus = LPCMapping.Empty;

            data.Add("bonus", bonus);

            int battleTime = 0;

            // 获取副本分组
            string group = data.GetValue<string>("group");
            if (string.IsNullOrEmpty(group))
                group = instanceId;

            if(npcData.ContainsKey(group))
                battleTime = npcData[group].AsInt;

            data.Add("cd_time", battleTime);

            if (i + 1 > mItems.Count)
            {
                mItems.Add(CreateItem(mItem));
            }

            GameObject go = mItems[i];

            go.SetActive(true);

            // 绑定数据
            go.GetComponent<ArenaNPCItemWnd>().Bind(data, instanceId);
        }

        // 激活排序组件
        mGrid.repositionNow = true;
    }

    /// <summary>
    /// 创建一批Gameobject
    /// </summary>
    void CreatedObjects()
    {
        mItem.SetActive(false);

        LPCMapping allInstanceConfig = InstanceMgr.GetAllInstanceInfo();

        foreach (string id in allInstanceConfig.Keys)
        {
            // 获取一个副本的配置信息
            LPCMapping data = InstanceMgr.GetInstanceInfo(id);

            if (data == null)
                continue;
            CsvRow mapConfig = MapMgr.GetMapConfig(data.GetValue<int>("map_id"));

            if (mapConfig == null)
                continue;

            if (mapConfig.Query<int>("map_type") != MapConst.ARENA_NPC_MAP)
                continue;

            GameObject go = CreateItem(mItem);

            if (!mItems.Contains(go))
                mItems.Add(go);
        }
    }

    GameObject CreateItem(GameObject go)
    {
        GameObject item = Instantiate(go);

        item.transform.SetParent(mGrid.transform);
        item.transform.localScale = Vector3.one;
        item.transform.localPosition = Vector3.zero;

        item.SetActive(false);

        return item;
    }

    public void GuideSelectNpc(int index)
    {
        mItems[index].GetComponent<ArenaNPCItemWnd>().GuideClickBattle();
    }
}
