using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LPC;

/// <summary>
/// 菜单管理
/// </summary>
public class MenuMgr
{
    /// <summary>
    /// 菜单关闭的回调
    /// </summary>
    public delegate void MenuClosed();
    public static event MenuClosed MenuClosedHandler;

    /// <summary>
    /// 解析当前菜单
    /// 分析ME/current_menu数据，并将结果写入到ME/current_menu/parsed
    /// 分析规则：
    /// 复制ME_D/current_menu/data到info中，然后将content和后续的选项
    /// 打断，前部分作为纯提示内容保存到info/content中， 后续内容作为
    /// 可选项进行必要的解析，字段提取，生成选项数组保存到 info/items
    /// 中。
    /// 分析结束以后，将info写入到ME/current_menu/parsed中
    /// </summary>
    public static void Parse()
    {
        LPCValue v = ME.dbase.Query("current_menu");
        if (v == null || ! v.IsMapping)
        {
            LogMgr.Trace("[MenuM.cs] 没有当前菜单，不分析，直接返回");
            return;
        }

        // 不允许重复分析
        Debug.Assert(v.AsMapping["parsed"] == null);

        // 开始解析
        float speed = 0;
        string soundName = string.Empty;
        if (v.AsMapping["speed"] != null && v.AsMapping["speed"].IsFloat)
            speed = v.AsMapping["speed"].AsFloat;
        if (v.AsMapping["sound"] != null && v.AsMapping["sound"].IsString)
            soundName = v.AsMapping["sound"].AsString;
        ME.dbase.Set("current_menu/parsed", Parse(v.AsMapping["data"], v.AsMapping["content"].AsString, speed, soundName));
        LogMgr.Trace("[MenuM.cs] 解析菜单：{0}", CurrMenu.ToString());
    }

    /// <summary>
    /// 当前菜单信息(解析后的)
    /// </summary>
    public static LPCValue CurrMenu
    {
        get
        {
            return ME.dbase.Query("current_menu/parsed");
        }
    }

    public static LPCValue Parse(LPCValue data, string content, float speed, string soundName)
    {
        LPCValue info = LPCValue.CreateMapping();
        int index_start;
        int index_end;
        int index_item;
        //LPCValue menu_info = LPCValue.CreateMapping();
        string parsed_content = "";
        string parsed_tip = "";
        string parsed_items = "";

        content = content.Replace("<br/>", "\\n");
        content = content.Replace("<BR/>", "\\n");
        do
        {
            index_item = content.IndexOf("<ITEM");
            if (index_item == -1)
            {
                //
                parsed_content = content;

                // Undefined
                parsed_items = "";
                break;
            }

            index_start = content.IndexOf("<DIALOG>");
            if (index_start == -1)
            {
                parsed_content = content.Substring(0, index_item);
                parsed_items = content.Substring(index_item);
                break;
            }

            index_end = content.IndexOf("</DIALOG>");
            index_end += "</DIALOG>".Length;

            if (index_end > index_start)
            {
                throw new Exception();
            }

            if (index_start == 0)
            {
                parsed_content = content.Substring(0, index_end);
                parsed_items = content.Substring(index_end);
                break;
            }

            if (index_end == content.Length - 1)
            {
                parsed_content = content.Substring(index_start);
                parsed_items = content.Substring(0, index_start);
            }

            parsed_content = content.Substring(index_start, index_end - index_start);
            parsed_items = content.Substring(0, index_start) + content.Substring(index_end);
        } while (false);

        if (parsed_content.IndexOf("<DIALOG>") != -1)
        {
            parsed_content = parsed_content.Substring("<DIALOG>".Length);
        }

        // 2、Parse menu content
        LPCValue dialog = data.AsMapping["dialog"];
        if ((dialog != null) && ! dialog.IsUndefined)
        {
            parsed_content = dialog.AsString.Replace("<br/>", "\\n");;
        }

        LPCValue tip = data.AsMapping["tip"];
        if ((tip != null) && ! tip.IsUndefined)
        {
            parsed_tip = tip.AsString.Replace("<br/>", "\\n");;
            // NPC POSITION
            // to be added
        }

        if (parsed_content.Length > 0)
        {
            // $N
            // $n
            // $R
            // $r
            // $S
            // $s
            // $P
            // $p
            //
        }

        // parse items
        LPCValue arr = ParseMenuItems(parsed_items);

        // info
        // Content
        info.AsMapping.Add("content", LPCValue.Create(parsed_content));

        // tip
        info.AsMapping.Add("tip", LPCValue.Create(parsed_tip));

        // items
        info.AsMapping.Add("items", arr);

        foreach (object k in data.AsMapping.Keys)
        {
            if (k is string)
            {
                string strKey = (string) k;
                if ((strKey == "dialog") || (strKey == "tip"))
                    continue;

                info.AsMapping.Add(strKey, data.AsMapping[strKey]);
            }
        }
        if (speed > 0)
            info.AsMapping.Add("speed", LPCValue.Create(speed));
        if (!string.IsNullOrEmpty(soundName))
            info.AsMapping.Add("sound", LPCValue.Create(soundName));
        return info;
    }

