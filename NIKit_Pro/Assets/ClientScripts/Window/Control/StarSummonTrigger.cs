using UnityEngine;
using System.Collections;

public class StarSummonTrigger : MonoBehaviour
{
    public Animator mOnSummonAni;

    public GameObject mBlackBg;

    public void OnStarSummonOver()
    {
        gameObject.GetComponent<Animator>().enabled = false;
        mOnSummonAni.enabled = true;
        mOnSummonAni.Play("star", 0 , 0f);

        mBlackBg.GetComponent<TweenAlpha>().PlayForward();
    }
}
