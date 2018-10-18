/// <summary>
/// SkillWnd.cs
/// Created by lic 7/8/2016
/// 宠物技能界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LPC;

public class SkillWnd : WindowBase<SkillWnd>
{
    // 技能格子组
    public GameObject[] mSkillGroup;

    // 技能名称
    public UILabel mSkillName;

    //文本显示组件
    public UILabel mContent;

    // 技能耗蓝
    public GameObject[] mMpGroup;

    public UIScrollView scroll;

    // 当前的宠物对象
    private Property item_ob = null;

    // 当前选中的classId
    private int selectId = -1;

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();
    }

    #region 内部方法

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    private void RegisterEvent()
    {
        for (int i = 0; i < mSkillGroup.Length; i++)
            UIEventListener.Get(mSkillGroup[i]).onClick = OnSkillBtn;
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        // 重置选中
        selectId = -1;

        // 默认先把所有的技能格子全部置空
        for (int i = 0; i < mSkillGroup.Length; i++)
        {
            mSkillGroup[i].GetComponent<SkillItem>().SetBind(-1);
            mSkillGroup[i].GetComponent<SkillItem>().SetSelected(false);
        }

        // 当前绑定宠物为空
        if(item_ob == null)
            return;

        // 获取绑定宠物的技能
        LPCArray skillInfo = item_ob.GetAllSkills();

        // 对字典按key（skillid）进行排序
        foreach (LPCValue mks in skillInfo.Values)
        {
            // 获取技能类型
            int skillId = mks.AsArray[0].AsInt;
            int type = SkillMgr.GetSkillPosType(skillId);

            if (type <= 0 || type > mSkillGroup.Length)
                continue;

            SkillItem item = mSkillGroup[type - 1].GetComponent<SkillItem>();
                
            item.SetBind(skillId);

            // 判断是否为队长技能
            if (!SkillMgr.IsLeaderSkill(skillId))
            {
                item.SetLevel(item_ob.GetSkillLevel(skillId));
                item.SetLeader(false);
            }
            else
            {
                item.SetLevel(-1);
                item.SetLeader(true);
            }

            // 设置第一个为选中
            if (type == 1)
                OnSkillBtn(mSkillGroup[0]);
            else
                item.SetSelected(false);
        }
    }

    /// <summary>
    /// 设置选中
    /// </summary>
    private void OnSkillBtn(GameObject ob)
    {
        SkillItem item = ob.GetComponent<SkillItem>();

        if (item.mSkillId <= 0)
            return;

        if (selectId > 0 && selectId == item.mSkillId)
            return;

        if (selectId > 0)
        {
            // 默认先把所有的技能格子全部取消选中
            for (int i = 0; i < mSkillGroup.Length; i++)
            {
                mSkillGroup[i].GetComponent<SkillItem>().SetSelected(false);
            }
        }
       
        // 设置选中
        item.SetSelected(true);

        selectId = item.mSkillId;

        // 刷新技能描述
        RedrawDesc(selectId);

        // 还原scroll位置
        scroll.ResetPosition();

        // 刷新耗蓝
        RedrawMP(selectId);
    }

    private void RedrawDesc(int skillId)
    {
        if (skillId <= 0)
            return;

        // 获取技能名称
        string skillName = SkillMgr.GetSkillName(skillId);

        string skillDesc = GET_SKILL_SUM_DESC.CALL(skillId, item_ob.GetSkillLevel(skillId));

        mSkillName.text = skillName;

        //添加文本;
        mContent.text = skillDesc;
    }


    private void RedrawMP(int skillId)
    {
        // 取得蓝耗的值
        LPCMapping mpMap = SkillMgr.GetCasTCost(item_ob, skillId);

        int mp = mpMap.ContainsKey("mp") ? mpMap.GetValue<int>("mp") : 0;

        if (mp < 0)
            mp = 0;

        if (mp > mMpGroup.Length)
            mp = mMpGroup.Length;

        for (int i = 0; i < mMpGroup.Length; i++)
        {
            if (i < mp)
                mMpGroup[i].SetActive(true);
            else
                mMpGroup[i].SetActive(false);
        }
    }

    #endregion


    #region 外部接口

    /// <summary>
    /// 设置当前宠物对象
    /// </summary>
    public void SetBind(Property ob)
    {
        // 重置绑定对象
        item_ob = ob;

        Redraw();
    }

    #endregion

}
