/// <summary>
/// ImportMgr.cs
/// Copy from zhangyg 2014-10-22
/// 对应于LPC的IMPORT_D功能，游戏中理论上不会用到本模块，而是交由编辑器调度
/// </summary>

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;
using UnityEngine;

/// <summary>
/// 对应于LPC的IMPORT_D功能，游戏中理论上不会用到本模块，而是交由编辑器调度
/// </summary>
public class ImportMgr
{
    /// <summary>
    /// 转换为一行一行的数据，返回值为数组, checkValid是否检查列上带*号是有效列
    /// </summary>
    public static LPCValue ImportToArray(string[] lines, bool checkValid)
    {
        CsvParser cp = new CsvParser();
        cp.Load(lines, checkValid);

        return Read(cp);
    }
    public static LPCValue ImportToArray(string file, bool checkValid)
    {
        CsvParser cp = new CsvParser();
        cp.Load(file, checkValid);
        return Read(cp);
    }

    /// <summary>
    /// 转换为mapping数据，key指定为某一列的值, checkValid是否检查列上带*号是有效列
    /// </summary>
    public static LPCValue ImportToIndexMapping(string file, string key, bool checkValid)
    {
        LPCValue m = LPCValue.CreateMapping();
        LPCValue arr = ImportToArray(file, checkValid);

        foreach (LPCValue v in arr.AsArray.Values)
        {
            LPCValue k = v.AsMapping [key];
            if (k.IsInt)
                m.AsMapping.Add(k.AsInt, v);
            else if (k.IsString)
                m.AsMapping.Add(k.AsString, v);
        }
        return m;
    }

    /// <summary>
    /// 取得一列的所有行的值, checkValid是否检查列上带*号是有效列
    /// </summary>
    public static LPCValue ImportToSingleColumn(string file, string field, bool checkValid)
    {
        LPCValue result = LPCValue.CreateArray();
        LPCValue arr = ImportToArray(file, checkValid);
        foreach (LPCValue v in arr.AsArray.Values)
        {
            result.AsArray.Add(v.AsMapping [field]);
        }
        return result;
    }

    /// <summary>
    /// 取得简单的数值对
    /// </summary>
    public static LPCValue ImportToSingleMapping(string file, string kField, string vField, bool checkValid)
    {
        LPCValue result = LPCValue.CreateMapping();
        LPCValue arr = ImportToArray(file, checkValid);
        foreach (LPCValue v in arr.AsArray.Values)
        {
            LPCValue kv = v.AsMapping [kField];
            LPCValue vv = v.AsMapping [vField];

            if (kv.IsInt)
                result.AsMapping.Add(kv.AsInt, vv);
            else if (kv.IsString)
                result.AsMapping.Add(kv.AsString, vv);
        }
        return result;
    }

    public static LPCValue Read(CsvParser cp)
    {
        List<CsvField> fields = cp.Fields;
        List<Dictionary<string, string>> records = cp.Records;

        LPCValue list = LPCValue.CreateArray();
        foreach (Dictionary<string, string> record in records)
        {
            LPCValue m = LPCValue.CreateMapping();
            foreach (CsvField field in fields)
            {
                LPCValue v = Parse(field.type, record [field.name]);
                m.AsMapping.Add(field.name, v);
            }
            list.AsArray.Add(m);
        }

        return list;
    }

    public static LPCValue Read(string file, bool checkValid)
    {
        CsvParser cp = new CsvParser();
        cp.Load(file, checkValid);
        return Read(cp);
    }

    private static LPCValue Parse(string type, string text)
    {
        try
        {
            switch (type)
            {
                case "auto":
                    return ParseAutoType(text);
                case "string":
                    return ParseStringType(text);
                case "int":
                    return ParseIntType(text);
                case "mapping":
                    return ParseMappingType(text);
                case "array":
                    return ParseArrayType(text);
                default:
                    throw new Exception("bad type to parse");
            }
        } catch (Exception e)
        {
            NIDebug.Log("读取csv错误\ntype = {0}, text = {1}", type, text);
            throw e;
        }
    }

    private static LPCValue ParseStringType(string text)
    {
        return LPCValue.Create(text);
    }

