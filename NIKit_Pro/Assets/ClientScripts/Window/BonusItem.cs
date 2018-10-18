/// <summary>
/// BonusItem.cs
/// Created by lic 2017/04/09
/// 奖励项
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using LPC;

public class BonusItem : WindowBase<BonusItem>
{

    #region 成员变量

    public UISprite m_Icon;
    public UILabel m_Desc;
    public GameObject m_Bg;
    public UISprite m_OutLook;

    #endregion


    #region 公共函数

    /// <summary>
    /// 赋值
    /// </summary>
    public void SetItem(LPCMapping item_data)
    {
        string icon = item_data.GetValue<string>("icon");
        string desc = item_data.GetValue<string>("desc");
        string type = item_data.GetValue<string>("type");
        string color = item_data.GetValue<string>("color");

        if (type != null && type != string.Empty)
        {
            if (type == "gold_coin")
            {
                color = ColorConfig.GetColor(ColorConfig.NC_DIAMOND);
            }else if (type == "money")
            {
                color = ColorConfig.GetColor(ColorConfig.NC_YELLOW);
            }
        }
        
        m_OutLook.color = StringToColor(color);
        m_Desc.color = StringToColor(color);
        m_Bg.GetComponent<UISprite>().color = StringToColor(color);
        m_Icon.spriteName = icon;
        m_Desc.text = desc;
    }

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {

    }
    
    // 16进制颜色转为uisprite可用颜色
    Color StringToColor(string color_str)
    {
        int r = (Convert.ToInt32(color_str, 16) & 0xFF0000) >> 16;
        int g = (Convert.ToInt32(color_str, 16) & 0x00FF00) >> 8;
        int b = (Convert.ToInt32(color_str, 16) & 0x0000FF);
        
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
    }
    #endregion
}
