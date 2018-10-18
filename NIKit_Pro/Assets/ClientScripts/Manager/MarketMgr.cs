/// <summary>
/// MarketMgr.cs
/// Created by fucj 2015-02-04
/// 商城
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;
using System;
using System.Linq;
using QCCommonSDK;
using QCCommonSDK.Addition;

public static class MarketMgr
{
    // 商城分页标识
    public const int MARKET_PAGE_ALL = 1;
    public const int MARKET_PAGE1 = 100;
    public const int MARKET_PAGE2 = 101;
    public const int MARKET_PAGE3 = 102;
    public const int MARKET_PAGE4 = 103;
    public const int MARKET_PAGE5 = 104;
    public const int MARKET_PAGE6 = 105;


    /// <summary>
    /// 商城配置
    /// </summary>
    public static CsvFile MarketCsv { get { return mMarketCsv; } }

    // 商城配置表信息
    private static CsvFile mMarketCsv;

    public static CsvFile GiftBagBonusCsv { get { return mGiftBagBonusCsv; } }

    // 商城配置表信息
    private static CsvFile mGiftBagBonusCsv;

    public static CsvFile GiftBagContentCsv { get { return mGiftBagContentCsv; } }

    // 从store下载商品价格列表（注：主要是为了做价格区域适应，在windows上不需要，直接读取的是本地配置表）
    private static Dictionary<string, string> mSkuDetails = new Dictionary<string, string>();

    // 商城配置表信息
    private static CsvFile mGiftBagContentCsv;

    // 商城页面数据
    private static Dictionary<int, Dictionary<int, List<LPCValue>>> mMarketPageData = new Dictionary<int, Dictionary<int, List<LPCValue>>>();

    // 商城礼包登录奖励数据
    private static Dictionary<int, Dictionary<int, LPCMapping>> mGiftBagData = new Dictionary<int, Dictionary<int, LPCMapping>>();

    // 商城配置数据
    private static Dictionary <int, CsvRow> mMarketConfig = new Dictionary<int, CsvRow>();

    // 礼包内容配置表
    private static Dictionary <int, CsvRow> mGiftBagContentConfig = new Dictionary<int, CsvRow>();

    /// <summary>
    /// 等级礼包
    /// </summary>
    private static Dictionary<int, List<CsvRow>> mLevelGiftMap = new Dictionary<int, List<CsvRow>>();

    // 登入礼包提示
    private static Property mTempProp;

