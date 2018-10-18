using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSpriteCtrl : IToggle
{
    public UIWidget mStarParent;

    public UIGrid mGrid;

    public GameObject mStarPrefab;

    private void Redraw()
    {
        if (mArg == null)
            return;

        int count = (int)mArg;

        mStarPrefab.SetActive(true);

        for (int i = 0; i < count; i++)
            NGUITools.AddChild(mGrid.gameObject, mStarPrefab);

        mStarPrefab.SetActive(false);
        mGrid.Reposition();
    }

    /// <summary>
    /// 开关关闭处理事件
    /// </summary>
    protected override void SwitchToOff()
    {
        mStarParent.alpha = 0.3f;
    }

    /// <summary>
    /// 开关打开处理事件
    /// </summary>
    protected override void SwitchToOn()
    {
        mStarParent.alpha = 1f;
    }

    /// <summary>
    /// 绑定参数
    /// </summary>
    /// <param name="arg"></param>
    public override void BindData(object arg)
    {
        base.BindData(arg);

        Redraw();
    }
}
