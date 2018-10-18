/// <summary>
/// ItemWnd.cs
/// Created by lic 11/14/2016
/// 道具格子
/// </summary>

using UnityEngine;
using System.Collections;

public class ItemWnd : WindowBase<SkillItem>
{
    public UITexture mIcon;
    public UILabel  mNumber;
    public UISprite mBg;
    public GameObject mSelect;

    #region 属性

    /// <summary>
    /// 窗口绑定对象
    /// </summary>
    /// <value>The item ob.</value>
    public int ClassId { get; private set; }

    /// <summary>
    /// 窗口选择状态
    /// </summary>
    public bool IsSelected { get; private set; }

    #endregion

    #region 内部函数

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        mNumber.gameObject.SetActive(false);
        mSelect.SetActive(false);
        mBg.spriteName = "skill";

        if(ClassId <= 0)
        {
            mIcon.gameObject.SetActive(false);

            return;
        }

        mIcon.mainTexture = ResourceMgr.LoadTexture(
            string.Format("Assets/Art/UI/Icon/item/{0}.png", ItemMgr.GetIcon(ClassId)));

        mIcon.gameObject.SetActive(true);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 设置选中
    /// </summary>
    public void SetSelected(bool is_selected)
    {
        IsSelected = is_selected;

        // 设置选中高光,同时播放动画
        if (IsSelected)
        {
            mBg.spriteName = "skill_select";
            mSelect.SetActive(true);
        }
        else
        {
            mBg.spriteName = "skill";
            mSelect.SetActive(false);
        }
    }

    /// <summary>
    /// 显示数量
    /// </summary>
    public void SetNumber(string number)
    {
        mNumber.text = number;

        mNumber.gameObject.SetActive(true);
    }

    /// <summary>
    /// 窗口绑定实体
    /// </summary>
    public void SetBind(int itemId)
    {
        // 重置绑定对象
        ClassId = itemId;

        // 重绘窗口
        Redraw();
    }

    #endregion
}
