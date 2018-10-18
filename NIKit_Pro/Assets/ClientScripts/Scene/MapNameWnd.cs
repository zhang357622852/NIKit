/// <summary>
/// MapNameWnd.cs
/// Created by zhaozy 2016/06/15
/// 地图名牌窗口处理脚本
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using LPC;

public class MapNameWnd : MonoBehaviour
{
    #region 变量

    // 地图遮挡
    public SpriteRenderer mMaskWnd;

    // 地图名牌窗口
    public GameObject mNameWnd;

    // 名牌图片
    public TextMeshPro mNameBtnWnd;

    // 窗口子控件
    public GameObject mTipWnd;

    // 窗口星级
    public List<SpriteRenderer> mStarWnd;

    // 窗口绑定地图id
    public int mMapId = 0;

    // 红点提示类型
    public int TipsType = 0;

    // alpha动画组件
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float mDuration = 2f;

    // 窗口点击回调
    public delegate void OnClickWndDelegate(GameObject ob,int mapId);

    // 地图星星图片资源
    private const string STAR_PATH = "Assets/Art/Scene/world/{0}.png";

    /// <summary>
    /// The m bounds size x.
    /// </summary>
    private float mBoundsSizeX = 0f;

    #endregion

    #region 属性

    // 点击窗口回调
    public OnClickWndDelegate OnClick { get; set; }

    // 点击敞口回调
    public int MapId
    {
        get
        {
            return mMapId;
        }
        set
        {
            mMapId = value;
        }
    }

    #endregion

    #region 私有接口

    /// <summary>
    /// Sets the window tween alpha.
    /// </summary>
    /// <param name="ob">Ob.</param>
    /// <param name="fromAlpha">From alpha.</param>
    /// <param name="toAlpha">To alpha.</param>
    /// <param name="duration">Duration.</param>
    private void SetWndTweenAlpha(GameObject ob, float fromAlpha, float toAlpha, float duration, bool isCallBack)
    {
        // 获取alpha组件
        TweenAlpha maskTweenAlpha = ob.GetComponent<TweenAlpha>();
        if (maskTweenAlpha == null)
            ob.AddComponent<TweenAlpha>();

        // 获取动画组件
        maskTweenAlpha = ob.GetComponent<TweenAlpha>();
        if (maskTweenAlpha == null)
            return;

        // 重置动画
        maskTweenAlpha.from = fromAlpha;
        maskTweenAlpha.to = toAlpha;
        maskTweenAlpha.duration = duration;
        maskTweenAlpha.animationCurve = alphaCurve;
        maskTweenAlpha.enabled = true;
        maskTweenAlpha.ResetToBeginning();

        // 添加动画完成回调
        if (isCallBack)
            maskTweenAlpha.AddOnFinished(OnFinished);
    }

