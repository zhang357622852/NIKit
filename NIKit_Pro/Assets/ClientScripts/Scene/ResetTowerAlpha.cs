/// <summary>
/// ResetTowerAlpha.cs
/// Created by fengsc 2017/08/29
/// 重置通天之塔的alpha值
/// </summary>
using UnityEngine;
using System.Collections;

public class ResetTowerAlpha : MonoBehaviour
{
    private SpriteRenderer mSpriteRenderer;

    private TweenAlpha mTweenAlpha;

    // Use this for initialization
    void Awake ()
    {
        mSpriteRenderer = GetComponent<SpriteRenderer>();

        mTweenAlpha = GetComponent<TweenAlpha>();
    }

    void OnEnable()
    {
        if (mSpriteRenderer != null)
        {
            Color color = mSpriteRenderer.color;

            // 设置mSpriteRenderer的alpha
            mSpriteRenderer.color = new Color(color.r, color.g, color.b, 0);
        }

        if (mTweenAlpha != null)
        {
            // 重置补间动画
            mTweenAlpha.ResetToBeginning();

            // 播放alpha动画
            mTweenAlpha.PlayForward();
        }
    }
}
