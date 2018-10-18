using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;

public class DefenceRecordWnd : MonoBehaviour
{
    #region 成员变量

    public GameObject mItem;

    // 列表格子对象;
    public GameObject mPanel;
    public GameObject mArenaWnd;

    /// <summary>
    /// The m sroll view.
    /// </summary>
    public UIScrollView mSrollView;

    // 格子限定数量
    int limitAmount = 10;

    // 副本id
    string mInstanceId = "arena_revenge";

    // 缓存格子列表
    List<GameObject> itemObList = new List<GameObject>();

    #endregion

    // Use this for initialization
    void Awake()
    {
        // 创建item格子
        CreateItem();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 注册事件
        RegisterEvent();

        // 整理位置
        mSrollView.ResetPosition();

        // 刷新界面
        Redraw();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册字段变化的事件
        ME.user.dbase.RegisterTriggerField("DefenceRecordWnd", new string[] { "revenge_data" }, new CallBack(OnRevengeDataChange));

        // 注册消息回调
        // 关注MSG_GET_ARENA_OPPONENT_DEFENSE_DATA消息
        MsgMgr.RegisterDoneHook("MSG_GET_ARENA_OPPONENT_DEFENSE_DATA", "DefenceRecordWnd", OnMsgGetArenaOpponentDefenseData);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDisable()
    {
        // 解注册时间
        if (ME.user != null)
            ME.user.dbase.RemoveTriggerField("DefenceRecordWnd");

        // 移除消息监听
        MsgMgr.RemoveDoneHook("MSG_GET_ARENA_OPPONENT_DEFENSE_DATA", "DefenceRecordWnd");
    }

    /// <summary>
    /// 请求对手防御信息,消息回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    void OnMsgGetArenaOpponentDefenseData(string cmd, LPCValue para)
    {
        // 获取防守信息
        LPCMapping args = para.AsMapping;
        if (args == null)
            return;

        LPCMapping defenseData = args.GetValue<LPCMapping>("opponent_data");
        if (defenseData == null)
            return;

        // 进入到选择战斗界面
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
        {
            LogMgr.Trace("SelectFighterWnd窗口创建失败");
            return;
        }

        // 窗口绑定对象
        wnd.GetComponent<SelectFighterWnd>().Bind(mArenaWnd.name, mInstanceId, defenseData);
    }


    // 创建格子
    void CreateItem()
    {
        if (mItem == null)
            return;

        mItem.SetActive(false);
        itemObList.Add(mItem);

        // 只需创建limit-1个格子，item能够复用
        for (int i = 0; i < limitAmount - 1; i++)
        {
            // 实例化列表格子
            GameObject clone = Instantiate(mItem);
            clone.transform.SetParent(mPanel.transform);
            clone.transform.localScale = Vector3.one;
            clone.transform.localPosition = new Vector3(mItem.transform.localPosition.x,
                mItem.transform.localPosition.y - (i + 1) * 117, 0);
            clone.name = "revenge_item_" + (i + 1);
            clone.SetActive(false);

            // 缓存格子对象
            itemObList.Add(clone);
        }
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    void Redraw()
    {
        LPCArray revengeData = ME.user.Query<LPCArray>("revenge_data");

        // 反击数据为空
        if (revengeData == null)
            return;

        for (int i = 0; i < itemObList.Count; i++)
        {
            // 隐藏多余的item
            if (i >= revengeData.Count)
            {
                itemObList[i].SetActive(false);
                continue;
            }

            itemObList[i].SetActive(true);
            itemObList[i].GetComponent<ArenaRevengeItemWnd>().Bind(revengeData[revengeData.Count - i - 1].AsMapping);
        }
    }

    /// <summary>
    /// 字段变化的回调
    /// </summary>
    void OnRevengeDataChange(object para, params object[] _params)
    {
        // 刷新界面
        Redraw();
    }
}
