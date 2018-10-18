using System;
using System.Collections.Generic;
using LPC;
using System.Diagnostics;

/// <summary>
/// 数据包的解析
/// </summary>
public class PktAnalyser
{
    /// <summary>
    /// 解包时的封装类型
    /// </summary>
    public enum EncapType
    {
        ET_NONE,
        ET_ARRAY,
        ET_MAPPING,
    }

    /// <summary>
    /// 类型
    /// </summary>
    private enum MatchType
    {
        MT_INT8,          // "int8"
        MT_INT16,         // "int16"
        MT_INT32,         // "int32"
        MT_UINT8,         // "uint8"
        MT_UINT16,        // "uint16"
        MT_UINT32,        // "uint32"
        MT_STRING,        // "string"
        MT_STRING2,       // "string2"
        MT_STRING4,       // "string4"
        MT_BUFFER,        // "buffer"
        MT_BUFFER2,       // "buffer2"
        MT_BUFFER4,       // "buffer4"
        MT_MAP,           // "map"
        MT_MIXED,         // "mixed"
        MT_USER_DEFINED,  // 用户自定义类型
    }

    // 静态构建器
    static PktAnalyser()
    {
        // 初始化一些数据
        m_CmdFilterMap ["internal_comm"] = CommConfig.INTERNAL_COMM;
        m_CmdFilterMap ["client_user"] = CommConfig.CLIENT_USER;
        m_CmdFilterMap ["from_server"] = CommConfig.FROM_SERVER;

        // 定义一些基础类型定义
        AddPrimitvePktDef("int8", MatchType.MT_INT8);
        AddPrimitvePktDef("int16", MatchType.MT_INT16);
        AddPrimitvePktDef("int32", MatchType.MT_INT32);
        AddPrimitvePktDef("uint8", MatchType.MT_UINT8);
        AddPrimitvePktDef("uint16", MatchType.MT_UINT16);
        AddPrimitvePktDef("uint32", MatchType.MT_UINT32);
        AddPrimitvePktDef("string", MatchType.MT_STRING);
        AddPrimitvePktDef("string2", MatchType.MT_STRING2);
        AddPrimitvePktDef("string4", MatchType.MT_STRING4);
        AddPrimitvePktDef("buffer", MatchType.MT_BUFFER);
        AddPrimitvePktDef("buffer2", MatchType.MT_BUFFER2);
        AddPrimitvePktDef("buffer4", MatchType.MT_BUFFER4);
        AddPrimitvePktDef("map", MatchType.MT_MAP);
        AddPrimitvePktDef("mixed", MatchType.MT_MIXED);
    }


    #region 包解析相关定义

    private class PktMember
    {
        public string ArgName = "";                                // 成员的名称
        public bool IsArray = false;                               // 是否是array
        public bool IsOptional = false;                            // 是否是可选的
        public PktDef ArgDef = null;                               // 成员的类型定义
    }

    private class PktDef
    {
        public string DefName = "";                                // 类型定义的名称
        public MatchType MType = MatchType.MT_USER_DEFINED;        // 基础类型还是自定义类型
        public EncapType EType = EncapType.ET_NONE;                // 自己成员的封装方式
        public List<PktMember> Members = null;                     // 成员列表
    }

    private class PktField
    {
        public int Index = 0;                                      // 字段的索引
        public string Name = "";                                   // 字段名称
        public string Desc = "";                                   // 字段描述
        public PktDef FiledDef = null;                             // 字段类型定义
    }

    private class PktInterface
    {
        public int MsgNo = 0;                                      // 消息标号
        public UInt32 CmdFilter = 0;                               // 消息过滤
        public PktDef InterfaceDef = null;                         // 接口类型定义
    }

    /// <summary>
    /// 根据消息编号很快地查询到PktInterface
    /// </summary>
    private static Dictionary<int, PktInterface> m_PktInterfaceMap = new Dictionary<int, PktInterface>();

    /// <summary>
    /// 通过定义的名称很快地索引到PktDef
    /// </summary>
    private static Dictionary<string, PktDef> m_PktDefMap = new Dictionary<string, PktDef>();

    /// <summary>
    /// 通过字段名称很快地索引到PktField
    /// </summary>
    private static Dictionary<string, PktField> m_PktFieldMap = new Dictionary<string, PktField>();

    /// <summary>
    /// 通过字段编号很快地索引到PktField
    /// </summary>
    private static Dictionary<int, PktField> m_PktIndexFiledMap = new Dictionary<int, PktField>();

