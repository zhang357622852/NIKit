/// <summary>
/// ActionCameraShake.cs
/// Created by wangxw 2014-12-4
/// 相机抖动
/// </summary>

using UnityEngine;
using System.Collections;

public class ActionCameraShake : ActionBase
{
    #region 成员变量

    // track移动
    private TrackInfo mTrack;
    private string mTrackName = string.Empty;

    // 持续时间
    private float mDuration = 0f;

    // 相机是否抖动结束
    private bool isShakeEnd = false;

    #endregion

    #region 内部函数

    /// <summary>
    /// move_speed速度变化
    /// </summary>
    private void DoShakeEnd(object param, params object[] paramEx)
    {
        isShakeEnd = true;
    }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionCameraShake(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mTrackName = para.GetProperty<string>("track", string.Empty);
        mDuration = para.GetProperty<float>("duration", 0f);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 如果是战斗客户端
        if (AuthClientMgr.IsAuthClient)
        {
            // 标识结束
            IsFinished = true;
            return;
        }

        // 获取轨迹文件
        mTrack = TrackMgr.GetTrack(mTrackName);

        // 没有配置的轨迹文件
        if (mTrack == null)
        {
            LogMgr.Trace("轨迹文件{0}，不存在。", mTrackName);
            IsFinished = true;
            return;
        }

        // 抖动效果
        bool ret = CameraShakeMgr.DoShake(SceneMgr.SceneCamera, mTrack, mDuration,
                       Game.NewCookie("shake"), new CallBack(DoShakeEnd, null));

        // 执行相机抖动失败
        if (!ret)
        {
            LogMgr.Trace("相机抖动失败！");
            IsFinished = true;
            return;
        }
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        base.End(isCancel);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
        // 如果还没有抖动结束
        if (!isShakeEnd)
            return;

        // 标识action序列已经结束
        IsFinished = true;
    }
}
