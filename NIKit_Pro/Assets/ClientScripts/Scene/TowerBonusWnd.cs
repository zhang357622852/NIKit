using UnityEngine;
using System.Collections;
using LPC;
using TMPro;

public class TowerBonusWnd : MonoBehaviour
{
    // 奖励物品图标
    public SpriteRenderer mIcon;

    // 奖励物品的数量
    public TextMeshPro mAmount;

    LPCMapping mBonus = LPCMapping.Empty;

    /// <summary>
    /// 重绘窗口
    /// </summary>
    void Redraw()
    {
        string iconName = string.Empty;

        int amount = 1;

        if (mBonus.ContainsKey("class_id"))
        {
            int classId = mBonus.GetValue<int>("class_id");

            if (ItemMgr.IsItem(classId))
            {
                iconName = ItemMgr.GetClearIcon(classId);
            }
            else if (MonsterMgr.IsMonster(classId))
            {
                int rank = MonsterMgr.GetDefaultRank(classId);
                iconName = MonsterMgr.GetIcon(classId, rank);
            }
            else
            {
            }
        }
        else
        {
            string fields = FieldsMgr.GetFieldInMapping(mBonus);

            amount = mBonus.GetValue<int>(fields);

            iconName = ItemMgr.GetClearIcon(FieldsMgr.GetFieldItemClassId(fields));
        }

        // 显示奖励物品数量
        mAmount.text = "×" + amount;

        Sprite sp = ResourceMgr.LoadSprite(string.Format("Assets/Art/Scene/tower/{0}.png", iconName));
        if (sp == null)
            return;

        // 显示奖励物品图标
        mIcon.sprite = sp;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping bonus)
    {
        if (bonus == null)
            return;

        mBonus = bonus;

        // 绘制窗口
        Redraw();
    }
}