    private static LPCValue ParseIntType(string text)
    {
        int i;
        if (!int.TryParse(text, out i))
        {
            NIDebug.LogError("无法将 " + text + " 转换为int");
            i = 0;
        }
        return LPCValue.Create(i);
    }

    private static LPCValue ParseMappingType(string text)
    {
        LPCValue m = LPCValue.CreateMapping();
        if (text == "0")
            return m;

        m = LPCRestoreString.RestoreFromString(text);
        return m;
    }

    private static LPCValue ParseArrayType(string text)
    {
        LPCValue arr = LPCValue.CreateArray();
        if (text == "0")
            return arr;

        arr = LPCRestoreString.RestoreFromString(text);
        return arr;
    }

    private static LPCValue ParseAutoType(string text)
    {
        if (text.Length < 1)
            return LPCValue.Create(text);

        char first_char = text [0];
        if (first_char == '@')
        {
            // alias
            string alias = text.Substring(1);
            if (! AliasMgr.ContainsAlias(alias))
            {
                NIDebug.LogError("字段没有配置别名 " + alias);
                return LPCValue.Create(text);
            }
            object v = AliasMgr.Get(alias);
            if (v is string)
                return LPCValue.Create((string)v);
            if (v is int)
                return LPCValue.Create((int)v);
            throw new Exception(string.Format("Unexpected alias name: {0}", alias));
        }

        if ((first_char == '"') || (first_char == ':'))
        {
            // string or buffer
            return LPCRestoreString.RestoreFromString(text);
        }

        if ((first_char == '(') && (text.Length > 1))
        {
            char second_char = text [1];
            if ((second_char == '{') || (second_char == '['))
            {
                // mapping or array
                return LPCRestoreString.RestoreFromString(text);
            }
        }
        if (((first_char >= '0') && (first_char <= '9')) || (first_char == '.') || (first_char == '-'))
        {
            // number
            return LPCRestoreString.RestoreFromString(text);
        }

        return LPCValue.Create(text);
    }
}

/// <summary>
/// CSV表的列
/// </summary>
/// <author>weism</authr>
public class CsvField
{
    public string name;
    public string type;
    public CsvField(string name, string type)
    {
        this.name = name;
        this.type = type;
    }
}

/// <summary>
/// Csv表的解析器
/// </summary>
public class CsvParser
{
    // 所有列
    private List<CsvField> fields = new List<CsvField>();

    // 数据
    private List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();

    // 主key
    private string primaryKey = "";

    /// <summary>
    /// 解析后的列字段
    /// </summary>
    public List<CsvField> Fields { get { return fields; } }

    /// <summary>
    /// 解析后的各行数据
    /// </summary>
    public List<Dictionary<string, string>> Records { get { return records; } }

    /// <summary>
    /// 主key
    /// </summary>
    public string PrimaryKey { get { return primaryKey; } }

    /// <summary>
    /// 解析一个csv文件，默认section一定载入，如果需要额外的字段，checkValid是否检查列上带*号是有效列
    /// </summary>
    public bool Load(string file, bool checkValid)
    {
        return LoadCsv(file, checkValid, out records, out fields, out primaryKey);
    }
    public bool Load(string[] lines, bool checkValid)
    {
        return Load(lines, checkValid);
    }

    // 载入一个csv文件并解析
    private static bool LoadCsv(string file, bool checkValid,
                                out List<Dictionary<string, string>> records,
                                out List<CsvField> fields,
                                out string primaryKey)
    {
        return ParseLines(FileMgr.ReadLines(file), checkValid, out records, out fields, out primaryKey);
    }

