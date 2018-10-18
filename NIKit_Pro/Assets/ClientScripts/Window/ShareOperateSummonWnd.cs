/// <summary>
/// ShareOperateSummonWnd.cs
/// Created by zhangwm 2018/07/10
/// 分享操作界面-召唤
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ShareOperateSummonWnd : WindowBase<ShareOperateSummonWnd>
{
    #region 成员变量
    //使用
    public UILabel mUseLab;
    //召唤卷icon
    public UITexture mItemTex;
    //(传说召唤卷)
    public UILabel mItemName;
    //成功召唤出
    public UILabel mSucSummonLab;
    //截图
    public UITexture mCaptureTex;
    //元素icon
    public UISprite mElementSp;
    //使魔名字
    public UILabel mNameLab;
    public UIGrid mStarGrid;
    public GameObject mStarGo;

    public GameObject mOneSummonPart;
    public GameObject mTenSummonPart;

    //一口气召唤这么多使魔，名字太多写不下！
    public UILabel mDesLab;

    // 绑定的宠物对象
    private Property mPetProperty = null;
    private CsvRow mItemCsvRow;
    private ShareOperateWnd.ShareOperateType mCurShareType = ShareOperateWnd.ShareOperateType.None;
    private Texture mCaptureTexture;
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
        mUseLab.text = LocalizationMgr.Get("ShareOperateWnd_13");
        mDesLab.text = LocalizationMgr.Get("ShareOperateWnd_16");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        if (mItemCsvRow == null || mCaptureTexture == null)
            return;

        if (mCurShareType == ShareOperateWnd.ShareOperateType.SingleSummon)
        {
            if (mPetProperty == null)
                return;

            mOneSummonPart.SetActive(true);
            mTenSummonPart.SetActive(false);

            mSucSummonLab.text = LocalizationMgr.Get("ShareOperateWnd_14");

            // 获取宠物的元素
            int element = MonsterMgr.GetElement(mPetProperty.GetClassID());
            mElementSp.spriteName = PetMgr.GetElementIconName(element);
            mElementSp.MakePixelPerfect();

            //姓名
            mNameLab.text = mPetProperty.Short();

            //星星
            string IconName = PetMgr.GetStarName(mPetProperty.GetRank());
            mStarGo.SetActive(true);
            for (int i = 0; i < mPetProperty.GetStar(); i++)
            {
                GameObject go = NGUITools.AddChild(mStarGrid.gameObject, mStarGo);
                go.GetComponent<UISprite>().spriteName = IconName;
            }
            mStarGo.SetActive(false);
            mStarGrid.Reposition();
        }
        else if (mCurShareType == ShareOperateWnd.ShareOperateType.TenSummon)
        {
            mOneSummonPart.SetActive(false);
            mTenSummonPart.SetActive(true);

            mSucSummonLab.text = LocalizationMgr.Get("ShareOperateWnd_15");
        }

        //截图
        mCaptureTex.mainTexture = mCaptureTexture;

        int type = mItemCsvRow.Query<int>("type");
        if (type == 16 && mPetProperty != null)
        {
            // 召唤卷名称
            mItemName.text = string.Format("({0})", string.Format(LocalizationMgr.Get(mItemCsvRow.Query<string>("title")), mPetProperty.Short()));

            //卷轴icon
            mItemTex.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", "s50011a"));
        }
        else
        {
            // 召唤卷名称
            mItemName.text = string.Format("({0})", LocalizationMgr.Get(mItemCsvRow.Query<string>("title")));

            //卷轴icon
            string iconName = SummonMgr.GetSummonIcon(mItemCsvRow.Query<string>("icon"));
            mItemTex.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName));
        }
    }

    #region 外部
    public void BindData(ShareOperateWnd.ShareOperateType type, Texture text, CsvRow row, Property prop = null)
    {
        if (type != ShareOperateWnd.ShareOperateType.SingleSummon && type != ShareOperateWnd.ShareOperateType.TenSummon)
            return;

        mCurShareType = type;
        mItemCsvRow = row;
        mCaptureTexture = text;
        mPetProperty = prop;

        Redraw();
    }
    #endregion
}
