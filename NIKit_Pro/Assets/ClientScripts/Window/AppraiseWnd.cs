/// <summary>
/// AppraiseWnd.cs
/// Created by fengsc 2018/01/09
/// 玩家评价窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using LPC;

public class AppraiseWnd : WindowBase<AppraiseWnd>
{
    #region 成员变量

    // 使魔头像
    public UITexture mPetIcon;

    // 使魔头像遮罩
    public GameObject mIconMask;

    // 元素图标
    public UISprite mElement;

    // 使魔名称
    public UILabel mName;

    // 使魔星级
    public UISprite[] mStars;

    // 元素选择窗口
    public ElementSelectWnd mElementSelectWnd;

    // 关闭按钮
    public GameObject mCloseBtn;

    // “最近” 选项
    public UIToggle mRecentToggle;
    public UILabel mRecentToggleLb;

    // “推荐”选项
    public UIToggle mRecommendToggle;
    public UILabel mRecommendToggleLb;

    // ”公开信息“ 选项
    public UIToggle mPublicInfoToggle;
    public UILabel mPublicInfoToggleLb;

    // 窗口标题
    public UILabel mTitle;

    // 表格排序组件
    public UITable mTable;

    // 评价基础格子
    public GameObject mAppraiseItemWnd;

    // 载入更多评价提示
    public UILabel mLoadTips;

    // 评价输入框
    public UIInput mInput;

    // 发送按钮
    public GameObject mSendBtn;
    public UILabel mSendBtnLb;

    public UIScrollBar mUIScrollBar;

    public UIScrollView mUIScrollView;

    public TweenScale mTweenScale;

    // 宠物数据
    [HideInInspector]
    public Property mPetOb;

    // 评论排序方式
    int mOrdertype = CommentMgr.COMMENT_SORT_TYPE_DEFAULT;

    // 每次查询评论的最大条数
    int mMaxCommentPieces = 0;

    // 是否分享使魔
    bool mIsSharePet = true;

    List<GameObject> mItemList = new List<GameObject>();

    LPCArray mCommentList = LPCArray.Empty;

    bool mIsLoadFinish = false;

    private List<Property> equipData = new List<Property>();

    [HideInInspector]
    public LPCMapping mData = LPCMapping.Empty;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 初始化本地化文本
        InitText();

