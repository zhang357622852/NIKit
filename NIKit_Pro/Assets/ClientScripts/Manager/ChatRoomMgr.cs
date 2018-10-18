/// <summary>
/// ChatRoomMgr.cs
/// Create by zhaozy 2015-08-24
/// 聊天室管理模块模块
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using LPC;

/// <summary>
/// 聊天室管理模块模块
/// </summary>
public static class ChatRoomMgr
{
    #region 变量

    // 表情配置表信息
    private static CsvFile mExpressionCsv;

    private static Dictionary<int, List<CsvRow>> mExpressionDic = new Dictionary<int, List<CsvRow>>();

    // 缓存消息队列
    private static Dictionary<string, LPCArray> mCacheChatMessage = new Dictionary<string, LPCArray>();

    // 缓存系统公告
    private static List<LPCMapping> mCacheSysAffiche = new List<LPCMapping>();

    private static LPCArray mItemDataList = new LPCArray();

    // 屏蔽的玩家列表
    private static List<string> mShieldList = new List<string>();


    #endregion

    #region 属性

    /// <summary>
    /// 获取表情配置表信息
    /// </summary>
    /// <value>The expression csv.</value>
    public static CsvFile ExpressionCsv {get {return mExpressionCsv;}}

    public static Dictionary<int, List<CsvRow>> ExpressionDic
    {
        get { return mExpressionDic; }
        private set { mExpressionDic = value; }
    }

    /// <summary>
    /// Gets the get property data list.
    /// </summary>
    /// <value>The get property data list.</value>
    public static LPCArray GetPropertyDataList {get {return mItemDataList;}}

    /// <summary>
    /// 获取屏蔽的用户列表
    /// </summary>
    public static List<string> GetShieldList {get {return mShieldList;}}

    /// <summary>
    /// 当前锁定聊天类型
    /// </summary>
    public static string LockChatType { get; set; }

    #endregion

    #region 内部接口

    /// <summary>
    /// 载入表情配置表
    /// </summary>
    private static void LoadExpressionCsv()
    {
        // 载入配置表
        mExpressionCsv = CsvFileMgr.Load("expression");

        // 清空缓存列表
        mExpressionDic.Clear();

        foreach (CsvRow row in mExpressionCsv.rows)
        {
            List<CsvRow> list = null;

            int group = row.Query<int>("group");

            if (! mExpressionDic.TryGetValue(group, out list))
                list = new List<CsvRow>();

            list.Add(row);

            mExpressionDic[group] = list;
        }
    }

    /// <summary>
    /// 从指定聊天类型消息缓存中过滤剔除屏蔽列表中的玩家消息
    /// </summary>
    /// <param name="chatType"></param>
    private static void FilterChaterByChatType(string chatType)
    {
        if (!mCacheChatMessage.ContainsKey(chatType))
            return;

        LPCArray mesList = mCacheChatMessage[chatType];
        if (mesList.Count <= 0)
            return;

        LPCArray newMesList = LPCArray.Empty;
        foreach (var item in mesList.Values)
        {
            if (mShieldList.Contains(item.AsMapping.GetValue<string>("rid")))
                continue;

            newMesList.Add(item);
        }

        mCacheChatMessage[chatType] = newMesList;
    }
    #endregion

    #region 公共接口

    /// <summary>
    /// 模块初始化
    /// </summary>
    public static void Init()
    {
        LoadExpressionCsv();
    }

    /// <summary>
    /// 获取聊天消息
    /// </summary>
    public static LPCArray GetChatMessage(string type)
    {
        // 该类型还没有聊天信息
        if (! mCacheChatMessage.ContainsKey(type))
            return LPCArray.Empty;

        // 消息缓存的最大数量
        return mCacheChatMessage[type];
    }

    /// <summary>
    /// Compare the specified x and y.
    /// </summary>
    public static string GetOrderWeight(LPCMapping data)
    {
        // 计算权重
        return string.Format("@{0:D2}{1:D10}", data.GetValue<int>("prioroty"), data.GetValue<int>("send_time"));
    }

    /// <summary>
    /// 添加系统公告
    /// </summary>
    public static void SetSysAffiche(List<LPCMapping> afficheList)
    {
        // 根据道具权重排序
        IEnumerable<LPCMapping> afficheListQuery = from ob in afficheList orderby GetOrderWeight(ob) descending select ob;

        // 记录新的公告信息
        mCacheSysAffiche = afficheListQuery.ToList();
    }

