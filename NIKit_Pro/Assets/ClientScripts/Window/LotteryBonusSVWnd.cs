/// <summary>
/// LotteryBonusSVWnd.cs
/// Created by zhangwm 2018/08/02
/// 许愿池奖励物品
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class LotteryBonusSVWnd : WindowBase<LotteryBonusSVWnd>
{
    #region 成员变量
    public UIGrid mItemGrid;
    public GameObject mBonusItemPrefab;

    public List<TrackInfo> mTrackInfoList = new List<TrackInfo>();

    //至少预创建item个数,如果配置的奖品数目大于这个的话，就为奖励数目的奇数
    //(这个数目前必须是奇数，因为父节点Grid锚点是中间，为了保证物品都会在发光球的镂空位置处)
    private int mMaxPreBonusNum = 15;

    //走一步的距离
    private float mFixedStepDis;

    //抽中的物品BonusId
    private int mCurTargetBonusId;

    //抽中的物品的index序号
    private int mCurTargetBonusIndex;

    //当前index
    private int mCurBonusIndex;

    //缓存记录创建item
    private List<BonusItemWnd> mCacheItemList = new List<BonusItemWnd>();

    //服务端发过来奖励列表（末尾有可能插入重复数据）
    private LPCArray mBonusInfoArr = LPCArray.Empty;

    //随机动画的index
    private int mRandomIndex = 0;

    // 记录服务端奖励信息
    private LPCArray mServerBonus = LPCArray.Empty;

    private bool mIsLottering = false;

    private bool mIsResetBonus = false;
    #endregion

    [System.Serializable]
    public class TrackInfo
    {
        [Header("固定至少需要走的步数")]
        public int mFixedSteps = 40;
        [Header("动画的总时间")]
        public float mDuration = 13f;

        public AnimationCurve mAnimCurve;
    }

    private void Start()
    {
        RecordServerBonus();

        Init();

        Redraw();

        RegisterEvent();
    }

    private void OnDestroy()
    {
        MsgMgr.RemoveDoneHook("MSG_LOTTERY_BONUS", "LotteryBonusSVWnd");

        Coroutine.StopCoroutine("PlayAni");

        // 取消属性字段关注变化
        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("LotteryBonusSVWnd");
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        if (ME.user == null)
            return;

        LPCArray bonusInfoArr = ME.user.Query<LPCArray>("lottery_bonus/lottery_list");

        for (int i = 0; i < bonusInfoArr.Count; i++)
        {
            mBonusInfoArr.Add(bonusInfoArr[i]);
        }

        if (mBonusInfoArr == null || mBonusInfoArr.Count <= 0)
            return;

        //使mBonusInfoArr的数据数量等于创建预制体数目(mBonusInfoArr的数据有可能重复,从末尾补数据)-为了便于后面的计算
        int spanValue;

        if (mBonusInfoArr.Count > mMaxPreBonusNum)
        {
            //算出需要创建的预制体数目mAtLeastPreBonusNum
            if (mBonusInfoArr.Count % 2 == 0)
                mMaxPreBonusNum = mBonusInfoArr.Count + 1;
            else
                mMaxPreBonusNum = mBonusInfoArr.Count;

            spanValue = mBonusInfoArr.Count - mMaxPreBonusNum;
        }
        else
            spanValue = mMaxPreBonusNum - mBonusInfoArr.Count;

        //数组尾巴补数据，spanValue是0的话，就无需补数据
        for (int i = 0; i < spanValue; i++)
            mBonusInfoArr.Add(LPCValue.Create(mBonusInfoArr[i].AsInt));

        //创建没有带数据的空预制体
        NGUITools.DestroyChildren(mItemGrid.transform);
        mCacheItemList.Clear();

        mBonusItemPrefab.SetActive(true);

        for (int i = 0; i < mMaxPreBonusNum; i++)
        {
            GameObject go = NGUITools.AddChild(mItemGrid.gameObject, mBonusItemPrefab);
            go.name = go.name + i;
            mCacheItemList.Add(go.GetComponent<BonusItemWnd>());
        }

        mBonusItemPrefab.SetActive(false);
        mItemGrid.Reposition();

        //获取走一步的距离
        mFixedStepDis = mItemGrid.cellWidth;

        //获取初始index
        mCurBonusIndex = (mMaxPreBonusNum - 1) / 2;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 抽奖成功服务端通知事件
        MsgMgr.RegisterDoneHook("MSG_LOTTERY_BONUS", "LotteryBonusSVWnd", OnMsgLotteryBonus);

        if (ME.user == null)
            return;

        ME.user.dbase.RegisterTriggerField("LotteryBonusSVWnd", new string[]
         {
            "lottery_bonus",
         }, new CallBack(OnBonusListChanged));
    }

    /// <summary>
    /// 奖励重置刷新事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    void OnBonusListChanged(object param, params object[] paramEx)
    {
        if (IsResetBonus())
        {
            mIsResetBonus = true;

            if (mIsLottering)
                return;

            ResetRedrawBonus();
        }
    }

    /// <summary>
    /// 记录服务端奖励信息
    /// </summary>
    private void RecordServerBonus()
    {
        if (ME.user == null)
            return;

        mServerBonus = ME.user.Query<LPCArray>("lottery_bonus/lottery_list");
    }

    /// <summary>
    /// 重置奖励
    /// 零点时，会重置奖励
    /// </summary>
    private void ResetRedrawBonus()
    {
        Init();

        Redraw();

        RecordServerBonus();
    }

    /// <summary>
    /// 是否改变奖励
    /// </summary>
    /// <returns></returns>
    private bool IsResetBonus()
    {
        if (mServerBonus == null || mServerBonus.Count <= 0)
            return false;

        LPCArray bonusInfoArr = ME.user.Query<LPCArray>("lottery_bonus/lottery_list");

        if (bonusInfoArr == null || bonusInfoArr.Count <= 0)
            return false;

        if (bonusInfoArr.Count != mServerBonus.Count)
            return true;

        for (int i = 0; i < mServerBonus.Count; i++)
        {
            if (mServerBonus[i].AsInt != bonusInfoArr[i].AsInt)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        if (mBonusInfoArr.Count != mCacheItemList.Count)
            return;

        //填数据
        for (int i = 0; i < mCacheItemList.Count; i++)
            mCacheItemList[i].BindData(mBonusInfoArr[i].AsInt);
    }

    /// <summary>
    /// 抽奖消息监听回调
    /// </summary>
    void OnMsgLotteryBonus(string cmd, LPCValue para)
    {
        LPCMapping args = para.AsMapping;
        if (args == null || mTrackInfoList.Count <= 0)
            return;

        mCurTargetBonusId = args.GetValue<int>("lottery_id");
        mRandomIndex = UnityEngine.Random.Range(0, mTrackInfoList.Count);

        mIsLottering = true;

        Coroutine.DispatchService(PlayAni(), "PlayAni");
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    private IEnumerator PlayAni()
    {
        float totalDis = GetTotalSteps() * mFixedStepDis;
        float lastDis = 0f;
        float curTime = 0f;
        float s;

        while (true)
        {
            curTime += Time.fixedDeltaTime;
            s = GetNextMoveDis(curTime, totalDis);

            MoveNextStep(s - lastDis);
            lastDis = s;

            if (s >= totalDis)
            {
                mCurBonusIndex = mCurTargetBonusIndex;

                EventMgr.FireEvent(EventMgrEventType.EVENT_LOTTERY_BONUS_ANI_DONE, null);

                mIsLottering = false;

                yield break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 获取当前时间的位移
    /// </summary>
    /// <param name="curValue"></param>
    /// <param name="changeValue"></param>
    /// <returns></returns>
    private float GetNextMoveDis(float curValue, float totalDis)
    {
        float x = Mathf.Clamp01(curValue / mTrackInfoList[mRandomIndex].mDuration);
        float y = Mathf.Clamp01(mTrackInfoList[mRandomIndex].mAnimCurve.Evaluate(x));

        return y * totalDis;
    }

    /// <summary>
    /// 全部item向前走一步
    /// </summary>
    private void MoveNextStep(float moveDis)
    {
        BonusItemWnd curItem;
        BonusItemWnd rearItem;
        Vector3 lastPos;
        Vector3 oriLocalPos;
        Vector3 rearLocalPos;
        Vector3 screenPos;
        int rearIndex;

        for (int i = 0; i < mCacheItemList.Count; i++)
        {
            curItem = mCacheItemList[i];
            oriLocalPos = curItem.transform.localPosition;
            lastPos = oriLocalPos;

            //先向前移动
            oriLocalPos = new Vector3(oriLocalPos.x - moveDis, oriLocalPos.y, oriLocalPos.z);

            //是否整个超出屏幕，如果是的话，就扔到队尾去。
            screenPos = SceneMgr.UiCamera.WorldToScreenPoint(curItem.transform.position);

            if (screenPos.x <= -mFixedStepDis)
            {
                if (i - 1 < 0)
                    rearIndex = mCacheItemList.Count - 1;
                else
                    rearIndex = i - 1;

                rearItem = mCacheItemList[rearIndex];
                rearLocalPos = rearItem.transform.localPosition;

                //如果前一个是mCacheItemList的最后一个，要多走一步
                if (i - 1 < 0)
                    oriLocalPos = new Vector3(rearLocalPos.x - moveDis + mFixedStepDis, rearLocalPos.y, rearLocalPos.z);
                else
                    oriLocalPos = new Vector3(rearLocalPos.x + mFixedStepDis, rearLocalPos.y, rearLocalPos.z);
            }

            curItem.transform.localPosition = oriLocalPos;

            // 播放lottery转盘跳动音效，必须保证单个音效完整播放
            if (lastPos.x > 1 && oriLocalPos.x <= 1)
                GameSoundMgr.PlayGroupSound("lottery");
        }

    }

    /// <summary>
    /// 获取从当前位置走到抽中的奖励物品处的总步数
    /// 有算上打底步数
    /// </summary>
    /// <returns></returns>
    private int GetTotalSteps()
    {
        int index = (mCurBonusIndex + mTrackInfoList[mRandomIndex].mFixedSteps) % mMaxPreBonusNum;
        int totalSteps = mTrackInfoList[mRandomIndex].mFixedSteps;

        while (true)
        {
            index = (index + 1) % mMaxPreBonusNum;
            totalSteps++;

            if (index >= mBonusInfoArr.Count)
            {
                totalSteps = -1;
                break;
            }
            else
            {
                if (mBonusInfoArr[index].AsInt.Equals(mCurTargetBonusId))
                {
                    mCurTargetBonusIndex = index;
                    break;
                }
            }
        }

        return totalSteps;
    }

    /// <summary>
    /// 刷新奖励
    /// </summary>
    public void RefreshResetBonus()
    {
        if (!mIsResetBonus)
            return;

        ResetRedrawBonus();

        mIsResetBonus = false;
    }
}
