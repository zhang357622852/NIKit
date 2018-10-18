/// <summary>
/// DrapOutPetItemWnd.cs
/// Created by fengsc 2016/07/19
///掉落物品栏宠物格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DrapOutPetItemWnd : WindowBase<DrapOutPetItemWnd>
{
    /// <summary>
    ///可掉落宠物的头像
    /// </summary>
    public UITexture mPetIcon;

    /// <summary>
    ///宠物元素图标
    /// </summary>
    public UISprite mElementIcon;

    /// <summary>
    ///宠物名称
    /// </summary>
    public UILabel mPetName;

    /// <summary>
    ///宠物星级
    /// </summary>
    public GameObject[] mStars;

    /// <summary>
    /// 窗口绑定对象
    /// </summary>
    /// <value>The item ob.</value>
    public int ClassId { get; private set; }

    /// <summary>
    ///重绘窗口
    /// </summary>
    void Redraw()
    {
        //获取宠物的信息;
        CsvRow petData = PropertyMgr.QueryMonsterBase(ClassId);

        if(petData == null)
            return;

        if(petData == null || ClassId <= 0)
            return;



        for (int i = 0; i < mStars.Length; i++)
            mStars[i].SetActive(false);

        //获取宠物星级;
        int starNum = petData.Query<int>("star");

        int count = starNum < mStars.Length ? starNum : mStars.Length;

        //获取宠物的rank;
        int rank = petData.Query<int>("rank");

        //根据觉醒获取星级的图片类型;
        string spriteName = PetMgr.GetStarName(rank);

        for (int i = 0; i < count; i++)
        {
            mStars[i].GetComponent<UISprite>().spriteName = spriteName;
            mStars[i].SetActive(true);
        }

        //根据class_id获取宠物元素;
        mElementIcon.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(ClassId));

        mPetName.text = LocalizationMgr.Get(petData.Query<string>("name"));

        mPetIcon.mainTexture = MonsterMgr.GetTexture(ClassId, rank);

        mPetIcon.gameObject.SetActive(true);
    }

    public void Bind(int class_id)
    {
        if(class_id <= 0)
            return;

         this.ClassId = class_id;

        Redraw();
    }
}
