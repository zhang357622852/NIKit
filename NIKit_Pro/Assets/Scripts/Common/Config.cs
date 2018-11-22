using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using LitJson;

public class Config
{
    // 版本号
    public string version { get { return Get<string>("version"); } }

    // 存放所有的配置信息
    Dictionary<string, object> mConfigs = new Dictionary<string, object>();
    public Dictionary<string, object> configs { get { return mConfigs; } }

    // 构建
    public Config()
    {
    }

    public Config(string content)
    {
        string[] lines = Explode(content, "\n");

        foreach (string l in lines)
        {
            string line = l.Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            if (line[0] == '#' || line[0] == '=')
                continue;

            int epos = line.IndexOf("=");
            if (epos < 0)
            {
                Add(line, string.Empty);
                continue;
            }

            string key = line.Substring(0, epos).Trim();
            if (string.IsNullOrEmpty(key))
                continue;

            string value = line.Substring(epos + 1, line.Length - epos - 1).Trim();

            // 非数组形式
            if (!value.StartsWith ("{") || !value.EndsWith ("}"))
            {
                if (value.StartsWith("@"))
                {
                    // 当前除了"@TRUE"和"@FALSE"其他都以字符串形式解析
                    if (value.Equals("@TRUE"))
                        Add(key, true);
                    else if (value.Equals("@FALSE"))
                        Add(key , false);
                }
                else
                    Add (key, value);

                continue;
            }

            value = value.TrimStart ('{').TrimEnd ('}');

            string[] values = value.Split (new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < values.Length; i++)
                values [i] = values [i].Trim ();

            Add (key, values);
        }
    }

    public Config(JsonData json)
    {
        foreach (var k in json.Keys)
        {
            // map和array直接以jsondata的形式存储
            if (json[k].IsObject || json[k].IsArray)
            {
                Add((string)k, json[k]);
                continue;
            }

            Add((string)k, json[k].ToString());
        }
    }

    // 序列化
    public bool Serialize(Stream stream)
    {
        try
        {
            foreach (KeyValuePair<string, object> kv in mConfigs)
            {
                string l = string.Format("{0}={1}\n", kv.Key, kv.Value);
                byte[] b = Encoding.UTF8.GetBytes(l);
                stream.Write(b, 0, b.Length);
            }

            return true;
        }
        catch (Exception e)
        {
            NIDebug.LogException(e);
            return false;
        }
    }

    // 打断字符串
    public static string[] Explode(string path, string seperator)
    {
        return path.Split(seperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
    }

    // 添加一个配置信息
    public void Add(string k, object v)
    {
        mConfigs[k] = v;
    }

    // 取得配置信息
    public T Get<T>(string k)
    {
        if (mConfigs.ContainsKey(k))
            return (T)mConfigs[k];

        return default(T);
    }

    // 取得配置信息
    public T Get<T>(string k, T def)
    {
        if (mConfigs.ContainsKey(k))
            return (T)mConfigs[k];

        return def;
    }

    // 从其他配置合并
    public void Merge(string content)
    {
        var cfg = new Config(content);

        Merge(cfg);
    }

    // 从其他配置合并
    public void Merge(Config cfg)
    {
        // 遍历所有配置
        foreach (var kv in cfg.configs)
        {
            mConfigs[kv.Key] = kv.Value;
        }
    }

    // 比较version和baseVersion, 0相等，1更高，-1更低
    public static int CompareVersion(string version, string baseVersion)
    {
        if (version == baseVersion)
            return 0;

        string[] va = version.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        string[] bva = baseVersion.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        int c = Math.Min(va.Length, bva.Length);
        for (int i = 0; i < c; i++)
        {
            ulong v = 0;
            if (! ulong.TryParse(va[i], out v))
            {
                Debug.Log(string.Format("version {0} 中包含非整数值", version));
            }

            ulong bv = 0;
            if (!ulong.TryParse(bva[i], out bv))
            {
                Debug.Log(string.Format("baseVersion {0} 中包含非整数值", baseVersion));
            }

            if (v > bv)
                return 1;
            else if (v < bv)
                return -1;
        }

        return va.Length > bva.Length ? 1 : (va.Length == bva.Length ? 0 : -1);
    }
}
