/// <summary>
/// DefenceDeployViewWnd.cs
/// Created by fengsc 2016/12/13
/// 玩家防守宠物查看界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DefenceDeployViewWnd : WindowBase<DefenceDeployViewWnd>
{
    #region 成员变量

    // 队长技能描述;
    public UILabel mLeaderSkillDesc;
    public UILabel mLeaderSkillDescLb;

    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 宠物模型
    public ModelWnd[] mModels;

    public GameObject[] mSelectEffect;

    public GameObject mMask;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    LPCArray defenceList = LPCArray.Empty;

    // 缓存创建的物件对象
    List<Property> mCacheCreateOb = new List<Property>();

    // 第一次打开时待窗口动画完成后再加载模型
    private bool isTweenOver = false;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        mLeaderSkillDescLb.text = LocalizationMgr.Get("DefenceDeployViewWnd_1");

        // 注册事件
        RegisterEvent();

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        // 播放动画
        mTweenAlpha.PlayForward();

        mTweenScale.PlayForward();

        // 重置动画组件
        mTweenAlpha.ResetToBeginning();

        mTweenScale.ResetToBeginning();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        for (int i = 0; i < mCacheCreateOb.Count; i++)
        {
            if (mCacheCreateOb[i] == null)
                continue;

            // 析构创建的物件对象
            mCacheCreateOb[i].Destroy();
        }
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // 选装模型底座的光效
        RotateSelectEffect();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;

        if (gameObject.GetComponent<TweenScale>() != null)
            gameObject.GetComponent<TweenScale>().AddOnFinished(OnTweenFinished);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    /// <summary>
    /// tween动画播放完后回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// TweenAlpha结束的回调
    /// </summary>
    private void OnTweenFinished()
    {
        isTweenOver = true;

        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 遍历各个防守成员, 载入宠物模型
        for (int i = 0; i < defenceList.Count; i++)
        {
            LPCMapping dbase = LPCValue.Duplicate(defenceList[i]).AsMapping;

            dbase.Add("rid", Rid.New());

            // 创建一个宠物对象
            Property ob = PropertyMgr.CreateProperty(dbase);

            // 宠物对象不存在
            if (ob == null)
                continue;

            if (mCacheCreateOb.Contains(ob))
                mCacheCreateOb.Remove(ob);

            mCacheCreateOb.Add(ob);

            // 异步载入模型
            mModels[i].LoadModelSync(ob, LayerMask.NameToLayer("UI"), new CallBack(OnClickModel));
        }

        if (mCacheCreateOb == null || mCacheCreateOb.Count == 0)
        {
            mLeaderSkillDesc.text = LocalizationMgr.Get("SelectFighterWnd_11");
            return;
        }

        // 获取队长技能
        LPCMapping leaderSkill = SkillMgr.GetLeaderSkill(mCacheCreateOb[0]);
        if (leaderSkill == null || leaderSkill.Count == 0)
            mLeaderSkillDesc.text = LocalizationMgr.Get("SelectFighterWnd_11");
        else
            mLeaderSkillDesc.text = SkillMgr.GetLeaderSkillDesc(mCacheCreateOb[0]);
    }


    /// <summary>
    /// 点击模型回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    void OnClickModel(object para, params object[] _params)
    {
        GameObject modelOb = (GameObject)_params[0];

        if (modelOb == null)
            return;

        ModelWnd modelWnd = modelOb.transform.parent.GetComponent<ModelWnd>();

        if (modelWnd == null)
            return;

        Property ob = modelWnd.mPetOb;

        if (ob == null)
            return;

        GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType);

        // 窗口创建失败
        if (wnd == null)
            return;

        PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

        script.Bind(ob);
        script.ShowBtn(true);
    }

    /// <summary>
    /// 旋转光效
    /// </summary>
    void RotateSelectEffect()
    {
        foreach (GameObject item in mSelectEffect)
            item.transform.Rotate(Vector3.forward * Time.unscaledDeltaTime * 40);
    }

    /// <summary>
    /// 窗口关闭按钮
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    #endregion

    public void Bind(LPCArray defenceList, string userName)
    {
        // 绑定数据格式不正确
        if (defenceList == null || string.IsNullOrEmpty(userName))
            return;

        this.defenceList = defenceList;

        mTitle.text = string.Format(LocalizationMgr.Get("DefenceDeployViewWnd_2"), userName);

        // 绘制窗口
        if(isTweenOver)
            Redraw();
    }
}