    /// <summary>
    /// 添加系统公告
    /// </summary>
    public static bool IsNewSysAffiche(LPCMapping affiche)
    {
        // 是否是新公告标识
        bool isNew = true;

        // 查找原来缓存消息列表中是否有该公告信息
        for (int i = 0; i < mCacheSysAffiche.Count; i++)
        {
            if (string.Equals(mCacheSysAffiche[i].GetValue<string>("rid"), affiche.GetValue<string>("rid")))
            {
                isNew = false;
                break;
            }
        }

        // 返回是否是新消息
        return isNew;
    }

    /// <summary>
    /// 获取所有的系统公告
    /// </summary>
    public static List<LPCMapping> GetAllSystemAffiche()
    {
        return mCacheSysAffiche;
    }

    /// <summary>
    /// 获取所有的系统公告
    /// </summary>
    public static LPCMapping GetLatelySystemAffiche()
    {
        // 没有公告信息
        if (mCacheSysAffiche.Count == 0)
            return null;

        // 返回最新信息
        return mCacheSysAffiche[0];
    }

    /// <summary>
    /// 举报玩家聊天信息
    /// </summary>
    public static bool ReportUserChat(string userRid, string userName, string chatId)
    {
        // 添加屏蔽列表
        AddShieldUser(userRid);

        // 通知服务器举报玩家聊天信息
        return Operation.CmdReportUserChat.Go(userName, chatId);
    }

