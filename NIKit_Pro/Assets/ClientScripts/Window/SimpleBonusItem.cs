/// <summary>
/// SimpleBonusItem.cs
/// Created by lic 11/14/2017
/// 奖励简单item
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class SimpleBonusItem : WindowBase<SimpleBonusItem> 
{
    #region 成员变量

    public UITexture mIcon;
    public UILabel mNum;
    public UISprite[] mStars;

    #endregion

    #region 私有变量

    LPCMapping mBonusData;


    #endregion

    #region 内部函数

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        for (int i = 0; i < mStars.Length; i++)
            mStars[i].gameObject.SetActive(false);

        // 非属性奖励
        if(mBonusData.ContainsKey("class_id"))
        {
            // 非属性奖励必须要有class_id和amount两个参数
            int classId = mBonusData.GetValue<int>("class_id");
            int amount = mBonusData.GetValue<int>("amount");

            // 如果是道具
            if(ItemMgr.IsItem(classId))
            {
                mIcon.mainTexture = ItemMgr.GetTexture(classId, true);
                mNum.text = string.Format("×{0}", amount);
            }
            else if(MonsterMgr.IsMonster(classId))
            {
                // 没有配置表示未觉醒
                int rank = mBonusData.ContainsKey("rank") ?
                    mBonusData.GetValue<int>("rank"):1;

                mIcon.mainTexture = MonsterMgr.GetTexture(classId, rank);
                mNum.text = string.Format("×{0}", amount == 0 ? 1:amount);

                //根据是否觉醒设置星级图标的类型;
                string starName = PetMgr.GetStarName(rank);

                for (int i = 0; i < mStars.Length; i++)
                {
                    if (i >= mBonusData.GetValue<int>("star"))
                        break;

                    mStars[i].GetComponent<UISprite>().spriteName = starName;

                    mStars[i].gameObject.SetActive(true);
                }
            }
            else if(EquipMgr.IsEquipment(classId))
            {
                LogMgr.Trace("此窗口暂时不支持配置装备");
            }

            return;
        }

        // 显示属性奖励
        string field = FieldsMgr.GetFieldInMapping(mBonusData);

        mIcon.mainTexture = ItemMgr.GetTexture(FieldsMgr.GetFieldTexture(field));

        mNum.text = string.Format("×{0}", mBonusData[field].AsInt);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="bonusData">Bonus data.</param>
    /// <param name="level">Level.</param>
    /// <param name="receiveState">Receive state.</param>
    public void BindData(LPCMapping bonusData)
    {
        mBonusData = bonusData;

        Redraw();
    }

    #endregion

}
