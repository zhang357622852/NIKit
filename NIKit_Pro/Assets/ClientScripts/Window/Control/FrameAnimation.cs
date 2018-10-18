/// <summary>
/// Created by fengsc 2018/07/11
/// FrameAnimation.cs
/// 聊天动态表情序列帧动画控件
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameAnimation : MonoBehaviour
{
    public int Group { get; set;}

    private bool mIsLoop = true;

    public bool IsLoop
    {
        get { return mIsLoop; }
        set { mIsLoop = value; }
    }

    private List<CsvRow> mExpressionList = new List<CsvRow>();

    private List<string> mSpriteNameList = new List<string>();

    private List<float> mDurationList = new List<float>();

    private bool mIsStart = false;

    private float mLastTime = 0f;

    private int mIndex = 0;

    private UISprite mSprite;

    // Use this for initialization
    void Start ()
    {
        mSprite = GetComponent<UISprite>();

        mExpressionList = ChatRoomMgr.GetExpressionListByGroup(Group);
        if (mExpressionList == null)
            return;

        // 收集图片名称和图片显示的时长
        foreach (CsvRow row in mExpressionList)
        {
            mSpriteNameList.Add(row.Query<string>("icon"));

            mDurationList.Add(row.Query<LPC.LPCValue>("duration").AsFloat);
        }

        if (mSpriteNameList.Count == 0
            || mDurationList.Count == 0
            || mSpriteNameList.Count != mDurationList.Count)
            return;

        mIsStart = true;

        // 更换图片
        mSprite.spriteName = mSpriteNameList[mIndex];
    }

    // Update is called once per frame
    void Update ()
    {
        if (!mIsStart)
            return;

        if (mLastTime == 0)
            mLastTime = Time.realtimeSinceStartup;

        if (Time.realtimeSinceStartup <= mLastTime + mDurationList[mIndex])
            return;

        // 记录当前时间
        mLastTime = Time.realtimeSinceStartup;

        mIndex++;

        if (mIndex + 1 > mSpriteNameList.Count)
        {
            // 重置索引
            if (IsLoop)
            {
                mLastTime = Time.realtimeSinceStartup;

                mIndex = 0;
            }
            else
            {
                mIsStart = false;

                mLastTime = 0;

                mIndex = 0;
            }
        }

        // 更换图片
        mSprite.spriteName = mSpriteNameList[mIndex];
    }
}
