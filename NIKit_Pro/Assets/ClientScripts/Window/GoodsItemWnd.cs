/// <summary>
/// GoodsItemWnd.cs
/// Created by fengsc 2017/ 12/05
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class GoodsItemWnd : WindowBase<GoodsItemWnd>
{
    public UITexture mIcon;

    public UILabel mAmonut;

    // 序列帧动画
    public UISpriteAnimation mSpriteAnimation;
    private Property mPropOb = null;
    private LPCMapping mData = LPCMapping.Empty;

    private void Start()
    {
        UIEventListener.Get(gameObject).onClick = OnItemBtn;
    }

    private void OnDestroy()
    {
        if (mPropOb != null)
            mPropOb.Destroy();
    }

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

                mAmonut.text = string.Format("{0}×{1}", MonsterMgr.GetName(classId, mData.GetValue<int>("rank")), amount);
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                mIcon.mainTexture = EquipMgr.GetTexture(classId, mData.GetValue<int>("rarity"));

                mAmonut.text = string.Format("{0}×{1}", EquipMgr.GetName(classId), amount);
            }
            else
            {
                mIcon.mainTexture = ItemMgr.GetTexture(ItemMgr.GetClearIcon(classId));

                mAmonut.text = string.Format("{0}×{1}", ItemMgr.GetName(classId), amount);
            }
        }
        else
        {
            string fields = FieldsMgr.GetFieldInMapping(mData);

            classId = FieldsMgr.GetClassIdByAttrib(fields);

            amount = mData.GetValue<int>(fields);

            mIcon.mainTexture = ItemMgr.GetTexture(ItemMgr.GetClearIcon(classId));

            mAmonut.text = string.Format("{0}+{1}", ItemMgr.GetName(classId), amount);
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
    /// 物体被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    private void OnItemBtn(GameObject ob)
    {
        // 获取奖励数据
        LPCMapping itemData = mData;
        if (itemData == null)
            return;

        if (itemData.ContainsKey("class_id"))
        {
            int classId = itemData.GetValue<int>("class_id");

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;

            dbase.Append(itemData);
            dbase.Add("rid", Rid.New());

            // 克隆物件对象
            if (mPropOb != null)
                mPropOb.Destroy();

            mPropOb = PropertyMgr.CreateProperty(dbase);

            if (MonsterMgr.IsMonster(classId))
            {
                // 显示宠物悬浮窗口
                GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

                script.Bind(mPropOb);
                script.ShowBtn(true, false, false);
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                script.SetEquipData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

                script.SetMask(true);
            }
            else
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

                script.SetMask(true);
            }
        }
        else
        {
            string fields = FieldsMgr.GetFieldInMapping(itemData);

            int classId = FieldsMgr.GetClassIdByAttrib(fields);

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;
            dbase.Add("class_id", classId);
            dbase.Add("amount", itemData.GetValue<int>(fields));
            dbase.Add("rid", Rid.New());

            if (mPropOb != null)
                mPropOb.Destroy();

            mPropOb = PropertyMgr.CreateProperty(dbase);

            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

            script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

            script.SetMask(true);
        }
    }

    /// <summary>
    /// 刷新数目文本
    /// </summary>
    public void RefreshAmountNoName()
    {
        if (mData == null)
            return;

        int amount = 0;

        if (mData.ContainsKey("class_id"))
        {
            amount = mData.GetValue<int>("amount");
        }
        else
        {
            string fields = FieldsMgr.GetFieldInMapping(mData);

            amount = mData.GetValue<int>(fields);
        }

        mAmonut.text = string.Format("×{0}", amount);
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
