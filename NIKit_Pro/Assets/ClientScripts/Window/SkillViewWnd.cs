/// <summary>
/// SkillViewWnd.cs
/// Created by lic 7/7/2016
/// 技能简介悬浮窗口
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class SkillViewWnd : WindowBase<SkillViewWnd>
{

    public UILabel mName;
    public UILabel mDesc;
    public UILabel mCD;
    public UISprite mBg;

    public GameObject mMp;
    public GameObject[] mMpGroup;

    #region 属性

    private int mSkillID = -1;

    private Property mPet = null;

    private GameObject mWnd;

    #endregion

    #region 内部函数

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {

    }

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();
    }

    void OnDestroy()
    {
        mWnd = null;

        // 暂停携程
        Coroutine.StopCoroutine("SyncCameraRemove");
        Coroutine.StopCoroutine("SyncLimitPosInScreen");
    }

    void OnEnable()
    {
        gameObject.GetComponent<UIPanel>().alpha = 0f;
        gameObject.GetComponent<TweenAlpha>().enabled = false;
    }

    /// <summary>
    /// 初始化界面
    /// </summary>
    void InitWnd()
    {
        if (mCD != null)
            mCD.gameObject.SetActive(false);
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw(bool isShow, bool isTween, bool isBase = false)
    {
        if (isShow)
        {
            gameObject.GetComponent<TweenAlpha>().enabled = false;

            // 取得技能等级
            int level = 0;

            if (isBase)
                level = 1;
            else
                level = mPet.GetSkillLevel(mSkillID);

            mDesc.text = SkillMgr.GetBaseEffectDesc(mSkillID, level);

            mName.text = SkillMgr.GetSkillName(mSkillID).ToString() + " " + level.ToString() + LocalizationMgr.Get("SkillViewWnd_2");

            // 冷却时间
            if (mCD != null)
            {
                if (CdMgr.GetSkillCd(mPet, mSkillID) > 0)
                {
                    mCD.gameObject.SetActive(true);
                    mCD.text = string.Format(LocalizationMgr.Get("SkillViewWnd_1"), CdMgr.GetSkillCd(mPet, mSkillID));
                }
                else
                    mCD.gameObject.SetActive(false);
            }

            // 取得蓝耗的值
            LPCMapping mpMap = SkillMgr.GetCasTCost(mPet, mSkillID);

            int mp = mpMap.ContainsKey("mp") ? mpMap.GetValue<int>("mp") : 0;

            if (mp < 0)
                mp = 0;

            if (mp > mMpGroup.Length)
                mp = mMpGroup.Length;

            for (int i = 0; i < mMpGroup.Length; i++)
            {
                if (i < mp)
                    mMpGroup[i].SetActive(true);
                else
                    mMpGroup[i].SetActive(false);
            }

            // 等待一帧
            Coroutine.StopCoroutine("SyncCameraRemove");
            Coroutine.DispatchService(SyncCameraRemove(), "SyncCameraRemove");
        }
        else
            gameObject.GetComponent<UIPanel>().alpha = 0f;


        if (isTween)
        {
            gameObject.GetComponent<TweenAlpha>().enabled = true;
            gameObject.GetComponent<TweenAlpha>().ResetToBeginning();
        }
    }

    /// <summary>
    /// 等待一帧
    /// </summary>
    /// <returns>The camera remove.</returns>
    private IEnumerator SyncCameraRemove()
    {
        yield return null;

        if (mWnd == null)
            yield break;

        UIPanel panel = mWnd.GetComponent<UIPanel>();

        if (panel == null)
            yield break;

        panel.alpha = 1.0f;
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 显示悬浮
    /// </summary>
    /// <param name="skillID">Skill I.</param>
    /// <param name="isTween">If set to <c>true</c> is tween.</param>
    public void ShowView(int skillID, Property target, bool isBase = false, bool isTween = false)
    {
        mPet = target;
        mSkillID = skillID;

        mWnd = this.gameObject;

        Redraw(true, isTween, isBase);
    }

    /// <summary>
    /// 限制悬浮的位置在屏幕范围内
    /// </summary>
    public void LimitPosInScreen()
    {
        Coroutine.StopCoroutine("SyncLimitPosInScreen");
        Coroutine.DispatchService(SyncLimitPosInScreen(), "SyncLimitPosInScreen");
    }

    /// <summary>
    /// 延时一帧限制技能描述框的位置，因为当前mBg显示体(UILabe)的size还未更新
    /// </summary>
    IEnumerator SyncLimitPosInScreen()
    {
        yield return null;

        if (this == null)
            yield break;

        // 限制悬浮在屏幕内
        if (mBg == null)
            yield break;

        float halfItemWidth = mBg.width / 2f;

        // x、y轴偏移
        float offset = 10f;

        // UI根节点
        Transform uiRoot = WindowMgr.UIRoot;

        if (uiRoot == null)
            yield break;

        UIPanel panel = uiRoot.GetComponent<UIPanel>();

        if (panel == null)
            yield break;

        // UI根节点panel四角的坐标
        Vector3[] pos = panel.localCorners;

        // 将mSkillViewWnd世界坐标转换成相对于UIRoot的本地坐标
        Vector3 transPoint = uiRoot.InverseTransformPoint(transform.position);

        // 限制transPoint在屏幕范围内
        transPoint.x = Mathf.Clamp(transPoint.x, pos[0].x + halfItemWidth + offset, pos[2].x - halfItemWidth - offset);

        transPoint.y = Mathf.Clamp(transPoint.y, pos[0].y + offset, pos[2].y - mBg.height - offset);

        // 将transPoint转换成世界坐标
        transform.position = uiRoot.TransformPoint(transPoint);
    }

    /// <summary>
    /// 隐藏悬浮
    /// </summary>
    /// <param name="isTween">If set to <c>true</c> is tween.</param>
    public void HideView(bool isTween = false)
    {
        Redraw(false, isTween);
    }

    #endregion
}

