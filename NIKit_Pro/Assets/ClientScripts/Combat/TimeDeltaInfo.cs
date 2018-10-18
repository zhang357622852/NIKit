/// <summary>
/// TimeDeltaInfo.cs
/// Created by wangxw 2014-11-06
/// 时间控制参数
/// </summary>

public struct TimeDeltaInfo
{
    // 原始流失时间
    public float SourceDeltaTime;

    // 缩放因子
    public float ScalFactor;

    // 最终时间
    public float DeltaTime;

    // 零时长常量
    public static readonly TimeDeltaInfo ZERO = new TimeDeltaInfo(0.0f, 1.0f);

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sourceTime">原始时间</param>
    /// <param name="factor">变速因子</param>
    public TimeDeltaInfo(float sourceTime, float factor)
    {
        SourceDeltaTime = sourceTime;
        ScalFactor = factor;
        DeltaTime = sourceTime * factor;
    }
}
