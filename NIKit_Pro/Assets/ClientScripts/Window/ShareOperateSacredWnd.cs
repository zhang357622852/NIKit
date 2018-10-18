/// <summary>
/// ShareOperateSacredWnd.cs
/// Created by zhangwm 2018/07/09
/// 分享操作界面-圣域
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;
using System;

public class ShareOperateSacredWnd : WindowBase<ShareOperateSacredWnd>
{
    #region 成员变量
    //{0}专属[-]隐藏圣域
    public UILabel mTitleLab;
    //{0}{1}[-]属性[FFDC00]{2}★[-]使魔{3}「{4}」[-]等你挑战！
    public UILabel mTitleDesLab;
    public GameObject mPetModel;
    //成为我的好友，跟我一起获得该使魔吧！
    public UILabel mContentLab;
    //使魔元素
    public UISprite mPropSp;
    //在该圣域收集{0}个「{1}碎片」即可在「召唤祭坛」召唤该使魔
    public UILabel mPropDesLab;

    // 绑定的宠物对象
    private Property mPetProperty = null;
    #endregion

    private void Start()
    {
        // 初始化文本
        InitText();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mContentLab.text = LocalizationMgr.Get("ShareOperateWnd_3");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        if (mPetProperty == null)
            return;

        string elementColor = MonsterConst.MonsterElementColorMap[MonsterMgr.GetElement(mPetProperty.GetClassID())];

        //标题
        mTitleLab.text = string.Format(LocalizationMgr.Get("ShareOperateWnd_10"), elementColor);

        //标题描述
        mTitleDesLab.text = string.Format(LocalizationMgr.Get("ShareOperateWnd_11"), elementColor, PetMgr.GetElementName(MonsterMgr.GetElement(mPetProperty.GetClassID())), mPetProperty.GetStar(), elementColor, mPetProperty.Short());

        // 获取宠物的元素
        int element = MonsterMgr.GetElement(mPetProperty.GetClassID());
        mPropSp.spriteName = PetMgr.GetElementIconName(element);
        mPropSp.MakePixelPerfect();

        //描述
        mPropDesLab.text = string.Format(LocalizationMgr.Get("ShareOperateWnd_12"), MonsterMgr.GetPieceAmount(mPetProperty.GetClassID()), mPetProperty.Short());
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    private void ShowModel()
    {
        // 道具对象不存在
        if (mPetProperty == null)
            return;

        // 获取窗口绑定的模型窗口组件
        ModelWnd pmc = mPetModel.GetComponent<ModelWnd>();

        // 没有绑定模型窗口组件
        if (pmc == null)
            return;

        // 异步载入模型
        pmc.LoadModelSync(mPetProperty.GetClassID(), mPetProperty.GetRank(), LayerMask.NameToLayer("UI"));
    }

    #region 外部
    public void BindData(Property petProp, ShareOperateWnd parentWnd)
    {
        if (petProp == null)
            return;

        mPetProperty = petProp;
        //显示模型
        parentWnd.mTweenFinishedCallback = ShowModel;

        Redraw();
    }
    #endregion
}
