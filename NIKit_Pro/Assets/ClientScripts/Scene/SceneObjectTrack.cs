/// <summary>
/// SceneObjectTrack.cs
/// Created by zhaozy 2016/06/15
/// 场景物件移动处理脚本
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneObjectTrack : MonoBehaviour
{
    [System.Serializable]
    public class SceneTrackCurve
    {
        public Vector3 posFrom;
        public Vector3 posTo;
    }

    // 移动轨迹列表
    public List<SceneTrackCurve> aniList;

    // 是否循环标识
    public bool isLoop = true;

    // 移动速度
    public float x_speed = 0.0f;
    private float mDistance = 0.0f;

    // 流逝时间
    private int mIndex = 0;
    private bool mIsEnd = false;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // 如果已经结束
        if (mIsEnd)
            return;

        mDistance = Time.unscaledDeltaTime * x_speed;

        // 驱动更新位置
        DoDrive();
    }

    /// <summary>
    /// 驱动
    /// </summary>
    void DoDrive()
    {
        // 已经驱动结束
        if (!isLoop && mIndex >= aniList.Count)
        {
            mIsEnd = true;
            return;
        }

        // 两者距离已经小于deltaDistance
        Vector3 curPos = gameObject.transform.localPosition;

        // 计算目标移动终点
        Vector3 TargetPos = Vector3.MoveTowards(curPos, aniList[mIndex].posTo, mDistance);

        // 本段路径已经驱动结束
        float distance = (curPos - aniList[mIndex].posTo).magnitude;
        if (distance < Mathf.Abs(mDistance))
        {
            mDistance -= (Mathf.Sign(mDistance) * distance);
            mIndex++;

            // 已经驱动到结束点，重新驱动
            if (mIndex >= aniList.Count)
                mIndex = 0;

            // 设置起点位置
            gameObject.transform.localPosition = aniList[mIndex].posFrom;

            // 在驱动一次
            DoDrive();

            // 返回
            return;
        }

        // 设置位置
        gameObject.transform.localPosition = TargetPos;
    }
}
