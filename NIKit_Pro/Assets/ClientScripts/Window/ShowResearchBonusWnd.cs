/// <summary>
/// ShowResearchBonusWnd.cs
/// Created by lic 2017/01/25
/// 探索奖励信息界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ShowResearchBonusWnd : WindowBase<ShowResearchBonusWnd>
{ 
    public UILabel mTitle;
    public Transform mTitleBg;

    public GameObject mBonusItem;
    public GameObject mLimitLb;
    public GameObject mPetItem;

    public GameObject mCloseBtn;

    public UISprite mBg;
    public UISprite mUnClearBg;

    public Transform mPanel;

    #region 私有字段

    // 当前界面最少按5的奖励大小的界面显示
    const int minBonusNum = 5;

    // 最多显示奖励列表数量
    const int max_bonus_item_num = 4;

    LPCArray data  = new LPCArray();

    List<Property> petList = new List<Property>();

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
        mTitle.text = string.Format("◆ {0} ◆",  LocalizationMgr.Get("ShowResearchBonusWnd_1"));
    }

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    void RegisterEvent()
    {
        // 关闭按钮
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        for (int i = 0; i < petList.Count; i++)
        {
            if (petList [i] != null)
                petList [i].Destroy ();
        }

        petList.Clear ();
    }

    // 刷新窗口
    void Redraw()
    {
        int maxBonus = minBonusNum;

        // 限制显示奖励显示数量
        int maxItem = data.Count > max_bonus_item_num ? max_bonus_item_num : data.Count;

        for (int i = 0; i < maxItem; i++)
        {
            int taskId = data[i].AsMapping.GetValue<int> ("task_id");

            int receive_times = data[i].AsMapping.GetValue<int> ("receive_times");

            CsvRow item = TaskMgr.GetTaskInfo(taskId);

            LPCArray bonus = item.Query<LPCArray> ("bonus_args");

            maxBonus = bonus.Count > maxBonus ? bonus.Count : maxBonus;

            GameObject petWnd;
            GameObject limitLb;

            petWnd = Instantiate (mPetItem) as GameObject;
            petWnd.transform.parent = mPanel;
            petWnd.name = string.Format ("pet_item_{0}", i);
            petWnd.transform.localScale = new Vector3 (0.75f, 0.75f, 0.75f);
            petWnd.transform.localPosition = new Vector3 (0f, -i * 120f, 0f);

            limitLb = Instantiate (mLimitLb) as GameObject;
            limitLb.transform.parent = mPanel;
            limitLb.transform.localScale = Vector3.one;
            limitLb.name = string.Format ("limit_lb_{0}", i);
            limitLb.transform.localPosition = new Vector3 (mLimitLb.transform.localPosition.x, mLimitLb.transform.localPosition.y -i * 120f, 0f);

            petWnd.GetComponent<PetItemWnd> ().SetBind (ClonePet(data[i].AsMapping.GetValue<LPCMapping> ("args")));
            petWnd.GetComponent<PetItemWnd> ().ShowLevel (false);

            // 获取限制天数
            int limitDays = item.Query<int> ("receive_days_limit");

            // 计算剩余天数
            int lastReceiveTimes = data[i].AsMapping.GetValue<int> ("last_receive_time");

            int starTime = data[i].AsMapping.GetValue<int> ("start_time");

            int restDays = lastReceiveTimes/86400 - starTime/86400;

            limitLb.GetComponent<UILabel>().text = string.Format(LocalizationMgr.Get("ShowResearchBonusWnd_2"), (limitDays - (restDays + 1)) < 0 ? 0 : (limitDays - (restDays + 1)));

            GameObject bonusWnd; 

            for (int j = 0; j < bonus.Count; j++)
            {
                bonusWnd = Instantiate (mBonusItem) as GameObject;
                bonusWnd.transform.parent = mPanel;
                bonusWnd.name = string.Format ("bonus_item_{0}", i);
                bonusWnd.transform.localPosition = new Vector3 (mBonusItem.transform.localPosition.x + j * 94, -i * 120f, 0f);
                bonusWnd.transform.localScale = Vector3.one;

                bonusWnd.GetComponent<SignItemWnd> ().Bind (bonus[j].AsMapping.GetValue<LPCMapping>("bonus"),
                    LocalizationMgr.Get("ResearchRewardWnd_9"), false, j < receive_times, j + 1);

                if (j == (receive_times - 1))
                    bonusWnd.GetComponent<SignItemWnd> ().ShowTipsEffect ();   
            }
        }

        // 横向扩大窗口
        if (maxBonus > minBonusNum)
        {
            mBg.width += (maxBonus - minBonusNum) * 94;
            mUnClearBg.width += (maxBonus - minBonusNum) * 94;

            mPanel.localPosition = new Vector3 (mPanel.localPosition.x - (maxBonus - minBonusNum) * 94/2, mPanel.localPosition.y, 0f);

            mCloseBtn.transform.localPosition = new Vector3 (mCloseBtn.transform.localPosition.x +
                (maxBonus - minBonusNum) * 94/2, mCloseBtn.transform.localPosition.y, 0f);
        }

        // 纵向扩大窗口
        if (maxItem > 1)
        {
            mBg.height += (maxItem - 1) * 120;
            mUnClearBg.height += (maxItem - 1) * 120;

            mPanel.localPosition = new Vector3 (mPanel.localPosition.x, mPanel.localPosition.y + (maxItem - 1) * 120f/2, 0f);

            mTitleBg.localPosition = new Vector3 (mTitleBg.localPosition.x, mTitleBg.localPosition.y + (maxItem - 1) * 120f/2, 0f);

            mCloseBtn.transform.localPosition = new Vector3 (mCloseBtn.transform.localPosition.x,
                mCloseBtn.transform.localPosition.y +(maxItem - 1) * 120f/2, 0f);
        }

        mBonusItem.SetActive (false);
        mPetItem.SetActive (false);
        mLimitLb.SetActive (false);
    }

    /// <summary>
    /// 克隆宠物
    /// </summary>
    /// <param name="debase">Debase.</param>
    Property ClonePet(LPCMapping dbase)
    {
        dbase.Add ("rid", Rid.New ());

        if (!dbase.ContainsKey ("class_id"))
            dbase.Add ("class_id", 2041);

        Property pet = PropertyMgr.CreateProperty (dbase);

        petList.Add (pet);

        return pet;
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow (gameObject.name);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="_data">Data.</param>
    public void BindData(LPCArray _data)
    {
        // 非法数据
        if (_data == null || _data.Count == 0)
            return;

        data = _data;

        Redraw ();
    }

    #endregion
}
