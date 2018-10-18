/// <summary>
/// AnimationTrigger.cs
/// Created by wangxw 2015-01-13
/// 动画触发器回调
/// 用于角色动画触发Event之后通知到战斗系统
/// 目前先给3个，定制
/// </summary>

using UnityEngine;
using System.Collections;

public class AnimationTrigger : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
    }

    /// <summary>
    /// Triggers the event.
    /// </summary>
    /// <param name="eventName">Event name.</param>
    public void TriggerEvent(string eventName)
    {
//        // 通过对象名查找逻辑对象
//        Property ob = Rid.FindObjectByRid(gameObject.name);
//        System.Diagnostics.Debug.Assert(ob != null);
//        ob.Actor.TriggerEvent(eventName);
    }
}
