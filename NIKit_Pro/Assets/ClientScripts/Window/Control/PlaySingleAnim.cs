/// <summary>
/// PlaySingleAnim.cs
/// Copy from fucj 2014-12-10
/// 播放单个动画
/// </summary>

using UnityEngine;
using System.Collections;

public class PlaySingleAnim : MonoBehaviour
{

    public string[] AnimName;
    public float timeRate = 0.1f;
    public bool whenEndHide = false;

    UISprite mSprite;
    bool isPlay = false;
    int frame = 0;
    float nexTime;
    bool isLoop = false;
    // Use this for initialization
    void Start()
    {
        mSprite = this.GetComponent<UISprite>();
    }

    void Update()
    {
        if (! gameObject.activeSelf)
        {
            StopAnim();
            return;
        }

        if (isPlay)
        {
            if (frame < AnimName.Length)
            {
                if (RealTime.time > nexTime)
                {
                    mSprite.enabled = true;
                    nexTime = Time.unscaledTime + timeRate;
                    mSprite.spriteName = AnimName [frame];
                    frame++;
                }
            } else
            {
                if (isLoop)
                    frame = 0;
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
        isLoop = loop;
        isPlay = true;
        frame = 0;
    }

    // 播放动画
    public void StopAnim()
    {
        isLoop = false;
        isPlay = false;
        frame = 0;
    }
}
