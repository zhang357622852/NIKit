/// <summary>
/// PlayShadowAnim.cs
/// Copy from tanzy 2016-05-16
/// 播放单个阴影动画
/// </summary>

using UnityEngine;
using System.Collections;

public class PlayShadowAnim : MonoBehaviour
{
    public float timeRate = 0.1f;
    public bool whenEndHide = false;

    public UISprite mSprite;
    bool isPlay = true;
    private float frame = 0f;
    float nexTime;
    private float target_alpha = 1f;
    private bool isShow = true;


    void Update()
    {
        if (!gameObject.activeSelf)
        {
            StopAnim();
            return;
        }

        if (isPlay)
        {
            // 显示
            if (frame < target_alpha && isShow)
            {
                if (RealTime.time > nexTime)
                {
                    mSprite.enabled = true;
                    nexTime = Time.unscaledTime + timeRate;
                    mSprite.alpha = frame;
                    frame += 0.1f;
                    if (frame >= target_alpha)
                        isShow = false;
                }
            }
            else
            {
                // 隐藏
                if (frame > 0.2f && !isShow)
                {
                    if (RealTime.time > nexTime)
                    {
                        mSprite.enabled = true;
                        nexTime = Time.unscaledTime + timeRate;
                        mSprite.alpha = frame;
                        frame -= 0.1f;
                        if (frame <= 0.2f)
                            isShow = true;
                    }
                }
                else
                {
                    isPlay = false;

                    if (whenEndHide)
                        mSprite.enabled = false;
                }
            }
        }
    }

    // 播放动画
    public void PlayAnim(bool loop = false)
    {
        isPlay = true;
        frame = 0;
    }

    // 播放动画
    public void StopAnim()
    {
        isPlay = false;
        frame = 0;
    }
}
