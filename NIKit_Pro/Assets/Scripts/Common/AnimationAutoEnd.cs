/// <summary>
/// AnimationAutoEnd.cs
/// Created by Who 2015-01-08
/// 动画自动结束组件
/// </summary>

using UnityEngine;
using System.Collections;

public partial class AnimationAutoEnd : MonoBehaviour
{
    #region 公共函数

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // 如果动画播放结束，则销毁本光效
        Animator aor = gameObject.GetComponent<Animator>();
        if (aor == null ||
            1.0f < aor.GetCurrentAnimatorStateInfo(CombatConfig.ANIMATION_BASE_LAYER_INEDX).normalizedTime)
        {
            GameObject.Destroy(gameObject);
            return;
        }
    }

    #endregion
}