        if (mTweenScale != null)
        {
            float scale = Game.CalcWndScale();
            mTweenScale.to = new Vector3(scale, scale, scale);
        }
    }

    void OnDestroy()
    {
        // 析构宠物对象
        if (mPetOb != null)
            mPetOb.Destroy();

        // 解注册事件
        EventMgr.UnregisterEvent("AppraiseWnd");
    }

    /// <summary>
    // 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("AppraiseWnd_3");
        mRecentToggleLb.text = LocalizationMgr.Get("AppraiseWnd_1");
        mRecommendToggleLb.text = LocalizationMgr.Get("AppraiseWnd_2");
        mPublicInfoToggleLb.text = LocalizationMgr.Get("AppraiseWnd_6");

        mInput.defaultText = LocalizationMgr.Get("AppraiseWnd_7");

        mSendBtnLb.text = LocalizationMgr.Get("AppraiseWnd_16");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mRecentToggle.gameObject).onClick = OnClickRecentBtn;
        UIEventListener.Get(mRecommendToggle.gameObject).onClick = OnClickRecommendBtn;
        UIEventListener.Get(mLoadTips.gameObject).onClick = OnClickLoadTips;
        UIEventListener.Get(mSendBtn).onClick = OnClickSendBtn;
        UIEventListener.Get(mInput.gameObject).onClick = OnClickInput;

        EventDelegate.Add(mPublicInfoToggle.onChange, OnClickPublicInfoBtn);

        EventDelegate.Add(mUIScrollBar.onChange, OnUIScrollBarChange);

        // 注册EVENT_COMMENT_OPERATE_DONE事件
        EventMgr.RegisterEvent("AppraiseWnd", EventMgrEventType.EVENT_COMMENT_OPERATE_DONE, OnEventCommentOperateDone);
    }

    void OnUIScrollBarChange()
    {
        RefreshLoadTips();
    }

    void RefreshLoadTips()
    {
        if (mIsLoadFinish)
            mLoadTips.text = LocalizationMgr.Get("AppraiseWnd_5");
        else
            mLoadTips.text = LocalizationMgr.Get("AppraiseWnd_4");

        if (mUIScrollBar.value >= 0.99f)
        {
            mLoadTips.gameObject.SetActive(true);
        }
        else
        {
            mLoadTips.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 输入框点击回调
    /// </summary>
    void OnClickInput(GameObject go)
    {
        int comment_limit_level = GameSettingMgr.GetSettingInt("comment_limit_level");

        if (ME.user.GetLevel() < comment_limit_level)
        {
            mInput.defaultText = LocalizationMgr.Get("AppraiseWnd_7");

            DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("AppraiseWnd_10"), comment_limit_level));
        }
    }

    /// <summary>
    /// 输入完成事件回调
    /// </summary>
    void OnClickSendBtn(GameObject go)
    {
        int min_comment_length = GameSettingMgr.GetSettingInt("min_comment_length");

        // Emoji表情 过滤 
        string value = Regex.Replace(mInput.value.Trim(), @"\p{Cs}", "");

        // 所输入的文字包含非法字符
        if (BanWordMgr.ContainsBanWords(value))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("AppraiseWnd_12"));
            return;
        }

        // 你至少需要输入 3 个以上的中文字符。
        if (value.Length < min_comment_length)
        {
            DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("AppraiseWnd_11"), min_comment_length));

            return;
        }

        int max_comment_length = GameSettingMgr.GetSettingInt("max_comment_length");

        // 最多只允许发布 300 个字的评价。
        if (value.Length > max_comment_length)
        {
            DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("AppraiseWnd_13"), max_comment_length));

            return;
        }

        // 增加评论
        CommentMgr.AddComment(mPetOb.GetClassID(), value, mIsSharePet);
    }

    /// <summary>
    /// 评论操作事件回调
    /// </summary>
    void OnEventCommentOperateDone(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null || data.Count == 0)
            return;

        // 操作失败
        if (CommentMgr.COMMENT_OPERATE_OK != data.GetValue<int>("result"))
            return;

        // 操作方式
        string oper = data.GetValue<string>("oper");

        LPCValue extra_data = data.GetValue<LPCValue>("extra_data");

        // 根据不同的操作做不同的处理
        switch (oper)
        {
            // 增加评论操作结果
            case "add_comment":

                mInput.value = string.Empty;

                // 查询默认排序的评论
                CommentMgr.QueryComments(mPetOb.GetClassID(), mOrdertype, true);

                break;

            // 删除评论操作结果
            case "delete_comment":

            // 查询评论操作
            case "query_comments":

                // 玩家评论列表
                LPCArray list = CommentMgr.CommentList;

                if (list.Count == mCommentList.Count)
                    mIsLoadFinish = true;

                mCommentList = list;

                // 重绘评论
                RedrawComment();

                break;

                // 点赞评论操作结果
            case "add_praise":

                mCommentList = CommentMgr.CommentList;

                // 重绘评论
                RedrawComment();

                mIsLoadFinish = false;

                RefreshLoadTips();

                break;

                // 查询评论分享使魔操作
            case "query_comment_share_pet":

                ShowSharePetInfoWnd(extra_data.AsMapping);

                break;

                // 默认操作
            default:
                break;
        }
    }

    /// <summary>
    /// 显示分享使魔弹框
    /// </summary>
    public void ShowSharePetInfoWnd(LPCMapping para)
    {
        if (para == null || para.Count == 0)
            return;

        // 宠物信息窗口
        GameObject wnd = WindowMgr.OpenWnd(PetInfoWnd.WndType);
        if (wnd == null)
            return;

        // 还原宠物对象
        Property ob = PropertyMgr.CreateProperty(para, true);
        if (ob == null)
            return;

        // 载入下属物件
        DoPropertyLoaded(ob);

        // 刷新宠物属性
        PropMgr.RefreshAffect(ob);

        wnd.GetComponent<PetInfoWnd>().Bind(ob.GetRid(), mData.GetValue<string>("owner_name"), mData.GetValue<int>("owner_level"));

        wnd.GetComponent<PetInfoWnd>().SetCallBack(new CallBack(OnClosePetInfoWnd, ob));
    }

    /// <summary>
    /// 载入下属物件
    /// </summary>
    private void DoPropertyLoaded(Property owner)
    {
        // 获取角色的附属道具
        LPCArray propertyList = owner.Query<LPCArray>("property_list");

        // 角色没有附属装备信息
        if (propertyList == null ||
            propertyList.Count == 0)
            return;

        // 转换Container
        Container container = owner as Container;
        Property proOb;

        // 遍历各个附属道具
        foreach (LPCValue data in propertyList.Values)
        {
            LPCMapping dbase = LPCValue.Duplicate(data).AsMapping;

            dbase.Add("rid", Rid.New());

            // 构建对象
            proOb = PropertyMgr.CreateProperty(dbase, true);
            equipData.Add(proOb);

            // 构建对象失败
            if (proOb == null)
                continue;

            // 将道具载入包裹中
            container.LoadProperty(proOb, dbase["pos"].AsString);
        }
    }

    /// <summary>
    /// 窗口关闭按钮点击回调
    /// </summary>
    void OnClosePetInfoWnd(object para, params object[] param)
    {
        if (!(bool)param[0])
        {
            Property ob = para as Property;
            if (ob == null)
                return;

            // 析构宠物对象;
            ob.Destroy();

            // 析构装备对象
            for (int i = 0; i < equipData.Count; i++)
            {
                if (equipData[i] != null)
                    equipData[i].Destroy();
            }
        }
    }

    /// <summary>
    /// 关闭按钮点击事件回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 最近选项点击事件
    /// </summary>
    void OnClickRecentBtn(GameObject go)
    {
        // 选择同一个选项
        if (mOrdertype == CommentMgr.COMMENT_SORT_TYPE_DEFAULT)
            return;

        Reset();

        mOrdertype = CommentMgr.COMMENT_SORT_TYPE_DEFAULT;

        // 查询默认排序的评论
        CommentMgr.QueryComments(mPetOb.GetClassID(), mOrdertype, true);
    }

    /// <summary>
    /// 推荐选项点击事件
    /// </summary>
    void OnClickRecommendBtn(GameObject go)
    {
        // 选择同一个选项
        if (mOrdertype == CommentMgr.COMMENT_SORT_TYPE_COMMEND)
            return;

        Reset();

        mOrdertype = CommentMgr.COMMENT_SORT_TYPE_COMMEND;

        // 查询默认排序的评论
        CommentMgr.QueryComments(mPetOb.GetClassID(), mOrdertype, true);
    }

    /// <summary>
    /// 公开使魔信息选项点击事件
    /// </summary>
    void OnClickPublicInfoBtn()
    {
        // 标识是否分享使魔
        mIsSharePet = mPublicInfoToggle.value;
    }

    /// <summary>
    /// 加载提示按钮点击事件
    /// </summary>
    void OnClickLoadTips(GameObject go)
    {
        if (mIsLoadFinish)
            return;

        // 加载更多的评论信息
        CommentMgr.QueryComments(mPetOb.GetClassID(), mOrdertype, false);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        int classId = mPetOb.GetClassID();

        int rank = mPetOb.GetRank();

        if (ManualMgr.IsCompleted(ME.user, classId, rank))
        {
            mIconMask.SetActive(false);
        }
        else
        {
            mIconMask.SetActive(true);
        }

        // 加载宠物图像
        mPetIcon.mainTexture = MonsterMgr.GetTexture(classId, rank);

        // 使魔元素图标
        mElement.spriteName = MonsterConst.MonsterElementSpriteMap[mPetOb.Query<int>("element")];

        // 使魔名称
        mName.text = LocalizationMgr.Get(mPetOb.GetName());

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].gameObject.SetActive(false);

        // 使魔星级图标
        string starName = PetMgr.GetStarName(rank);

        for (int i = 0; i < mPetOb.GetStar(); i++)
        {
            mStars[i].spriteName = starName;

            mStars[i].gameObject.SetActive(true);
        }

        // 元素选择窗口绑定数据
        mElementSelectWnd.Bind(classId, mPetOb.Query<int>("element"), new CallBack(OnElementSelectCallBack));
    }

    /// <summary>
    /// 绘制评价内容
    /// </summary>
    void RedrawComment()
    {
        if (mCommentList == null)
            return;

        if (mCommentList.Count > mItemList.Count)
            CreatedGameObject();

        for (int i = 0; i < mCommentList.Count; i++)
        {
            if (mCommentList[i] == null || !mCommentList[i].IsMapping)
                continue;

            GameObject item = mItemList[i];

            item.SetActive(true);

            // 绑定数据
            item.GetComponent<AppraiseItemWnd>().Bind(mCommentList[i].AsMapping, (i + 1) % 2 == 0 ? false : true);
        }

        // 隐藏多余的基础格子
        for (int i = mCommentList.Count; i < mItemList.Count; i++)
            mItemList[i].SetActive(false);

        mTable.Reposition();
    }

    /// <summary>
    /// 创建GameObject
    /// </summary>
    void CreatedGameObject()
    {
        if (mAppraiseItemWnd == null)
            return;

        mAppraiseItemWnd.SetActive(false);

        for (int i = 0; i < mMaxCommentPieces; i++)
        {
            GameObject clone = Instantiate(mAppraiseItemWnd);

            clone.transform.SetParent(mTable.transform);

            clone.transform.localPosition = Vector3.zero;

            clone.transform.localScale = Vector3.one;

            clone.SetActive(true);

            mItemList.Add(clone);
        }

        mTable.Reposition();
    }

    /// <summary>
    /// 元素选择回调
    /// </summary>
    void OnElementSelectCallBack(object para, params object[] param)
    {
        Reset();

        // 刷新窗口
        int classId = (int) param[0];

        // 构造参数
        LPCMapping data = LPCMapping.Empty;
        data.Add("class_id", classId);
        data.Add("rank", mPetOb.GetRank());
        data.Add("star", mPetOb.GetStar());
        data.Add("rid", Rid.New());

        // 析构上一个宠物对象
        mPetOb.Destroy();

        // 创建一个新的宠物对象
        mPetOb = PropertyMgr.CreateProperty(data);

        // 查询默认排序的评论
        CommentMgr.QueryComments(classId, mOrdertype, true);

        Redraw();
    }

    /// <summary>
    /// 重置数据
    /// </summary>
    void Reset()
    {
        mUIScrollView.ResetPosition();

        mIsLoadFinish = false;

        mCommentList = LPCArray.Empty;
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int classId, int rank, int star)
    {
        // 构造参数
        LPCMapping data = LPCMapping.Empty;
        data.Add("class_id", classId);
        data.Add("rank", rank);
        data.Add("star", star);
        data.Add("rid", Rid.New());

        // 创建宠物对象
        mPetOb = PropertyMgr.CreateProperty(data);

        // 一次获取评论最大条数
        mMaxCommentPieces = GameSettingMgr.GetSettingInt("max_comment_pieces");

        // 创建gameobjct
        CreatedGameObject();

        Reset();

        for (int i = 0; i < mItemList.Count; i++)
            mItemList[i].SetActive(false);

        // 查询默认排序的评论
        CommentMgr.QueryComments(classId, mOrdertype, true);

        Redraw();
    }

    #endregion
}