    /// <summary>
    /// 发送聊天消息
    /// 不是私聊玩家toRid给空字符串
    /// </summary>
    public static bool SendChatMessage(Property who, string type, string toRid, LPCArray publishArray, string inputText)
    {
        // 空消息不允许发送
        if (inputText.Length == 0)
            return false;

        // 如果玩家没有开启接受该类型的聊天信息
        LPCValue closeWorldChat = OptionMgr.GetOption(who, "close_world_chat");
        if (closeWorldChat == null || closeWorldChat.AsInt == 1)
            return false;

        // 玩家等级限制当前不能聊天
        int limitLevel = GameSettingMgr.GetSettingInt("chat_limit_level");
        if (who.GetLevel() < limitLevel)
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("ChatWnd_19"), limitLevel));
            return false;
        }

        // 如果玩家当前处于禁言中不能发言
        int forbidChatTime = who.Query<int>("forbid_chat_time");
        int serverTime = TimeMgr.GetServerTime();
        if (serverTime < forbidChatTime)
        {
            LPCMapping data = new LPCMapping();
            data.Add("name", LocalizationMgr.Get("ChatWnd_18"));

            LPCArray message = LPCArray.Empty;
            message.Add(string.Format(LocalizationMgr.Get("ChatWnd_20"), TimeMgr.ConvertTimeToChinese(forbidChatTime - serverTime, false)));

            data.Add("message", message);
            data.Add("chat_type", ChatConfig.SYSTEM_CHAT);
            data.Add("type", ChatConfig.WORLD_CHANNEL);
            data.Add("rid", "system");

            // 模拟服务器下发消息
            LPCMapping msgArgs = new LPCMapping();
            msgArgs.Add("type", ChatConfig.WORLD_CHANNEL);
            msgArgs.Add("message_list", new LPCArray(data));

            // 模拟服务器下发MSG_CHAT_MESSAGE消息
            MsgMgr.Execute("MSG_CHAT_MESSAGE", LPCValue.Create(msgArgs));
            return false;
        }

        // 获取玩家的发言时间
        string cdPath = string.Format("talk_time/{0}", type);
        int talkTime = who.Query<int>(cdPath, true);
        int curTime = TimeMgr.GetTime();

        // 还处于CD时间限制中
        if (talkTime != 0 && (curTime - talkTime) < 0)
        {
            DialogMgr.Notify(LocalizationMgr.Get("ChatWnd_21"));
            return false;
        }

        // 发言字数超过限制，直接截断
        int maxLength = GameSettingMgr.GetSettingInt("max_chat_message_length");
        if (inputText.Length > maxLength)
            inputText = inputText.Substring(0, maxLength);

        // 匹配获得最终消息
        LPCArray messageArray = FixedMessageList(publishArray, inputText);

        if (messageArray.Count == 0)
            return false;

        // 处理最终得到的发布信息
        for (int i = 0; i < messageArray.Count; i++)
        {
            // 对于字符串做屏蔽词处理,对于发布消息不需要做处理
            if (messageArray[i].IsString)
                messageArray[i] = LPCValue.Create(BanWordMgr.GenHarmoniousWord(messageArray[i].AsString));
            else if (messageArray[i].IsMapping)
            {
                // 剔除不需要的元素
                LPCMapping publishMap = LPCValue.Duplicate(messageArray[i]).AsMapping;
                publishMap.Remove("publish_str");

                messageArray[i] = LPCValue.Create(publishMap);
            }
        }

        // 获取发言时间间隔
        int talkInterval = GameSettingMgr.GetSetting<int>("talk_interval");

        // 记录下一次发言时间
        if (talkInterval > 0)
            who.Set(cdPath, LPCValue.Create(curTime + talkInterval));

        // 通知服务器发送聊天信息
        return Operation.CmdChatSay.Go(type, toRid, messageArray);
    }

    /// <summary>
    /// Dos the cache chat message.
    /// </summary>
    private static void DoCacheChatMessage(string type, LPCArray messageList)
    {
        // 该类型还没有聊天信息，初始化数据
        if (! mCacheChatMessage.ContainsKey(type))
            mCacheChatMessage.Add(type, LPCArray.Empty);

        //过滤屏蔽用户名单:只屏蔽世界频道，其他不屏蔽(我在世界频道屏蔽了你，但是你还是可以私聊找我)
        LPCArray mesList;
        if (type.Equals(ChatConfig.WORLD_CHANNEL))
            mesList = FilterChaterByMesList(messageList);
        else
            mesList = messageList;

        if (mesList.Count <= 0)
            return;

        // 添加缓存数据
        mCacheChatMessage[type].Append(mesList);

        // 消息缓存的最大数量
        int max_amount = GameSettingMgr.GetSettingInt("max_cache_chat_message_amount");

        do
        {
            // 判断消息是否达到了最大缓存数量
            if (mCacheChatMessage[type].Count <= max_amount)
                break;

            // 移除第一个元素
            mCacheChatMessage[type].RemoveAt(0);

        } while(true);
    }

    /// <summary>
    /// 增加消息
    /// </summary>
    public static void AddChatMessage(string type, LPCArray messageList)
    {
        // 所有消息都需要添加到ChatConfig.WORLD_CHANNEL中
        if (! string.Equals(type, ChatConfig.WORLD_CHANNEL))
            DoCacheChatMessage(ChatConfig.WORLD_CHANNEL, messageList);

        // 添加对应类型的缓存列表中
        DoCacheChatMessage(type, messageList);
    }

    /// <summary>
    /// 清空缓存的聊天消息
    /// </summary>
    public static void ClearChatMessage()
    {
        LockChatType = string.Empty;
        mCacheChatMessage.Clear();
        mCacheSysAffiche.Clear();
    }

    /// <summary>
    /// 从消息列表中过滤屏蔽列表中的玩家消息
    /// </summary>
    /// <param name="mesList"></param>
    /// <returns></returns>
    public static LPCArray FilterChaterByMesList(LPCArray mesList)
    {
        if (mesList == null || mesList.Count <= 0)
            return LPCArray.Empty;

        LPCArray newMesList = LPCArray.Empty;
        foreach (var item in mesList.Values)
        {
            if (item == null)
                continue;

            if (mShieldList.Contains(item.AsMapping.GetValue<string>("rid")))
                continue;

            newMesList.Add(item);
        }

        return newMesList;
    }

    /// <summary>
    /// 是否屏蔽了聊天
    /// </summary>
    public static bool IsCloseChat()
    {
        // 如果玩家没有开启接受该类型的聊天信息
        LPCValue closeWorldChat = OptionMgr.GetOption(ME.user, "close_world_chat");
        if (closeWorldChat == null || closeWorldChat.AsInt == 1)
            return true;

        // 没有屏蔽聊天
        return false;
    }

    /// <summary>
    /// 根据玩家输入修正message列表
    /// </summary>
    /// <returns>The message list.</returns>
    /// <param name="messageList">Message list.</param>
    /// <param name="inputText">Input text.</param>
    public static LPCArray FixedMessageList(LPCArray publishArray, string inputText)
    {
        // 消息字符串为空是不允许发送的
        if(string.IsNullOrEmpty(inputText))
            return null;

        LPCArray fixedArray = new LPCArray();

        List<string> messageShowList = new List<string>();

        bool isPublishVideo = false;

        for (int i = 0; i < publishArray.Count; i++)
        {
            if (publishArray[i].AsMapping.ContainsKey("video_id") || publishArray[i].AsMapping.ContainsKey("invite_id"))
            {
                fixedArray.Add(publishArray[i].AsMapping);

                isPublishVideo = true;

                continue;
            }

            messageShowList.Add(publishArray[i].AsMapping.GetValue<string>("publish_str"));
        }

        if (isPublishVideo)
        {
            fixedArray.Add(inputText);

            return fixedArray;
        }

        // 正则表达式匹配发布信息
        // 匹配道具（装备）在固定的显示格式为如"<鞋子>"
        Match reg = Regex.Match(inputText, "「.*?」");
        int index = 0;

        // 匹配成功
        while (reg.Success)
        {
            if(messageShowList.Contains(reg.Value))
            {
                string lastStr = inputText.Substring(index, reg.Index - index);

                if(!string.IsNullOrEmpty(lastStr))
                {
                    if(fixedArray.Count > 0 &&
                        fixedArray[fixedArray.Count - 1].IsString)
                        fixedArray[fixedArray.Count - 1] =
                            LPCValue.Create(fixedArray[fixedArray.Count - 1].AsString + lastStr);
                    else
                        fixedArray.Add(lastStr);
                }

                int strIndex = messageShowList.IndexOf(reg.Value);

                fixedArray.Add(publishArray[strIndex]);

                messageShowList.RemoveAt(strIndex);
                publishArray.RemoveAt(strIndex);
            }
            else
            {
                string curIndex = inputText.Substring(index, reg.Index + reg.Value.Length - index);

                if(fixedArray.Count > 0 &&
                    fixedArray[fixedArray.Count - 1].IsString)
                    fixedArray[fixedArray.Count - 1] =
                        LPCValue.Create(fixedArray[fixedArray.Count - 1].AsString + curIndex);
                else
                    fixedArray.Add(curIndex);
            }

            // 重置
            index = reg.Index + reg.Value.Length;

            // 匹配下一个
            reg = reg.NextMatch();
        }

        string endStr = inputText.Substring(index, inputText.Length - index);

        if(!string.IsNullOrEmpty(endStr))
            fixedArray.Add(endStr);

        return fixedArray;
    }

    /// <summary>
    /// 根据分组获取表情列表
    /// </summary>
    public static List<CsvRow> GetExpressionListByGroup(int group)
    {
        List<CsvRow> list = new List<CsvRow>();

        mExpressionDic.TryGetValue(group, out list);

        return list;
    }

    public static CsvRow GetExpressionRow(int id)
    {
        if (mExpressionCsv == null)
            return null;

        CsvRow row = mExpressionCsv.FindByKey(id);

        return row;
    }

    /// <summary>
    /// 解析字符串
    /// </summary>
    public static LPCArray AnaltzeString(string str)
    {
        LPCArray array = new LPCArray();

        if (string.IsNullOrEmpty(str))
            return array;

        if (mExpressionCsv == null)
            return array;

        // 遍历字符串，将表情和普通文本拆分开
        string tempStr = string.Empty;
        for (int i = 0; i < str.Length; i++)
        {
            tempStr += str[i];
            int length = 0;
            foreach (CsvRow row in mExpressionCsv.rows)
            {
                // 表情中文显示字符串
                string chStr = LocalizationMgr.Get(row.Query<string>("ch_string"));

                // 表情快捷输入字符串
                string enQuick = row.Query<string>("en_quick");

                string icon = row.Query<string>("icon");

                string ex = string.Empty;

                int group = row.Query<int>("group");

                // 获取表情大小
                LPCArray size = row.Query<LPCArray>("size");

                List<CsvRow> list = ChatRoomMgr.GetExpressionListByGroup(group);

                if (tempStr.IndexOf(chStr) != -1)
                {
                    length = chStr.Length;

                    // 截取两个表情之间的文字
                    array.Add(tempStr.Substring(0, tempStr.IndexOf(chStr)));

                    if (list.Count > 1)
                        ex = "<split>" + string.Format("animationSprite={0},{1},{2},{3}", icon, size[0].AsInt, size[1].AsInt, group);
                    else
                        ex = "<split>" + string.Format("sprite={0},{1},{2}", icon, size[0].AsInt, size[1].AsInt);

                    ex += "</split>";
                    array.Add(ex);
                }
                if (tempStr.IndexOf(enQuick) != -1)
                {
                    length = enQuick.Length;
                    array.Add(tempStr.Substring(0, tempStr.IndexOf(enQuick)));

                    if (list.Count > 1)
                        ex = "<split>" + string.Format("animationSprite={0},{1},{2},{3}", icon, size[0].AsInt, size[1].AsInt, group);
                    else
                        ex = "<split>" + string.Format("sprite={0},{1},{2}", icon, size[0].AsInt, size[1].AsInt);

                    ex += "</split>";
                    array.Add(ex);
                }
                if (length != 0)
                    tempStr = string.Empty;
            }

            if (!string.IsNullOrEmpty(tempStr) && i + 1 == str.Length)
                array.Add(tempStr);
        }

        return array;
    }

    /// <summary>
    /// 解析属性显示
    /// </summary>
    public static LPCMapping AnaltzeAttrib(LPCMapping data)
    {
        LPCMapping para = LPCMapping.Empty;

        string fields = FieldsMgr.GetFieldInMapping(data);

        if (string.IsNullOrEmpty(fields))
            return para;

        int classId = FieldsMgr.GetClassIdByAttrib(fields);

        para.Add("name", ItemMgr.GetName(classId));
        para.Add("class_id", classId);
        para.Add("amount", data.GetValue<int>(fields));

        return para;
    }

    /// <summary>
    /// 使用该方法前先调用UpdateNGUIText()方法
    /// 解析消息，当消息中包含非字符信息时需要添加标签"<split></split>"
    /// </summary>
    public static string AnalyzeMessage(LPCMapping messageData, string ChatType, string tipsChatType = "", bool isClick = true, int characterLimit = 0)
    {
        LPCArray messageList = messageData.GetValue<LPCArray>("message");

        if (messageList == null || messageList.Count < 1)
            return string.Empty;

        // 计算一个中文字符的宽高
        float singleWidth = 16f;

        // 单行的字符限制
        float characterLimited = 0;

        // 聊天消息字符限制
        float chatMessageLength = 0;

        // 每行显示文本的宽度
        float width = 0f;

        if (ChatType.Equals("tips"))
        {
            // 其他界面聊天消息提示显示的字符限制
            characterLimited = (float) characterLimit;

            width = singleWidth * characterLimited;
        }
        else
        {
            // 聊天栏字符限制
            characterLimited = (float) GameSettingMgr.GetSettingInt("line_max_chat_msg_length");

            if (ChatType.Equals(ChatConfig.SYSTEM_CHAT)
                || ChatType.Equals(ChatConfig.SYSTEM_NOTIFY)
                || ChatType.Equals(ChatConfig.GAME_NOTIFY)
                || ChatType.Equals(ChatConfig.SYSTEM_MESSAGE_GANG))
            {
                chatMessageLength = (float) GameSettingMgr.GetSettingInt("max_system_message_length");

                width = 900f;
            }
            else
            {
                chatMessageLength = (float) GameSettingMgr.GetSettingInt("max_chat_message_length");

                width = 800f;
            }
        }

        float lineAmount = chatMessageLength % characterLimited == 0 ? chatMessageLength / characterLimited : chatMessageLength / characterLimited + 1;

        Dictionary<int, int> indexList = new Dictionary<int, int>();
        mItemDataList = LPCArray.Empty;

        string msg = string.Empty;
        foreach (LPCValue item in messageList.Values)
        {
            if (item.IsMapping)
            {
                LPCMapping map = item.AsMapping;
                if (map.ContainsKey("class_id"))
                {
                    mItemDataList.Add(map);
                }
                else if (map.ContainsKey("gang_name"))
                {
                    mItemDataList.Add(map);
                }
                else if (map.ContainsKey("video_id"))
                {
                    mItemDataList.Add(map);
                    continue;
                }
                else if (map.ContainsKey("invite_id"))
                {
                    mItemDataList.Add(map);
                    continue;
                }
                else
                {
                    mItemDataList.Add(AnaltzeAttrib(map));
                }

                if (isClick)
                    msg += "<split>" + "buttonLink=";
                else
                    msg += "<split>" + "button=";

                msg += "</split>";
            }
            else if (item.IsString)
            {
                string str = string.Empty;

                if (ChatType.Equals(ChatConfig.SYSTEM_CHAT)
                    || ChatType.Equals(ChatConfig.SYSTEM_NOTIFY)
                    || ChatType.Equals(ChatConfig.GAME_NOTIFY)
                    || (ChatType.Equals("tips")
                        && (tipsChatType.Equals(ChatConfig.SYSTEM_CHAT)
                            || tipsChatType.Equals(ChatConfig.SYSTEM_NOTIFY)
                            || tipsChatType.Equals(ChatConfig.GAME_NOTIFY))))
                {
                    str = LocalizationMgr.GetServerDesc(LPCRestoreString.SafeRestoreFromString(item.AsString));
                }
                else
                {
                    str = item.AsString;
                }

                // 解析字符串
                LPCArray array = AnaltzeString(str);

                if (array == null || array.Count < 1)
                    continue;

                foreach (LPCValue value in array.Values)
                    msg += value.AsString;
            }
        }

        // 匹配字符
        Regex reg = new Regex(@"(?<=<split>).*?(?=</split>)");
        MatchCollection mc = reg.Matches(msg);
        for (int i = 0; i < mc.Count; i++)
        {
            int key = mc[i].Index - 7 > 0 ? mc[i].Index - 7 : 0;
            indexList.Add(key, i);
        }

        string tempStr = string.Empty;

        // 图文的总宽度
        float imgTextWidth = 0;
        int lineIndex = 0;
        int wrapAmount = 0;

        int buttonIndex = 0;

        bool isOperate = false;

        int lastSpaceAmount = 0;
        for (int i = 0; i < msg.Length; i++)
        {
            string realityStr = tempStr;
            tempStr += msg[i];

            int startIndex = 0;
            float tempWidth = 0;

            // 字符的宽度
            float characterWidth = 0;

            string value = string.Empty;

            if (indexList.ContainsKey(i))
            {
                int subLength = i == lineIndex ? 1 : i - lineIndex;
                int amount = GetStrSpaceAmount(msg.Substring(lineIndex, subLength)) + 1;

                startIndex = i;
                value = mc[indexList[i]].Value;

                if (value.StartsWith("img=") || value.StartsWith("animationImg=")
                    || value.StartsWith("sprite=") || value.StartsWith("animationSprite="))
                {
                    // 分割字符串,获取icon大小
                    string[] strArr = value.Split(',');

                    float.TryParse(strArr[1], out tempWidth);

                    i += "<split></split>".Length + value.Length - 1;
                }
                else if (value.StartsWith("button=") || value.StartsWith("buttonLink="))
                {
                    i += "<split></split>".Length + value.Length - 1;

                    LPCMapping data = mItemDataList[buttonIndex].AsMapping;

                    buttonIndex++;

                    float nameWidth = 0f;

                    if (data.ContainsKey("class_id"))
                    {
                        int classId = data.GetValue<int>("class_id");

                        if (MonsterMgr.IsMonster(classId))
                        {
                            nameWidth = NGUIText.CalculatePrintedSize(MonsterMgr.GetName(classId, data.GetValue<int>("rank"))).x;

                            LPCValue prefixDescV = data.GetValue<LPCValue>("prefix_desc");

                            string prefixDesc = string.Empty;

                            if (prefixDescV != null)
                                prefixDesc = LocalizationMgr.GetServerDesc(prefixDescV);

                            if (string.IsNullOrEmpty(prefixDesc))
                                tempWidth = nameWidth + 21 + 35 + 45;
                            else
                                tempWidth = nameWidth + 21 + 35 + 45 + NGUIText.CalculatePrintedSize(prefixDesc).x;
                        }
                        else if (EquipMgr.IsEquipment(classId))
                        {
                            // 装备短描述
                            string shortDesc = EquipMgr.GetShortDesc(EquipMgr.GetSuitId(classId), data.GetValue<int>("rarity"), data.GetValue<int>("star"));

                            // 计算长度
                            nameWidth = NGUIText.CalculatePrintedSize(shortDesc).x;

                            // 装备
                            tempWidth = 13 * 2 + nameWidth;
                        }
                        else
                        {
                            nameWidth = NGUIText.CalculatePrintedSize(LocalizationMgr.GetServerDesc(data.GetValue<LPCValue>("name"))).x;

                            // 道具
                            tempWidth = nameWidth + NGUIText.CalculatePrintedSize(string.Format("{0}{1}", "×", data.GetValue<int>("amount"))).x + 20 + 34;
                        }
                    }
                    else
                    {
                        // 计算长度
                        nameWidth = NGUIText.CalculatePrintedSize(data.GetValue<string>("gang_name")).x;

                        tempWidth = 13 * 2 + nameWidth;
                    }
                }
                else if(value.StartsWith("labelclick="))
                {
                    if (!isOperate)
                    {
                        imgTextWidth -= NGUIText.CalculatePrintedSize("<split>labelclick=[u][/u]</split>").x;

                        isOperate = true;
                    }

                    // 计算一个字符的宽度
                    characterWidth = NGUIText.CalculatePrintedSize(msg[i].ToString()).x;
                    imgTextWidth += characterWidth;
                }

                realityStr = tempStr.Substring(0, tempStr.Length - 1);

                imgTextWidth = tempWidth + imgTextWidth + (amount - lastSpaceAmount) * 5;

                lastSpaceAmount = amount;
            }
            else
            {
                // 计算一个字符的宽度
                characterWidth = NGUIText.CalculatePrintedSize(msg[i].ToString()).x;
                imgTextWidth += characterWidth;
            }

            string subString = string.Empty;

            if (tempWidth != 0)
            {
                subString = msg.Substring(startIndex, "<split></split>".Length + value.Length);

                tempStr = realityStr + subString;
            }

            float diff = NGUIText.CalculatePrintedSize(msg[startIndex].ToString()).x;

            if (width - imgTextWidth >= diff + singleWidth)
                continue;

            // 每次换行重新计算
            imgTextWidth = 0;
            lastSpaceAmount = 0;

            // 如果是界面上的提示信息直接阶段之后的字符串
            if (ChatType.Equals("tips"))
            {
                // 截掉本次累加的字符
                tempStr = tempStr.Substring(0, realityStr.Length);

                break;
            }

            if (wrapAmount > lineAmount - 2)
            {
                // 截掉本次累加的字符
                tempStr = tempStr.Substring(0, realityStr.Length);

                break;
            }

            // 添加换行符
            if (width == 0)
            {
                tempStr = tempStr.Substring(0, tempStr.Length - 1) + "<br>" + tempStr.Substring(tempStr.Length, 1);
            }
            else
            {
                tempStr = tempStr.Substring(0, tempStr.Length - subString.Length) + "<br>" + tempStr.Substring(tempStr.Length - subString.Length, subString.Length);
            }

            wrapAmount++;

            // 换行后重置字符串开始截取的索引
            lineIndex = tempStr.Length - "<br>".Length;
        }

        return tempStr;
    }

    /// <summary>
    /// 计算该文本信息显示需要多少个空格
    /// </summary>
    public static int GetStrSpaceAmount(string message)
    {
        if (string.IsNullOrEmpty(message))
            return 0;

        // 匹配<>内的信息
        Regex reg = new Regex(@"(?<=<split>).*?(?=</split>)");

        Match m = reg.Match(message);

        // 该字符串中没有需要匹配的内容
        if (!m.Success)
            return 0;

        int index = 0;

        int amount = 0;

        int endIndex = 0;
        while (m.Success)
        {
            if (index > 0)
            {
                if (m.Index - "<split>".Length - endIndex != 1)
                    amount += 2;
                else
                    amount += 1;
            }
            else
            {
                if (m.Index - "<split>".Length > 0)
                    amount += 1;
            }

            endIndex = m.Index + m.Value.Length + "<split>".Length;

            m = m.NextMatch();
            index++;
        }

        if (message.Length > endIndex + 1)
            amount += 1;

        return amount;
    }

    /// <summary>
    /// 添加屏蔽用户
    /// </summary>
    public static void AddShieldUser(string userRid, bool isFilter = true)
    {
        if (string.IsNullOrEmpty(userRid))
            return;

        if (mShieldList.Contains(userRid))
            return;

        mShieldList.Add(userRid);

        //从缓存消息中过滤剔除屏蔽列表中的玩家
        if (isFilter)
            FilterChaterByChatType(ChatConfig.WORLD_CHANNEL);
    }

    /// <summary>
    /// 移除屏蔽的用户
    /// </summary>
    public static void RemoveShieldUser(string userRid)
    {
        if (string.IsNullOrEmpty(userRid))
            return;

        if (!mShieldList.Contains(userRid))
            return;

        mShieldList.Remove(userRid);
    }

    #endregion
}
