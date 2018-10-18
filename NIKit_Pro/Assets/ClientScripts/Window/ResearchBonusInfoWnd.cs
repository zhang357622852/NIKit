/// <summary>
/// ResearchBonusInfoWnd.cs
/// Created by lic 2017/01/25
/// 探索奖励信息界面
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class ResearchBonusInfoWnd : WindowBase<ResearchBonusInfoWnd>
{
    public UILabel mTitle;

    public UILabel mCompleteLb;
    public UILabel mCompleteTitle;

    public UILabel mBonusTitle;

    public UILabel mTips;

    public GameObject[] mBonusGroup; 

    public GameObject mCloseBtn;
    public GameObject mOkBtn;
    public UILabel mOkBtnLb;

    public Transform mLeftItems;
    public Transform mRightItems;
    public UISprite mBg;
    public UISprite mUnClearBg;

    #region 私有字段

    // 当前界面最少按5的奖励大小的界面显示
    const int min_bonus_num = 5;

    LPCMapping data  = new LPCMapping();

    #endregion

    #region 内部函数

    void Start()
    {
        // 初始化显示
        InitWnd();

        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mTitle.text = LocalizationMgr.Get("ResearchBonusInfoWnd_1");
        mTips.text = LocalizationMgr.Get("ResearchBonusInfoWnd_2");
        mCompleteTitle.text = LocalizationMgr.Get("ResearchRewardWnd_1");
        mBonusTitle.text = LocalizationMgr.Get("ResearchRewardWnd_3");
        mOkBtnLb.text = LocalizationMgr.Get("ResearchBonusInfoWnd_3");
    }

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    void RegisterEvent()
    {
        // 关闭按钮
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;

        // 跳过按钮
        UIEventListener.Get(mOkBtn).onClick = OnOkBtn;
    }

    // 刷新窗口
    void Redraw()
    {
        int taskId = data.GetValue<int> ("task_id"); 

        CsvRow item = TaskMgr.GetTaskInfo(taskId);
        if(item == null)
            return;

        mCompleteLb.text = LocalizationMgr.Get(item.Query<string> ("desc_args"));

        LPCArray bonus = item.Query<LPCArray> ("bonus_args");

        if (bonus.Count > 5)
        {
            mLeftItems.localPosition = new Vector3(- (bonus.Count - 5) * 100f / 2, 0f, 0f);
            mRightItems.localPosition = new Vector3((bonus.Count - 5) * 100f / 2, 0f, 0f);
            mBg.width += (bonus.Count - 5) * 100;
            mUnClearBg.width += (bonus.Count - 5) * 100;
        }

        int isOneTimeReceive = item.Query<int> ("is_one_time_receive");

        bool isShowReceive = true;

        bool isShowDays = false;

        for (int i = 0; i < mBonusGroup.Length; i++)
        {
            if (i >= bonus.Count)
            {
                mBonusGroup [i].SetActive (false);
                continue;
            }

            mBonusGroup [i].SetActive (true);

            if (isOneTimeReceive == 0)
            {
                if (i >= data.GetValue<int> ("receive_times"))
                    isShowReceive = false;

                isShowDays = true;
            }

            LPCMapping bonusData = bonus[i].AsMapping.GetValue<LPCMapping>("bonus");

            mBonusGroup [i].GetComponent<SignItemWnd> ().Bind (bonusData,
                LocalizationMgr.Get("ResearchRewardWnd_9"), false, isShowReceive, isShowDays ? i + 1 : -1);

            if (isShowReceive)
                mBonusGroup [i].GetComponent<SignItemWnd> ().ShowTipsEffect ();
        }
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow (gameObject.name);
    }

    /// <summary>
    /// 确定按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnOkBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow (gameObject.name);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="_data">Data.</param>
    public void BindData(LPCMapping _data)
    {
        if (_data == null)
            return;

        data = _data;

        Redraw ();
    }

    #endregion
}
