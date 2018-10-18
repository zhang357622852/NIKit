/// <summary>
/// ShareOperatePetUpStarWnd.cs
/// Created by zhangwm 2018/07/10
/// 分享操作界面-圣域
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ShareOperatePetUpStarWnd : WindowBase<ShareOperatePetUpStarWnd>
{
    #region 成员变量
    //属性提升
    public UILabel mPropUpLab;
    //星级提升
    public UILabel mStarUpLab;
    public UILabel mStarUpValueLab;
    //最大等级提升
    public UILabel mMaxLevelLab;
    public UILabel mMaxLevelValueLab;
    //生命提升
    public UILabel mHpLab;
    public UILabel mHpValueLab;
    //攻击力提升
    public UILabel mAttackLab;
    public UILabel mAttackValueLab;
    //防御提升
    public UILabel mDefenceLab;
    public UILabel mDefenceValueLab;

    //星级提升
    public UILabel mStarUpTips;
    //元素icon
    public UISprite mElementSp;
    public UILabel mPetNameLab;
    public GameObject mPetModel;
    public UIGrid mStarGrid;
    public UISprite mStarPrefab;

    // 绑定的宠物对象
    private Property mOldPetProperty = null;
    private Property mNewPetProperty = null;
    #endregion

    private void Start()
    {
        // 初始化文本
        InitText();
    }

    private void OnDestroy()
    {
        if (mOldPetProperty != null)
            mOldPetProperty.Destroy();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mPropUpLab.text = LocalizationMgr.Get("ShareOperateWnd_4");
        mStarUpLab.text = LocalizationMgr.Get("ShareOperateWnd_5");
        mMaxLevelLab.text = LocalizationMgr.Get("ShareOperateWnd_6");
        mHpLab.text = LocalizationMgr.Get("ShareOperateWnd_7");
        mAttackLab.text = LocalizationMgr.Get("ShareOperateWnd_8");
        mDefenceLab.text = LocalizationMgr.Get("ShareOperateWnd_9");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        if (mOldPetProperty == null || mNewPetProperty == null)
            return;

        int star = mNewPetProperty.GetStar();
        //星级
        mStarUpValueLab.text = string.Format("{0}★->{1}★", star - 1, star);
        //最大等级
        mMaxLevelValueLab.text = string.Format("{0}->{1}", MonsterMgr.GetMaxLevel(star-1), MonsterMgr.GetMaxLevel(star));
        //生命提升
        mHpValueLab.text = string.Format("{0}->{1}", mOldPetProperty.Query<int>("max_hp"), mNewPetProperty.Query<int>("max_hp"));
        //攻击力提升
        mAttackValueLab.text = string.Format("{0}->{1}", mOldPetProperty.Query<int>("attack"), mNewPetProperty.Query<int>("attack"));
        //防御提升
        mDefenceValueLab.text = string.Format("{0}->{1}", mOldPetProperty.Query<int>("defense"), mNewPetProperty.Query<int>("defense"));

        // 获取宠物的元素
        int element = MonsterMgr.GetElement(mNewPetProperty.GetClassID());
        mElementSp.spriteName = PetMgr.GetElementIconName(element);
        mElementSp.MakePixelPerfect();

        //宠物名字
        mPetNameLab.text = string.Format("{0}{1}", PetMgr.GetAwakeColor(mNewPetProperty.GetRank()), mNewPetProperty.Short()) ;

        //星星
        string IconName = PetMgr.GetStarName(mNewPetProperty.GetRank());
        mStarPrefab.gameObject.SetActive(true);
        for (int i = 0; i < star; i++)
        {
            GameObject go = NGUITools.AddChild(mStarGrid.gameObject, mStarPrefab.gameObject);
            go.GetComponent<UISprite>().spriteName = IconName;
        }
        mStarPrefab.gameObject.SetActive(false);
        mStarGrid.Reposition();
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    void ShowModel()
    {
        // 道具对象不存在
        if (mNewPetProperty == null)
            return;

        // 获取窗口绑定的模型窗口组件
        ModelWnd pmc = mPetModel.GetComponent<ModelWnd>();

        // 没有绑定模型窗口组件
        if (pmc == null)
            return;

        // 异步载入模型
        pmc.LoadModelSync(mNewPetProperty.GetClassID(), mNewPetProperty.GetRank(), LayerMask.NameToLayer("UI"));
    }

    #region 外部
    public void BindData(Property newPetProp, ShareOperateWnd parentWnd)
    {
        if (newPetProp == null)
            return;

        mNewPetProperty = newPetProp;
        parentWnd.mTweenFinishedCallback = ShowModel;

        LPCMapping debase = new LPCMapping();
        debase.Add("level", 1);
        debase.Add("star", mNewPetProperty.GetStar() - 1);
        debase.Add("exp", 0);
        mOldPetProperty = PropertyMgr.DuplicateProperty(mNewPetProperty, debase); ;

        Redraw();
    }
    #endregion
}
