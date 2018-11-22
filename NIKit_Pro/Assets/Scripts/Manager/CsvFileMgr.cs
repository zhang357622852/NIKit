/// <summary>
/// CsvFileMgr.cs
/// Copy from zhangyg 2014-10-22
/// 负责管理CSV文件
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text;
using UnityEngine;
using LPC;

// 为什么没有做各种类型检查？
// 因为如果类型不正确会抛出异常，而且数值的类型验证是在Import做的，本类不再关心。
public static class CsvFileMgr
{
    #region 成员变量

    // 当前csv序列化版本，如果版本不符会重新生成csv序列化文件
    public static int Version = 1;

    // 序列化文件的扩展名
    public const string CSV_EXT = ".bytes";

    // 序列化的缓存
    public static byte[] mSerBuffer;

#if UNITY_EDITOR
    // 包含csv的目录, "nocheck"表示csv中的所有内容都被序列化, "check"标志只有在csv注释头中有"*"号的列才会被序列化
    static string[] mCsvFolder = new string[]
    {
        Application.dataPath + "/LocalSet", "nocheck",
        Application.dataPath + "/../../server/server_scripts/etc/gs", "check",
    };
#endif

    #endregion

    #region 内部接口

    public static void InitStartCsv()
    {
        // 初始化SystemTipsMgr
        //SystemTipsMgr.InitStart();
    }

    /// <summary>
    /// 加载某个csv
    /// </summary>
    public static CsvFile Load(string csvName)
    {
        try
        {
            // 载入资源
            string assetPath = ConfigMgr.ETC_PATH + "/" + csvName + CSV_EXT;
            byte[] csvBytes = ResourceMgr.Instance.LoadByte(assetPath);

            // 资源不存在
            if (csvBytes == null || csvBytes.Length == 0)
                return null;

            // 反序列化
            MemoryStream csvStr = new MemoryStream(csvBytes, 0, csvBytes.Length, true, true);
            CsvFile csv = CsvFileMgr.Deserialize(csvStr);
            csvStr.Close();

            // 返回数据
            return csv;
        }
        catch (Exception e)
        {
            NIDebug.Log(e.Message);
            return null;
        }
        finally
        {
            // do something
        }
    }

    #if UNITY_EDITOR

    /// <summary>
    /// 序列化csv文件操作.
    /// </summary>
    public static void Save(string filePath, bool checkValid)
    {
        // 序列化csv文件操作
        Save(filePath, checkValid, ConfigMgr.ETC_PATH);
    }

    /// <summary>
    /// 序列化csv文件操作.
    /// </summary>
    public static void Save(string filePath, bool checkValid, string dir)
    {
        CsvFile csv = new CsvFile(System.IO.Path.GetFileNameWithoutExtension(filePath));

        string[] lines = FileMgr.ReadLines(filePath);
        if (lines.Length == 0)
        {
            NIDebug.Log("空文件 {0}", lines);
            return;
        }

        // 解析csv文件
        CsvParser cp = new CsvParser();
        LPCValue m;
        try
        {
            cp.Load(filePath, checkValid);
            if (cp.Fields.Count == 0 || cp.Records.Count == 0)
                return;
            m = ImportMgr.Read(cp);
        }
        catch (Exception e)
        {
            NIDebug.Log("读取csv错误 {0}\n{1}", filePath, e.ToString());
            return;
        }

        // 主key
        csv.primaryKey = cp.PrimaryKey;

        // 列名字对应的索引
        for (int i = 0; i < cp.Fields.Count; i++)
        {
            csv.columns.Add(cp.Fields[i].name, i);
        }

        // 每列值
        csv.rows = new CsvRow[m.AsArray.Count];
        for (int i = 0; i < m.AsArray.Count; i++)
        {
            LPCValue v = m.AsArray[i];
            CsvRow row = new CsvRow(csv);
            for (int idx = 0; idx < cp.Fields.Count; idx++)
            {
                row.Add(idx, v.AsMapping[cp.Fields[idx].name]);
            }

            csv.AddRow(i, row);
        }

        // 序列化
        MemoryStream ms = new MemoryStream();
        CsvFileMgr.Serialize(ms, csv);

        // 写入数据
        string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

        // 确保路径存在
        Directory.CreateDirectory(dir);

        FileStream fs = new FileStream(dir + "/" + fileName + CSV_EXT, FileMode.Create, FileAccess.Write);
        fs.Write(ms.GetBuffer(), 0, (int) ms.Length);
        fs.Close();
    }

