/// <summary>
/// DungeonsDropGoodsItemWnd.cs
/// Created by fengsc 2017/01/06
/// 地下城掉落物体解除格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DungeonsDropGoodsItemWnd : WindowBase<DungeonsDropGoodsItemWnd>
{
    public UITexture mIcon;

    public UILabel mName;

    // 角标
    public GameObject mSub;

    [HideInInspector]
    public LPCMapping mData = LPCMapping.Empty;

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mSub.SetActive(false);

        if (mData.ContainsKey("suit_id"))
        {
            // 获取套装id
            int suitId = mData.GetValue<int>("suit_id");

            // 获取套装的配置数据
            CsvRow row = EquipMgr.SuitTemplateCsv.FindByKey(suitId);

            if (row == null)
                return;

            // 获取图标名称
            string iconName = row.Query<string>("icon");
            string resPath = string.Format("Assets/Art/UI/Icon/equipment/{0}.png", iconName);
            mIcon.mainTexture = ResourceMgr.LoadTexture(resPath);

            // 使用图片的原始大小
            mIcon.MakePixelPerfect();

            // 套装名称
            mName.text = LocalizationMgr.Get(row.Query<string>("name"));
        }
        else if (mData.ContainsKey("class_id"))
        {
            // 获取套装id
            int classId = mData.GetValue<int>("class_id");

            // 获取图标名称
            string iconName = string.Empty;

            string resPath = string.Empty;

            if (MonsterMgr.IsMonster(classId))
            {
                iconName = MonsterMgr.GetIcon(classId, MonsterMgr.GetDefaultRank(classId));

                resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);
            }
            else if (ItemMgr.IsItem(classId))
            {
                iconName = ItemMgr.GetClearIcon(classId);

                resPath = string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName);
            }
            else
            {
                
            }

            mIcon.mainTexture = ResourceMgr.LoadTexture(resPath);

            // 设置图片的大小
            mIcon.height = 70;
            mIcon.width = 70;

            mIcon.transform.localPosition = Vector3.zero;

            mName.gameObject.SetActive(false);
        }
        else
        {
            
        }

    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        if (data == null)
            return;

        mData = data;

        // 绘制窗口
        Redraw();
    }

    /// <summary>
    /// 显示角标
    /// </summary>
    public void ShowSub(bool isShow)
    {
        mSub.SetActive(isShow);
    }
}
