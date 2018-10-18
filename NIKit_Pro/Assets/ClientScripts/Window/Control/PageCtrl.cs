using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 页签的控制器
/// </summary>
public class PageCtrl : MonoBehaviour
{
    public UISprite mCheckmarkSp;
    public BoxCollider mBoxCol;
    public UILabel mPageLab;

    /// <summary>
    /// 选中
    /// </summary>
    /// <param name="isSelected"></param>
    public void SetSelected(bool isSelected)
    {
        if (mCheckmarkSp != null)
            mCheckmarkSp.gameObject.SetActive(isSelected);

        if (mBoxCol != null)
            mBoxCol.enabled = !isSelected;
    }

    /// <summary>
    /// 设置文本
    /// </summary>
    /// <param name="str"></param>
    public void SetLabel(string str)
    {
        if (mPageLab != null)
            mPageLab.text = LocalizationMgr.Get(str);
    }
}
