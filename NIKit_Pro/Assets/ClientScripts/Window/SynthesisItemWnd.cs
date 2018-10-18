using UnityEngine;
using System.Collections;
using LPC;

public class SynthesisItemWnd : WindowBase<SynthesisItemWnd>
{
    public UISprite mIcon;
    public UILabel mTitle;
    public GameObject[] mItems;

    #region 属性

    public LPCMapping mData { get; private set; }

    #endregion

    #region 内部函数

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        if(mData == null || mData.Count == 0)
            return;

        mTitle.text = LocalizationMgr.Get(mData.GetValue<string>("name"));
        mIcon.spriteName = mData.GetValue<string>("icon");

        LPCArray itemList = mData.GetValue<LPCArray>("rules");

        for(int i = 0; i < mItems.Length; i++)
        {
            if(i >= itemList.Count)
            {
                mItems[i].SetActive(false);
                continue;
            }

            mItems[i].SetActive(true);
            mItems[i].GetComponent<ItemWnd>().SetBind(itemList[i].AsInt);
        }
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 窗口绑定实体
    /// </summary>
    public void SetBind(LPCMapping itemId)
    {
        // 重置绑定对象
        mData = itemId;

        // 重绘窗口
        Redraw();
    }

    #endregion
}