    /// <summary>
    /// 序列化所有的csv
    /// </summary>
    public static void SaveAll()
    {
        // 确保目录存在
        if (!Directory.Exists(ConfigMgr.ETC_PATH))
            Directory.CreateDirectory(ConfigMgr.ETC_PATH);

        // 删除目标文件夹下的所有BIN_CSV_EXT文件
        foreach (string f in Directory.GetFiles(ConfigMgr.ETC_PATH, "*" + CSV_EXT))
        {
            File.Delete(f);
            File.Delete(f + ".meta");
        }

        for (int i = 0; i < mCsvFolder.Length; i += 2)
        {
            string csvPath = mCsvFolder[i];
            string flag = mCsvFolder[i + 1];

            string[] csvFiles = Directory.GetFiles(csvPath, "*.csv");
            foreach (string file in csvFiles)
                Save(file, flag == "check");
        }
    }

    #endif

    public static int WriteLpcValue(MemoryStream stream, LPCValue v)
    {
        int len = LPCValue.SaveToBuffer(mSerBuffer, 0, v);
        stream.Write(mSerBuffer, 0, len);
        return len;
    }

    public static LPCValue ReadLpcValue(MemoryStream stream)
    {
        LPCValue v;
        byte[] buf = stream.GetBuffer();
        int cnt = LPCValue.RestoreFromBuffer(buf, (int)stream.Position, out v);
        stream.Seek(cnt, SeekOrigin.Current);
        return v;
    }

    /// <summary>
    /// 序列化
    /// </summary>
    public static void Serialize(MemoryStream stream, CsvFile csv)
    {
        // 初始化缓存
        if (mSerBuffer == null)
            mSerBuffer = new byte[1024 * 1024];

        FileMgr.WriteInt(stream, CsvFileMgr.Version);

        FileMgr.WriteString(stream, csv.Name);

        FileMgr.WriteString(stream, csv.primaryKey);

        // 写入主key类型
        LPC.LPCValue.ValueType pkeyType = LPCValue.ValueType.INT;
        if (csv.rows.Length > 0)
        {
            var row = csv.rows[0];
            var pkey = row.Query<LPCValue>(csv.primaryKey);
            pkeyType = pkey.type;
            FileMgr.WriteInt(stream, (int)pkeyType);
        }

        // 列名
        FileMgr.WriteInt(stream, csv.columns.Count);
        foreach (var kv in csv.columns)
        {
            FileMgr.WriteString(stream, kv.Key);
            FileMgr.WriteInt(stream, kv.Value);
        }

        // 行数
        FileMgr.WriteInt(stream, csv.rows.Length);

        // 写入每行的主key
        for (int i = 0; i < csv.rows.Length; i++)
        {
            var row = csv.rows[i];
            if (pkeyType == LPCValue.ValueType.INT)
            {
                var pkey = row.Query<int>(csv.primaryKey);
                FileMgr.WriteInt(stream, pkey);
            }
            else
            {
                var pkey = row.Query<string>(csv.primaryKey);
                FileMgr.WriteString(stream, pkey);
            }
        }

        // 写入行长度和内容
        for (int i = 0; i < csv.rows.Length; i++)
        {
            var row = csv.rows[i];
            int len = 0;

            for (var idx = 0; idx < csv.columns.Count; idx++)
            {
                len += LPCValue.SaveToBuffer(mSerBuffer, len, row.properties[idx]);
            }

            FileMgr.WriteInt(stream, len);
            stream.Write(mSerBuffer, 0, len);
        }

        // 释放
        mSerBuffer = null;
    }

