/// <summary>
/// GoodsItemWnd.cs
/// Created by fengsc 2017/ 12/05
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class MonthCardItemWnd : WindowBase<MonthCardItemWnd>
{
    public UITexture mIcon;

    public UILabel mName;

    public UILabel mAmonut;

    // 序列帧动画
    public UISpriteAnimation mSpriteAnimation;

    private LPCMapping mData = LPCMapping.Empty;

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mData == null)
            return;

        int classId = 0;

        int amount = 0;

        if (mData.ContainsKey("class_id"))
        {
            classId = mData.GetValue<int>("class_id");

            amount = mData.GetValue<int>("amount");

            if (MonsterMgr.IsMonster(classId))
            {
                mIcon.mainTexture = MonsterMgr.GetTexture(classId, mData.GetValue<int>("rank"));

                mAmonut.text = string.Format("×{0}", amount);

                mName.text = MonsterMgr.GetName(classId, mData.GetValue<int>("rank"));
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                mIcon.mainTexture = EquipMgr.GetTexture(classId, mData.GetValue<int>("rarity"));

                mAmonut.text = string.Format("×{0}", amount);

                mName.text = EquipMgr.GetName(classId);
            }
            else
            {
                mIcon.mainTexture = ItemMgr.GetTexture(ItemMgr.GetClearIcon(classId));

                mAmonut.text = string.Format("×{0}", amount);

                mName.text = ItemMgr.GetName(classId);
            }
        }
        else
        {
            string amountDesc = string.Empty;

            if (mData.ContainsKey("display_time"))
            {
                classId = FieldsMgr.GetClassIdByAttrib(mData.GetValue<string>("attrib"));

                CsvRow row = ItemMgr.GetRow(classId);

                LPCMapping applyArg = row.Query<LPCMapping>("apply_arg");

                amount = mData.GetValue<int>("amount") * applyArg["valid_time"].AsInt;

                if (amount >= 86400)
                {
                    amountDesc = string.Format("+{0}{1}", amount / 86400, LocalizationMgr.Get("QuickMarketWnd_5"));
                }
                else
                {
                    amountDesc = string.Format("+{0}{1}", amount / 3600, LocalizationMgr.Get("QuickMarketWnd_6"));
                }
            }
            else
            {
                string fields = FieldsMgr.GetFieldInMapping(mData);

                classId = FieldsMgr.GetClassIdByAttrib(fields);

                amount = mData.GetValue<int>(fields);

                amountDesc = amount.ToString();
            }

            mIcon.mainTexture = ItemMgr.GetTexture(ItemMgr.GetClearIcon(classId));

            mAmonut.text = amountDesc;

            mName.text = ItemMgr.GetName(classId);
        }

        if (mSpriteAnimation == null)
            return;

        mSpriteAnimation.gameObject.SetActive(false);

        if (!mData.ContainsKey("show_effect"))
            return;

        mSpriteAnimation.gameObject.SetActive(true);

        // 播放序列帧动画
        mSpriteAnimation.Play();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="data">Data.</param>
    public void Bind(LPCMapping data)
    {
        mData = data;

        // 重绘窗口
        Redraw();
    }
}