    /// <summary>
    /// TweenAlpha结束的回调
    /// </summary>
    private void OnFinished()
    {
        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// Dos the unlock map.
    /// </summary>
    private void DoUnlockTween()
    {
        // 设置名牌渐显（由于使用3D字效果，如果需要渐变效果需要特殊处理）
        // 实际上应该通过TextMeshPro的color(vertex)属性设置
        // SetWndTweenAlpha(mNameBtnWnd.gameObject, 0f, 1f, mDuration, false);

        // 设置星级渐显
        for (int i = 0; i < mStarWnd.Count; i++)
        {
            if (mStarWnd[i] == null)
                continue;

            SetWndTweenAlpha(mStarWnd[i].gameObject, 0f, 1f, mDuration, false);
        }

        // 设置遮盖渐隐
        SetWndTweenAlpha(mMaskWnd.gameObject, 1f, 0f, mDuration, true);
    }

    /// <summary>
    /// Redraw this instance.
    /// </summary>
    private void Redraw()
    {
        // 地图未解锁
        if (! MapMgr.IsUnlocked(ME.user, MapId))
        {
            // 显示云层遮罩
            mMaskWnd.gameObject.SetActive(true);

            // 隐藏名牌窗口
            mNameWnd.gameObject.SetActive(false);

            // 返回
            return;
        }

        // 显示云层遮罩
        mMaskWnd.gameObject.SetActive(false);

        // 隐藏名牌窗口
        mNameWnd.gameObject.SetActive(true);

        // 设置是否显示新地图开启提示
        RefreshRedTip();

        // 显示星级
        int star = MapMgr.GetMapUnlockStar(ME.user, MapId);

        // 填充各个星级图标
        for (int i = 0; i < mStarWnd.Count; i++)
        {
            if (mStarWnd[i] == null)
                continue;

            mStarWnd[i].sprite = ResourceMgr.LoadSprite(string.Format(STAR_PATH, (i >= star) ? "star_bg" : "star"));
        }
    }

    /// <summary>
    /// 刷新红点提示
    /// </summary>
    void RefreshRedTip()
    {
        // 获取地图信息
        CsvRow data = MapMgr.GetMapConfig(MapId);

        // 没有该地图的配置信息
        if (data == null)
        {
            mTipWnd.SetActive(false);
            return;
        }

        // 如果不是竞技场地图
        if (data.Query<int>("map_type") != MapConst.ARENA_MAP)
        {
            mTipWnd.SetActive(MapMgr.IsNewMap(MapId));
            return;
        }

        // 如果是审核模式不需要显示反击列表提示小红点
        if (ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            mTipWnd.SetActive(false);
            return;
        }

        LPCValue revengeData = ME.user.Query<LPCValue>("revenge_data");
        if (revengeData == null || !revengeData.IsArray)
        {
            mTipWnd.SetActive(false);
            return;
        }

        LPCArray revenges = revengeData.AsArray;

        // 累计未挑战过的反击数量
        int amount = 0;

        for (int i = 0; i < revenges.Count; i++)
        {
            if (revenges[i] == null || !revenges[i].IsMapping)
                continue;

            if (revenges[i].AsMapping.GetValue<int>("revenged") > ArenaConst.ARENA_REVENGE_TYPE_NONE)
                continue;

            amount++;
        }

        if (amount < 1)
        {
            mTipWnd.SetActive(false);
        }
        else
        {
            mTipWnd.SetActive(true);
        }
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    void InitText()
    {
        CsvRow row = MapMgr.GetMapConfig(mMapId);
        if (row == null)
            return;

        mNameBtnWnd.text = LocalizationMgr.Get(row.Query<string>("name"));
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        switch (TipsType)
        {
            // 竞技场红点提示处理
            case RedTipsConst.ARENA_TIPS_TYPE:

                // 关注MSG_DO_ONE_GUIDE消息
                MsgMgr.RegisterDoneHook("MSG_DO_ONE_GUIDE", "MapNameWnd", OnMsgDoOneGuide);

                // 关注玩家角色属性字段变化
                ME.user.dbase.RegisterTriggerField("MapNameWnd",
                    new string[] { "revenge_data" }, new CallBack(OnRevengeDataChange));

                break;

            // 不处理
            default:
                break;
        }
    }

    /// <summary>
    /// MSG_DO_ONE_GUIDE消息回调
    /// </summary>
    void OnMsgDoOneGuide(string cmd, LPCValue para)
    {
        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// revenge_data字段变化回调
    /// </summary>
    void OnRevengeDataChange(object para, params object[] param)
    {
        // 设置是否显示新地图开启提示
        RefreshRedTip();
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        switch (TipsType)
        {
            // 竞技场红点提示处理
            case RedTipsConst.ARENA_TIPS_TYPE:

                // 移除消息关注
                MsgMgr.RemoveDoneHook("MSG_DO_ONE_GUIDE", "MapNameWnd");

                // 取消字段监听
                if (ME.user != null)
                    ME.user.dbase.RemoveTriggerField("MapNameWnd");

                break;

                // 不处理
            default:
                break;
        }
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
    {
        // 调整窗口位置
        AdjustTipWndPosition();
    }

    /// <summary>
    /// Adjusts the tip window position.
    /// </summary>
    void AdjustTipWndPosition()
    {
        // 控件没有显示，不需要处理
        if (! mTipWnd.activeSelf)
            return;

        // mNameBtnWnd窗口大小
        Bounds bounds = mNameBtnWnd.bounds;

        // 如果bounds.size没有发生变化
        if (Game.FloatEqual(bounds.size.sqrMagnitude, mBoundsSizeX))
            return;

        // 记录mBoundsSizeX
        mBoundsSizeX = bounds.size.sqrMagnitude;

        // 设置红点位置
        mTipWnd.transform.localPosition = new Vector3(
            mNameBtnWnd.transform.localPosition.x + bounds.size.x - 0.25f,
            mNameBtnWnd.transform.localPosition.y,
            mNameBtnWnd.transform.localPosition.z
        );
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// Start this instance.
    /// </summary>
    public void Start()
    {
        // 注册事件
        RegisterEvent();

        // 重绘窗口
        Redraw();

        // 初始化文本
        InitText();
    }

    /// <summary>
    /// OnEnable
    /// </summary>
    public void OnEnable()
    {
        // 玩家对象不存在，不处理
        if (ME.user == null)
            return;

        // 解锁标识
        if (ME.user.QueryTemp<int>("unlock_map") == MapId)
        {
            // 删除数据
            ME.user.DeleteTemp("unlock_map");

            // 播放解锁动画
            DoUnlockTween();

            // 返回
            return;
        }

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// Raises the click window event.
    /// </summary>
    public void OnClickWnd()
    {
        // 地图未解锁
        if (!MapMgr.IsUnlocked(ME.user, MapId))
            return;

        List<object> para = new List<object>();
        para.Add(mMapId);
        para.Add(transform);

        // 抛出窗口点击事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLICK_MAP_WND, MixedValue.NewMixedValue<List<object>>(para));

        // 通知音效播放点击音效
        GameSoundMgr.OnUIClicked();

        // 移除新开地图缓存
        MapMgr.RemoveNewMap(ME.user, MapId);

        mTipWnd.SetActive(false);

        // 没有注册点击回调
        if (OnClick == null)
            return;

        // 执行点击回调
        OnClick(gameObject, mMapId);
    }

    #endregion
}
