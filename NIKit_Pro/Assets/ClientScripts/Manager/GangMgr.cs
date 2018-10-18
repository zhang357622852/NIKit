/// <summary>
/// GangMgr.cs
/// Created from zhaozy 2018/01/24
/// 公会管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using LPC;

public class GangMgr
{

    #region 变量

    private static CsvFile mGangFlagCsv;

    private static List<string> mBaseIconList = new List<string>();

    private static List<string> mIconList = new List<string>();

    private static List<string> mStyleIconList = new List<string>();

    // 公会请求数据
    private static LPCArray mAllGangRequest = LPCArray.Empty;

    /// <summary>
    /// 公会成员列表
    /// </summary>
    private static LPCArray mGangMemberList = LPCArray.Empty;

    /// <summary>
    /// 公会信息
    /// </summary>
    private static LPCMapping mGangDetail = LPCMapping.Empty;

    /// <summary>
    /// 公会权限配置表
    /// </summary>
    private static CsvFile mGangGrantCsv;

    #endregion

    #region 属性

    public static List<string> BaseIconList
    {
        get
        {
            return mBaseIconList;
        }
    }

    public static List<string> IconList
    {
        get
        {
            return mIconList;
        }
    }

    public static List<string> StyleIconList
    {
        get
        {
            return mStyleIconList;
        }
    }

    /// <summary>
    /// 公会成员列表
    /// </summary>
    public static LPCArray GangMemberList
    {
        get
        {
            // 返回排序后的成员列表
            return SortGangMemberList(mGangMemberList);
        }

        set
        {
            mGangMemberList = value;
        }
    }

    /// <summary>
    /// 公会成员列表
    /// </summary>
    public static LPCMapping GangDetail
    {
        get
        {
            // relation_tag不一致, 返回空
            if (! string.Equals(ME.user.Query<string>("my_gang_info/relation_tag"),
                mGangDetail.GetValue<string>("relation_tag")))
                return LPCMapping.Empty;

            // 返回公会数据
            return mGangDetail;
        }

        set
        {
            mGangDetail = value;
        }
    }