    #endregion

    #region 解析通信文件时的相关逻辑接口

    /// <summary>
    /// 增加一个类型定义
    /// 范例："encap:array     REQ_ATTR = string attrib, uint8 operator, int32 expect"
    /// </summary>
    /// <param name="content">参见上面的那个范例</param>
    private static void AddDef(string content)
    {
        Debug.Assert(content.StartsWith("encap:"));

        string[] parts = content.Split('=');
        Debug.Assert(parts.Length == 2);

        string[] parts2 = parts [0].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        Debug.Assert(parts2.Length == 2);

        // 获取解析到的数据
        string encapType = parts2 [0].Trim();
        string name = parts2 [1].Trim();
        string members = parts [1].Trim();

        // 创建这个定义类型
        PktDef def = new PktDef();
        def.DefName = name;
        def.MType = MatchType.MT_USER_DEFINED;

        if (encapType == "encap:array")
            def.EType = EncapType.ET_ARRAY;
        else if (encapType == "encap:mapping")
            def.EType = EncapType.ET_MAPPING;
        else if (encapType == "encap:none")
            def.EType = EncapType.ET_NONE;
        else
            throw new Exception("Bad format: " + content);

        // 解析参数列表
        def.Members = DeriveArguments(members);

        // 将这个定义记录下来
        m_PktDefMap [def.DefName] = def;
    }

    /// <summary>
    /// 增加一个字段定义
    /// 范例："string          rid(77);                // RID"
    /// </summary>
    /// <param name="index">在上例中，为77</param>
    /// <param name="name">在上例中，为rid</param>
    /// <param name="desc">在上例中，为RID</param>
    /// <param name="defName">在上例中，为string</param>
    private static void AddField(int index, string name, string desc, string defName)
    {
        // 如果这个名字的字段已经定义过了，直接覆盖
        //if (m_PktFieldMap.ContainsKey(name))
        //    throw new Exception("Alread exist: " + name);

        // 先查找出这个定义名称对应的对象
        PktDef def = m_PktDefMap [defName];

        // 创建出这种类型的字段对象
        PktField field = new PktField();
        field.Index = index;
        field.Name = name;
        field.Desc = desc;
        field.FiledDef = def;

        // 将其添加到映射表中
        m_PktFieldMap [name] = field;
        m_PktIndexFiledMap [index] = field;
        Debug.Assert(m_PktFieldMap.Count == m_PktIndexFiledMap.Count);
    }

    /// <summary>
    /// 增加一个消息接口定义
    /// 范例："void cmd_admin_clone(string class_id_name, int32 amount)"
    /// </summary>
    /// <param name="cmdFilter">消息过滤掩码(服务端到客户端、客户端到服务端、服务器内部消息)</param>
    /// <param name="content">参见上面的范例中的格式</param>
    /// <param name="type">解包时的封装类型</param>
    /// <param name="attribs">额外附加属性集合</param>
    private static void AddInterface(UInt32 cmdFilter, string content, EncapType type, List<string> attribs)
    {
        // 解析出各个部分的信息
        Debug.Assert(content.StartsWith("void "));
        string line = content.Substring("void ".Length);
        string msg = CutString(line, out line, '(').Trim();
        string members = line.Substring(1, line.LastIndexOf(')') - 1).Trim();

        // 通过查询其它模块，获知这个消息对应的消息号是多少
        int msgNo = MsgMgr.MsgID(msg.ToUpper());

        // 生成这个PktInterface对象
        PktInterface pktInt = new PktInterface();
        pktInt.CmdFilter = cmdFilter;
        pktInt.MsgNo = msgNo;
        pktInt.InterfaceDef = new PktDef();

        pktInt.InterfaceDef.DefName = "*";
        pktInt.InterfaceDef.EType = type;
        pktInt.InterfaceDef.MType = MatchType.MT_USER_DEFINED;
        pktInt.InterfaceDef.Members = DeriveArguments(members);

        // 将这个接口信息记录下来
        m_PktInterfaceMap [pktInt.MsgNo] = pktInt;

        // 如果有额外的属性，也记录下来
        if (attribs.Count > 0)
        {
            List<string> attr = new List<string>(attribs);
            m_CmdAttributeMap [msgNo] = attr;
        }
    }

