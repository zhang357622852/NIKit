/// <summary>
/// MailWnd.cs
/// Created by fengsc 2016/11/03
/// 邮件主界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class MailWnd : WindowBase<MailWnd>
{
    #region 成员变量

    // 系统邮件查看按钮
    public UIToggle mSystemMailBtn;
    public UILabel mSystemMailLb;

    // 友情点数查看按钮
    public UIToggle mFriendPointBtn;
    public UILabel mFriendPointLb;

    // 邮件红点提示
    public GameObject mMailRedPoint;

    // 邮件数量
    public UILabel mMailAmount;

    // 友情点数红点提示
    public GameObject mFriendRedPoint;

    // 友情点数条数
    public UILabel mFriendPointAmount;

    // 邮件基础格子
    public GameObject mMailItem;

    // 友情点数基础格子
    public GameObject mFriendPointItem;

    // 邮件排序按钮
    public UIToggle mSortBtn;
    public UILabel mSortLb;

    // 全部收取按钮
    public GameObject mAllGetBtn;
    public UILabel mAllGetLb;

    // 邮件有效提示
    public UILabel mValidTips;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 邮件标题
    public UILabel mMailTitle;

    // 邮件有效期剩余时间
    public UILabel mRemainTime;

    public GameObject mContent;

    // 邮件内容
    public UILabel mMailContent;

    // 查看详情按钮
    public UILabel mViewBtn;

    // 邮件附件基础格子
    public GameObject mItem;

    // 收取按钮
    public GameObject mGetBtn;
    public UILabel mGetLb;

    public UIWrapContent mSystemWrapContent;

    public UIWrapContent mUserWrapContent;

    public UIPanel mSystemPanel;

    public UIPanel mUserPanel;

    public GameObject mItemGrid;

    // 选择提示
    public UILabel mSelectTips;

    public GameObject mFriendMailWnd;

    public GameObject mSystemMailWnd;

    public GameObject mFriendBox;

    public GameObject mSystemBox;

    // 缓存选中的附件信息
    LPCArray mSelect = new LPCArray();

    // 选中的物品对象
    List<SignItemWnd> mSelectObject = new List<SignItemWnd>();

    // 系统邮件列表格子实体对象列表
    Dictionary<string, GameObject> mMailObjectList = new Dictionary<string, GameObject>();

    // 系统邮件列表
    List<LPCMapping> mSystemMailList = new List<LPCMapping>();

    // 用户邮件系统列表
    List<LPCMapping> mUserMailList = new List<LPCMapping>();

    Dictionary<int, int> mSystemIndexMap = new Dictionary<int, int>();

    Dictionary<int, int> mUserIndexMap = new Dictionary<int, int>();

    // 默认创建7个格子
    int mItemAmount = 7;

    // 选中的邮件rid
    string mSelectRid = string.Empty;

    // 是否全部领取
    bool mIsAllRecieve = true;

    // 可领取的附件数量
    int mReceiveAmount = 0;

    GameObject mSystemDefault;

    GameObject mUserDefault;

    GameObject mWnd;

    #endregion

    // Use this for initialization
    void Start()
    {
        mWnd = this.gameObject;

        // 初始化本地化文本
        InitLocalText();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();

        // 请求邮件列表
        MailMgr.RequestGetExpressList();
    }

    void OnDestroy()
    {
        // 移除消息关注
        MsgMgr.RemoveDoneHook("MSG_EXPRESS_OPERATE_DONE", "MailWnd");
        MsgMgr.RemoveDoneHook("MSG_NOTIFY_NEW_MAIL", "MailWnd");
        MsgMgr.RemoveDoneHook("MSG_NOTIFY_PACKS_BONUS", "MailWnd");

        // 解注册事件
        EventMgr.UnregisterEvent("MailWnd_MailTake");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mSystemMailLb.text = LocalizationMgr.Get("MailWnd_1");
        mFriendPointLb.text = LocalizationMgr.Get("MailWnd_2");
        mSortLb.text = LocalizationMgr.Get("MailWnd_4");
        mAllGetLb.text = LocalizationMgr.Get("MailWnd_5");
        mValidTips.text = LocalizationMgr.Get("MailWnd_6");
        mViewBtn.text = LocalizationMgr.Get("MailWnd_7");
        mGetLb.text = LocalizationMgr.Get("MailWnd_8");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mSystemMailBtn.gameObject).onClick = OnClickSystemBtn;

        UIEventListener.Get(mFriendPointBtn.gameObject).onClick = OnClickFriendBtn;
        UIEventListener.Get(mGetBtn).onClick = OnClickGetBtn;

        UIEventListener.Get(mSortBtn.gameObject).onClick = OnClickSortBtn;
        UIEventListener.Get(mAllGetBtn).onClick = OnClickAllGetBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mViewBtn.gameObject).onClick = OnClickViewBtn;

        UIEventListener.Get(mMailContent.gameObject).onClick = OnClickGotoDiscuss;

        // 注册格子复用事件
        mSystemWrapContent.onInitializeItem = SystemUpdateItem;

        mUserWrapContent.onInitializeItem = UserUpdateItem;

        MsgMgr.RegisterDoneHook("MSG_NOTIFY_PACKS_BONUS", "MailWnd", OnMsgNotifyPacksBonus);

        //关注MSG_EXPRESS_OPERATE_DONE消息
        MsgMgr.RegisterDoneHook("MSG_EXPRESS_OPERATE_DONE", "MailWnd", OnMsgExpressOperateDone);
        MsgMgr.RegisterDoneHook("MSG_NOTIFY_NEW_MAIL", "MailWnd", OnMsgNotifyNewMail);
    }

    /// <summary>
    /// 邮件操作完成消息回调
    /// </summary>
    void OnMsgExpressOperateDone(string cmd, LPCValue para)
    {
        mSelectObject.Clear();

        LPCMapping data = para.AsMapping;

        string oper = data.GetValue<string>("oper");

        switch (oper)
        {
            case "read":
                if (mSystemMailBtn.value)
                {
                    foreach (KeyValuePair<int, int> item in mSystemIndexMap)
                    {
                        // 填充数据
                        FillData(item.Key, item.Value, true);
                    }
                }
                else
                {
                    foreach (KeyValuePair<int, int> item in mUserIndexMap)
                    {
                        // 填充数据
                        FillData(item.Key, item.Value, false);
                    }
                }
                return;

            case "get_list":
                // 刷新邮件列表数据
                InitMailData();
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 新邮件统计回调消息
    /// </summary>
    void OnMsgNotifyNewMail(string cmd, LPCValue para)
    {
        // 请求邮件列表
        MailMgr.RequestGetExpressList();
    }

    /// <summary>
    /// 礼包奖励消息
    /// </summary>
    void OnMsgNotifyPacksBonus(string cmd, LPCValue para)
    {
        LPCArray args = para.AsMapping.GetValue<LPCArray>("bonus_data");

        // 奖励列表为空
        if (args == null || args.Count == 0)
            return;

        for (int i = 0; i < args.Count; i++)
        {
            // 道具或宠物
            if (args[i].AsMapping.ContainsKey("rid"))
            {
                Property ob = Rid.FindObjectByRid(args[i].AsMapping.GetValue<string>("rid"));

                if (ob == null)
                    continue;

                if (MonsterMgr.IsMonster(ob))
                {
                    // 构造参数
                    Dictionary<string, object> needPara = new Dictionary<string, object>()
                    {
                        { "data", ob },
                        { "is_single", true },
                    };

                    WindowTipsMgr.AddWindow("PetSimpleInfoWnd", needPara);
                }
                else
                {
                    // 构造参数
                    Dictionary<string, object> needPara = new Dictionary<string, object>()
                    {
                        { "data", ob },
                        { "is_singleBtn", true },
                        { "is_showMask", true },
                        { "single_text", LocalizationMgr.Get("RewardEquipInfoWnd_7") },
                    };

                    WindowTipsMgr.AddWindow("RewardItemInfoWnd", needPara);
                }
            }
            else
            {
                //TODO 属性给提示
            }
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 创建一批基础格子
        CreatedObject();
    }

    /// <summary>
    /// 初始化邮件数据
    /// </summary>
    void InitMailData()
    {
        // 获取系统邮件列表
        mSystemMailList = MailMgr.GetSystemExpressList();

        // 获取用户邮件列表
        mUserMailList = MailMgr.GetUserExpressList();

        mSelectRid = string.Empty;

        // 重置panel的位置
        ResetPositionPanel();

        if (mSystemMailList.Count < mItemAmount)
        {
            mSystemWrapContent.enabled = false;

            BindData(true);
        }
        else
        {
            mSystemWrapContent.minIndex = -mSystemMailList.Count + 1;
            mSystemWrapContent.maxIndex = 0;

            // 激活格子复用组件
            if (! mSystemWrapContent.enabled)
            {
                mSystemWrapContent.enabled = true;
            }
            else
            {
                foreach (KeyValuePair<int, int> map in mSystemIndexMap)
                    FillData(map.Key, map.Value, true);
            }
        }

        if (mUserMailList.Count < mItemAmount)
        {
            mUserWrapContent.enabled = false;
            BindData(false);
        }
        else
        {
            mUserWrapContent.minIndex = -mUserMailList.Count + 1;
            mUserWrapContent.maxIndex = 0;

            if (! mUserWrapContent.enabled)
            {
                mUserWrapContent.enabled = true;
            }
            else
            {
                foreach (KeyValuePair<int, int> map in mUserIndexMap)
                    FillData(map.Key, map.Value, false);
            }
        }

        // 设置邮件的数量
        RefreshTips();

        Coroutine.DispatchService(SelectDefault());
    }

    IEnumerator SelectDefault()
    {
        // 等待一帧刷新
        yield return null;

        if (mWnd == null)
            yield break;

        RedrawMailContent(null, 0);

        if (mSystemMailBtn.value)
        {
            OnClickSystemBtn(mSystemMailBtn.gameObject);
        }
        else
        {
            OnClickFriendBtn(mFriendPointBtn.gameObject);
        }
    }

    /// <summary>
    /// 重置panel的位置和偏移
    /// </summary>
    void ResetPositionPanel()
    {
        mUserPanel.transform.localPosition = new Vector3(-228, -10, 0);
        mSystemPanel.transform.localPosition = new Vector3(-228, -10, 0);

        for (int i = 0; i < mSystemWrapContent.transform.childCount; i++)
            mSystemWrapContent.transform.GetChild(i).localPosition = new Vector3(0, -mSystemWrapContent.itemSize * i, 0);

        for (int i = 0; i < mUserWrapContent.transform.childCount; i++)
            mUserWrapContent.transform.GetChild(i).localPosition = new Vector3(0, -mUserWrapContent.itemSize * i, 0);

        mUserPanel.clipOffset = Vector2.zero;

        mSystemPanel.clipOffset = Vector2.zero;

        mSystemIndexMap.Clear();
        mUserIndexMap.Clear();

        for (int i = 0; i < mItemAmount; i++)
        {
            mSystemIndexMap.Add(i, -i);
            mUserIndexMap.Add(i, -i);
        }
    }

    /// <summary>
    /// 刷新邮件数量提示
    /// </summary>
    void RefreshTips()
    {
        if (mUserMailList.Count > 0)
        {
            mFriendPointAmount.text = mUserMailList.Count.ToString();
            mFriendRedPoint.SetActive(true);
        }
        else
        {
            mFriendRedPoint.SetActive(false);
        }

        if (mSystemMailList.Count > 0)
        {
            mMailAmount.text = mSystemMailList.Count.ToString();
            mMailRedPoint.SetActive(true);
        }
        else
        {
            mMailAmount.text = mSystemMailList.Count.ToString();
            mMailRedPoint.SetActive(false);
        }
    }

    /// <summary>
    /// 创建基础格子
    /// </summary>
    void CreatedObject()
    {
        mMailItem.SetActive(false);
        mFriendPointItem.SetActive(false);
        for (int i = 0; i < mItemAmount; i++)
        {
            GameObject go = GameObject.Instantiate(mMailItem);
            go.transform.SetParent(mSystemWrapContent.transform);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(
                mMailItem.transform.localPosition.x,
                - i * mSystemWrapContent.itemSize,
                mMailItem.transform.localPosition.z);

            go.name = "system_" + i;
            go.SetActive(false);
            mMailObjectList.Add(go.name, go);

            // 注册格子点击事件
            UIEventListener.Get(go).onClick = OnClickSystemMailItem;

            // 友情点数基础格子
            go = GameObject.Instantiate(mFriendPointItem);
            go.transform.SetParent(mUserWrapContent.transform);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(
                mFriendPointItem.transform.localPosition.x,
                - i * mUserWrapContent.itemSize,
                mFriendPointItem.transform.localPosition.z);

            go.name = "friend_" + i;
            go.SetActive(false);
            mMailObjectList.Add(go.name, go);

            // 注册格子点击事件
            UIEventListener.Get(go).onClick = OnClickUserMailItem;
        }
    }

    /// <summary>
    /// 当邮件数量不能铺满面板时，绑定数据
    /// </summary>
    void BindData(bool isSystem)
    {
        string namePrefix = string.Empty;

        List<LPCMapping> mailList = new List<LPCMapping>();

        if (isSystem)
        {
            namePrefix = "system_";

            mailList = mSystemMailList;
        }
        else
        {
            namePrefix = "friend_";

            mailList = mUserMailList;
        }

        for (int i = 0; i < mailList.Count; i++)
        {
            GameObject wnd = null;
            if (! mMailObjectList.TryGetValue(namePrefix + i, out wnd))
                continue;

            if (isSystem)
            {
                SystemMailItemWnd script = wnd.GetComponent<SystemMailItemWnd>();

                script.Bind(mailList[i]);

                if (i == 0)
                    mSystemDefault = wnd;
            }
            else
            {
                UserMailItemWnd script = wnd.GetComponent<UserMailItemWnd>();

                script.Bind(mailList[i]);

                if (i == 0)
                    mUserDefault = wnd;
            }

            wnd.SetActive(true);

            string rid = mailList[i].GetValue<string>("rid");

            float rgb = 0f;
            if (rid.Equals(mSelectRid))
                rgb = 255f / 255;
            else
                rgb = 150f / 255;

            SetClickItemState(wnd, rgb);
        }

        for (int i = mailList.Count; i < mItemAmount; i++)
        {
            GameObject wnd = null;
            if (! mMailObjectList.TryGetValue(namePrefix + i, out wnd))
                continue;

            if (isSystem)
            {
                SystemMailItemWnd script = wnd.GetComponent<SystemMailItemWnd>();

                script.Bind(null);
            }
            else
            {
                UserMailItemWnd script = wnd.GetComponent<UserMailItemWnd>();

                script.Bind(null);
            }

            wnd.SetActive(false);
        }
    }

    void SystemUpdateItem(GameObject go, int wrapIndex, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (! mSystemIndexMap.ContainsKey(wrapIndex))
            mSystemIndexMap.Add(wrapIndex, realIndex);
        else
            mSystemIndexMap[wrapIndex] = realIndex;

        // 填充数据
        FillData(wrapIndex, realIndex, true);

        if (realIndex == 0)
            mSystemDefault = go;
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    void FillData(int wrapIndex, int realIndex, bool isSystem)
    {
        // 没有获取到排行榜数据
        if (mSystemMailList == null || mMailObjectList == null)
            return;

        int index = Mathf.Abs(realIndex);

        string name = string.Empty;

        string rid = string.Empty;

        GameObject wnd = null;

        if (isSystem)
        {
            name = "system_" + wrapIndex;

            if (!mMailObjectList.TryGetValue(name, out wnd))
                return;

            SystemMailItemWnd script = wnd.GetComponent<SystemMailItemWnd>();

            if (index + 1 > mSystemMailList.Count || mSystemMailList[index] == null)
            {
                wnd.SetActive(false);
                script.Bind(null);
                return;
            }

            script.Bind(mSystemMailList[index]);

            rid = mSystemMailList[index].GetValue<string>("rid");
        }
        else
        {
            name = "friend_" + wrapIndex;

            if (!mMailObjectList.TryGetValue(name, out wnd))
                return;

            UserMailItemWnd script = wnd.GetComponent<UserMailItemWnd>();

            if (index + 1 > mUserMailList.Count || mUserMailList[index] == null)
            {
                wnd.SetActive(false);
                script.Bind(null);
                return;
            }

            script.Bind(mUserMailList[index]);

            rid = mUserMailList[index].GetValue<string>("rid");
        }

        wnd.SetActive(true);

        float rgb = 0f;
        if (rid.Equals(mSelectRid))
            rgb = 255f / 255;
        else
            rgb = 150f / 255;

        SetClickItemState(wnd, rgb);
    }

    void UserUpdateItem(GameObject go, int wrapIndex, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (! mUserIndexMap.ContainsKey(wrapIndex))
            mUserIndexMap.Add(wrapIndex, realIndex);
        else
            mUserIndexMap[wrapIndex] = realIndex;

        // 填充数据
        FillData(wrapIndex, realIndex, false);

        if (realIndex == 0)
            mUserDefault = go;
    }

    /// <summary>
    /// 绘制邮件内容
    /// </summary>
    void RedrawMailContent(LPCMapping data, int time)
    {
        // 销毁子物体
        for (int i = 0; i < mItemGrid.transform.childCount; i++)
            Destroy(mItemGrid.transform.GetChild(i).gameObject);

        if (data == null)
        {
            mContent.SetActive(false);

            mSelectRid = string.Empty;

            return;
        }

        if (!mContent.activeSelf)
            mContent.SetActive(true);

        // 获取邮件内容
        LPCValue message = data.GetValue<LPCValue>("message");
        if (message == null)
            return;

        // 显示邮件内容
        mMailContent.text = LocalizationMgr.GetServerDesc(message);
        int day = time / 86400;

        if (day > 0)
        {
            day = time % 86400 != 0 ? day + 1 : day;

            // 剩余时间
            mRemainTime.text = string.Format(LocalizationMgr.Get("MailWnd_3"), day);
        }
        else
        {
            if (time >= 3600)
                mRemainTime.text = string.Format(LocalizationMgr.Get("MailWnd_13"), time / 3600);
            else
                mRemainTime.text = string.Format(LocalizationMgr.Get("MailWnd_14"), Mathf.Max(1, time / 60));
        }

        if (time != 0)
            Invoke("InitMailData", time);
        else
            CancelInvoke("InitMailData");

        // 显示邮箱附件列表
        // 获取邮箱附件列表
        LPCValue v = data.GetValue<LPCValue>("belonging_list");

        // 获取邮件附件物品可领取的数量,数量为0或者等于附件列表的长度表示可以全部领取
        mReceiveAmount = data.GetValue<int>("receive_amount");

        mItem.SetActive(false);
        if (v == null || !v.IsArray)
            return;

        mViewBtn.transform.localPosition = new Vector3(mViewBtn.transform.localPosition.x, -17, 0);

        mSelectTips.gameObject.SetActive(false);

        mIsAllRecieve = true;

        float rgb = 255f / 255;

        mGetBtn.GetComponent<UISprite>().color = new Color(rgb, rgb, rgb, rgb);

        UIEventListener.Get(mGetBtn).onClick = OnClickGetBtn;

        if (mReceiveAmount != 0 &&
            mReceiveAmount != v.AsArray.Count &&
            data.GetValue<string>("from_rid").Equals(ExpressStateType.SYSTEM_EXPRESS))
        {
            mViewBtn.transform.localPosition = new Vector3(mViewBtn.transform.localPosition.x, 11, 0);

            mSelectTips.text = string.Format(LocalizationMgr.Get("MailWnd_11"), Game.ConvertChineseNumber(mReceiveAmount));

            mSelectTips.gameObject.SetActive(true);

            mIsAllRecieve = false;

            rgb = 120f / 255;

            mGetBtn.GetComponent<UISprite>().color = new Color(rgb, rgb, rgb, rgb);

            UIEventListener.Get(mGetBtn).onClick -= OnClickGetBtn;
        }

        int index = 0;
        foreach (LPCValue item in v.AsArray.Values)
        {
            GameObject go = GameObject.Instantiate(mItem);

            go.transform.SetParent(mItemGrid.transform);

            go.transform.localPosition = new Vector3(0 + 105 * index,
                mItemGrid.transform.localPosition.y,
                mItemGrid.transform.localPosition.z);

            go.transform.localScale = Vector3.one;

            go.name = index.ToString();

            go.SetActive(true);

            go.GetComponent<SignItemWnd>().NormalItemBind(item.AsMapping, mIsAllRecieve);

            index++;

            // 附件可以全部领取
            if (mIsAllRecieve)
                continue;

            // 添加点击事件
            UIEventListener.Get(go).onClick = OnClickBelongingItem;
        }
    }

    /// <summary>
    /// 显示邮件列表
    /// </summary>
    void ShowMailList(bool systemValue, bool friendValue)
    {
        if (systemValue)
        {
            mSystemMailLb.transform.localPosition = new Vector3(
                mSystemMailLb.transform.localPosition.x, 11,
                mSystemMailLb.transform.localPosition.z);

            mFriendPointLb.transform.localPosition = new Vector3(
                mFriendPointLb.transform.localPosition.x, 7,
                mFriendPointLb.transform.localPosition.z);

            mMailRedPoint.transform.localPosition = new Vector3(93, 37, 1);
            mFriendRedPoint.transform.localPosition = new Vector3(97, 32, 1);

            mSystemMailWnd.SetActive(true);

            mFriendMailWnd.SetActive(false);

            mSystemBox.SetActive(true);
            mFriendBox.SetActive(false);

            mSortBtn.gameObject.SetActive(true);
            mAllGetBtn.gameObject.SetActive(false);

            if (mSystemDefault == null)
            {
                RedrawMailContent(null, 0);

                return;
            }

            OnClickSystemMailItem(mSystemDefault);
        }
        if (friendValue)
        {
            mSystemMailLb.transform.localPosition = new Vector3(
                mSystemMailLb.transform.localPosition.x, 7,
                mSystemMailLb.transform.localPosition.z);

            mFriendPointLb.transform.localPosition = new Vector3(
                mFriendPointLb.transform.localPosition.x, 11,
                mFriendPointLb.transform.localPosition.z);

            mMailRedPoint.transform.localPosition = new Vector3(97, 32, 1);
            mFriendRedPoint.transform.localPosition = new Vector3(93, 37, 1);

            mSortBtn.gameObject.SetActive(false);
            mAllGetBtn.gameObject.SetActive(true);

            mSystemMailWnd.SetActive(false);

            mFriendMailWnd.SetActive(true);

            mSystemBox.SetActive(false);
            mFriendBox.SetActive(true);

            if (mUserDefault == null)
            {
                RedrawMailContent(null, 0);
                return;
            }

            OnClickUserMailItem(mUserDefault);
        }
    }

    /// <summary>
    /// 设置点击的邮件列表基础格子的状态
    /// </summary>
    void SetClickItemState(GameObject go, float rgb)
    {
        Transform bg = go.transform.Find("bg");

        if (bg == null)
            return;

        bg.GetComponent<UISprite>().color = new Color(rgb, rgb, rgb);
    }

    /// <summary>
    /// 系统邮件按钮点击事件
    /// </summary>
    void OnClickSystemBtn(GameObject go)
    {
        // 显示邮件列表
        ShowMailList(true, false);
    }

    /// <summary>
    /// 邮件附件物品格子点击事件
    /// </summary>
    void OnClickBelongingItem(GameObject go)
    {
        if (go == null)
            return;

        SignItemWnd item = go.GetComponent<SignItemWnd>();

        if (item == null)
            return;

        // 该附件物品信息
        LPCMapping data = item.mData;

        if (data == null || data.Count < 1)
            return;

        if (item.mSelect && mReceiveAmount == 1)
            return;

        // 显示信息显示窗口
        GameObject wnd = WindowMgr.OpenWnd(BelongingGoodsInfoWnd.WndType);

        if (wnd == null)
        {
            LogMgr.Trace("BelongingGoodsInfoWnd窗口创建失败");
            return;
        }

        // 绑定数据
        wnd.GetComponent<BelongingGoodsInfoWnd>().Bind(data, mReceiveAmount, mSelectObject.Count, item.mSelect, new CallBack(SelectCallBack, item));
    }

    /// <summary>
    /// 附件确认选择回调函数
    /// </summary>
    void SelectCallBack(object para, params object[] param)
    {
        if (!(bool) param[0])
            return;

        SignItemWnd item = (SignItemWnd) para;

        if (item == null)
            return;

        LPCMapping data = item.mData;

        if (data == null)
            return;

        if (mReceiveAmount <= 1)
        {
            if (mSelectObject != null && mSelectObject.Count > 0)
            {
                mSelectObject[0].Select(false);
                mSelectObject.Clear();
            }

            // 缓存格子对象
            mSelectObject.Add(item);

            item.Select(true);
        }
        else
        {
            if (item.mSelect)
                mSelectObject.Remove(item);
            else
            {
                if (!mSelectObject.Contains(item))
                    mSelectObject.Add(item);
            }

            item.Select(!item.mSelect);
        }

        if (mSelectObject.Count > 0)
        {
            float rgb = 255f / 255;

            mGetBtn.GetComponent<UISprite>().color = new Color(rgb, rgb, rgb, rgb);

            UIEventListener.Get(mGetBtn).onClick = OnClickGetBtn;
        }
    }

    /// <summary>
    /// 友情点数查看按钮点击事件
    /// </summary>
    void OnClickFriendBtn(GameObject go)
    {
        // 显示邮件列表
        ShowMailList(false, true);
    }

    /// <summary>
    /// 排序按钮点击事件
    /// </summary>
    void OnClickSortBtn(GameObject go)
    {
        ResetPositionPanel();

        // 没有读取的邮件列表
        List<LPCMapping> mNoReadMail = new List<LPCMapping>();

        // 已经读取的邮件
        List<LPCMapping> mReadMail = new List<LPCMapping>();

        for (int i = 0; i < mSystemMailList.Count; i++)
        {
            LPCMapping data = mSystemMailList[i];
            if (data == null)
                continue;

            if (data.GetValue<int>("state") == ExpressStateType.EXPRESS_STATE_READ)
                mReadMail.Add(data);
            else
                mNoReadMail.Add(data);
        }

        mNoReadMail.Reverse();

        mReadMail.Reverse();

        mNoReadMail.AddRange(mReadMail);

        // 反转邮件列表
        mSystemMailList = mNoReadMail;

        float revertRgb = 150f / 255;

        for (int i = 0; i < mSystemWrapContent.transform.childCount; i++)
        {
            Transform bg = mSystemWrapContent.transform.GetChild(i).Find("bg");

            if (bg == null)
                continue;

            bg.GetComponent<UISprite>().color = new Color(
                revertRgb, revertRgb, revertRgb);
        }

        foreach (KeyValuePair<int, int> item in mSystemIndexMap)
        {
            // 填充数据
            FillData(item.Key, item.Value, true);
        }

        // 排序完成后默认选中第一个
        OnClickSystemMailItem(mMailObjectList["system_0"]);
    }

    /// <summary>
    /// 全部收取按钮点击事件
    /// </summary>
    void OnClickAllGetBtn(GameObject go)
    {
        // 提取所有邮件
        MailMgr.TakeAllExpressProperty(ME.user);
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 查看详情按钮点击事件
    /// </summary>
    void OnClickViewBtn(GameObject go)
    {

    }

    /// <summary>
    /// 点击去讨论区的超链接回调
    /// </summary>
    void OnClickGotoDiscuss(GameObject go)
    {
        if (mMailContent != null)
        {
            string url = mMailContent.GetUrlAtPosition(UICamera.lastWorldPosition);

            // 没有论坛地址不显示
            if (string.IsNullOrEmpty(url))
                return;

            GameObject wnd = WindowMgr.OpenWnd("WebViewWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            wnd.GetComponent<WebViewWnd>().BindData(url);
        }
    }

    /// <summary>
    /// 收取按钮点击事件
    /// </summary>
    void OnClickGetBtn(GameObject go)
    {
        LPCMapping mailData = MailMgr.GetExpressDetialData(mSelectRid);
        if (mailData == null)
            return;

        LPCArray belonging = LPCArray.Empty;
        if (mailData.ContainsKey("belonging_list"))
            belonging = mailData.GetValue<LPCArray>("belonging_list");

        int petPosCount = 0;

        int equipCount = 0;

        int group = -1;

        foreach (LPCValue v in belonging.Values)
        {
            if (! v.IsMapping)
                continue;

            LPCMapping data = v.AsMapping;
            if (! data.ContainsKey("class_id"))
                continue;

            int classId = data.GetValue<int>("class_id");

            if (MonsterMgr.IsMonster(classId))
            {
                petPosCount += data.GetValue<int>("amount");
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                equipCount += data.GetValue<int>("amount");
            }
            else
            {
                // 道具配置信息
                CsvRow row = ItemMgr.GetRow(classId);
                if (row == null)
                    continue;

                LPCValue scriptNo = row.Query<LPCValue>("pos_amount_script");
                if (scriptNo == null || scriptNo.AsInt == 0)
                    continue;

                // 包裹格子计算脚本参数
                LPCMapping args = row.Query<LPCMapping>("pos_amount_args");

                group = args.GetValue<int>("container_group");

                int count = (int) ScriptMgr.Call(scriptNo.AsInt, data.GetValue<int>("class_id"), data, args);

                if (group.Equals(ContainerConfig.POS_PET_GROUP))
                {
                    petPosCount += count;
                }
                else
                {
                    equipCount += count;
                }
            }
        }

        // 容器对象
        Container container = ME.user as Container;

        // 装备剩余包裹格子数量
        int freeItemPosCount = container.baggage.GetFreePosCount(ContainerConfig.POS_ITEM_GROUP);

        // 宠物包裹空间不足
        if (petPosCount != 0
            && !BaggageMgr.TryStoreToBaggage(ME.user, ContainerConfig.POS_PET_GROUP, petPosCount))
        {
            return;
        }

        // 装备包裹格子不足
        if (freeItemPosCount < equipCount)
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("MailWnd_15"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        // 构建参数
        LPCMapping para = new LPCMapping();

        foreach (SignItemWnd item in mSelectObject)
        {
            int index = -1;

            if (!int.TryParse(item.gameObject.name, out index))
                continue;

            if (index < 0)
                continue;

            mSelect.Add(index);
        }

        // 不是全部领取，需要添加领取列表
        if (!mIsAllRecieve)
        {
            para.Add("receive_list", mSelect);
        }
        else
        {
            foreach (LPCValue v in belonging.Values)
            {
                if (! v.IsMapping)
                    continue;

                LPCMapping data = v.AsMapping;
                if (! data.ContainsKey("class_id"))
                    continue;

                CsvRow row = ItemMgr.GetRow(data.GetValue<int>("class_id"));
                if (row == null)
                    continue;

                LPCMapping dbase = row.Query<LPCMapping>("dbase");

                // 装备卸载卷的处理
                if (dbase.ContainsKey("free_unequip") &&
                    dbase.GetValue<int>("free_unequip") == 1)
                {
                    // 开启了免费卸装活动
                    if (ActivityMgr.IsAcitvityValid(ActivityConst.FREE_UNEQUIP))
                    {
                        // 已经存在免费卸载装备活动
                        DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("MailWnd_16"));

                        return;
                    }

                    // 装备卸载卷数据
                    LPCValue unequp_data = ME.user.Query<LPCValue>("free_unequip_data");
                    if (unequp_data != null &&
                        unequp_data.IsMapping &&
                        unequp_data.AsMapping.GetValue<int>("end_time") > TimeMgr.GetServerTime())
                    {
                        // 已经使用了免费卸载装备道具
                        DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("MailWnd_17"));

                        return;
                    }

                    // 道具作用参数
                    LPCMapping applyArgs = row.Query<LPCMapping>("apply_arg");

                    // 有效时间
                    int valid_time = applyArgs.GetValue<int>("valid_time");

                    // 给出使用提示框
                    DialogMgr.ShowDailog(
                        new CallBack(UseFreeUnequipCallback),
                        string.Format(LocalizationMgr.Get("MailWnd_18"), LocalizationMgr.Get(row.Query<string>("name")), valid_time / 86400)
                    );

                    return;
                }

                // 改名卡道具的处理
                if (dbase.ContainsKey("can_modify_char_name") && dbase.GetValue<int>("can_modify_char_name") == 1)
                {
                    GameObject wnd = WindowMgr.OpenWnd(SetUserNameWnd.WndType);
                    if (wnd == null)
                        return;

                    SetUserNameWnd script = wnd.GetComponent<SetUserNameWnd>();
                    if (script == null)
                        return;

                    dbase.Add("class_id", data.GetValue<int>("class_id"));

                    // 绑定数据
                    script.Bind(dbase, true);

                    script.OpenColseWndEvent();

                    // 设置回调
                    script.SetCallBack(new CallBack(ModifyNameCallBack, para));

                    return;
                }

                //套装箱子处理
                if (dbase.ContainsKey("can_select_suit") && dbase.GetValue<int>("can_select_suit") == 1)
                {
                    GameObject wnd = WindowMgr.OpenWnd(MailSelectBonusWnd.WndType);
                    if (wnd == null)
                        return;

                    MailSelectBonusWnd script = wnd.GetComponent<MailSelectBonusWnd>();
                    if (script == null)
                        return;

                    script.SetCallBack(new CallBack(OnSelectSuitCallback));

                    return;
                }

            }
        }

        // 通知服务器提取邮件附件
        MailMgr.TakeExpressProperty(mSelectRid, para);
    }

    /// <summary>
    /// 套装确认弹框回调
    /// </summary>
    void OnSelectSuitCallback(object para, params object[] _params)
    {
        int suitId = (int)_params[0];

        LPCMapping extraPara = LPCMapping.Empty;
        extraPara.Add("suit_id", suitId);

        // 通知服务器提取邮件附件
        MailMgr.TakeExpressProperty(mSelectRid, extraPara);
    }

    /// <summary>
    /// 装备卸载卷确认弹框回调
    /// </summary>
    void UseFreeUnequipCallback(object para, params object[] _params)
    {
        if (!(bool)_params[0])
            return;

        // 通知服务器提取邮件附件
        MailMgr.TakeExpressProperty(mSelectRid, LPCMapping.Empty);
    }

    /// <summary>
    /// 改名卡确认弹框回调
    /// </summary>
    void ModifyNameCallBack(object para, params object[] _params)
    {
        if (!(bool)_params[0])
            return;

        string newName = _params[1] as string;

        LPCMapping extraPara = para as LPCMapping;
        extraPara.Add("new_name", newName);

        // 通知服务器提取邮件附件
        MailMgr.TakeExpressProperty(mSelectRid, extraPara);
    }

    /// <summary>
    /// 系统邮件基础格子点击事件
    /// </summary>
    void OnClickSystemMailItem(GameObject go)
    {
        if (go == null)
            return;
//        mViewBtn.gameObject.SetActive(true);

        SystemMailItemWnd item = go.GetComponent<SystemMailItemWnd>();

        if (item == null)
        {
            RedrawMailContent(null, 0);
            return;
        }

        LPCMapping mailData = item.mMailData;

        string mailRid = string.Empty;

        if (mailData != null && mailData.ContainsKey("rid"))
            mailRid = mailData.GetValue<string>("rid");

        if (mailRid.Equals(mSelectRid))
            return;

        foreach (GameObject mailItem in mMailObjectList.Values)
        {
            SystemMailItemWnd script = mailItem.GetComponent<SystemMailItemWnd>();
            if (script == null
                || script.mMailData == null
                || !script.mMailData.ContainsKey("rid"))
                continue;

            if (script.mMailData.GetValue<string>("rid").Equals(mailRid))
            {
                SetClickItemState(mailItem, 255 / 255f);
            }
            else
            {
                SetClickItemState(mailItem, 128 / 255f);
            }
        }

        if (mailData == null || mailData.Count < 1)
        {
            mSelectRid = string.Empty;

            mMailTitle.text = string.Empty;

            RedrawMailContent(null, 0);

            return;
        }

        mSelectRid = mailRid;

        LPCValue title = item.mMailData.GetValue<LPCValue>("title");
        if (title == null)
            return;

        // 还原title数据
        title = LPCRestoreString.SafeRestoreFromString(title.AsString);

        // 显示邮件标题
        mMailTitle.text = LocalizationMgr.GetServerDesc(title);

        RedrawMailContent(mailData, item.mTime);

        int state = mailData.GetValue<int>("state");
        if (state != ExpressStateType.EXPRESS_STATE_READ)
        {
            // 读取邮件
            MailMgr.DoReadExpress(mSelectRid);
        }
    }

    /// <summary>
    /// 用户邮件基础格子点击事件
    /// </summary>
    void OnClickUserMailItem(GameObject go)
    {
        if (go == null)
            return;

        mViewBtn.gameObject.SetActive(false);

        UserMailItemWnd item = go.GetComponent<UserMailItemWnd>();

        if (item == null)
        {
            RedrawMailContent(null, 0);
            return;
        }

        LPCMapping mailData = item.mMailData;

        string mailRid = string.Empty;

        if (mailData != null && mailData.ContainsKey("rid"))
            mailRid = mailData.GetValue<string>("rid");

        if (mailRid.Equals(mSelectRid))
            return;

        // 显示邮件标题
        mMailTitle.text = LocalizationMgr.Get("MailWnd_10");

        foreach (GameObject mailItem in mMailObjectList.Values)
        {
            UserMailItemWnd script = mailItem.GetComponent<UserMailItemWnd>();
            if (script == null
                || script.mMailData == null
                || !script.mMailData.ContainsKey("rid"))
                continue;

            if (script.mMailData.GetValue<string>("rid").Equals(mailRid))
            {
                SetClickItemState(mailItem, 255 / 255f);
                continue;
            }

            SetClickItemState(mailItem, 128 / 255f);
        }

        if (mailData == null || mailData.Count < 1)
        {
            mSelectRid = string.Empty;

            RedrawMailContent(null, 0);

            return;
        }

        mSelectRid = mailRid;

        // 绘制邮件内容
        RedrawMailContent(mailData, item.mTime);
    }

}
