using UnityEngine;
using System.Collections;
using LPC;

public class PetValueDebugWnd  : WindowBase<PetValueDebugWnd>
{

    public UILabel mDesc;

    // 宠物的Rid
    private string mRid = string.Empty;

    // 宠物对象
    private Char target;

    bool mIsBoss = false;

    // 原始位置
    private Vector3 lastUiPos = Vector3.one;

    // Use this for initialization
    void Start ()
    {
    }

    void OnDestroy()
    {
        // 注销事件
        EventMgr.UnregisterEvent(string.Format("pet_value_debug_wnd_value_{0}", mRid));

        // 取消角色的属性字段变化回调
        if (target != null)
        {
            // Remove血条变化Trigger
            target.dbase.RemoveTriggerField("pet_value_debug_wnd_value");
            target.tempdbase.RemoveTriggerField("pet_value_debug_wnd_value");
        }
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
    {
        // 调整窗口位置
        AdjustWnd();
    }

    /// <summary>
    /// 调整窗口位置
    /// </summary>
    private void AdjustWnd()
    {
        // 角色对象不存在
        if (target == null ||
            target.IsDestroyed ||
            target.Actor == null)
            return;

        // 计算当前位置
        Vector3 actorPos = target.Actor.GetPosition();

        Vector3 curPos = new Vector3(actorPos.x, actorPos.y + (mIsBoss ? 0.75f : 1f)*target.Actor.GetHpbarOffestY(), actorPos.z);
        Vector3 curUiPos = Game.WorldToUI(curPos);

        // 判断位置是否需要变化
        if (Game.FloatEqual((lastUiPos - curUiPos).sqrMagnitude, 0f))
            return;

        // 设置位置
        transform.position = curUiPos;
        lastUiPos = curUiPos;
    }

    /// <summary>
    /// 角色死亡事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void WhenCharDie(int eventId, MixedValue para)
    {
        // 获取参数获取targetOb
        Property ob = para.GetValue<Property>();
        if (ob == null)
            return;

        // 是否为同一个宠物
        if (! mRid.Equals(ob.GetRid()))
            return;

        // 隐藏血条
        WindowMgr.HideWindow(gameObject);
    }

    /// <summary>
    /// 战斗结束回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void WhenRoundCombatEnd(int eventId, MixedValue para)
    {
        WindowMgr.HideWindow(gameObject);
    }

    ///<summary>
    /// 宠物生命值变化
    /// </summary>
    private void WhenValueChange(object param, params object[] paramEx)
    {
        Redraw();
    }

    /// <summary>
    /// 清除附加状态事件回调    
    /// </summary>
    void WhenStatusClear(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 目标相同
        if (args.ContainsKey("rid") && args["rid"].IsString &&
            string.Equals(target.GetRid(), args["rid"].AsString))
        {
            // 判断角色是否是清除死亡状态
            LPCArray statusList = args.GetValue<LPCArray>("status_list");

            // 如果是清除死亡状态, 需要显示血条
            if (statusList.IndexOf(StatusMgr.GetStatusIndex("DIED")) != -1)
                target.Actor.ShowValueWnd(!target.CheckStatus("DIED"));
        }
    }


    // 设置调试信息
    private void Redraw()
    {
        if(target == null)
            return;

        mDesc.text = string.Format("HP: [F32727FF]{0}/{1}[-]\n[F3EE27FF]attack: {2}[-]\n[F327EEFF]defen: {3}[-]\n[27F3DAFF]agility: {4}[-]\n[F327E5FF]speed: {5}[-]\n[F327E5FF]resist: {6}[-]\n[F327E5FF]accuracy: {7}[-]\n[F327E5FF]crt: {8}[-]\n",
            target.Query<int>("hp"), target.QueryAttrib("max_hp"), 
            target.QueryAttrib("attack"), 
            target.QueryAttrib("defense"), 
            target.QueryAttrib("agility"), 
            target.QueryAttrib("speed"),
            target.QueryAttrib("resist_rate"),
            target.QueryAttrib("accuracy_rate"),
            target.QueryAttrib("crt_rate"));

    }

    #region 外部接口

    /// <summary>
    /// 设置当前宠物对象
    /// </summary>
    public void SetBind(string rid)
    {
        // 窗口绑定rid
        mRid = rid;

        // 获取绑定对象
        target = Rid.FindObjectByRid(mRid) as Char;
        string listenerId = string.Format("pet_value_debug_wnd_value_{0}", mRid);

        // 注册玩家死亡事件
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_DIE, WhenCharDie);

        // 注册战斗结束
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_ROUND_COMBAT_END, WhenRoundCombatEnd);

        // 注册清除状态
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_CLEAR_STATUS, WhenStatusClear);

        // 关注角色属性字段变化
        if (target != null)
        {
            // 监听属性变化回调
            target.dbase.RegisterTriggerField("pet_value_debug_wnd_value", new string[]
                {
                    "hp",
                    "max_hp",
                    "attack",
                    "defense",
                    "agility",
                    "speed",
                    "resist_rate",
                    "accuracy_rate",
                    "crt_rate",
                }, new CallBack(WhenValueChange));

            // 监听属性变化回调
            target.tempdbase.RegisterTriggerField("pet_value_debug_wnd_value", new string[]
                {
                    "improvement"
                }, new CallBack(WhenValueChange));
        }

        // 重绘窗口
        Redraw();
    }


    /// <summary>
    /// 显示血条
    /// </summary>
    public void ShowWnd(bool isBoss)
    {
        // 获取角色的世界缩放
        if (target == null)
            return;

        mIsBoss = isBoss;

        // 获取角色的世界缩放
        float worldScale = target.GetWorldScale();
        transform.localScale = new Vector3(worldScale, worldScale, worldScale);
    }
        
    /// <summary>
    /// 隐藏血条
    /// </summary>
    public void HideWnd()
    {
        // 隐藏血条窗口
        WindowMgr.HideWindow(gameObject);
    }

    #endregion
}