    // 解析csv内容
    private static bool ParseLines(string[] lines, bool checkValid,
                                   out List<Dictionary<string, string>> records,
                                   out List<CsvField> fields,
                                   out string primaryKey)
    {
        // 初始化返回值
        fields = new List<CsvField>();
        records = new List<Dictionary<string, string>>();
        primaryKey = "";
        int valid_line = 0;
        int field_count = 0;
        string[] field_type = null;
        string[] field_name = null;
        List<int> validFields = new List<int>(); // 有效行

        string section_format = "\"section:";
        string comentFields = ""; // 注释列的行，通常在第一条有效行的上方
        foreach (string line in lines)
        {
            if (IsCommentLine(line))
            {
                if (valid_line == 0)
                    comentFields = line;
                continue;
            }

            // section check
            if (line.StartsWith(section_format))
                continue;

            string[] record = FetchFields(line), temprecord;
            bool is_empty_line = true;
            foreach (string v in record)
            {
                if (v.Length > 0)
                {
                    is_empty_line = false;
                    break;
                }
            }
            if (is_empty_line)
            {
                // ,,,,,,, is treated as blank line
                continue;
            }

            valid_line++;
            if (valid_line == 1)
            {
                // first valid line is field type
                field_type = record;
                field_count = field_type.Length;

                continue;
            }

            if (record.Length != field_count)
            {
                throw new Exception("field name does not match field type");
            }

            if (valid_line == 2)
            {
                // second valid line is field name
                // 检查主key和有效行
                string[] columns = FetchFields(comentFields);
                if (columns.Length != field_count)
                {
                    NIDebug.Log("csv表头注释行数量错误!");
                    return false;
                }

                // 获取有效行
                for (int i = 0; i < columns.Length; i++)
                {
                    if (!checkValid || columns [i].IndexOf("*") >= 0)
                        validFields.Add(i);
                }

                if (validFields.Count == 0)
                    // 没有有效列
                    return true;

                field_name = record;
                for (int i = 0; i < validFields.Count; i++)
                {
                    int idx = validFields [i];
                    fields.Add(new CsvField(field_name [idx], field_type [idx]));

                    // 获取主key
                    if (columns [i].IndexOf("!") >= 0)
                        primaryKey = field_name [idx];
                }

                // 如果没有在csv表中配置主key，默认第一列是主key
                if (string.IsNullOrEmpty(primaryKey) && fields.Count > 0)
                    primaryKey = fields [0].name;

                continue;
            }

            if (validFields.Count != record.Length)
            {
                temprecord = new string[validFields.Count];
                for (int i = 0; i < validFields.Count; i++)
                {
                    int idx = validFields [i];
                    temprecord [i] = record [idx];
                }
                record = temprecord;
            }

            Dictionary<string, string> recordMap = new Dictionary<string, string>();
            for (int i = 0; i < fields.Count; i++)
            {
                recordMap.Add(fields [i].name, record [i]);
            }

            records.Add(recordMap);
        }

        return true;
    }

    private static string[] FetchFields(string content)
    {
        List<string> fields = new List<string>();
        int offset = 0;
        int len = content.Length;
        int semi_count = 0;
        int start_pos = 0;

        while (offset < len)
        {
            char c = content [offset];
            switch (c)
            {
                case '"':
                    {
                        semi_count++;
                    }
                    break;
                case ',':
                    if (semi_count % 2 == 0)
                    {
                        fields.Add(FetchField(content, start_pos, offset - start_pos));
                        start_pos = offset + 1;
                    }
                    break;
            }
            offset++;
        }

        fields.Add(FetchField(content, start_pos, offset - start_pos));
        return fields.ToArray();
    }

    static string FetchField(string content, int start_pos, int len)
    {
        if (len < 1)
            return "";
        if (content [start_pos] != '"')
        {
            // Not start with '"', return directly
            return content.Substring(start_pos, len);
        }

        string value_format = "\"\"";
        if (len <= value_format.Length)
        {
            return "";
        }

        // remove the first '"' and the last '#'
        start_pos++;
        len -= value_format.Length;

        // Replace "\"\"" to "\""
        string str = content.Substring(start_pos, len);
        str = str.Replace(value_format, "\"");

        // 字符串转义
        str = GameUtility.ConvertToNGUIFormat(str);

        return str;
    }

    private static bool IsCommentLine(string line)
    {
        int len = line.Length;
        if (len < 1)
            return true;

        bool got_valid_char = false;
        for (int i = 0; i < len; i++)
        {
            char c = line [i];
            if (! got_valid_char && (c == '#'))
                return true;

            if (c == '"')
                continue;

            if (c == 65279)
                continue;

            return false;
        }

        return false;
    }
}

#endif
