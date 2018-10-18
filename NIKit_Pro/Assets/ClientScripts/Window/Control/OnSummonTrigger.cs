using UnityEngine;
using System.Collections;

public class OnSummonTrigger : MonoBehaviour
{
    public GameObject mBlackBg;

    public void OnStarOver()
    {
        gameObject.GetComponent<Animator>().Play("idle", 0, 0f);
    }

    public void OnIdleOver()
    {
        mBlackBg.GetComponent<TweenAlpha>().PlayReverse();
    }
}