    /// <summary>
    /// 反序列化.
    /// </summary>
    public static CsvFile Deserialize(MemoryStream stream)
    {
        int ver = FileMgr.ReadInt(stream);
        if (ver != CsvFileMgr.Version)
        {
            NIDebug.Log("Csv版本 {0} 错误, 最新版本 {1}", ver, CsvFileMgr.Version);
            return null;
        }

        // 文件名
        string name = FileMgr.ReadString(stream);

        CsvFile csv = new CsvFile(name);
        csv.primaryKey = FileMgr.ReadString(stream);

        // 主key类型
        var pkeyType = (LPC.LPCValue.ValueType)FileMgr.ReadInt(stream);

        // 列名
        int n = FileMgr.ReadInt(stream);
        csv.columns = new Dictionary<string, int>(n);
        for (int i = 0; i < n; i++)
        {
            string k = FileMgr.ReadString(stream);
            int v = FileMgr.ReadInt(stream);
            csv.columns.Add(k, v);
        }

        // 行数
        n = FileMgr.ReadInt(stream);
        csv.rows = new CsvRow[n];

        // 主key的列
        int pkeyIdx = csv.columns[csv.primaryKey];

        // 每行主key
        for (var i = 0; i < n; i++)
        {
            var row = new CsvRow(csv);
            if (pkeyType == LPCValue.ValueType.INT)
            {
                int pkey = FileMgr.ReadInt(stream);
                row.Add(pkeyIdx, LPCValue.Create(pkey));
            }
            else
            {
                string pkey = FileMgr.ReadString(stream);
                row.Add(pkeyIdx, LPCValue.Create(pkey));
            }
            csv.AddRow(i, row);
        }

        // 行数据
        for (int i = 0; i < n; i++)
        {
            int len = FileMgr.ReadInt(stream);
            csv.rows[i].rowData = new byte[len];
            stream.Read(csv.rows[i].rowData, 0, len);
        }

        return csv;
    }

    #endregion
}

// csv的一行数据
public class CsvFile
{
    #region 属性

    // 表名
    string mName;

    public string Name { get { return mName; } }

    // 主key
    string mPrimaryKey;

    public string primaryKey { get { return mPrimaryKey; } set { mPrimaryKey = value; } }

    // 列信息
    Dictionary<string, int> mColumns = new Dictionary<string, int>();

    public Dictionary<string, int> columns { get { return mColumns; } set { mColumns = value; } }

    // 每一行信息
    CsvRow[] mRows = null;

    public CsvRow[] rows { get { return mRows; } set { mRows = value; } }

    // 每行中的主key建立的行信息
    Dictionary<int, CsvRow> mIntRowIndexs = new Dictionary<int, CsvRow>();
    Dictionary<string, CsvRow> mStrRowIndexs = new Dictionary<string, CsvRow>();

    // 行数
    public int count { get { return mRows.Length; } }

    // 通过下标索引访问第N行数据
    public CsvRow this [int idx]
    {
        get
        {
            if (idx >= count)
                return null;

            return mRows[idx];
        }
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="name">Name.</param>
    public CsvFile(string name)
    {
        mName = name;
    }

    // 新增一行数据
    public void AddNewRow(CsvRow row)
    {
        // 计算数组长度
        int newLength = 0;
        if (mRows == null)
            newLength = 1;
        else
            newLength = count + 1;

        // 初始化数据
        CsvRow[] newRows = new CsvRow[newLength];

        // 转移数据
        if (mRows != null)
        {
            for (int i = 0; i < count; i++)
                newRows[i] = mRows[i];
        }

        // 重置数据
        mRows = newRows;
        AddRow(newLength - 1, row);
    }

    // 增加一列
    public void AddRow(int i, CsvRow row)
    {
        mRows[i] = row;
        LPCValue pk = row.Query<LPCValue>(mPrimaryKey);
        if (pk != null)
        {
            bool hasRow = false;
            CsvRow nrow;
            if (pk.IsInt)
            {
                hasRow = mIntRowIndexs.TryGetValue(pk.AsInt, out nrow);
                if (!hasRow)
                    mIntRowIndexs.Add(pk.AsInt, row);
            }
            else if (pk.IsString)
            {
                hasRow = mStrRowIndexs.TryGetValue(pk.AsString, out nrow);
                if (!hasRow)
                    mStrRowIndexs.Add(pk.AsString, row);
            }
            else
            {
                NIDebug.Log("无法添加 {0} 为主key, 主key只能是int或string!", pk.GetDescription());
                return;
            }

            if (hasRow)
            {
                // 主key对应的值已经存在，挂接到链表尾
                while (nrow.next != null)
                    nrow = nrow.next;
                nrow.next = row;
                row.next = null;
            }
        }
        else
        {
            NIDebug.Log("主key {0} 没有值!", mPrimaryKey);
        }
    }

    // 根据key值查找
    public CsvRow FindByKey(int key)
    {
        CsvRow row;
        if (mIntRowIndexs.TryGetValue(key, out row))
            return row;
        return null;
    }

    // 根据key值查找
    public CsvRow FindByKey(string key)
    {
        CsvRow row;
        if (mStrRowIndexs.TryGetValue(key, out row))
            return row;
        return null;
    }

    #endregion
}

// csv的一行数据
public class CsvRow
{
    #region 成员变量