    /// <summary>
    /// 给定字符串，解析出member信息
    /// </summary>
    /// <param name="content">需要解析的字符串内容</param>
    /// <returns>member列表</returns>
    private static List<PktMember> DeriveArguments(string content)
    {
        string[] args = content.Split(',');
        if (args.Length >= 16)
            throw new Exception(string.Format("Too many ({0}) arguments.", args.Length));

        List<PktMember> list = new List<PktMember>();
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args [i].Trim();
            if (arg.Length <= 0)
                continue;

            string[] parts = arg.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(parts.Length == 2 || (parts.Length == 3 && parts [0].Trim() == "optional"));

            // 创建一个member对象
            PktMember member = new PktMember();

            // 析取出类型和名称
            string argType = "";
            string argName = "";
            if (parts.Length == 2)
            {
                argType = parts [0].Trim();
                argName = parts [1].Trim();
                member.IsOptional = false;
            } else
            {
                Debug.Assert(parts [0].Trim() == "optional");
                argType = parts [1].Trim();
                argName = parts [2].Trim();
                member.IsOptional = true;
            }

            // 处理是不是封装成array
            if (argName.EndsWith("[]"))
            {
                member.IsArray = true;
                argName = argName.Substring(0, argName.Length - 2);
            }

            // 找到这个定义
            member.ArgDef = m_PktDefMap [argType];
            member.ArgName = argName;

            // 将这个成员变量加入到列表中
            list.Add(member);
        }