    private static LPCValue ParseMenuItems(string item_text)
    {
        LPCValue arr = LPCValue.CreateArray();

        string xmlStr = "<ITEMS>" + item_text + "</ITEMS>";
        XmlDocument doc = new XmlDocument();
        try
        {
            doc.LoadXml(xmlStr);
        }
        catch (Exception e)
        {
            LogMgr.Trace("[MenuM.cs] 解析菜单出错了：{0}", e.ToString());
            return arr;
        }
        XmlNode itemsNode = doc.SelectSingleNode("ITEMS");
        System.Diagnostics.Debug.Assert(itemsNode != null);

        XmlNodeList nodeList = itemsNode.ChildNodes;
        foreach (XmlNode nd in nodeList)
        {
            System.Diagnostics.Debug.Assert(nd.HasChildNodes);
            int count = nd.Attributes.Count;
            if (count < 1)
                continue;

            LPC.LPCValue v = LPC.LPCValue.CreateMapping();
            foreach (XmlAttribute attr in nd.Attributes)
            {
                if (attr.Name == "type")
                {
                    v.AsMapping.Add(attr.Name, LPC.LPCValue.Create(attr.Value));
                    continue;
                }

                if (attr.Name == "arg")
                {
                    char[] sep = new char[]{'&'};
                    string[] result = attr.Value.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string kvp in result)
                    {
                        // kvp format: key=value
                        int index_equal = kvp.IndexOf('=');
                        if (index_equal == -1)
                            continue;

                        string key = kvp.Substring(0, index_equal);
                        string val = kvp.Substring(index_equal + 1);
                        v.AsMapping.Add(key, LPC.LPCValue.Create(val));
                    }
                    continue;
                }
            }

            if (! v.AsMapping.ContainsKey("name"))
            {
                v.AsMapping.Add("name", LPCValue.Create(nd.FirstChild.Value));
            }

            if (! v.AsMapping.ContainsKey("index"))
            {
                v.AsMapping.Add("index", LPCValue.Create("2147483647"));
            }

            arr.AsArray.Add(v);
        }

        arr.AsArray.Sort(CompareByIndex);
        return arr;
    }

    public static int CompareByIndex(LPCValue x, LPCValue y)
    {
        int xi = Convert.ToInt32(x.AsMapping["index"].AsString);
        int yi = Convert.ToInt32(y.AsMapping["index"].AsString);

        if (xi < yi)
            return -1;

        if (xi > yi)
            return 1;

        return 0;
    }

    /// <summary>
    /// 选择菜单
    /// </summary>
    public static void SelectMenuItem(LPC.LPCValue v)
    {
        if (v == null || ! v.IsMapping)
            return;
    }

    /// <summary>
    /// 关闭菜单
    /// </summary>
    public static void CloseMenu()
    {
        if (MenuClosedHandler != null)
            MenuClosedHandler();
    }
}