    /// <summary>
    /// 绑定账号提示框点击回调
    /// </summary>
    private static void BindAccountCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 调用sdk绑定账号窗口
        QCCommonSDK.Addition.AuthSupport.bindAccount(string.Empty);
    }

    /// <summary>
    /// 实名提示框点击回调
    /// </summary>
    private static void RealNameCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 调用sdk绑定账号窗口
        QCCommonSDK.Addition.AuthSupport.RealNameAuth();
    }

    /// <summary>
    /// 实名提示框点击回调
    /// </summary>
    private static void RealNameBuyCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
        {
            List<object> list = para as List<object>;

            // 执行购买
            DoBuy(list[0] as Property, list[1] as LPCMapping, (int)list[2]);

            return;
        }

        // 调用sdk绑定账号窗口
        QCCommonSDK.Addition.AuthSupport.RealNameAuth();
    }

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private static void WhenLoginOk(int eventId, MixedValue para)
    {
        if (ME.user != null)
        {
            if (!CommonBonusMgr.HasNewSign(ME.user))
                ShowGiftLoginTips();
        }

#if !UNITY_EDITOR

        // 已经开启实名认证功能，并且没有实名认证
        if (ME.user.QueryTemp<int>("real_name") == 1 && QCCommonSDK.Addition.AuthSupport.NeedRealNameAuth())
        {
            // 未实名游戏限制开始时间
            int start_time = ME.user.Query<int>("real_name_time");

            // 实名限制时间
            int real_name_limit = GameSettingMgr.GetSettingInt("real_name_limit");

            int curTime = TimeMgr.GetServerTime();

            // 限制到时
            if (curTime < start_time + real_name_limit)
            {
                // 提示玩家实名认证(显示剩余期限天数)
                DialogMgr.ShowDailog(
                    new CallBack(RealNameCallBack),
                    string.Format(LocalizationMgr.Get("WindowMgr_5"), (real_name_limit - curTime + start_time) / 86400),
                    LocalizationMgr.Get("WindowMgr_4"));
            }
        }
#endif
    }

    /// <summary>
    /// 检测实名认证
    /// </summary>
    private static bool CheckBuy(Property who, LPCMapping data, int amount)
    {
        if (data == null || data.Count == 0)
            return false;

        // 获取道具的内购id
        string purchaseId = data.GetValue<string>("purchase_id");

        // 内购道具
        if (!string.IsNullOrEmpty(purchaseId))
        {
            // 关闭内购
            if (ConfigMgr.IsClosePurchase)
            {
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("MarketWnd_21"));

                return false;
            }

#if !UNITY_EDITOR
            // 没有绑定账号
            if (QCCommonSDK.Addition.AuthSupport.needBindAccount())
            {
                // 提示玩家绑定账号
                DialogMgr.ShowDailog(new CallBack(BindAccountCallBack), LocalizationMgr.Get("WindowMgr_3"), LocalizationMgr.Get("WindowMgr_2"));

                return false;
            }

            // 已经开启实名认证功能，并且没有实名认证
            if (ME.user.QueryTemp<int>("real_name") == 1 && QCCommonSDK.Addition.AuthSupport.NeedRealNameAuth())
            {
                // 未实名游戏限制开始时间
                int start_time = ME.user.Query<int>("real_name_time");

                // 实名限制时间
                int real_name_limit = GameSettingMgr.GetSettingInt("real_name_limit");

                int curTime = TimeMgr.GetServerTime();

                // 限制到时
                if (curTime >= start_time + real_name_limit)
                {
                    // 提示玩家绑定账号
                    DialogMgr.ShowDailog(new CallBack(RealNameCallBack), LocalizationMgr.Get("WindowMgr_6"), LocalizationMgr.Get("WindowMgr_4"));

                    return false;
                }
                else
                {
                    List<object> list = new List<object>();
                    list.Add(who);
                    list.Add(data);
                    list.Add(amount);

                    // 提示玩家绑定账号
                    DialogMgr.ShowDailog(
                        new CallBack(RealNameBuyCallBack, list),
                        string.Format(LocalizationMgr.Get("WindowMgr_5"), (real_name_limit - curTime + start_time) / 86400),
                        LocalizationMgr.Get("WindowMgr_4"));

                    return false;
                }
            }
#endif
        }
        else
        {
            // 获取道具的购买价格
            LPCMapping buy_price = data.GetValue<LPCMapping>("buy_price");
            string field = FieldsMgr.GetFieldInMapping(buy_price);
            int price = buy_price.GetValue<int>(field) * amount;

            // 消耗数据
            LPCMapping cost_map = new LPCMapping();
            cost_map.Add(field, price);

            // 判断或者钻石，金钱是否足够
            if (who.Query<int>(field) < price)
            {
                DialogMgr.Notify(string.Format(LocalizationMgr.Get("MarketWnd_18"), FieldsMgr.GetFieldName(field)));
                return false;
            }

        }

        return true;
    }

    /// <summary>
    /// 执行购买操作
    /// </summary>
    private static void DoBuy(Property who, LPCMapping data, int amount)
    {
        // 获取道具的内购id
        string purchaseId = data.GetValue<string>("purchase_id");

        // 内购道具
        if (!string.IsNullOrEmpty(purchaseId))
        {
            // 付费功能关闭,无法购买
            if (who.QueryTemp<int>("close_pay") == 1)
            {
                DialogMgr.Notify(LocalizationMgr.Get("MarketWnd_37"));

                return;
            }

#if UNITY_ANDROID && ! UNITY_EDITOR

            if (QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL").EndsWith("qc"))
            {
                // 打开选择支付窗口
                GameObject wnd = WindowMgr.OpenWnd(SelectPayWnd.WndType);
                if (wnd == null)
                return;

                // 绑定数据
                wnd.GetComponent<SelectPayWnd>().Bind(purchaseId);
            }
            else
            {
                // 显示购买等待窗口
                WindowMgr.AddWaittingWnd("do_purchase");

                // 执行内购流程
                PurchaseMgr.DoPurchase(purchaseId);
            }
#elif UNITY_IPHONE && ! UNITY_EDITOR

            // 显示购买等待窗口
            WindowMgr.AddWaittingWnd("do_purchase", LocalizationMgr.Get("MarketWnd_22"));

            // 执行内购流程
            PurchaseMgr.DoPurchase(purchaseId);
#endif
        }
        else
        {
            // 通知服务器购买商城道具
            Operation.CmdBuyMarketItem.Go(data.GetValue<int>("class_id"), amount);
        }
    }

    /// <summary>
    ///  获取限购类型比较
    ///  递减排序(大->小)
    /// </summary>
    /// <param name="who"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private static int GetLimitTypeCompare(Property who, int classId)
    {
        CsvRow row = GetMarketConfig(classId);

        // 没有该商品的配置数据
        if (row == null)
            return -1;

        LPCMapping buyArgs = row.Query<LPCMapping>("buy_args");

        LPCMapping sortMap = GetLoginTipsSortIndex(who, row.Query<int>("class_id"));

        if (sortMap.ContainsKey("sort_id"))
        {
            // 非额外机制排序
            int limitType = ShopConfig.NO_LIMIT_MARKET;

            if (buyArgs.ContainsKey("limit_type"))
                limitType = buyArgs.GetValue<int>("limit_type");

            int sortId = sortMap.GetValue<int>("sort_id");

            // 仅限 1 次购买
            if (limitType == ShopConfig.ACCOUNT_LIMIT_MARKET)
                return 5000 - sortId;

            // 月限 1 次购买
            else if (limitType == ShopConfig.MONTH_CYCLE_MARKET)
                return 4000 - sortId;

            // 周限 1 次购买
            else if (limitType == ShopConfig.WEEK_CYCLE_MARKET)
                return 3000 - sortId;

            // 日限 1 次购买
            else if (limitType == ShopConfig.DAY_CYCLE_MARKET)
                return 2000 - sortId;

            // 无限购
            else
                return 1000 - sortId;
        }
        else if (sortMap.ContainsKey("extra_sort_id"))
        {
            // 额外机制排序
            return 100000 - sortMap.GetValue<int>("extra_sort_id");
        }
        else
            return -1;
    }

    /// <summary>
    /// 获取排序ID
    /// </summary>
    /// <param name="who"></param>
    /// <param name="classId"></param>
    /// <returns></returns>
    private static LPCMapping GetLoginTipsSortIndex(Property who, int classId)
    {
        CsvRow row = GetMarketConfig(classId);

        // 没有该商品的配置数据
        if (row == null)
            return LPCMapping.Empty;

        LPCValue scriptNo = row.Query<LPCValue>("tips_sort_script");

        // 没有配置显示脚本
        if (scriptNo == null || !scriptNo.IsInt || scriptNo.AsInt == 0)
            return LPCMapping.Empty;

        return (LPCMapping)ScriptMgr.Call(scriptNo.AsInt, who, row.ConvertLpcMap());
    }

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 重置数据
        mMarketPageData.Clear();

        mGiftBagData.Clear();

        mMarketConfig.Clear();

        mGiftBagContentConfig.Clear();

        // 载入配置表
        LoadFile();

        // 载入礼包配置表
        LoadGiftBagFile();

        // 载入礼包内容配置表
        LoadGiftBagContentFile();

        // 注册登陆成功回调
        EventMgr.UnregisterEvent("MarketMgr");
        EventMgr.RegisterEvent("MarketMgr", EventMgrEventType.EVENT_LOGIN_OK, WhenLoginOk);

#if ! UNITY_EDITOR

        SkuSupport.OnSkuResult -= SkuHook;
        SkuSupport.OnSkuResult += SkuHook;

        // 协程中初始化
        Coroutine.DispatchService(RequestPriceDetail());
#endif
    }

    /// <summary>
    /// 购买商城道具
    /// </summary>
    public static void Buy(Property who, LPCMapping data, int amount)
    {
        // 购买检测不通过
        if (!CheckBuy(who, data, amount))
            return;

        // 执行购买
        DoBuy(who, data, amount);
    }

    /// <summary>
    /// 是否可以购买
    /// </summary>
    public static object IsBuy(Property who, int classId)
    {
        // 玩家对象不存在, 无法购买
        if (ME.user == null)
            return LocalizationMgr.Get("MarketWnd_28");

        // 商品配置数据
        CsvRow row = GetMarketConfig(classId);

        // 商品不存在
        if (row == null)
            return LocalizationMgr.Get("MarketWnd_30");

        LPCValue scprintNo = row.Query<LPCValue>("can_buy_script");

        // 没有配置脚本
        if (! scprintNo.IsInt || scprintNo.AsInt == 0)
            return true;

        // 调用判断脚本
        return ScriptMgr.Call(scprintNo.AsInt, who, row.ConvertLpcMap());
    }

    /// <summary>
    /// 获得一个分页的数据
    /// </summary>
    public static List<LPCValue> GetPageData(Property who, int page)
    {
        // 取的是所有商品
        if (page == MARKET_PAGE_ALL)
        {
            List<LPCValue> list = new List<LPCValue>();

            foreach (int key in mMarketPageData.Keys)
                list.AddRange(GetRealList(who, mMarketPageData[key]));

            return list;
        }

        if (!mMarketPageData.ContainsKey(page))
            return new List<LPCValue>();

        return GetRealList(who, mMarketPageData[page]);
    }

    /// <summary>
    /// 获取限时商城分页数据
    /// </summary>
    public static List<LPCValue> GetLimitMarketPageData(Property who, int page)
    {
        if (! mMarketPageData.ContainsKey(page))
            return new List<LPCValue>();

        // 获取限时商城分页所有数据
        List<LPCValue> list = GetRealList(who, mMarketPageData[page]);

        Dictionary<int, LPCValue> dic = new Dictionary<int, LPCValue>();

        List<LPCValue> newList = new List<LPCValue>();

        for (int i = 0; i < list.Count; i++)
        {
            LPCValue v = list[i];

            if (!v.IsMapping)
                continue;

            LPCMapping dbase = v.AsMapping.GetValue<LPCMapping>("dbase");
            if (dbase == null)
                continue;

            if (!dbase.ContainsKey("type"))
            {
                newList.Add(v);

                continue;
            }

            int type = dbase.GetValue<int>("type");

            if (!dic.ContainsKey(type))
                dic.Add(type, v);
        }

        newList.AddRange(dic.Values.ToList());

        // 按照位置排序列表
        newList.Sort((Comparison<LPCValue>)delegate(LPCValue a, LPCValue b){

            string posA = a.AsMapping.GetValue<string>("pos");

            int x1 = 0;
            int y1 = 0;
            int z1 = 0;

            POS.Read(posA, out x1, out y1, out z1);

            string posB = b.AsMapping.GetValue<string>("pos");

            int x2 = 0;
            int y2 = 0;
            int z2 = 0;

            POS.Read(posB, out x2, out y2, out z2);

            return z1.CompareTo(z2);
        });

        return newList;
    }

    /// <summary>
    /// 获取限时商城列表
    /// </summary>
    public static LPCArray GetLimitMarketList(Property who)
    {
        if (who == null)
            return null;

        // 动态礼包数据
        LPCValue v = who.Query<LPCValue>("dynamic_gift");
        if (v == null || !v.IsArray)
            return null;

        // 返回显示商城列表
        return v.AsArray;
    }

    /// <summary>
    ///  获取元素强化限时礼包列表
    /// </summary>
    /// <returns>The limit strength L ist.</returns>
    /// <param name="who">Who.</param>
    public static LPCArray GetLimitStrengthList(Property who)
    {
        if (who == null)
            return null;

        // 强化限时包数据
        LPCValue v = who.Query<LPCValue>("intensify_gift");
        if (v == null || !v.IsArray)
            return null;

        // 返回显示元素强化限时礼包列表
        return v.AsArray;
    }

    /// <summary>
    /// 获取快捷购买的商城数据
    /// </summary>
    public static List<LPCValue> GetQucikMarketData(Property who, string priorityGroup)
    {
        // 快捷商城数据
        List<LPCValue> allData = GetPageData(who, MARKET_PAGE_ALL);

        List<LPCValue> list = new List<LPCValue>();

        for (int i = 0; i < allData.Count; i++)
        {
            if (allData[i] == null || !allData[i].IsMapping)
                continue;

            if (allData[i].AsMapping.GetValue<int>("quick_group") != 1)
                continue;

            list.Add(allData[i]);
        }

        if (string.IsNullOrEmpty(priorityGroup))
            return list;

        List<LPCValue> newList = new List<LPCValue>();

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null || !list[i].IsMapping)
                continue;

            if (list[i].AsMapping.GetValue<string>("group").Equals(priorityGroup))
                newList.Insert(0, list[i]);
            else
                newList.Add(list[i]);
        }

        return newList;
    }

    /// <summary>
    /// 获取商城配置数据
    /// </summary>
    public static CsvRow GetMarketConfig(int classId)
    {
        CsvRow row = null;

        mMarketConfig.TryGetValue(classId, out row);

        return row;
    }

    /// <summary>
    /// 获取指定等级的等级礼包配置列表
    /// </summary>
    public static List<CsvRow> GetLevelGifts(int level)
    {
        List<CsvRow> giftList;

        // 该等级没有配置礼包
        if (! mLevelGiftMap.TryGetValue(level, out giftList))
            return new List<CsvRow>();

        // 返回等级礼包配置列表
        return giftList;
    }

    /// <summary>
    /// 获取动态礼包-周末特惠礼包列表
    /// </summary>
    /// <param name="who"></param>
    /// <returns></returns>
    public static List<int> GetValidWeekendGifts(Property who)
    {
        List<int> list = new List<int>();

        LPCArray dynamicGifts = GetLimitMarketList(who);

        if (dynamicGifts == null)
            return list;

        foreach (var item in dynamicGifts.Values)
        {
            if (IsWeekendGift(item.AsInt))
                list.Add(item.AsInt);
        }

        return list;
    }

    /// <summary>
    /// 获取相同分组的商城道具列表
    /// </summary>
    public static List<int> GetTypeList(int type)
    {
        List<int> list = new List<int>();

        foreach (CsvRow row in mMarketConfig.Values)
        {
            if (row == null)
                continue;

            LPCMapping dbase = row.Query<LPCMapping>("dbase");
            if (dbase == null || dbase.Count == 0)
                continue;

            if (type != dbase.GetValue<int>("type"))
                continue;

            list.Add(row.Query<int>("class_id"));
        }

        return list;
    }

    /// <summary>
    /// 展示登入礼包提示
    /// </summary>
    public static void ShowGiftLoginTips()
    {
        if (ME.user == null)
            return;

        List<int> loginTipsList = GetLoginTipsList(ME.user);

        int showClassId = 0;

        LPCValue classIdArray = OptionMgr.GetLocalOption(ME.user, "login_gift_tips");

        // 挑选显示哪个礼包
        for (int i = 0; i < loginTipsList.Count; i++)
        {
            // 如果第一个开启额外机制，优先显示第一个
            if (IsExtraShow(ME.user, loginTipsList[i]))
            {
                showClassId = loginTipsList[i];

                break;
            }
            else
            {
                if (classIdArray == null)
                {
                    showClassId = loginTipsList[i];

                    break;
                }

                if (classIdArray.AsArray.IndexOf(loginTipsList[i]) < 0)
                {
                    showClassId = loginTipsList[i];

                    break;
                }
            }
        }

        CsvRow row = GetMarketConfig(showClassId);

        // 没有该商品的配置数据
        if (row == null)
            return;

        // 构建参数
        LPCMapping para = LPCMapping.Empty;
        para.Add("rid", Rid.New());
        para.Add("class_id", row.Query<int>("class_id"));

        // 克隆一个物品对象
        if (mTempProp != null)
            mTempProp.Destroy();

        mTempProp = PropertyMgr.CreateProperty(para);
        if (mTempProp == null)
            return;

        LPCValue scriptNo = row.Query<LPCValue>("click_script");

        // 没有配置显示脚本
        if (scriptNo == null || !scriptNo.IsInt || scriptNo.AsInt == 0)
            return;

        GameObject wnd = (GameObject) ScriptMgr.Call(scriptNo.AsInt, row.ConvertLpcMap(), mTempProp);

        if (wnd != null)
        {
            // 已经展示过的礼包，记录到本地
            if (classIdArray == null)
                classIdArray = LPCValue.CreateArray();

            classIdArray.AsArray.Add(LPCValue.Create(showClassId));

            // 如果已经展示完一轮的话，就清空，从新按顺序展示
            if (classIdArray.AsArray.Count >= loginTipsList.Count)
                classIdArray = LPCValue.CreateArray();

            OptionMgr.SetLocalOption(ME.user, "login_gift_tips", classIdArray);

            // 零时存储礼包ID和礼包界面名
            LPCValue giftTipsInfo = LPCValue.CreateMapping();
            giftTipsInfo.AsMapping.Add("gift_class_id", LPCValue.Create(showClassId));
            giftTipsInfo.AsMapping.Add("gift_wnd_name", LPCValue.Create(wnd.name));

            ME.user.SetTemp("gift_tips_info", giftTipsInfo);
        }
    }

    /// <summary>
    /// 此礼包是否开启礼包登入提示额外机制
    /// </summary>
    /// <param name="who"></param>
    /// <param name="classId"></param>
    /// <returns></returns>
    public static bool IsExtraShow(Property who, int classId)
    {
        CsvRow row = GetMarketConfig(classId);

        // 没有该商品的配置数据
        if (row == null)
            return false;

        LPCMapping sortMap = GetLoginTipsSortIndex(who, classId);

        if (sortMap.ContainsKey("extra_sort_id"))
            return true;

        return false;
    }

    /// <summary>
    /// 获取登入有效礼包提示列表
    /// 1.限购分类排序 2.额外机制排序
    /// </summary>
    /// <param name="who"></param>
    /// <returns></returns>
    public static List<int> GetLoginTipsList(Property who)
    {
        List<int> list = new List<int>();

        if (who == null)
            return list;

        int classId;

        LPCMapping buyPrice;

        bool isClosePay = who.QueryTemp<int>("close_pay") == 1;

        // 筛选条件按优先级
        foreach (CsvRow row in mMarketConfig.Values)
        {
            if (row == null)
                continue;

            classId = row.Query<int>("class_id");

            // 关闭付费功能时，付费礼包就不要显示
            buyPrice = row.Query<LPCMapping>("buy_price");

            if (isClosePay && buyPrice.ContainsKey("rmb"))
                continue;

            // 筛选
            if (!IsShowTips(who, classId))
                continue;

            list.Add(classId);
        }

        // 排序
        list.Sort((a, b) =>
        {
            return GetLimitTypeCompare(who, b).CompareTo(GetLimitTypeCompare(who, a));
        });

        return list;
    }

    /// <summary>
    /// 礼包内容配置表
    /// </summary>
    public static CsvRow GetGiftContentConfig(int classId)
    {
        CsvRow row = null;

        mGiftBagContentConfig.TryGetValue(classId, out row);

        return row;
    }

    /// <summary>
    /// 获取商城道具配置信息
    /// </summary>
    public static LPCMapping GetMarketDataByPos(Property who, string pos)
    {
        if (string.IsNullOrEmpty(pos))
            return LPCMapping.Empty;

        int x = 0;
        int y = 0;
        int z = 0;
        POS.Read(pos, out x, out y, out z);

        List<LPCValue> list = new List<LPCValue>();
        if (mMarketPageData.ContainsKey(x))
            list = GetRealList(who, mMarketPageData[x]);

        LPCMapping marketData = LPCMapping.Empty;
        for (int i = 0; i < list.Count; i++)
        {
            if (! list[i].IsMapping)
                continue;

            LPCMapping data = list[i].AsMapping;

            if (!data.GetValue<string>("pos").Equals(pos))
                continue;

            marketData = data;
            break;
        }

        return marketData;
    }

    /// <summary>
    /// 是否是周末特惠礼包
    /// </summary>
    /// <param name="classId"></param>
    /// <returns></returns>
    public static bool IsWeekendGift(int classId)
    {
        CsvRow row = GetMarketConfig(classId);

        // 没有该商品的配置数据
        if (row == null)
            return false;

        if (row.Query<string>("group") == ShopConfig.WEEKEND_GIFT_GROUP)
            return true;

        return false;
    }

    /// <summary>
    /// 商品是否需要显示
    /// </summary>
    public static bool IsShow(Property who, int classId)
    {
        CsvRow row = GetMarketConfig(classId);

        // 没有该商品的配置数据
        if (row == null)
            return false;

        LPCValue scriptNo = row.Query<LPCValue>("can_show_script");

        // 没有配置显示脚本
        if (scriptNo == null || ! scriptNo.IsInt || scriptNo.AsInt == 0)
            return true;

        return (bool)ScriptMgr.Call(scriptNo.AsInt, who, row.ConvertLpcMap());
    }

    /// <summary>
    /// 是否在登入提示该礼包
    /// </summary>
    /// <param name="who"></param>
    /// <param name="classId"></param>
    /// <returns></returns>
    public static bool IsShowTips(Property who, int classId)
    {
        CsvRow row = GetMarketConfig(classId);

        // 没有该商品的配置数据
        if (row == null)
            return false;

        LPCValue scriptNo = row.Query<LPCValue>("tips_filter_script");

        // 没有配置显示脚本
        if (scriptNo == null || !scriptNo.IsInt || scriptNo.AsInt == 0)
            return true;

        return (bool)ScriptMgr.Call(scriptNo.AsInt, who, row.ConvertLpcMap());
    }

    /// <summary>
    /// 是否有改分页
    /// </summary>
    public static bool IsHavePage(int page)
    {
        return mMarketPageData.ContainsKey(page);
    }

    /// <summary>
    /// 判断是否限购
    /// </summary>
    public static object IsLimitBuy(Property who, LPCMapping itemData)
    {
        LPCMapping buyArgs = itemData.GetValue<LPCMapping>("buy_args");

        // 商品class_id
        int class_id = itemData.GetValue<int>("class_id");

        LPCMapping limitBuyData = LPCMapping.Empty;

        // 获取玩家购买的限购商品数据
        LPCValue v = who.Query<LPCValue>("limit_buy_data");
        if (v != null && v.IsMapping)
            limitBuyData = v.AsMapping;

        // 已经购买的数量
        int haveBuyAmount;
        // 上次购买的时间
        int buyTime;

        LPCMapping dbase = itemData.GetValue<LPCMapping>("dbase");

        int tempId = 0;

        if (dbase.ContainsKey("type"))
        {
            foreach (int key in limitBuyData.Keys)
            {
                // 商城配置数据
                CsvRow config = GetMarketConfig(key);
                if (config == null)
                    continue;

                LPCMapping data = config.Query<LPCMapping>("dbase");
                if (!data.ContainsKey("type"))
                    continue;

                if (data.GetValue<int>("type") != dbase.GetValue<int>("type"))
                    continue;

                tempId = key;

                break;
            }
        }
        else
        {
            tempId = class_id;
        }

        // 没有该商品的限购数据
        if (! limitBuyData.ContainsKey(tempId))
        {
            haveBuyAmount = 0;
            buyTime = 0;
        }
        else
        {
            LPCMapping data = limitBuyData.GetValue<LPCMapping>(tempId);

            haveBuyAmount = data.GetValue<int>("amount");

            buyTime = data.GetValue<int>("buy_time");
        }

        // 商品的限购类型
        int limitType = ShopConfig.NO_LIMIT_MARKET;

        if (buyArgs.ContainsKey("limit_type"))
            limitType = buyArgs.GetValue<int>("limit_type");

        // 商品限购次数
        int amountLimit = 0;
        int levelLimit = 0;

        switch (limitType)
        {
            // 购买周期为天
            case ShopConfig.DAY_CYCLE_MARKET:

                amountLimit = buyArgs.GetValue<int>("amount_limit");

                // 判断购买周期是否是同一天
                if (!Game.IsSameDay(TimeMgr.GetServerTime(), buyTime))
                {
                    // 判断是否有时间段限制
                    if (buyArgs.ContainsKey("buy_time_zone"))
                    {
                        System.DateTime time = TimeMgr.ConvertIntDateTime(TimeMgr.GetServerTime());
                        LPCMapping butTimeZone = buyArgs.GetValue<LPCMapping>("buy_time_zone");
                        int curHour = time.Hour;
                        int startHour = butTimeZone.GetValue<int>("start");
                        int endHour = butTimeZone.GetValue<int>("end");
                        if (curHour >= startHour && curHour < endHour)
                            return true;
                    }

                    return string.Empty;
                }

                // 没有数量限制
                if (amountLimit < 1)
                    return true;

                // 购买次数达到上限
                if (haveBuyAmount + 1 > amountLimit)
                    return LocalizationMgr.Get("MarketWnd_20");

                return true;

                // 购买周期为周
            case ShopConfig.WEEK_CYCLE_MARKET:

                amountLimit = buyArgs.GetValue<int>("amount_limit");

                // 判断购买周期是否是同一天
                if (!Game.IsSameWeek(TimeMgr.GetServerTime(), buyTime))
                    return true;

                // 没有数量限制
                if (amountLimit < 1)
                    return true;

                // 购买次数达到上限
                if (haveBuyAmount + 1 > amountLimit)
                    return LocalizationMgr.Get("MarketWnd_20");

                return true;

                // 购买周期为月
            case ShopConfig.MONTH_CYCLE_MARKET:

                amountLimit = buyArgs.GetValue<int>("amount_limit");

                // 判断购买周期是否是同一月
                if (!Game.IsSameMonth(TimeMgr.GetServerTime(), buyTime))
                    return true;

                // 没有数量限制
                if (amountLimit < 1)
                    return true;

                // 购买次数达到上限
                if (haveBuyAmount + 1 > amountLimit)
                    return LocalizationMgr.Get("MarketWnd_20");

                return true;

                // 购买周期为年
            case ShopConfig.YEAR_CYCLE_MARKET:

                amountLimit = buyArgs.GetValue<int>("amount_limit");

                // 判断购买周期是否是同一年
                if (!Game.IsSameYear(TimeMgr.GetServerTime(), buyTime))
                    return true;

                // 没有数量限制
                if (amountLimit < 1)
                    return true;

                // 购买次数达到上限
                if (haveBuyAmount + 1 > amountLimit)
                    return LocalizationMgr.Get("MarketWnd_20");

                return true;

                // 账号限制
            case ShopConfig.ACCOUNT_LIMIT_MARKET:

                amountLimit = buyArgs.GetValue<int>("amount_limit");

                // 没有数量限制
                if (amountLimit < 1)
                    return true;

                // 购买次数达到上限
                if (haveBuyAmount + 1 > amountLimit)
                    return LocalizationMgr.Get("MarketWnd_20");

                return true;

                // 等级限制
            case ShopConfig.LEVEL_LIMIT_MARKET:

                // 次数限制
                amountLimit = buyArgs.GetValue<int>("amount_limit");

                // 等级限制
                levelLimit = buyArgs.GetValue<int>("level_limit");

                // 等级不满足要求，无法购买。
                if (who.Query<int>("level") < levelLimit)
                    return LocalizationMgr.Get("MarketWnd_27");

                // 没有数量限制
                if (amountLimit < 1)
                    return true;

                // 购买次数达到上限
                if (haveBuyAmount + 1 > amountLimit)
                    return LocalizationMgr.Get("MarketWnd_20");

                return true;

            case ShopConfig.NO_LIMIT_MARKET:
                return true;

            case ShopConfig.NONE_LIMIT_MARKET:
                return true;

            default:

                // 购买失败
                return LocalizationMgr.Get("MarketWnd_28");
        }
    }

    /// <summary>
    /// 礼包奖励
    /// </summary>
    public static LPCMapping GetGiftBagData(int classId, int day)
    {
        Dictionary<int, LPCMapping> data = null;

        if (!mGiftBagData.TryGetValue(classId, out data))
            return null;

        if (data == null)
            return null;

        LPCMapping bonusData = null;

        if (!data.TryGetValue(day % data.Count == 0 ? day / data.Count : day % data.Count, out bonusData))
            return null;

        return bonusData;
    }

    /// <summary>
    /// 根据purchase_id查询sku信息
    /// </summary>
    public static string GetSkuPrice(int class_id)
    {
        CsvRow item = MarketMgr.mMarketCsv.FindByKey(class_id);

        string skuId = item.Query<string>("purchase_id");

        if (mSkuDetails.ContainsKey(skuId))
            return mSkuDetails [skuId];

        LPCMapping buyPrice = item.Query<LPCMapping>("buy_price");

        string costFields = FieldsMgr.GetFieldInMapping(buyPrice);
        LPCValue price = buyPrice.GetValue<LPCValue>(costFields);

        // 构建货币单位加上数值（eg. ¥6）
        return string.Format("{0}{1}", FieldsMgr.GetFieldUnit(costFields), price.AsString);
    }

    /// <summary>
    /// 检测价格
    /// 某些渠道（如google）下可能会出现获取不到商品价格的情况，
    /// 需要在打开商店时去检测是否需要重新获取价格信息。
    /// </summary>
    /// <returns>The sku price.</returns>
    public static void CheckSkuPrice()
    {
        // 非发布模式，不处理
        if(! string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_PUBLISH))
            return;

        // 请求获取商品列表信息
        SkuSupport.RequestSkusInfo();
    }

    /// <summary>
    /// 取得真实的显示列表
    /// </summary>
    /// <returns>The real list.</returns>
    /// <param name="list">List.</param>
    private static List<LPCValue> GetRealList(Property who, Dictionary<int, List<LPCValue>> page_data)
    {
        LPCValue market_data = who.Query("market");
        List<int> pos_arr = page_data.Keys.ToList();
        List<LPCValue> list = new List<LPCValue>();
        int class_id = 0;
        int number = 0;
        LPCMapping item_data = null;
        pos_arr.Sort();

        foreach (int pos in pos_arr)
        {
            // 排序每个位置的道具
            List<LPCValue> pos_list = (page_data[pos]).OrderBy(c => c.AsMapping.GetValue<int>(pos)).ToList();

            // 取得每个位置可以出售的道具
            foreach (LPCValue data in pos_list)
            {
                item_data = data.AsMapping;
                class_id = item_data.GetValue<int>("class_id");
                number = item_data.GetValue<int>("number");

                // 购买次数已经用完
                if (number != 0 &&
                    market_data != null &&
                    market_data.AsMapping.Count > 0 &&
                    market_data.AsMapping.GetValue<int>("" + class_id) >= number)
                    continue;

                // 不能购买的不显示
                if (!IsShow(who, class_id))
                    continue;

                list.Add(data);
            }
        }

        return list;
    }

    /// <summary>
    /// 载入商城配置表
    /// </summary>
    private static void LoadFile()
    {
        int x, y, z;
        string pos;
        Dictionary<int, List<LPCValue>> page_data = null;
        List<LPCValue> pos_data = null;

        // 载入道具类型配置表信息
        mMarketCsv = CsvFileMgr.Load("market");
        if (mMarketCsv == null)
            return;

        mMarketPageData.Clear();
        mLevelGiftMap.Clear();

        // 遍历各行数据
        foreach (CsvRow data in mMarketCsv.rows)
        {
            // 转换数据格式为LPC mapping数据格式
            LPCValue dataMap = data.ConvertLpc();
            if (dataMap == null)
                continue;

            int classId = data.Query<int>("class_id");

            if (!mMarketConfig.ContainsKey(classId))
                mMarketConfig.Add(classId, data);
            else
                LogMgr.Trace("market.csv中存在相同的class_id:{0}", classId);

            // 没有配置位置
            pos = dataMap.AsMapping.GetValue<string>("pos");

            // 如果是等级礼包需要特殊处理LEVEL_GIFT_GROUP
            if (string.Equals(data.Query<string>("group"), ShopConfig.LEVEL_GIFT_GROUP))
            {
                // 获取天数
                LPCMapping dbase = data.Query<LPCMapping>("dbase");
                if (dbase != null)
                {
                    int level = dbase.GetValue<int>("level");
                    if (! mLevelGiftMap.ContainsKey(level))
                        mLevelGiftMap[level] = new List<CsvRow>() { data };
                    else
                        mLevelGiftMap[level].Add(data);
                }
            }

            if (string.IsNullOrEmpty(pos))
                continue;

            POS.Read(pos, out x, out y, out z);

            // 取得此页的信息
            mMarketPageData.TryGetValue(x, out page_data);
            if (page_data == null)
                page_data = new Dictionary<int, List<LPCValue>>();

            // 取得此页此位置的道具
            page_data.TryGetValue(y, out pos_data);
            if (pos_data == null)
                pos_data = new List<LPCValue>();

            // 记录位置
            dataMap.AsMapping.Add(y, z);

            // 加入位置列表
            pos_data.Add(dataMap);

            // 记录此位置道具信息
            page_data[y] = pos_data;

            mMarketPageData[x] = page_data;
        }
    }

    // 载入礼包登录奖励配置表
    private static void LoadGiftBagFile()
    {
        // 载入道具类型配置表信息
        mGiftBagBonusCsv = CsvFileMgr.Load("gift_bag_bonus");
        if (mGiftBagBonusCsv == null)
            return;

        foreach (CsvRow config in mGiftBagBonusCsv.rows)
        {
            if (config == null)
                continue;

            Dictionary<int, LPCMapping> data = null;

            int classId = config.Query<int>("class_id");

            if (!mGiftBagData.TryGetValue(classId, out data))
                data = new Dictionary<int, LPCMapping>();

            LPCMapping bonusData = null;

            int id = config.Query<int>("id");

            if (!data.TryGetValue(id, out bonusData))
                bonusData = config.ConvertLpcMap();

            data[id] = bonusData;

            mGiftBagData[classId] = data;
        }
    }

    private static void LoadGiftBagContentFile()
    {
        // 载入道具类型配置表信息
        mGiftBagContentCsv = CsvFileMgr.Load("gift_bag_content");
        if (mGiftBagContentCsv == null)
            return;

        foreach (CsvRow config in mGiftBagContentCsv.rows)
        {
            if (config == null)
                continue;

            int classId = config.Query<int>("class_id");

            mGiftBagContentConfig.Add(classId, config);
        }
    }

    /// <summary>
    /// 获取sku消息的回调
    /// </summary>
    private static void SkuHook(QCEventResult result)
    {
        mSkuDetails = result.result;
    }

    /// <summary>
    /// 获取价格信息
    /// </summary>
    /// <returns>The price.</returns>
    private static IEnumerator RequestPriceDetail()
    {
        yield return null;

        CheckSkuPrice();
    }

    #endregion
}
