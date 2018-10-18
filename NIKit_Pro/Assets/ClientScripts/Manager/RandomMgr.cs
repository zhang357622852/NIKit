/// <summary>
/// RandomMgr.cs
/// Create by zhaozy 2016-07-29
/// Random管理模块
/// </summary>

using System;
using System.Diagnostics;
using System.Collections.Generic;
using LPC;

/// 道具管理器
public static class RandomMgr
{
    #region 变量声明

    // 最大随机范围
    private static int MAX_RANDOM_RANG = 1000;

    // 随机种子
    private static int gNativeRandSeed = 0;

    #endregion

    #region 功能接口

    /// <summary>
    /// 获取随机数
    /// 取值范围是[0..1000)
    /// </summary>
    public static int GetRandom()
    {
        // 返回随机数
        return GetRandom(MAX_RANDOM_RANG);
    }

    /// <summary>
    /// 获取随机数
    /// 取值范围是[0..range)
    /// </summary>
    public static int GetRandom(int range)
    {
        // 重新计算gNativeRandSeed
        gNativeRandSeed = (1664525 * gNativeRandSeed) + 1013904223;

        // 返回随机数
        return ((gNativeRandSeed >> 16) & 0xFFFF) * range >> 16;
    }

    /// <summary>
    /// 重置随机种子
    /// </summary>
    public static void RestRandomSeed(int randomSeed)
    {
        // 重置gNativeRandSeed
        gNativeRandSeed = randomSeed;
    }

    /// <summary>
    /// 根据权重列表随机元素
    /// 空数组返回-1，否则根据数组中配置的权重，返回被抽中元素的位置
    /// 如果权重元素小于0，则在权重抽取是按照0处理
    /// </summary>
    public static int RandomSelect(LPCArray weightList)
    {
        // 获取权重列表数量
        int Count = weightList.Count;

        // 权重列表为空
        if (Count == 0)
            return -1;

        // 统计总权重
        int totalWeight = 0;
        int weight = 0;

        //计算权重总和
        for (int i = 0; i < weightList.Count; i++)
        {
            // 统计权重为0, 则表示不可能选择到该元素
            weight = weightList[i].AsInt;
            if (weight <= 0)
                continue;

            // 汇总中权重
            totalWeight += weight;
        }

        // 如果总权重
        if (totalWeight <= 0)
            return -1;

        // 生成伪随机权重值[0...totalWeight)
        int selectWeight = GetRandom(totalWeight);

        // 遍历权重分段信息
        for (int i = 0; i < weightList.Count; i++)
        {
            // 统计权重为0, 则表示不可能选择到该元素
            weight = weightList[i].AsInt;
            if (weight <= 0)
                continue;

            // 不在区间范围内不能选择,区间范围选择方式左开右闭[min, max)
            if (selectWeight >= weight)
            {
                selectWeight -= weight;
                continue;
            }

            // 合符条件推出循环, 返回抽中元素id
            return i;
        }

        // 没有符合要求则返回-1
        return -1;
    }

    /// <summary>
    /// 根据权重列表随机元素
    /// 空数组返回-1，否则根据数组中配置的权重，返回被抽中元素的位置
    /// 如果权重元素小于0，则在权重抽取是按照0处理
    /// </summary>
    public static int RandomSelect(List<int> weightList)
    {
        // 获取权重列表数量
        int Count = weightList.Count;

        // 权重列表为空
        if (Count == 0)
            return -1;

        // 统计总权重
        int totalWeight = 0;

        //计算权重总和
        for (int i = 0; i < weightList.Count; i++)
        {
            // 统计权重为0, 则表示不可能选择到该元素
            if (weightList[i] <= 0)
                continue;

            // 汇总中权重
            totalWeight += weightList[i];
        }

        // 如果总权重
        if (totalWeight <= 0)
            return -1;

        // 生成伪随机权重值[0...totalWeight)
        int selectWeight = GetRandom(totalWeight);

        // 遍历权重分段信息
        for (int i = 0; i < weightList.Count; i++)
        {
            // 统计权重为0, 则表示不可能选择到该元素
            if (weightList[i] <= 0)
                continue;

            // 不在区间范围内不能选择,区间范围选择方式左开右闭[min, max)
            if (selectWeight >= weightList[i])
            {
                selectWeight -= weightList[i];
                continue;
            }

            // 合符条件推出循环, 返回抽中元素id
            return i;
        }

        // 没有符合要求则返回-1
        return -1;
    }

    /// <summary>
    /// 根据权重列表随机元素
    /// 空数组返回-1，否则根据数组中配置的权重，返回被抽中元素的位置
    /// </summary>
    public static int CompleteRandomSelect(LPCArray weightList)
    {
        // 获取权重列表数量
        int Count = weightList.Count;

        // 权重列表为空
        if (Count == 0)
            return -1;

        // 统计总权重
        int totalWeight = 0;
        int weight = 0;

        //计算权重总和
        for (int i = 0; i < weightList.Count; i++)
        {
            // 统计权重为0, 则表示不可能选择到该元素
            weight = weightList[i].AsInt;
            if (weight <= 0)
                continue;

            // 汇总中权重
            totalWeight += weight;
        }

        // 如果总权重
        if (totalWeight <= 0)
            return -1;

        // 随即一个权重值
        System.Random ran = new System.Random(GetRandomSeed());
        int selectWeight = ran.Next(1, totalWeight + 1);

        // 遍历权重分段信息
        for (int i = 0; i < weightList.Count; i++)
        {
            // 统计权重为0, 则表示不可能选择到该元素
            weight = weightList[i].AsInt;
            if (weight <= 0)
                continue;

            // 不在区间范围内不能选择,区间范围选择方式左开右闭[min, max)
            if (selectWeight >= weight)
            {
                selectWeight -= weight;
                continue;
            }

            // 合符条件推出循环, 返回抽中元素id
            return i;
        }

        // 没有符合要求则返回-1
        return -1;
    }

    /// <summary>
    /// 随机种子值
    /// </summary>
    public static int GetRandomSeed()
    {
        byte[] bytes = new byte[4];
        System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        rng.GetBytes(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }

    #endregion
}