    /// <summary>
    /// 公会请求列表
    /// </summary>
    public static LPCArray AllRequestList
    {
        get
        {
            // 返回公会数据
            return mAllGangRequest;
        }

        set
        {
            mAllGangRequest = value;
        }
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// Loads the gang flag file.
    /// </summary>
    private static void  LoadGangFlagFile()
    {
        // 重置数据
        mIconList.Clear();

        mStyleIconList.Clear();

        mGangFlagCsv = CsvFileMgr.Load("gang_flag");

        foreach (CsvRow row in mGangFlagCsv.rows)
        {
            string baseIcon = row.Query<string>("base_icon");
            if (!string.IsNullOrEmpty(baseIcon) && !mBaseIconList.Contains(baseIcon))
                mBaseIconList.Add(baseIcon);

            string icon = row.Query<string>("icon");
            if (!string.IsNullOrEmpty(icon) && !mIconList.Contains(icon))
                IconList.Add(icon);

            string styleIcon = row.Query<string>("style_icon");
            if (!string.IsNullOrEmpty(styleIcon) && !mStyleIconList.Contains(styleIcon))
                mStyleIconList.Add(styleIcon);
        }
    }

    /// <summary>
    /// Loads the gang flag file.
    /// </summary>
    private static void  LoadGangGrantFile()
    {
        // 载入公会权限配置表
        mGangGrantCsv = CsvFileMgr.Load("gang_grant");
    }

    /// <summary>
    /// 执行创建公会操作结果
    /// </summary>
    private static void DoCreateGangOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 创建失败，请重试。
            case GangConst.CREATE_GANG_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_142"));

                break;

                // 等级限制
            case GangConst.CREATE_GANG_LEVEL_LIMIT:

                // 获取创建帮派等级要求
                int levelRequest = GameSettingMgr.GetSettingInt("create_gang_level_limit");

                DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("CreateGuildWnd_21"), levelRequest));

                break;

            // 创建公会失败，公会名字无效。
            case GangConst.CREATE_GANG_NAME_INVALID:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("CreateGuildWnd_28"));

                break;

            // 创建公会失败，已有公会。
            case GangConst.CREATE_GANG_IN_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("CreateGuildWnd_25"));

                break;

            // 创建公会失败，输入公会名已被占用。
            case GangConst.CREATE_GANG_NAME_REPEAT:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("CreateGuildWnd_27"));

                break;

            // 创建公会失败，钻石不足。
            case GangConst.CREATE_GANG_COST_FAILED:

                DialogMgr.ShowSingleBtnDailog(null,
                    string.Format(LocalizationMgr.Get("GangWnd_100"), FieldsMgr.GetFieldName(extraData.AsString)));

                break;

            default:

                // 创建公会成功,抛出事件
                EventMgr.FireEvent(EventMgrEventType.EVENT_CRETAE_GANG_SUCCESS, null);

                break;
        }
    }

    /// <summary>
    /// 执行解散公会操作结果
    /// </summary>
    private static void DoDismissGangOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 解散公会失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_98"));

                break;

            // 你当前没有公会，不能执行该操作。
            case GangConst.GANG_OPERATE_NONE_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_86"));

                break;

            // 你没有相应的权限，不能执行该操作。
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            // 公会中还有其他玩家，不允许解散
            case GangConst.GANG_OPERATE_MEMBER_LIMIT:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_99"));

                break;

            default:
                // 解散公会成功

                break;
        }
    }

    /// <summary>
    /// 执行转让工会长操作结果
    /// </summary>
    private static void DoAbdicateGangLeaderOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 转让会长失败，请重试
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_97"));

                break;

            // 你当前没有公会，不能执行该操作。
            case GangConst.GANG_OPERATE_NONE_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_86"));

                break;

            // 你没有相应的权限，不能执行该操作。
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            // 转让会长失败， 目标不是公会成员
            case GangConst.GANG_OPERATE_NONE_MEMBER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_39"));

                break;

            default:

                // 抛出事件,会长转让成功
                EventMgr.FireEvent(EventMgrEventType.EVENT_TRANSFER_LEADER, null);

                // 转让工会长成功
                break;
        }
    }

    /// <summary>
    /// 执行任命副会长操作结果
    /// </summary>
    private static void DoAppointDeputyLeaderOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 任命副会长失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_96"));

                break;

            // 你当前没有公会，不能执行该操作。
            case GangConst.GANG_OPERATE_NONE_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_86"));

                break;

            // 你没有相应的权限，不能执行该操作。
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            // 任命副会长失败， 目标不是公会成员
            case GangConst.GANG_OPERATE_NONE_MEMBER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_48"));

                break;

            // 该玩家已经是副会长，无法继续任命！
            case GangConst.GANG_OPERATE_AREADY_STATION:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_47"));

                break;

            // 副会长人数已达上限，无法继续任命！
            case GangConst.GANG_OPERATE_STATION_NUM_LIMIT:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_44"));

                break;

            // 任命副会长操作失败，目标已经不是该职位
            case GangConst.GANG_OPERATE_NONE_STATION:
                break;

            default:

                // 抛出事件任命副会长成功
                EventMgr.FireEvent(EventMgrEventType.EVENT_TRANSFER_DEPUTY_LEADER, null);

                break;
        }
    }

    /// <summary>
    /// 执行获取公会成员列表操作结果
    /// </summary>
    private static void DoGetGangMemberListOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 获取公会成员列表失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_94"));

                break;

            // 无效公会，获取成员列表失败
            case GangConst.GANG_OPERATE_INVALID_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_95"));

                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 执行移除公会成员操作结果
    /// </summary>
    private static void DoRemoveGangMemberOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 移除公会成员失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_91"));

                break;

            // 角色当前没有公会
            case GangConst.GANG_OPERATE_NONE_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_86"));

                break;

            // 操作失败，不能移除自己
            case GangConst.GANG_OPERATE_FORBID_REMOVE_SELF:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_92"));

                break;

            // 权限不允许
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            // 目标不是公会成员
            case GangConst.GANG_OPERATE_NONE_MEMBER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_93"));

                break;

            default:

                // 公会成员移除成功
                EventMgr.FireEvent(EventMgrEventType.EVENT_REMOVE_GANG_MEMBER, null);

                break;
        }
    }

    /// <summary>
    /// 执行设置公会信息操作结果
    /// </summary>
    private static void DoSetGangInformationOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 设置公会信息失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_90"));

                break;

            // 角色当前没有公会
            case GangConst.GANG_OPERATE_NONE_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_86"));

                break;

            // 权限不允许
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            default:
                // 设置公会信息成功
                if (string.Equals(GangDetail.GetValue<string>("relation_tag"),
                    extraData.AsMapping.GetValue<string>("relation_tag")))
                    GangDetail.Append(extraData.AsMapping);

                // 修改成功抛出事件
                EventMgr.FireEvent(EventMgrEventType.EVENT_CHANGE_GANG_INFO, MixedValue.NewMixedValue<LPCValue>(extraData));

                break;
        }
    }

    /// <summary>
    /// 执行设置公会旗帜操作结果
    /// </summary>
    private static void DoSetGangFlagOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 设置公会旗帜失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_88"));

                break;

            // 角色当前没有公会
            case GangConst.GANG_OPERATE_NONE_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_86"));

                break;

            // 权限不允许
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            // 设置公会旗帜失败，钻石不足
            case GangConst.GANG_OPERATE_COST_FAILED:

                DialogMgr.ShowSingleBtnDailog(null,
                    string.Format(LocalizationMgr.Get("GangWnd_89"), FieldsMgr.GetFieldName(extraData.AsString)));

                break;

            default:
                // 设置公会旗帜成功
                if (string.Equals(GangDetail.GetValue<string>("relation_tag"),
                    extraData.AsMapping.GetValue<string>("relation_tag")))
                    GangDetail.Append(extraData.AsMapping);

                // 修改成功抛出事件
                EventMgr.FireEvent(EventMgrEventType.EVENT_CHANGE_GANG_INFO, null);

                break;
        }
    }

    /// <summary>
    /// 请求加入公会结果
    /// </summary>
    /// <param name="result">Result.</param>
    /// <param name="extraData">Extra data.</param>
    private static void DoRequestJoinOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 申请加入公会失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_78"));

                break;

            // 你已有公会，不能再申请加入其他公会
            case GangConst.GANG_OPERATE_IN_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_77"));

                break;

            // 申请加入公会失败，公会已解散。
            case GangConst.GANG_OPERATE_INVALID_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_79"));

                break;

            // 申请加入公会失败，处于cd中。
            case GangConst.GANG_OPERATE_CD_LIMIT:

                DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("GangWnd_145"),
                    TimeMgr.ConvertTimeToChineseTimer(extraData.AsInt, false, true)));

                break;

            // 申请加入公会失败，重复申请/邀请。
            case GangConst.GANG_OPERATE_REQUESTED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_81"));

                break;

            // 申请加入公会失败，你请求列表已满。
            case GangConst.GANG_OPERATE_MAX_REQUEST:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_82"));
                break;

            // 申请加入公会失败，公会请求列表已满。
            case GangConst.GANG_OPERATE_MAX_RECEIVE:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_83"));

                break;

            // 申请加入公会失败，条件不满足。
            case GangConst.GANG_OPERATE_CONDITION_UNSATISFIED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_84"));

                break;

            // 申请加入公会失败，公会拒绝接请求。
            case GangConst.GANG_OPERATE_NOT_ACCEPT:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_146"));

                break;

            // 申请加入公会失败，公会人数已满。
            case GangConst.GANG_OPERATE_MAX_MEMBER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_147"));

                break;

            default:

                // 获取参数信息
                LPCMapping para = extraData.AsMapping;

                // 申请公会成功
                if (para.ContainsKey("join_gang"))
                    DialogMgr.ShowSingleBtnDailog(null,
                        string.Format(LocalizationMgr.Get("GangWnd_148"), para.GetValue<string>("gang_name")));
                else
                    DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_76"));

                // 申请公会成功，抛出事件
                EventMgr.FireEvent(EventMgrEventType.EVENT_APPLICATION_GANG_SUCCESS, null);

                break;
        }
    }

    /// <summary>
    /// 邀请玩家加入公会
    /// </summary>
    /// <param name="result">Result.</param>
    /// <param name="extraData">Extra data.</param>
    private static void DoInviteJoinOperateResult(int result,LPCValue extraData)
    {
        switch (result)
        {
            // 邀请加入失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_121"));

                break;

            // 你当前没有公会，不能执行该操作。
            case GangConst.GANG_OPERATE_NONE_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_86"));

                break;

            // 邀请玩家不存在，邀请失败
            case GangConst.GANG_OPERATE_INVALID_USER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_123"));

                break;

            // 权限不允许，邀请失败
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            // 邀请失败，重复申请/邀请。
            case GangConst.GANG_OPERATE_REQUESTED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_124"));

                break;

            // 邀请失败，公会请求列表已满。
            case GangConst.GANG_OPERATE_MAX_REQUEST:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_127"));

                break;

            // 玩家已有公会，不能邀请。
            case GangConst.GANG_OPERATE_IN_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_126"));

                break;

            // 邀请失败，条件不满足。
            case GangConst.GANG_OPERATE_CONDITION_UNSATISFIED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_149"));

                break;

            // 邀请失败，玩家请求列表已满！
            case GangConst.GANG_OPERATE_MAX_RECEIVE:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_125"));

                break;

            default:

                // 邀请成功
                DialogMgr.ShowSingleBtnDailog(null,
                    string.Format(LocalizationMgr.Get("GangWnd_128"), extraData.AsMapping.GetValue<string>("name")));

                break;
        }
    }

    /// <summary>
    /// 获取和角色相关的帮派请求列表结果
    /// </summary>
    /// <param name="result">Result.</param>
    /// <param name="extraData">Extra data.</param>
    private static void DoGetRequestOperateResult(int result,LPCValue extraData)
    {
        switch (result)
        {
            // 获取和角色相关的帮派请求列表失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 接受某个id的公会请求
    /// </summary>
    private static void DoAcceptGangRequestOperateResult(int result,LPCValue extraData)
    {
        switch (result)
        {
            // 接受公会请求失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_114"));

                break;

            // 请求不存在，操作失败。
            case GangConst.GANG_OPERATE_INVALID_REQUEST:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_113"));

                break;

            // 权限不允许，邀请失败
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            // 接受请求失败，公会成员已满。
            case GangConst.GANG_OPERATE_MAX_MEMBER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_109"));

                break;

            // 只有玩家自己同意的时候才有这个限制
            case GangConst.GANG_OPERATE_CD_LIMIT:

                DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("GangWnd_145"),
                    TimeMgr.ConvertTimeToChineseTimer(extraData.AsInt, false, true)));

                break;

            default:

                // 同意申请成功
                EventMgr.FireEvent(EventMgrEventType.EVENT_ACCEPT_GANG_REQUEST, null);

                break;
        }
    }

    /// <summary>
    /// 取消公会请求
    /// </summary>
    private static void DoCancelGangRequestOperateResult(int result,LPCValue extraData)
    {
        switch (result)
        {
            // 取消公会请求失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_112"));

                break;

            // 请求不存在，操作失败。
            case GangConst.GANG_OPERATE_INVALID_REQUEST:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_113"));

                break;

            // 权限不允许，取消公会请求失败
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            default:

                // 取消申请成功
                EventMgr.FireEvent(EventMgrEventType.EVENT_CANCEl_GANG_REQUEST, null);

                break;
        }
    }

    /// <summary>
    /// 获取公会详情结果
    /// </summary>
    private static void DoGetGangDetailsOperateResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 获取公会详情失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_118"));

                break;

            // 无效公会获取数据失败
            case GangConst.GANG_OPERATE_INVALID_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_119"));

                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 获取公会推荐玩家
    /// </summary>
    private static void DoGetRecommendUserOperateResult(int result,LPCValue extraData)
    {
        switch (result)
        {
            // 获取公会详情失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_120"));

                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 发送公会宣传
    /// </summary>
    private static void DoSendGangSloganOperateResult(int result,LPCValue extraData)
    {
        switch (result)
        {
            // 发送公会宣传失败，请重试。
            case GangConst.GANG_OPERATE_FAILED:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_138"));

                break;

            // 不在公会中
            case GangConst.GANG_OPERATE_NONE_GANG:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_139"));

                break;

            // 权限不足
            case GangConst.GANG_OPERATE_INVALID_OPER:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_87"));

                break;

            // 发送字数太长
            case GangConst.GANG_OPERATE_CONTENT_SIZE_LIMIT:

                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_140"));

                break;

            // 发送公会宣传失败, 喇叭不够
            case GangConst.GANG_OPERATE_COST_FAILED:

                DialogMgr.ShowSingleBtnDailog(null,
                    string.Format(LocalizationMgr.Get("GangWnd_141"), FieldsMgr.GetFieldName(extraData.AsString)));

                break;

            default:

                EventMgr.FireEvent(EventMgrEventType.EVENT_SEND_GANG_SLOGAN, null);

                break;
        }
    }

    /// <summary>
    /// 判断是否具备操作权限，目前最多支持32个操作，每个操作由grant_info的每一位来指明
    /// </summary>
    /// <returns><c>true</c>, if operation was checked, <c>false</c> otherwise.</returns>
    /// <param name="operation">Operation.</param>
    /// <param name="grant">Grant.</param>
    private static bool CheckOperation(string operation, int grant)
    {
        // 没有配置信息
        if (mGangGrantCsv == null)
            return false;

        // 应该有配置本操作的信息
        CsvRow data = mGangGrantCsv.FindByKey(operation);
        if (data == null)
            return false;

        // 判断对应的第index位是否为1
        return (((grant >> data.Query<int>("index")) & 1) == 1);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 重置公会相关数据数据
    /// </summary>
    public static void DoResetAll()
    {
        // 重置公会相关数据数据
        GangMemberList = LPCArray.Empty;
        GangDetail = LPCMapping.Empty;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 载入配置表
        LoadGangFlagFile();

        // 载入公会权限配置表
        LoadGangGrantFile();
    }

    /// <summary>
    /// 权限验证
    /// </summary>
    /// <returns><c>true</c>, if operation was valided, <c>false</c> otherwise.</returns>
    /// <param name="operation">Operation.</param>
    /// <param name="duty">Duty.</param>
    /// <param name="stations">Stations.</param>
    public static bool ValidOperation(string operation, string duty, LPCMapping stations)
    {
        // 先进行全局职位的判断
        if (stations.ContainsKey("*") &&
            stations["*"].IsMapping &&
            CheckOperation(operation, stations["*"].AsMapping.GetValue<int>("grants")))
            return true;

        // 判断对应职位是否有该操作权限
        if (stations.ContainsKey(duty) &&
            stations[duty].IsMapping)
            return CheckOperation(operation, stations[duty].AsMapping.GetValue<int>("grants"));

        // 返回没有权限
        return false;
    }

    /// <summary>
    /// 获取公会详情
    /// </summary>
    public static void GetGangDetails(string gangName)
    {
        // 通知服务器获获取公会详情
        Operation.CmdGetGangDetails.Go(gangName);
    }

    /// <summary>
    /// 获取所有公会请求
    /// </summary>
    public static void GetAllGangRequest()
    {
        // 通知服务器获取所有公会请求
        Operation.CmdGetAllGangRequest.Go();
    }

    /// <summary>
    /// 获取所有公会请求
    /// </summary>
    public static void RequestJoinGang(string relationTag)
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家在公会中
        if (myGangInfo != null &&
             myGangInfo.IsMapping &&
            myGangInfo.AsMapping.Count != 0)
            return;

        // 通知服务器获取所有公会请求
        Operation.CmdRequestJoinGang.Go(relationTag);
    }

    /// <summary>
    /// 获取公会列表
    /// </summary>
    public static void NotifyGangSummary(int step, int checkFlag, int startIndex)
    {
        // 通知服务器获取公会信息
        Operation.CmdNotifyGangSummary.Go(step, checkFlag, startIndex);
    }

    /// <summary>
    /// 获取公会信息
    /// </summary>
    public static void GetGangInfo()
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在公会中
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 通知服务器获取公会信息
        Operation.CmdGetGangInfo.Go();
    }

    /// <summary>
    /// 获取公会推荐玩家
    /// </summary>
    public static void GetRecommendUser(int step)
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在公会中
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("add_relation_member",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (! ret)
            return;

        // 通知服务器获取公会推荐玩家
        Operation.CmdGetRecommendUser.Go(step);
    }

    /// <summary>
    /// 发送公会宣传
    /// </summary>
    public static void SendGangSlogan(LPCArray message)
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在公会中
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("send_gang_slogan",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (! ret)
            return;

        // 通知服务器发送公会宣传
        Operation.CmdSendGangSlogan.Go(message);
    }

    /// <summary>
    /// 设置公会入会条件
    /// </summary>
    public static void SetGangInformation(LPCMapping GangInfo)
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在公会中
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("add_relation_member",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (! ret)
            return;

        // 通知服务器设置公会信息
        Operation.CmdSetGangInformation.Go(GangInfo);
    }

    /// <summary>
    /// 设置公会旗帜
    /// </summary>
    public static void SetGangFlag(LPCArray flag)
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在公会中
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("set_gang_flag",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (!ret)
            return;

        // 判断设置公会旗帜消耗是否足够
        LPCMapping costMap = GameSettingMgr.GetSetting<LPCMapping>("set_gang_flag_cost");
        if (!ME.user.CanCostAttrib(costMap))
        {
            string fields = FieldsMgr.GetFieldInMapping(costMap);

            DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("GangWnd_89"), FieldsMgr.GetFieldName(fields)));
            return;
        }

        // 通知服务器设置公会旗帜
        Operation.CmdSetGangFlag.Go(flag);
    }

    /// <summary>
    /// 任命工会副会长
    /// </summary>
    /// <param name="targetRid">Target rid.</param>
    /// <param name="status">If set to <c>true</c> status.</param>
    public static void AppointDeputyLeader(string targetRid, bool status)
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在公会中
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("appoint_deputy_leader",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (! ret)
            return;

        // 通知服务器任命工会副会长
        Operation.CmdAppointDeputyLeader.Go(targetRid, status);
    }

    /// <summary>
    /// 转让公会长
    /// </summary>
    public static void AbdicateGangLeader(string targetRid)
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在公会中
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("abdicate_gang_leader",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (! ret)
            return;

        // 通知服务器转让公会长
        Operation.CmdAbdicateGangLeader.Go(targetRid);
    }

    /// <summary>
    /// 踢出公会成员
    /// </summary>
    public static void RemoveGroupMember(string targetRid)
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在公会中
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("invalid_relation_member",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (! ret)
            return;

        // 通知服务器踢出公会成员
        Operation.CmdRemoveGroupMember.Go(targetRid);
    }

    /// <summary>
    /// 解散公会
    /// </summary>
    public static void DismissGang()
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在工会中，不允许退出公会
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("dismiss_relation",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (! ret)
            return;

        // 通知服务器退出公会
        Operation.CmdDismissGang.Go();
    }

    /// <summary>
    /// 退出公会
    /// </summary>
    public static void LeftGang()
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在工会中，不允许退出公会
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("left_gang",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (! ret)
            return;

        // 通知服务器退出公会
        Operation.CmdLeftGang.Go();
    }

    /// <summary>
    /// 获取公会成员列表
    /// </summary>
    public static void GetGangMemberList(string relationTag)
    {
        // 通知服务器获取公会成员列表
        Operation.CmdGetGangMemberList.Go(relationTag);
    }

    /// <summary>
    /// 邀请玩家加入公会
    /// inviteData参数中只能包含（name或者rid）两个字段
    /// </summary>
    public static void InviteJoinGang(string userName)
    {
        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家不在工会中，不允许退出公会
        if (myGangInfo == null ||
            ! myGangInfo.IsMapping ||
            myGangInfo.AsMapping.Count == 0)
            return;

        // 根据玩家职位判断是否有权限执行相应操作
        bool ret = ValidOperation("add_relation_member",
            myGangInfo.AsMapping.GetValue<string>("station"),
            GangDetail.GetValue<LPCMapping>("stations"));

        // 权限检查没有通过
        if (! ret)
            return;

        // 通知服务器退出公会
        Operation.CmdInviteJoinGang.Go(userName);
    }

    /// <summary>
    /// Creates the gang.
    /// </summary>
    /// <param name="gangName">Gang name.</param>
    /// <param name="flag">Flag.</param>
    /// <param name="introduce">Introduce.</param>
    /// <param name="condition">Condition.</param>
    public static void CreateGang(string gangName, LPCArray flag, string introduce, LPCMapping condition)
    {
        // 获取创建帮派等级要求
        int levelRequest = GameSettingMgr.GetSettingInt("create_gang_level_limit");

        // 判断创建帮派等级是否满足
        if (ME.user.Query<int>("level") < levelRequest)
        {
            DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("CreateGuildWnd_21"), levelRequest));
            return;
        }

        if (string.IsNullOrEmpty(gangName))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("CreateGuildWnd_24"));
            return;
        }

        // 取得游戏配置的帮派名长度限制
        int minLen = GameSettingMgr.GetSettingInt("min_gang_name_len");
        int maxLen = GameSettingMgr.GetSettingInt("max_gang_name_len");
        if (gangName.Length < minLen)
        {
            DialogMgr.ShowSingleBtnDailog(null,
                string.Format(LocalizationMgr.Get("CreateGuildWnd_32"), minLen, maxLen));
            return;
        }

        if (gangName.Length > maxLen)
            gangName = gangName.Substring(0, maxLen - 1);

        // 公会名称包含非法字符
        if (BanWordMgr.ContainsBanWords(gangName))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("CreateGuildWnd_22"));
            return;
        }

        int maxIntroduceLen = GameSettingMgr.GetSettingInt("max_introduce_len");

        // 公会介绍内容长度不合要求
        if (introduce.Length > maxIntroduceLen)
            introduce = introduce.Substring(0, maxIntroduceLen - 1);

        // 公会介绍包含非法字符
        if (BanWordMgr.ContainsBanWords(introduce))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("CreateGuildWnd_23"));
            return;
        }

        if (condition == null || condition.Count == 0)
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("CreateGuildWnd_26"));
            return;
        }

        // 获取玩家当前帮派信息
        LPCValue myGangInfo = ME.user.Query<LPCValue>("my_gang_info");

        // 玩家已经有帮派了，不能再创建
        if (myGangInfo != null &&
            myGangInfo.IsMapping &&
            myGangInfo.AsMapping.Count > 0)
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("CreateGuildWnd_25"));
            return;
        }

        // 2. 获取创建帮派消耗
        LPCMapping costMap = GameSettingMgr.GetSetting<LPCMapping>("create_gang_cost");
        if (!ME.user.CanCostAttrib(costMap))
        {
            DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("CreateGuildWnd_15"), FieldsMgr.GetFieldName(FieldsMgr.GetFieldInMapping(costMap))));
            return;
        }

        // 通知服务器创建帮派
        Operation.CmdCreateGang.Go(gangName, flag, introduce, condition);
    }

    /// <summary>
    /// 取消公会请求/邀请
    /// </summary>
    public static void CancelGangRequest(string requestId)
    {
        // 通知服务器获取公会成员列表
        Operation.CmdCancelGangRequest.Go(requestId);
    }

    /// <summary>
    /// 接收公会请求/邀请
    /// </summary>
    public static void AcceptGangRequest(string requestId)
    {
        // 通知服务器获取公会成员列表
        Operation.CmdAcceptGangRequest.Go(requestId);
    }

    /// <summary>
    /// 是否发起申请
    /// </summary>
    public static bool IsRequest(string requestId)
    {
        if (string.IsNullOrEmpty(requestId))
            return false;

        LPCArray requestList = LPCArray.Empty;

        LPCValue value = ME.user.Query<LPCValue>("user_requests");
        if (value != null && value.IsArray)
            requestList = value.AsArray;

        for (int i = 0; i < requestList.Count; i++)
        {
            if (requestList[i].AsString == requestId)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 是否需要获取新的请求列表
    /// </summary>
    public static bool IsGetAllRequest(Property user, bool isUserRequest)
    {
        if (user == null)
            return false;

        LPCArray newList = LPCArray.Empty;

        for (int i = 0; i < AllRequestList.Count; i++)
        {
            LPCMapping requestData = AllRequestList[i].AsMapping;

            newList.Add(string.Format("{0}_{1}", requestData.GetValue<string>("rid"), requestData.GetValue<string>("relation_tag")));
        }

        if (isUserRequest)
        {
            // 玩家请求
            LPCArray userRequests = LPCArray.Empty;

            LPCValue value = user.Query<LPCValue>("user_requests");
            if (value != null && value.IsArray)
                userRequests = value.AsArray;

            for (int i = 0; i < userRequests.Count; i++)
            {
                // 和本地玩家申请列表中数据不一致
                if (newList.IndexOf(userRequests[i].AsString) == -1)
                    return true;
            }

            if (newList.Count > userRequests.Count)
                return true;

            return false;
        }
        else
        {
            // 公会请求

            LPCArray gangRequests = LPCArray.Empty;

            LPCMapping myGangInfo = LPCMapping.Empty;

            // 公会数据
            LPCValue gangInfoV = user.Query<LPCValue>("my_gang_info");
            if (gangInfoV != null && gangInfoV.IsMapping)
                myGangInfo = gangInfoV.AsMapping;

            if (myGangInfo.ContainsKey("gang_requests") && myGangInfo["gang_requests"].IsArray)
                gangRequests = myGangInfo["gang_requests"].AsArray;

            for (int i = 0; i < gangRequests.Count; i++)
            {
                // 和本地公会申请数据不一致
                if (newList.IndexOf(gangRequests[i].AsString) == -1)
                    return true;
            }

            if (newList.Count > gangRequests.Count)
                return true;

            return false;
        }
    }

    /// <summary>
    /// 执行公会操作结果
    /// </summary>
    public static void DoMsgGangOperateDone(string oper, int result, LPCValue extraData)
    {
        // 根据不同的操作做不同的处理
        switch (oper)
        {
            // 创建公会操作
            case "create_gang":

                // 执行创建公会操作结果
                DoCreateGangOperateResult(result, extraData);

                break;

            // 解散公会操作
            case "dismiss_gang":

                // 执行解散公会操作结果
                DoDismissGangOperateResult(result, extraData);

                break;

            // 转让工会长操作
            case "abdicate_gang_leader":

                // 执行转让工会长操作结果
                DoAbdicateGangLeaderOperateResult(result, extraData);

                break;

            // 任命副会长操作
            case "appoint_deputy_leader":

                // 执行任命副会长操作结果
                DoAppointDeputyLeaderOperateResult(result, extraData);

                break;

            // 获取公会成员列表
            case "get_gang_member":

                // 执行获取公会成员列表操作结果
                DoGetGangMemberListOperateResult(result, extraData);

                break;

            // 移除公会成员
            case "remove_gang_member":

                // 执行移除公会成员操作结果
                DoRemoveGangMemberOperateResult(result, extraData);

                break;

            // 设置公会信息
            case "set_gang_information":

                // 执行设置公会信息操作结果
                DoSetGangInformationOperateResult(result, extraData);

                break;

            // 设置公会旗帜
            case "set_gang_flag":

                // 执行设置公会旗帜操作结果
                DoSetGangFlagOperateResult(result, extraData);

                break;

            // 申请加入公会
            case "request_join":

                // 执行申请加入公会操作结果
                DoRequestJoinOperateResult(result, extraData);

                break;

            // 邀请玩家加入公会
            case "invite_join":

                // 执行获取和角色相关的帮派请求列表操作结果
                DoInviteJoinOperateResult(result, extraData);

                break;

            // 获取和角色相关的帮派请求列表
            case "get_request":

                // 执行获取和角色相关的帮派请求列表操作结果
                DoGetRequestOperateResult(result, extraData);

                break;

            // 接受某个id的公会请求
            case "accept_gang_request":

                // 执行接受某个id的公会请求操作结果
                DoAcceptGangRequestOperateResult(result, extraData);

                break;

            // 取消公会请求
            case "cancel_gang_request":

                // 执行取消公会请求操作结果
                DoCancelGangRequestOperateResult(result, extraData);

                break;

            // 获取公会详情
            case "get_gang_details":

                // 执行获取公会详情操作结果
                DoGetGangDetailsOperateResult(result, extraData);

                break;

            // 获取公会推荐玩家
            case "get_recommend_user":

                // 执行获取公会推荐玩家操作结果
                DoGetRecommendUserOperateResult(result, extraData);

                break;

            // 发送公会宣传
            case "send_gang_slogan":

                // 执行发送公会宣传操作结果
                DoSendGangSloganOperateResult(result, extraData);

                break;

            // 默认操作
            default:
                break;
        }
    }

    public static LPCArray SortGangMemberList(LPCArray memberList)
    {
        List<LPCMapping> list = new List<LPCMapping>();

        foreach (LPCValue item in memberList.Values)
            list.Add(item.AsMapping);

        // 根据道具权重排序
        IEnumerable<LPCMapping> ItemQuery = from data in list orderby CALC_GANG_MEMBER_LIST_SORT_RULE.Call(data) ascending select data;

        LPCArray sortList = LPCArray.Empty;

        foreach (LPCMapping item in ItemQuery)
            sortList.Add(item);

        return  sortList;
    }

    #endregion
}
