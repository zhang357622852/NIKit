/// <summary>
/// SynthesisWnd.cs
/// Created by lic 2017/11/17
/// 精髓合成界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SynthesisWnd : WindowBase<SynthesisWnd>
{
    #region 属性

    public UILabel mTitle;

    public UILabel mPanelTitle;
    public GameObject mCloseBtn;

    // 帮助
    public GameObject mTip;
    public UILabel mTipTitle;

    // 合成信息
    public GameObject mLeftWnd;
    public GameObject mRightWnd;
    public UILabel mLeftItem;
    public UILabel mRightItem;
    public UILabel mSelectDesc;
    public UILabel mNotEnoughDesc;

    // 合成按钮
    public GameObject mSynthesisBtn;
    public UILabel mBtnLabel;
    public UILabel mBtnCost;
    public UISprite mBtnIcon;
    public GameObject mBtnCover;

    public Transform mPanel;

    public UISpriteAnimation mMaterialAnima;
    public UISpriteAnimation mSuccessAnima;

    public GameObject mCover;

    public GameObject mSynthesisItem;

    public TweenScale mTweenScale;

    #endregion

    #region 私有变量

    // 当前选择的规则
    private int mSelect = -1;

    // item格子缓存
    private List<GameObject> mItemList = new List<GameObject>();

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化窗口
        InitWnd();

        // 由于合成规则固定，后续的刷新
        // 只是刷新数量，此处创建格子，
        CreateItem();

        // 刷新窗口
        Redraw();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        EventMgr.UnregisterEvent("SynthesisWnd");
    }


    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mSynthesisBtn).onClick = OnSynthesisBtn;

        UIEventListener.Get(mLeftWnd).onClick = OnItemBtn;
        UIEventListener.Get(mRightWnd).onClick = OnItemBtn;

        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;

        UIEventListener.Get(mTip).onClick = OnTipBtn;

        // 注册精髓合成事件
        EventMgr.RegisterEvent("SynthesisWnd", EventMgrEventType.EVENT_SYNTHESIS, OnSynthesis);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// tween动画播放完成回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        mTitle.text = LocalizationMgr.Get("SynthesisWnd_1");
        mBtnLabel.text = LocalizationMgr.Get("SynthesisWnd_2");
        mPanelTitle.text = LocalizationMgr.Get("SynthesisWnd_3");
        mSelectDesc.text = LocalizationMgr.Get("SynthesisWnd_7");
        mTipTitle.text = LocalizationMgr.Get("SynthesisWnd_9");

        mSynthesisItem.SetActive (false);
    }


    /// <summary>
    /// 宠物升星消息回调
    /// </summary>
    private void OnSynthesis(int eventId, MixedValue para)
    {
        bool result = para.GetValue<bool>();

        // 失败
        if (!result)
        {
            StopAni();
            return;
        }

        // 播放进度条动画
        Coroutine.DispatchService(SynthesisAnimaCoroutine());
    }

    /// <summary>
    /// 终止合成动画
    /// </summary>
    /// <returns>The anima coroutine.</returns>
    private void StopAni()
    {
        mMaterialAnima.gameObject.SetActive(false);
        mSuccessAnima.gameObject.SetActive(false);
        Redraw();
        mCover.SetActive(false);
    }

    /// <summary>
    /// 升星动画
    /// </summary>
    /// <returns>The anima coroutine.</returns>
    private IEnumerator SynthesisAnimaCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        mMaterialAnima.gameObject.SetActive(false);

        mSuccessAnima.gameObject.SetActive(true);
        mSuccessAnima.enabled = true;
        mSuccessAnima.ResetToBeginning();

        yield return new WaitForSeconds(2f);

        mSuccessAnima.gameObject.SetActive(false);
        Redraw();
        mCover.SetActive(false);
    }

    /// <summary>
    /// 帮助按钮点击
    /// </summary>
    void OnTipBtn(GameObject go)
    {
        // 获取历史排名窗口
        GameObject wnd = WindowMgr.GetWindow(HelpWnd.WndType);

        if (wnd == null)
            wnd = WindowMgr.CreateWindow(HelpWnd.WndType, HelpWnd.PrefebResource);

        if (wnd == null)
        {
            LogMgr.Trace("HelpWnd窗口创建失败");
            return;
        }

        WindowMgr.ShowWindow(wnd);
    }

    /// <summary>
    /// 合成按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSynthesisBtn(GameObject ob)
    {
        if (mSelect == -1)
            return;

        CsvRow data = BlacksmithMgr.GetSyntheticData(mSelect);
        LPCMapping cost = data.Query<LPCMapping>("attrib_cost");
        LPCArray costArray = new LPCArray(cost);

        if (!BlacksmithMgr.CheckCostEnough(ME.user, costArray))
            return;

        LPCMapping para = new LPCMapping();
        para.Add("rule", mSelect);

        if (!BlacksmithMgr.DoAction(ME.user, "synthesis", para))
            return;

        mMaterialAnima.gameObject.SetActive(true);
        mMaterialAnima.enabled = true;
        mMaterialAnima.ResetToBeginning();

        mCover.SetActive(true);
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        // 打开主窗口
        WindowMgr.OpenWnd("MainWnd");

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 合成item被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnItemBtn(GameObject ob)
    {
        // 没有选中
        if (mSelect == -1)
            return;

        // 取消之前的选择
        foreach (GameObject itemOb in mItemList)
        {
            if (itemOb.GetComponent<ItemWnd>().ClassId == mSelect)
            {
                itemOb.GetComponent<ItemWnd>().SetSelected(false);
                break;
            }
        }

        mLeftWnd.GetComponent<ItemWnd>().SetBind(-1);
        mRightWnd.GetComponent<ItemWnd>().SetBind(-1);

        mSelect = -1;
        RedrawBtn();
        RedrawSynInfo();
    }

    /// <summary>
    /// item被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnPanelItemBtn(GameObject ob)
    {
        ItemWnd item = ob.GetComponent<ItemWnd>();

        if (item == null)
            return;

        int rule = item.ClassId;

        if (!BlacksmithMgr.CheckCanSynthesis(rule))
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("SynthesisWnd_6"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        if (mSelect == -1)
        {
            mSelect = rule;
            item.SetSelected(true);
        }
        else
        {
            if (mSelect == rule)
            {
                mSelect = -1;
                item.SetSelected(false);
            }
            else
            {
                // 取消之前的选择
                foreach (GameObject itemOb in mItemList)
                {
                    if (itemOb.GetComponent<ItemWnd>().ClassId == mSelect)
                    {
                        itemOb.GetComponent<ItemWnd>().SetSelected(false);
                        break;
                    }
                }

                mSelect = rule;
                item.SetSelected(true);
            }
        }

        RedrawBtn();
        RedrawSynInfo();
    }

    /// <summary>
    /// 根据合成规则创建格子
    /// </summary>
    void CreateItem()
    {
        List<LPCMapping> syntheData = BlacksmithMgr.GetSyntheticList(BlacksmithConst.SYNTHESIS_ITEM);

        // 数据为空
        if (syntheData == null || syntheData.Count == 0)
            return;

        for (int i = 0; i < syntheData.Count; i++)
        {
            LPCMapping data = syntheData[i];

            if (data == null || data.Count == 0)
                continue;

            GameObject item = Instantiate (mSynthesisItem) as GameObject;
            item.transform.parent = mPanel;
            item.name = string.Format("Synthesis_item_{0}", i);
            item.transform.localScale = Vector3.one;
           
            item.transform.localPosition = new Vector3(0, -i * 190, 0);

            item.SetActive(true);

            item.GetComponent<SynthesisItemWnd>().SetBind(data);

            mItemList.AddRange(item.GetComponent<SynthesisItemWnd>().mItems);

            foreach (GameObject itemOB in item.GetComponent<SynthesisItemWnd>().mItems)
                UIEventListener.Get(itemOB).onClick = OnPanelItemBtn;
        }
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    void Redraw()
    {
        // 刷新数值显示
        foreach (GameObject ob in mItemList)
        {
            ItemWnd itemWnd = ob.GetComponent<ItemWnd>();

            int ruleId = itemWnd.ClassId;

            if (ruleId <= 0)
                continue;

            int number = UserMgr.GetAttribItemAmount(ME.user, ruleId);

            ob.GetComponent<ItemWnd>().SetNumber(number.ToString());
        }

        // 刷新按钮显示
        RedrawBtn();

        // 刷新合成信息
        RedrawSynInfo();
    }

    /// <summary>
    /// 刷新按钮
    /// </summary>
    void RedrawBtn()
    {
        if (mSelect == -1)
        {
            mBtnIcon.spriteName = "money";
            mBtnCost.text = "0";
            mBtnCover.SetActive(true);
            return;
        }

        if (IsSlectEnough())
            mBtnCover.SetActive(false);
        else
            mBtnCover.SetActive(true);

        CsvRow data = BlacksmithMgr.GetSyntheticData(mSelect);
        LPCMapping cost = data.Query<LPCMapping>("attrib_cost");
        string field = FieldsMgr.GetFieldInMapping(cost);

        mBtnIcon.spriteName = FieldsMgr.GetFieldIcon(field);
        mBtnCost.text = cost.GetValue<int>(field).ToString();
    }

    /// <summary>
    /// 刷新合成信息
    /// </summary>
    void RedrawSynInfo()
    {
        if (mSelect == -1)
        {
            mLeftWnd.GetComponent<ItemWnd>().SetBind(-1);
            mRightWnd.GetComponent<ItemWnd>().SetBind(-1);

            mSelectDesc.gameObject.SetActive(true);
            mNotEnoughDesc.gameObject.SetActive(false);
            mLeftItem.gameObject.SetActive(false);
            mRightItem.gameObject.SetActive(false);

            return;
        }

        mSelectDesc.gameObject.SetActive(false);
        mLeftItem.gameObject.SetActive(true);
        mRightItem.gameObject.SetActive(true);

        CsvRow data = BlacksmithMgr.GetSyntheticData(mSelect);

        LPCArray cost = data.Query<LPCArray>("material_cost");

        LPCMapping cost_map = cost[0].AsMapping;

        int cost_id = cost_map.GetValue<int>("class_id");
        int cost_num = cost_map.GetValue<int>("amount");

        mRightWnd.GetComponent<ItemWnd>().SetBind(mSelect);
        mLeftWnd.GetComponent<ItemWnd>().SetBind(cost_id);
        mRightWnd.GetComponent<ItemWnd>().SetNumber("1");
        mLeftItem.text = ItemMgr.GetName(cost_id);
        mRightItem.text = ItemMgr.GetName(mSelect);

        if (IsSlectEnough())
        {
            mNotEnoughDesc.gameObject.SetActive(false);
            mLeftWnd.GetComponent<ItemWnd>().SetNumber(cost_num.ToString());
        }
        else
        {
            mNotEnoughDesc.gameObject.SetActive(true);
            mLeftWnd.GetComponent<ItemWnd>().SetNumber(string.Format("[FF2A28FF]{0}[-]", cost_num));
            mNotEnoughDesc.text = string.Format(LocalizationMgr.Get("SynthesisWnd_8"), ItemMgr.GetName(cost_id), cost_num);
        }
    }

    /// <summary>
    /// 检测魂石合成是否足够
    /// </summary>
    /// <returns><c>true</c> if this instance is slect enough; otherwise, <c>false</c>.</returns>
    bool IsSlectEnough()
    {
        if (mSelect == -1)
            return false;

        LPCArray cost = BlacksmithMgr.GetSyntheticData(mSelect)
            .Query<LPCArray>("material_cost");

        if (cost == null || cost.Count == 0)
            return false;

        int need = cost[0].AsMapping.GetValue<int>("amount");
        int classId = cost[0].AsMapping.GetValue<int>("class_id");

        int amount = UserMgr.GetAttribItemAmount(ME.user, classId);

        return amount >= need;
    }

    #endregion
}
