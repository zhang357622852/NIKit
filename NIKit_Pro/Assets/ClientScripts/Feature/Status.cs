using System;
using System.Collections.Generic;
using LPC;

// 状态属性
public class Status
{
    /// <summary>
    /// 状态属性
    /// </summary>
    private List<LPCMapping> mStatusList = new List<LPCMapping>();

    /// <summary>
    /// 状态计数表
    /// </summary>
    private Dictionary<int, int> mStatusCountMap = new Dictionary<int,int>();

    // buf类型数量统计
    private Dictionary<int, int> mStatusTypeMap = new Dictionary<int,int>();

    /// <summary>
    /// 构造函数
    /// </summary>
    public Status(Property property)
    {
    }

    /// <summary>
    /// 清空数据 
    /// </summary>
    public void Destroy()
    {
        mStatusList.Clear();
        mStatusCountMap.Clear();
        mStatusTypeMap.Clear();
    }

    /// <summary>
    /// 取得所有的状态
    /// </summary>
    /// <returns>The all status.</returns>
    public List<LPCMapping> GetAllStatus()
    {
        return mStatusList;
    }

    /// <summary>
    /// 获取状态叠加层数
    /// </summary>
    public Dictionary<int, int> GetStatusCountMap()
    {
        return mStatusCountMap;
    }

    /// <summary>
    /// 获取buf类型数量
    /// </summary>
    public int GetStatusAmountByType(int type)
    {
        int amount = 0;

        // 获取mStatusTypeMap指定类型的buff数据量
        if (! mStatusTypeMap.TryGetValue(type, out amount))
            return 0;

        // 返回该类型buf的数量
        return amount;
    }

    /// <summary>
    /// 检测状态
    /// </summary>
    public bool CheckStatus(int statusId)
    {
        int amount = 0;

        // 还没有改状态的引用计数
        if (! mStatusCountMap.TryGetValue(statusId, out amount))
            return false;

        // 如果指定statusId的数量大于0，则返回true否则返回false
        return (amount > 0) ? true : false;
    }

    /// <summary>
    /// 增加状态
    /// </summary>
    public void AddStatus(int statusId, int cookie, LPCMapping statusMap)
    {
        // 添加列表
        statusMap.Add("status_id", statusId);
        statusMap.Add("cookie", cookie);
        mStatusList.Add(statusMap);

        // 增加类型
        int statusType = statusMap.GetValue<int>("status_type");
        if (!mStatusTypeMap.ContainsKey(statusType))
            mStatusTypeMap.Add(statusType, 1);
        else
            mStatusTypeMap[statusType] = mStatusTypeMap[statusType] + 1;

        // 还没有改状态的引用计数
        if (!mStatusCountMap.ContainsKey(statusId))
        {
            mStatusCountMap.Add(statusId, 1);
            return;
        }

        // 增加应用计数 
        mStatusCountMap[statusId] = mStatusCountMap[statusId] + 1;
    }

    /// <summary>
    /// 获取状态的condition
    /// </summary>
    public LPCMapping GetStatusCondition(int cookie)
    {
        LPCMapping condition = null;

        // 找到需要移除的数据
        for (int i = 0; i < mStatusList.Count; i++)
        {
            if (mStatusList[i] == null)
                continue;

            // status cookie不一致
            if (mStatusList[i].GetValue<int>("cookie") != cookie)
                continue;

            // statusMap
            condition = mStatusList[i];

            // 退出循环
            break;
        }

        // 返回数据
        return condition;
    }

    /// <summary>
    /// 移除状态
    /// </summary>
    public void RemoveStatus(LPCMapping data)
    {
        // 没有状态
        if (mStatusList.Count == 0)
            return;

        // 需要移除的index
        if (mStatusList.IndexOf(data) == -1)
            return;

        // 移除数据
        mStatusList.Remove(data);

        // buf类型减1
        int statusType = data.GetValue<int>("status_type");
        if (mStatusTypeMap.ContainsKey(statusType))
            mStatusTypeMap[statusType] = mStatusTypeMap[statusType] - 1;

        // 还没有改状态的引用计数
        int statusId = data.GetValue<int>("status_id");
        if (!mStatusCountMap.ContainsKey(statusId))
            return;

        // 应用计数
        if (mStatusCountMap[statusId] == 1)
        {
            mStatusCountMap.Remove(statusId);
            return;
        }

        // 应用计数-1
        mStatusCountMap[statusId] = mStatusCountMap[statusId] - 1;
    }

    /// <summary>
    /// 移除状态
    /// </summary>
    public void RemoveStatus(int cookie)
    {
        // 没有状态
        if (mStatusList.Count == 0)
            return;

        // 需要移除的index
        LPCMapping condition = null;
        LPCMapping data = null;

        // 找到需要移除的数据
        for (int i = 0; i < mStatusList.Count; i++)
        {
            data = mStatusList[i];
            if (data == null)
                continue;

            // status cookie不一致
            if (data.GetValue<int>("cookie") != cookie)
                continue;

            // statusMap
            condition = data;

            // 移除数据
            mStatusList.RemoveAt(i);

            // 退出循环
            break;
        }

        // 没有该cookie的状态
        if (condition == null)
            return;

        // buf类型减1
        int statusType = condition.GetValue<int>("status_type");
        if (mStatusTypeMap.ContainsKey(statusType))
            mStatusTypeMap[statusType] = mStatusTypeMap[statusType] - 1;

        // 还没有改状态的引用计数
        int statusId = condition.GetValue<int>("status_id");
        if (!mStatusCountMap.ContainsKey(statusId))
            return;

        // 应用计数
        if (mStatusCountMap[statusId] == 1)
        {
            mStatusCountMap.Remove(statusId);
            return;
        }

        // 应用计数-1
        mStatusCountMap[statusId] = mStatusCountMap[statusId] - 1;
    }
}