        // 最终返回匹配到的列表
        return list;
    }

    #endregion

    /// <summary>
    /// 加载通信描述文件(可以多次调用，从而加载多张表中的信息)
    /// </summary>
    public static bool LoadCommDescFile(string fileContent, EncapType type)
    {
        // 将文件内容读入进来
        if (fileContent == "")
            return false;

        // 解析表的内容
        string[] lines = fileContent.Split('\n');
        UInt32 cmdFilter = 0;
        List<string> attribs = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines [i].Trim();

            // 如果为空行或者是注释行，就跳过
            if (line.Length <= 0 || line [0] == '#')
                continue;

            // 如果含有 :: 符号，需要解析到后面的
            attribs.Clear();
            if (line.Contains("::"))
            {
                string[] parts = line.Split(new string[1] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new Exception("Bad format: " + line);

                // 这里的额外信息需要记录下来
                string[] parts2 = parts [1].Split('/');
                attribs.AddRange(parts2);

                // 赋值到line上，接下来还要继续解析
                line = parts [0];
            }

            // 如果是限定符号，比如 [Client_user], [from_server/internal_comm]
            if (line [0] == '[' && line [line.Length - 1] == ']')
            {
                cmdFilter = 0;
                string tmp = line.Substring(1, line.Length - 2);
                foreach (string desc in tmp.Split('/'))
                {
                    if (!m_CmdFilterMap.ContainsKey(desc.ToLower()))
                        throw new Exception("Bad format: " + line);
                    cmdFilter |= m_CmdFilterMap [desc.ToLower()];
                }

                // 这行已经解析完毕，跳过，处理下一行
                continue;
            }
            // 如果是定义，比如："encap:none  INT_ARRAY = int32  numeric[]"
            else if (line.StartsWith("encap:"))
            {
                AddDef(line);

                // 这行已经解析完毕，跳过，处理下一行
                continue;
            }
            // 如果是消息体定义，比如："void cmd_catch_mount(string target, string cookie);"
            else if (line.StartsWith("void"))
            {
                AddInterface(cmdFilter, line, type, attribs);

                // 这行已经解析完毕，跳过，处理下一行
                continue;
            }
            // 其它情况算是字段定义，比如："uint8  gang_station(800);  // 帮派职务"
            else
            {
                string str = line;
                string defName = CutString(str, out str, ' ').Trim();
                string fieldName = CutString(str, out str, '(').Trim();
                string tk = CutString(str, out str, ')').Trim();
                int index = int.Parse(tk.Substring(1, tk.Length - 1));
                string desc = str.Substring(str.IndexOf("//") + 2).Trim();

                AddField(index, fieldName, desc, defName);

                // 这行已经解析完毕，跳过，处理下一行
                continue;
            }
        }

        return true;
    }

    /// <summary>
    /// 判断在这个filter下指定的消息是否合法
    /// </summary>
    /// <param name="msgNo">消息编号</param>
    /// <param name="cmdFilter">消息过滤掩码</param>
    /// <returns>如果这个组合合法，返回true；否则返回false</returns>
    public static bool IsValidMessage(int msgNo, UInt32 cmdFilter)
    {
        if (!m_PktInterfaceMap.ContainsKey(msgNo))
            return false;
        return true;
    }

    /// <summary>
    /// 获取指定消息的额外属性。即 ::sync/combat 部分的信息。
    /// </summary>
    /// <param name="msgNo">消息编号</param>
    /// <returns>如果这个消息有额外属性，则返回这个属性列表；否则返回null</returns>
    public static List<string> GetAttributes(int msgNo)
    {
        if (!m_CmdAttributeMap.ContainsKey(msgNo))
            return null;
        return m_CmdAttributeMap [msgNo];
    }

    /// <summary>
    /// 打包一个数据包(会根据消息编号，找到对应的描述，打包的时候会检查类型是否匹配)
    /// </summary>
    /// <param name="msgNo">消息编号</param>
    /// <param name="arrOrMapValue">需要打包的数据，只能是array或者mapping</param>
    /// <returns>返回打包好的数据，头两个字节是消息编号</returns>
    public static byte[] Pack(int msgNo, LPCValue arrOrMapValue)
    {
        // 获取该消息编号对应的用户类型
        PktInterface pktInt = m_PktInterfaceMap [msgNo];
        Debug.Assert(pktInt.InterfaceDef.MType == MatchType.MT_USER_DEFINED);

        // 递归打包这个值
        int len = PadUserDefinedValue(m_Buffer, 0, pktInt.InterfaceDef, arrOrMapValue);

        // 分配一段缓冲，将pack好的数据拷贝到新缓冲中
        byte[] buffer = new byte[len];
        System.Buffer.BlockCopy(m_Buffer, 0, buffer, 0, len);

        // 将最终包含数据的缓冲返回给调用者
        return buffer;
    }

    /// <summary>
    /// 解包用 Pack 接口产生的数据。
    /// 注意：这个接口解包失败时会触发异常。外部调用者有责任捕获这个异常。
    /// </summary>
    /// <param name="msgNo">消息编号</param>
    /// <param name="buffer">数据缓冲区</param>
    /// <returns>如果解包成功，那么返回解包出来的数据；如果失败(包含输入数据异常)，会抛异常。</returns>
    public static LPCValue Unpack(int msgNo, byte[] buffer)
    {
        // 查映射表，获取该消息编号对应的接口定义(如果没有，让它抛异常)
        PktInterface pktInt = null;
        if (! m_PktInterfaceMap.TryGetValue(msgNo, out pktInt))
            return null;

        // 接口上的类型一定是用户定义类型
        Debug.Assert(pktInt.InterfaceDef.MType == MatchType.MT_USER_DEFINED);

        // 调用递归函数，解析这个网络包
        int offset = 0;
        LPCValue result = MatchPktArgument(pktInt.InterfaceDef, buffer, ref offset);

        // 返回最终的结果
        return result;
    }

    #region 内部实现接口

    /// <summary>
    /// 将用户定义类型pack到指定的缓冲区内
    /// </summary>
    /// <returns>成功保存了多少个字节到指定的缓冲区内。</returns>
    private static int PadUserDefinedValue(byte[] buf, int offset, PktDef pktDef, LPCValue value)
    {
        Debug.Assert(pktDef.MType == MatchType.MT_USER_DEFINED);

        int skip = 0;

        // 检查一下类型
        if (pktDef.EType == EncapType.ET_ARRAY)
        {
            if (!value.IsArray)
                throw new Exception(string.Format("Bad encapsulation type, expected array got {0} for \"{1}\".\n", value.type, pktDef.DefName));
        } else if (pktDef.EType == EncapType.ET_MAPPING)
        {
            if (!value.IsMapping)
                throw new Exception(string.Format("Bad encapsulation type, expected mapping got {0} for \"{1}\".\n", value.type, pktDef.DefName));
        } else
        {
            Debug.Assert(pktDef.EType == EncapType.ET_NONE);
            Debug.Assert(pktDef.Members.Count == 1);
        }

        // 创建一个临时的undefined值
        LPCValue tmpUndefined = LPCValue.Create();

        // 处理每一个成员
        for (int i = 0; i < pktDef.Members.Count; i++)
        {
            LPCValue subValue = null;

            // 根据不同的情况，将sub值指向不同的地方
            if (pktDef.EType == EncapType.ET_ARRAY)
            {
                if (i < value.AsArray.Count)
                    subValue = value.AsArray [i];
                else
                    subValue = tmpUndefined;
            } else if (pktDef.EType == EncapType.ET_MAPPING)
            {
                if (value.AsMapping.ContainsKey(pktDef.Members [i].ArgName))
                    subValue = value.AsMapping [pktDef.Members [i].ArgName];
                else
                    subValue = tmpUndefined;
            } else
            {
                subValue = value;
            }

            // 处理是否具备数组属性
            if (pktDef.Members [i].IsArray)
            {
                if (subValue.IsArray)
                {
                    // 如果值本来就是数组，那么我们按数组打包
                    int _skip = LPCUtil._PAD_16(subValue.AsArray.Count, buf, offset);
                    skip += _skip;
                    offset += _skip;

                    for (int j = 0; j < subValue.AsArray.Count; j++)
                    {
                        _skip = PadDefValue(buf, offset, pktDef.Members [i].ArgDef, subValue.AsArray [j]);
                        skip += _skip;
                        offset += _skip;
                    }
                } else if (subValue.IsBuffer)
                {
                    // 缓冲区，当已经pad过的数组值处理
                    int _skip = LPCUtil._PAD_BUFFER(subValue.AsBuffer, buf, offset);
                    skip += _skip;
                    offset += _skip;
                } else if (subValue.AsInt == 0)
                {
                    // 空数组
                    int _skip = LPCUtil._PAD_16(0, buf, offset);
                    skip += _skip;
                    offset += _skip;
                }
            } else
            {
                // 单值
                int _skip = PadDefValue(buf, offset, pktDef.Members [i].ArgDef, subValue);
                skip += _skip;
                offset += _skip;
            }
        }

        return skip;
    }

    /// <summary>
    /// 将一个值pack到指定的缓冲区内
    /// </summary>
    /// <returns>成功保存了多少个字节到指定的缓冲区内。</returns>
    private static int PadDefValue(byte[] buf, int offset, PktDef pktDef, LPCValue value)
    {
        int skip = 0;
        switch (pktDef.MType)
        {
            case MatchType.MT_INT8:
                {
                    skip = LPCUtil._PAD_8(value.IsInt ? value.AsInt : 0, buf, offset);
                    return skip;
                }
            case MatchType.MT_INT16:
                {
                    skip = LPCUtil._PAD_16(value.IsInt ? value.AsInt : 0, buf, offset);
                    return skip;
                }
            case MatchType.MT_INT32:
                {
                    skip = LPCUtil._PAD_32(value.IsInt ? value.AsInt : 0, buf, offset);
                    return skip;
                }
            case MatchType.MT_UINT8:
                {
                    skip = LPCUtil._PAD_8(value.IsInt ? value.AsInt : 0, buf, offset);
                    return skip;
                }
            case MatchType.MT_UINT16:
                {
                    skip = LPCUtil._PAD_16(value.IsInt ? value.AsInt : 0, buf, offset);
                    return skip;
                }
            case MatchType.MT_UINT32:
                {
                    skip = LPCUtil._PAD_32(value.IsInt ? value.AsInt : 0, buf, offset);
                    return skip;
                }
            case MatchType.MT_STRING:
                {
                    if (!value.IsString)
                    {
                        skip = LPCUtil._PAD_8(0, buf, offset);
                        return skip;
                    } else
                    {
                        skip = LPCUtil._PAD_LEN_STRING(value.AsString, buf, offset);
                        return skip;
                    }
                }
            case MatchType.MT_STRING2:
                {
                    if (!value.IsString)
                    {
                        skip = LPCUtil._PAD_16(0, buf, offset);
                        return skip;
                    } else
                    {
                        skip = LPCUtil._PAD_LEN_STRING2(value.AsString, buf, offset);
                        return skip;
                    }
                }
            case MatchType.MT_STRING4:
                {
                    if (!value.IsString)
                    {
                        skip = LPCUtil._PAD_32(0, buf, offset);
                        return skip;
                    } else
                    {
                        //int len = value.AsString.Length;
                        skip = LPCUtil._PAD_LEN_STRING4(value.AsString, buf, offset);
                        return skip;
                    }
                }
            case MatchType.MT_BUFFER:
                {
                    if (!value.IsBuffer)
                    {
                        skip = LPCUtil._PAD_8(0, buf, offset);
                        return skip;
                    } else
                    {
                        int len = value.AsBuffer.Length;
                        if (len > 255)
                            len = 255;

                        int _skip = LPCUtil._PAD_8(len, buf, offset);
                        skip += _skip;
                        offset += _skip;

                        _skip = LPCUtil._PAD_BUFFER(value.AsBuffer, buf, offset);
                        skip += _skip;
                        offset += _skip;
                        return skip;
                    }
                }
            case MatchType.MT_BUFFER2:
                {
                    if (!value.IsBuffer)
                    {
                        skip = LPCUtil._PAD_16(0, buf, offset);
                        return skip;
                    } else
                    {
                        int len = value.AsBuffer.Length;
                        if (len > 65536)
                            len = 65536;

                        int _skip = LPCUtil._PAD_16(len, buf, offset);
                        skip += _skip;
                        offset += _skip;

                        _skip = LPCUtil._PAD_BUFFER(value.AsBuffer, buf, offset);
                        skip += _skip;
                        offset += _skip;
                        return skip;
                    }
                }
            case MatchType.MT_BUFFER4:
                {
                    if (!value.IsBuffer)
                    {
                        skip = LPCUtil._PAD_32(0, buf, offset);
                        return skip;
                    } else
                    {
                        int len = value.AsBuffer.Length;

                        int _skip = LPCUtil._PAD_32(len, buf, offset);
                        skip += _skip;
                        offset += _skip;

                        _skip = LPCUtil._PAD_BUFFER(value.AsBuffer, buf, offset);
                        skip += _skip;
                        offset += _skip;
                        return skip;
                    }
                }
            case MatchType.MT_MIXED:
                {
                    // 先pad一个临时长度，此时还不清楚多少，我们稍后填上
                    int _skip = LPCUtil._PAD_32(0, buf, offset);

                    // pad数据
                    int len = LPCValue.SaveToBuffer(buf, offset + 4, value);

                    // 我们将长度回填一下
                    LPCUtil._PAD_32(len, buf, offset);

                    skip += _skip;
                    skip += len;
                    return skip;
                }
            case MatchType.MT_MAP:
                {
                    if (value.IsBuffer)
                    {
                        // 如果是缓冲，那么说明mapping已经被pad过了
                        skip = LPCUtil._PAD_BUFFER(value.AsBuffer, buf, offset);
                        return skip;
                    }

                    if (!value.IsMapping)
                    {
                        // 不是mapping类型，我们理解为空mapping
                        skip = LPCUtil._PAD_16(0, buf, offset);
                        return skip;
                    }

                    skip = PadMap(buf, offset, value);
                    return skip;
                }
            case MatchType.MT_USER_DEFINED:
                {
                    int _skip = PadUserDefinedValue(buf, offset, pktDef, value);
                    skip += _skip;
                    return skip;
                }
            default:
                throw new Exception("不识别的类型 " + pktDef.MType);
        }
    }

    /// <summary>
    /// 将一个mapping pack到指定的缓冲区内
    /// </summary>
    /// <returns>成功保存了多少个字节到指定的缓冲区内。</returns>
    private static int PadMap(byte[] buf, int offset, LPCValue value)
    {
        Debug.Assert(value.IsMapping);

        int count = 0;
        int skip = 0;

        // 当前我们不知道map中有多少有效数据，暂时先填写一个值占着位置
        int oldOffset = offset;
        int _skip = LPCUtil._PAD_16(count, buf, oldOffset);
        skip += _skip;
        offset += _skip;

        foreach (object key in value.AsMapping.Keys)
        {
            // 我们只处理key为字符串类型的
            if (!(key is string))
                continue;

            string fieldName = key as string;

            // 如果这个字段之前没有定义过，那么不处理
            if (!m_PktFieldMap.ContainsKey(fieldName))
                continue;

            // 又多了一个有效的成员
            count++;

            PktField pktField = m_PktFieldMap [fieldName];

            // pad index值作为key
            _skip = LPCUtil._PAD_16(pktField.Index, buf, offset);
            skip += _skip;
            offset += _skip;

            // pad value值
            _skip = PadDefValue(buf, offset, pktField.FiledDef, value.AsMapping [fieldName]);
            skip += _skip;
            offset += _skip;
        }

        // 好，此时我知道有多少个count了，需要将这个信息回填一下
        LPCUtil._PAD_16(count, buf, oldOffset);

        // 返回一个保存了多少个字节
        return skip;
    }

    /// <summary>
    /// 递归调用，解析传入的网络包
    /// </summary>
    /// <param name="pktDef">最前一个的类型定义</param>
    /// <param name="buffer">消息缓冲</param>
    /// <returns>解析回来的数据</returns>
    private static LPCValue MatchPktArgument(PktDef pktDef, byte[] buffer, ref int off)
    {
        Debug.Assert(off <= buffer.Length);
        switch (pktDef.MType)
        {
            case MatchType.MT_INT8:
                {
                    int val = LPCUtil._GET_8(buffer, ref off);
                    return LPCValue.Create((int)val);
                }
            case MatchType.MT_INT16:
                {
                    Int16 val = LPCUtil._GET_16(buffer, ref off);
                    return LPCValue.Create((int)val);
                }
            case MatchType.MT_INT32:
                {
                    Int32 val = LPCUtil._GET_32(buffer, ref off);
                    return LPCValue.Create((int)val);
                }
            case MatchType.MT_UINT8:
                {
                    Byte val = LPCUtil._GET_U8(buffer, ref off);
                    return LPCValue.Create((int)val);
                }
            case MatchType.MT_UINT16:
                {
                    UInt16 val = LPCUtil._GET_U16(buffer, ref off);
                    return LPCValue.Create((int)val);
                }
            case MatchType.MT_UINT32:
                {
                    UInt32 val = LPCUtil._GET_U32(buffer, ref off);
                    return LPCValue.Create((int)val);
                }
            case MatchType.MT_STRING:
                {
                    int len = LPCUtil._GET_U8(buffer, ref off);
                    string val = LPCUtil._GET_STR(buffer, ref off, len);
                    return LPCValue.Create(val);
                }
            case MatchType.MT_STRING2:
                {
                    int len = LPCUtil._GET_U16(buffer, ref off);
                    string val = LPCUtil._GET_STR(buffer, ref off, len);
                    return LPCValue.Create(val);
                }
            case MatchType.MT_STRING4:
                {
                    int len = (int)LPCUtil._GET_U32(buffer, ref off);
                    string val = LPCUtil._GET_STR(buffer, ref off, len);
                    return LPCValue.Create(val);
                }
            case MatchType.MT_BUFFER:
                {
                    int len = LPCUtil._GET_U8(buffer, ref off);
                    byte[] val = new byte[len];
                    System.Buffer.BlockCopy(buffer, off, val, 0, len);
                    off += len;
                    return LPCValue.Create(val);
                }
            case MatchType.MT_BUFFER2:
                {
                    int len = LPCUtil._GET_U16(buffer, ref off);
                    byte[] val = new byte[len];
                    System.Buffer.BlockCopy(buffer, off, val, 0, len);
                    off += len;
                    return LPCValue.Create(val);
                }
            case MatchType.MT_BUFFER4:
                {
                    int len = (int)LPCUtil._GET_U32(buffer, ref off);
                    byte[] val = new byte[len];
                    System.Buffer.BlockCopy(buffer, off, val, 0, len);
                    off += len;
                    return LPCValue.Create(val);
                }
            case MatchType.MT_MIXED:
                {
                    int len = (int)LPCUtil._GET_U32(buffer, ref off);
                    LPCValue val;
                    int cnt = LPCValue.RestoreFromBuffer(buffer, off, out val);
                    Debug.Assert(cnt <= len);
                    off += cnt;
                    return val;
                }
            case MatchType.MT_MAP:
                {
                    int count = LPCUtil._GET_U16(buffer, ref off);

                    LPCValue val = LPCValue.CreateMapping();
                    for (int i = 0; i < count; i++)
                    {
                        int index = LPCUtil._GET_16(buffer, ref off);
                        if (! m_PktIndexFiledMap.ContainsKey(index))
                        {
                            LogMgr.Error("[PktAnalyser.cs] pktanalyser:bad index={0}", index);
                        }

                        PktField pktField = m_PktIndexFiledMap [index];
                        LPCValue subValue = MatchPktArgument(pktField.FiledDef, buffer, ref off);
                        try
                        {
                            val.AsMapping.Add(pktField.Name, subValue);
                        } catch (Exception)
                        {
                            LogMgr.Error("[PktAnalyser.cs] {0} : {1}", pktField.Name, index);
                        }
                    }

                    return val;
                }
            case MatchType.MT_USER_DEFINED:
                {
                    LPCValue val = null;

                    if (pktDef.EType == EncapType.ET_ARRAY)
                    {
                        // 按数组进行封装
                        val = LPCValue.CreateArray();
                    } else if (pktDef.EType == EncapType.ET_MAPPING)
                    {
                        // 按映射进行封装
                        val = LPCValue.CreateMapping();
                    } else
                    {
                        // 按单个值进行封装
                        Debug.Assert(pktDef.Members.Count == 1);
                        val = null;
                    }

                    // 处理每一个成员
                    for (int i = 0; i < pktDef.Members.Count; i++)
                    {
                        LPCValue subVal = null;

                        // 如果是optional关键字修饰；并且不是mapping封装类型；并且已经没有数据可以解析了
                        if (pktDef.Members [i].IsOptional &&
                            pktDef.EType != EncapType.ET_MAPPING &&
                            off >= buffer.Length)
                        {
                            while (i < pktDef.Members.Count)
                            {
                                if (pktDef.Members [i].IsArray)
                                {
                                    // 创建一个空数组
                                    subVal = LPCValue.CreateArray();
                                } else
                                {
                                    // 所有基础类型的大小都不超过8字节
                                    byte[] tempBuf = new byte[8];
                                    int tempOff = 0;
                                    subVal = MatchPktArgument(pktDef.Members [i].ArgDef, tempBuf, ref tempOff);
                                }

                                // 根据不同的封装类型，将值插入进不同的容器
                                if (pktDef.EType == EncapType.ET_ARRAY)
                                    val.AsArray.Add(subVal);
                                else
                                    val = subVal;

                                // 处理下一个值
                                i++;
                            }
                        }

                        // 如果是数组修饰的
                        if (pktDef.Members [i].IsArray)
                        {
                            int arrCnt = LPCUtil._GET_U16(buffer, ref off);

                            subVal = LPCValue.CreateArray();
                            for (int j = 0; j < arrCnt; j++)
                            {
                                LPCValue arrMemberVal = MatchPktArgument(pktDef.Members [i].ArgDef, buffer, ref off);
                                subVal.AsArray.Add(arrMemberVal);
                            }
                        }
                // 如果是单个值的
                else
                        {
                            subVal = MatchPktArgument(pktDef.Members [i].ArgDef, buffer, ref off);
                        }

                        // 根据不同的封装类型，将值插入进不同的容器
                        if (pktDef.EType == EncapType.ET_ARRAY)
                            val.AsArray.Add(subVal);
                        else if (pktDef.EType == EncapType.ET_MAPPING)
                            val.AsMapping.Add(pktDef.Members [i].ArgName, subVal);
                        else
                            val = subVal;
                    }

                    // 返回最终的值
                    return val;
                }
            default:
                {
                    Debug.Assert(false, "什么情况？");
                    throw new Exception("遇到不认识的类型 " + pktDef.MType);
                }
        }
    }

    /// <summary>
    /// 字节数组转换为十六进制字符串
    /// </summary>
    public static string ByteToHexStr(byte[] bytes)
    {
        string returnStr = "";
        if (bytes != null)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                returnStr += bytes [i].ToString("X2");
                if ((i + 1) % 8 == 0)
                    returnStr += "\n";
                else if (i % 8 == 3)
                    returnStr += " - ";
            }
        }
        return returnStr;
    }

    /// <summary>
    /// 工具类函数，截取字符串的一部分
    /// </summary>
    private static string CutString(string line, out string line2, char t)
    {
        int idx = line.IndexOf(t);
        string ret = line.Substring(0, idx);
        line2 = line.Substring(idx);
        return ret;
    }

    /// <summary>
    /// 增加基础类型定义
    /// </summary>
    private static void AddPrimitvePktDef(string name, MatchType type)
    {
        PktDef pktdef = new PktDef();
        pktdef.DefName = name;
        pktdef.MType = type;
        m_PktDefMap [pktdef.DefName] = pktdef;
    }

    #endregion

    /// <summary>
    /// 消息过滤文字描述:消息过滤mask 映射描述表
    /// </summary>
    private static Dictionary<string, UInt32> m_CmdFilterMap = new Dictionary<string, uint>();

    /// <summary>
    /// 消息编号:消息属性(::sync/combat) 映射表
    /// </summary>
    private static Dictionary<int, List<string>> m_CmdAttributeMap = new Dictionary<int, List<string>>();

    /// <summary>
    /// 用于pack时充当临时缓冲区
    /// </summary>
    private static byte[] m_Buffer = new byte[CommConfig.MAX_PACKET_SIZE];
}
