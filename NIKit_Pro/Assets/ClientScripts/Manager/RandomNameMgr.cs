/// <summary>
/// RandomNameMgr.cs
/// Created by fengxl 2015-4-9
/// 随机取名字管理
/// </summary>

using LPC;
using UnityEngine;
using System.Collections.Generic;

public static class RandomNameMgr
{
    # region 字段

    // 名字前缀
    public static List<string> prefix;

    // 姓氏
    public static  List<string> surname;

    // 男名
    public static  List<string> maleName;

    // 女名
    public static  List<string> femaleName;

    // 玩家随机名称列表
    public static List<string> nameList = null;

    // 玩家随机名称列表个数
    public static int nameCount = 0;

    #endregion

    public static void Init()
    {
        // 载入名字库
        CsvFile nameLib = CsvFileMgr.Load("name_lib");
        if (nameLib == null)
            return;

        prefix = new List<string>();
        surname = new List<string>();
        maleName = new List<string>();
        femaleName = new List<string>();

        // 装配前缀、姓、男名、女名
        string p, s, m, f;
        for (int i = 0; i < nameLib.rows.Length; i++)
        {
            p = LocalizationMgr.Get(nameLib.rows[i].Query<string>("prefix"));
            if (!string.IsNullOrEmpty(p))
                prefix.Add(p);

            s = LocalizationMgr.Get(nameLib.rows[i].Query<string>("surname"));
            if (!string.IsNullOrEmpty(s))
                surname.Add(s);

            m = LocalizationMgr.Get(nameLib.rows[i].Query<string>("male_name"));
            if (!string.IsNullOrEmpty(m))
                maleName.Add(m);

            f = LocalizationMgr.Get(nameLib.rows[i].Query<string>("female_name"));
            if (!string.IsNullOrEmpty(f))
                femaleName.Add(f);
        }
    }

    /// <summary>
    /// 随机名字按钮点击事件
    /// </summary>
    public static string RandomName()
    {
        if (RandomNameMgr.prefix.Count < 1 ||
            RandomNameMgr.surname.Count < 1 ||
            RandomNameMgr.femaleName.Count < 1 ||
            RandomNameMgr.maleName.Count < 1)
            return null;

        //range(int min,int max)不会取到max
        int prefixIndex = Random.Range(0, RandomNameMgr.prefix.Count);
        int surnameIndex = Random.Range(0, RandomNameMgr.surname.Count);

        //  如果没有设置随机名字列表
        if (nameCount < 1)
            SetNameList(SexType.FEMALE);

        int nameIndex = Random.Range(0, nameCount);

        // 随机生成的玩家名字
        string name = RandomNameMgr.prefix[prefixIndex] +
                      RandomNameMgr.surname[surnameIndex] +
                      nameList[nameIndex];

        return name;
    }

    /// <summary>
    /// 根据玩家职业类型设置名字列表
    /// </summary>
    /// <param name="style">玩家职业类型</param>
    private static void SetNameList(int sex)
    {
        switch (sex)
        {
            case SexType.FEMALE:
                nameList = RandomNameMgr.femaleName;
                nameCount = RandomNameMgr.femaleName.Count;
                break;
            case SexType.MALE:
                nameList = RandomNameMgr.maleName;
                nameCount = RandomNameMgr.maleName.Count;
                break;
            default:
                nameList = RandomNameMgr.femaleName;
                nameCount = RandomNameMgr.femaleName.Count;
                break;
        }
    }
}
