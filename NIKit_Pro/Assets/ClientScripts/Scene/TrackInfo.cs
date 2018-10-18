/// <summary>
/// TrackInfo.cs
/// Modify by zhaozy 2016-06-02
/// 轨迹
/// </summary>

using UnityEngine;
using System;
using System.Collections;

public class TrackInfo : ScriptableObject
{
    #region 成员变量

    // 绑定物体的朝向类型
    public enum DirectionType
    {
        // 切线方向
        Tangent,

        // 朝向目标
        Target,

        // 固定方向
        Fix,
    }

    // 路程曲线，实际控制速度变化
    public AnimationCurve SpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve XMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
    public AnimationCurve YMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
    public AnimationCurve ZMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

    // 节点朝向类型
    public DirectionType NodeDirection = DirectionType.Tangent;

    // 默认时间
    public float AniTime = 1;

    #endregion

    public TrackInfo()
    {
    }

    /// <summary>
    /// 拷贝构造函数
    /// </summary>
    public TrackInfo(TrackInfo source)
    {
        AniTime = source.AniTime;
        NodeDirection = source.NodeDirection;
        XMoveCurve = new AnimationCurve(source.XMoveCurve.keys);
        YMoveCurve = new AnimationCurve(source.YMoveCurve.keys);
        ZMoveCurve = new AnimationCurve(source.ZMoveCurve.keys);
    }

    /// <summary>
    /// 获取采样点
    /// </summary>
    /// <return>相对偏移的坐标值</return>
    /// <param name="percent">曲线时长百分比，范围在[0,1]之间</param>
    public Vector3 Evaluate(float timePercent)
    {
        float p = Mathf.Clamp01(timePercent);
        p = Mathf.Clamp01(SpeedCurve.Evaluate(p));
        return new Vector3(XMoveCurve.Evaluate(p),
            YMoveCurve.Evaluate(p),
            ZMoveCurve.Evaluate(p));
    }

    /// <summary>
    /// 根据行进轨迹，获取采样点，计算的最终坐标
    /// </summary>
    /// <param name="timePercent">曲线时长百分比，范围在[0,1]之间</param>
    /// <param name="start">轨迹起点.</param>
    /// <param name="end">轨迹终点.</param>
    public Vector3 Evaluate(float timePercent, Vector3 start, Vector3 end)
    {
        float p = Mathf.Clamp01(timePercent);
        p = Mathf.Clamp01(SpeedCurve.Evaluate(p));
        Vector3 offsetPos = new Vector3(XMoveCurve.Evaluate(p),
                                YMoveCurve.Evaluate(p),
                                ZMoveCurve.Evaluate(p));

        // 加和轨迹与偏移
        return offsetPos + Vector3.Lerp(start, end, p);
    }
}
