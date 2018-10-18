/// <summary>
/// PetSelectItemWnd.cs
/// Created by lic 2017/02/15
/// 选择宠物组件
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class PetSelectItemWnd : WindowBase<PetSelectItemWnd>
{
    public GameObject mPetItemWnd;
    public GameObject[] mEquipGroup;
    public GameObject[] mSkillGroup;

    public GameObject mChangeBtn;
    public UILabel mBtnLb;
    public GameObject mBtnCover;

    public UILabel mStateDescLb;

    public UISprite mBg;

    #region 私有字段

    [HideInInspector]
    public Property
    item_ob;

    [HideInInspector]
    public bool
    isSelected = false;

    CallBack task = null;

    #endregion

    #region 内部函数

    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        // 设置选中图标位置
        mPetItemWnd.GetComponent<PetItemWnd>().SetSelectPos(SelectPos.BottonRightCorner);
    }

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mChangeBtn).onClick += OnChangeBtn;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mBtnLb.text = LocalizationMgr.Get("PetSelectItemWnd_1");
        mStateDescLb.text = LocalizationMgr.Get("PetSelectItemWnd_2");
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    void Redraw()
    {
        mPetItemWnd.GetComponent<PetItemWnd> ().SetBind (item_ob);

        int type;
        Property equipData;

        // 绑定装备数据
        for (int i = 0; i < mEquipGroup.Length; i++)
        {
            type = mEquipGroup[i].GetComponent<EquipItemWnd>().equipType;

            // 获取宠物身上该位置的装备
            equipData = (item_ob as Container).baggage.GetCarryByPos(EquipMgr.GetEquipPos(type));

            mEquipGroup[i].GetComponent<EquipItemWnd>().SetBind(equipData);
        }

        // 默认先把所有的技能格子全部置空
        for (int i = 0; i < mSkillGroup.Length; i++)
            mSkillGroup[i].GetComponent<SkillItem>().SetBind(-1);

        // 获取绑定宠物的技能
        LPCArray skillInfo = item_ob.GetAllSkills();

        SkillItem item;

        // 对字典按key（skillid）进行排序
        foreach (LPCValue mks in skillInfo.Values)
        {
            // 获取技能类型
            int skillId = mks.AsArray[0].AsInt;
            type = SkillMgr.GetSkillPosType(skillId);

            if (type <= 0 || type > mSkillGroup.Length)
                continue;

            item = mSkillGroup[type - 1].GetComponent<SkillItem>();

            item.SetBind(skillId);

            // 判断是否为队长技能
            if (SkillMgr.IsLeaderSkill(skillId))
            {
                item.SetLevel (-1);
                item.SetLeader(true);
            }
            else
            {
                item.SetMaxLevel(item_ob.GetSkillLevel(skillId));
                item.SetLeader(false);
            }
        }
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnChangeBtn(GameObject ob)
    {
        if (task != null)
            task.Go ();
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="pet_ob">Pet ob.</param>
    /// <param name="task">Task.</param>
    public void BindData(Property _item_ob)
    {
        if (_item_ob == null)
            return;

        this.item_ob = _item_ob;

        Redraw ();
    }

    /// <summary>
    /// 设置选中
    /// </summary>
    /// <param name="_isSelect">If set to <c>true</c> is select.</param>
    /// <param name="selectDesc">Select desc.</param>
    public void SetSelect(bool _isSelect)
    {
        mPetItemWnd.GetComponent<PetItemWnd> ().SetSelected (_isSelect);

        mChangeBtn.SetActive (!_isSelect);

        mStateDescLb.gameObject.SetActive (_isSelect);

        mBg.alpha = _isSelect ? 0.5f : 1.0f;
    }

    /// <summary>
    /// 设置能否改替换
    /// </summary>
    /// <param name="_canChange">If set to <c>true</c> can change.</param>
    public void SetChangeState(bool _canChange)
    {
        mBtnCover.SetActive (! _canChange);
    }

    /// <summary>
    /// 设置按钮点击回调
    /// </summary>
    /// <param name="task">Task.</param>
    public void SetCallBack(CallBack task)
    {
        this.task = task;
    }

    #endregion
}
