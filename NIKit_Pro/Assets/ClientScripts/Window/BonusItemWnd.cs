using UnityEngine;
using System.Collections;

public class BonusItemWnd : WindowBase<BonusItemWnd>
{

    #region 公共字段
    public UITexture mIcon;
    public UILabel mDesc;

    #endregion

    #region 公共字段
    public int mBonusId { get; private set; }
    #endregion

    #region 内部函数

    /// <summary>
    /// 刷新窗口
    /// </summary>
    void Redraw()
    {
        CsvRow item = LotteryBonusMgr.GetLotteryBonus(mBonusId);

        if(item == null)
        {
            mIcon.gameObject.SetActive(false);
            mDesc.gameObject.SetActive(false);
            return;
        }

        mIcon.gameObject.SetActive(true);
        mDesc.gameObject.SetActive(true);

        string iconName = item.Query<string>("icon");

        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName));
        mDesc.text = LocalizationMgr.Get(item.Query<string>("num_desc"));
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="bonusId">Bonus identifier.</param>
    public void BindData(int bonusId)
    {
        mBonusId = bonusId;

        Redraw();
    }
    #endregion
}
