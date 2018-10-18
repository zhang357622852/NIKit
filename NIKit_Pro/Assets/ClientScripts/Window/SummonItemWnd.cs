/// <summary>
/// SummonItemWnd.cs
/// Created by lic 7/15/2016
/// 召唤格子
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class SummonItemWnd : WindowBase<SummonItemWnd>
{
    #region 成员变量

    public UISprite mBg;
    public UILabel  mNameLb;
    public UILabel  mStarLb;
    public UILabel mOwnLb;

    // 宠物碎片召唤需要显示星级和合成标记
    public GameObject[] mStars;
    public GameObject mPiece;
    public UITexture mMonsterIcon;

    // 非宠物碎片召唤图标
    public UITexture mIcon;

    #endregion


    #region 私有变量

    public CsvRow mSummonItem { get; private set; }

    public int mSubId{ get; private set; }

    public bool mIsSelected { get; private set; }

    public string mItemId {get; private set; }

    #endregion


    #region 内部函数

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        if(mSummonItem == null)
            return;

        // 显示能召唤的星级
        mStarLb.text = LocalizationMgr.Get(mSummonItem.Query<string>("star_desc"));

        // 显示消耗描述
        mOwnLb.text = GetCostDesc();

        if(mSubId > 0)
        {
            foreach(GameObject starOb in mStars)
                starOb.SetActive(false);

            CsvRow item = MonsterMgr.GetRow(mSubId);

            if (item == null)
                return;

            mNameLb.text = string.Format(LocalizationMgr.Get(mSummonItem.Query<string>("title")), LocalizationMgr.Get(item.Query<string>("name")));

            mMonsterIcon.mainTexture = MonsterMgr.GetTexture(mSubId, item.Query<int>("rank"));

            int star = item.Query<int>("star");

            string StarName = PetMgr.GetStarName(item.Query<int>("rank"));

            for(int i = 0; i < star; i++)
            {
                mStars[i].GetComponent<UISprite>().spriteName = StarName;
                mStars[i].SetActive(true);
            }

            mPiece.SetActive(true);

            return;
        }

        // 召唤卷名称
        mNameLb.text = LocalizationMgr.Get(mSummonItem.Query<string>("title"));

        string iconName = mSummonItem.Query<string>("icon");
        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName));
    }

    /// <summary>
    /// 获取消耗描述
    /// </summary>
    /// <returns>The cost desc.</returns>
    /// <param name="item">Item.</param>
    private string GetCostDesc()
    {
        int scriptNo = mSummonItem.Query<int>("cost_desc_script");

        string desc = string.Empty;

        if(scriptNo == 0)
            return desc;

        desc = (string) ScriptMgr.Call(scriptNo, ME.user, mSummonItem.Query<LPCMapping>("cost_desc_args"), mSubId);

        return desc;
    }

    #endregion


    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void BindData(CsvRow summonItem, int _mSubId = 0)
    {
        this.mSummonItem = summonItem;

        mSubId = _mSubId;

        mItemId = string.Empty;

        if(summonItem != null)
        {
            mItemId = summonItem.Query<int>("type").ToString();

            if(mSubId > 0)
                mItemId =mItemId + mSubId.ToString();
        }

        // 重绘界面
        Redraw();
    }

    /// <summary>
    /// 设置选中
    /// </summary>
    public void SetSelected(bool is_selected)
    {
        this.mIsSelected = is_selected;

        if (mIsSelected)
            mBg.spriteName = "summonSelectBg";
        else
            mBg.spriteName = "summonNoSelectBg";
    }

    #endregion
}