    CsvFile mOwner;
    byte[] mRowData = null;
    LPCValue[] mProperties = null;
    CsvRow mNext = null;

    #endregion

    #region 属性

    public byte[] rowData { get { return mRowData; } set { mRowData = value; } }

    public LPCValue[] properties
    {
        get
        {
            if (mRowData != null)
                Deserialize();

            return mProperties;
        }

        set { mProperties = value; }
    }

    // 当多行的主key一样时，被串联成一个链表
    public CsvRow next { get { return mNext; } set { mNext = value; } }

    #endregion

    #region 外部接口

    /// <summary>
    /// 构造函数
    /// </summary>
    public CsvRow(CsvFile owner)
    {
        mOwner = owner;

        mProperties = new LPCValue[mOwner.columns.Count];
    }

    // 序列化这行数据
    bool Deserialize()
    {
        if (mRowData == null)
            return true;

        try
        {
            // 反序列化
            int offset = 0;
            for (var idx = 0; idx < mOwner.columns.Count; idx++)
            {
                LPCValue v;
                offset += LPCValue.RestoreFromBuffer(mRowData, offset, out v);
                Add(idx, v);
            }
            return true;
        }
        catch (Exception e)
        {
            NIDebug.Log(e.Message);
            return false;
        }
        finally
        {
            mRowData = null;
        }
    }

    /// <summary>
    /// Query the specified columnName.
    /// </summary>
    /// <param name="columnName">Column name.</param>
    public LPCValue Query(string columnName)
    {
        return Query<LPCValue>(columnName);
    }

    // 根据列名获取数据
    public T Query<T>(string columnName, T def = default(T))
    {
        // 不是获取主key值，且还没有序列化完成，立刻序列化
        if (columnName != mOwner.primaryKey && mRowData != null && !Deserialize())
        {
            NIDebug.Log("csv序列化失败({0})", mOwner.Name);
            return def;
        }

        int index;
        if (mOwner.columns.TryGetValue(columnName, out index))
        {
            LPCValue prop = mProperties[index];

            // 如果是LPCValue类型
            if (typeof(T) == typeof(LPCValue))
                return (T)(object)prop;

            // 不是string数据，直接转换
            if (!prop.IsString)
                return prop.As<T>();

            // 就是要string，直接返回
            if (typeof(T) == typeof(string))
                return prop.As<T>();

            // 如果外部不是想要string，但是却得到了一个空string
            // 则表示csv表格上填写的是空值，此时返回默认值
            if (prop.AsString == string.Empty)
                return def;

            // 外部不是想要string，却得到了有字符的string
            // 走到这儿的时候已经出错了，直接使用As借口中的异常处理即可
            return prop.As<T>();
        }
        else
            NIDebug.Log("{0}.csv 没有列 {1}。", mOwner.Name, columnName);

        // 返回默认值
        return def;
    }

    // 是否包含某列
    public bool Contains(string columnName)
    {
        return mOwner.columns.ContainsKey(columnName);
    }

    // 设置数据
    public void Add(string columnName, LPCValue v)
    {
        int index = 0;
        if (!mOwner.columns.TryGetValue(columnName, out index))
            return;

        // 重置属性
        mProperties[index] = v;
    }

    // 设置数据
    public void Add(int i, LPCValue v)
    {
        mProperties[i] = v;
    }

    // 转换成lpcvalue
    public LPCMapping ConvertLpcMap()
    {
        LPCMapping lpcMap = LPCMapping.Empty;
        foreach (KeyValuePair<string, int> c in mOwner.columns)
            lpcMap.Add(c.Key, properties[c.Value]);

        return lpcMap;
    }

    // 转换成lpcvalue
    public LPCValue ConvertLpc()
    {
        LPCMapping lpcMap = LPCMapping.Empty;
        foreach (KeyValuePair<string, int> c in mOwner.columns)
            lpcMap.Add(c.Key, properties[c.Value]);

        return LPCValue.Create(lpcMap);
    }

    #endregion

}
