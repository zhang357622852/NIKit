using System.Collections.Generic;
using LPC;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

/// 道具管理器
public class ItemMgr
{
    #region 变量

    // 道具配置表信息
    private static CsvFile mItemCsv;
    private static Dictionary<string, List<int>> mItemMap = new Dictionary<string, List<int>>();
    /// <summary>
    /// 使魔升级材料
    /// class_id
    /// </summary>
    private static List<int> mPetUpgradeMaterialList = new List<int>();

    #endregion

    #region 属性

    // 获取道具配置表信息
    public static CsvFile ItemCsv { get { return mItemCsv; } }

    #endregion

    #region 私有接口

    /// <summary>
    /// 初始化道具数据
    /// </summary>
    private static void InitItemData()
    {
        mItemMap.Clear();
        mPetUpgradeMaterialList.Clear();

        // 遍历各个药品数据
        foreach (CsvRow data in mItemCsv.rows)
        {
            // 数据格式不正确
            if (data == null)
                continue;

            //使魔升级材料
            int isMaterial = data.Query<int>("is_material");

            if (isMaterial.Equals(1))
                mPetUpgradeMaterialList.Add(data.Query<int>("class_id"));

            // 道具分组
            string group = data.Query<string>("group");
            if (string.IsNullOrEmpty(group))
                continue;

            List<int> list = new List<int>();

            int classId = data.Query<int>("class_id");

            // 尝试获取缓存中的分组数据
            if (! mItemMap.TryGetValue(group, out list))
                list = new List<int>();

            list.Add(classId);

            // 缓存数据
            mItemMap[group] = list;
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入道具配置表信息
        mItemCsv = CsvFileMgr.Load("item");

        // 初始化道具数据
        InitItemData();
    }

    /// <summary>
    /// 是否是道具
    /// </summary>
    public static bool IsItem(Property ob)
    {
        return IsItem(ob.Query<int>("class_id"));
    }

    /// <summary>
    /// 是否是道具
    /// </summary>
    public static bool IsItem(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row == null)
            return false;

        return true;
    }

    /// <summary>
    /// 使用道具
    /// </summary>
    public static bool ApplyItem(Property target, Property item, int amount = 1)
    {
        // 获取道具的使用标示
        string applyValue = item.Query<string>("apply");

        // 没有使用标示不能使用道具
        if (string.IsNullOrEmpty(applyValue))
        {
            // 给出提示提示信息
            DialogMgr.Notify("道具不能使用。");

            return false;
        }

        // 获取道具当前的数量
        int curr = item.GetAmount();

        // 数量不能大于当前道具的数量，否则使用道具失败
        if (amount > curr)
        {
            // 给出提示提示信息“道具数量不够。”
            DialogMgr.Notify("道具数量不够，无法使用道具");

            return false;
        }

        // 获取目标等级和道具的使用等级
        int userLevel = target.Query<int>("level");
        int itemLevel = item.Query<int>("level_request");

        // 等级不符不能使用
        if (userLevel < itemLevel)
        {
            // 给出提示提示信息
            DialogMgr.Notify("等级不足，无法使用道具");

            return false;
        }

        // 调用道具的检测脚本判断能否使用
        int checkScript = item.BasicQueryNoDuplicate<int>("check_script");
        if (checkScript != 0)
        {
            // 调用脚本判断能否使用
            bool ret = (bool)ScriptMgr.Call(checkScript, target, item);

            // 脚本判断不能使用道具
            if (ret == false)
                return false;
        }

        // 添加列表
        LPCMapping items = new LPCMapping();
        items.Add(item.GetRid(), amount);

        // 向服务器发送消息
        Operation.CmdApply.Go(items, target.GetRid());

        // 返回成功
        return true;
    }

    ///获得名称
    public static string GetName(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return LocalizationMgr.Get(row.Query<string>("name"));

        return null;
    }

    /// <summary>
    /// 根据分组获取classId
    /// </summary>
    public static List<int> GetClassIdByGroup(string group)
    {
        if (string.IsNullOrEmpty(group))
            return new List<int>();

        if (! mItemMap.ContainsKey(group))
            return new List<int>();

        return mItemMap[group];
    }

    /// <summary>
    /// 获得icon
    /// </summary>
    public static string GetIcon(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<string>("icon");

        return string.Empty;
    }

    /// <summary>
    /// 获取使魔升级材料列表
    /// </summary>
    /// <returns></returns>
    public static List<int> GetPetUpgradeMaterials()
    {
        return mPetUpgradeMaterialList;
    }

    /// <summary>
    /// 获取道具透明图标（如果没有配置，返回透明图标）
    /// </summary>
    /// <returns>The clear icon.</returns>
    /// <param name="classId">Class identifier.</param>
    public static string GetClearIcon(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row == null)
            return string.Empty;

        string icon = row.Query<string>("clear_icon");

        if (!string.IsNullOrEmpty(icon))
            return icon;

        return row.Query<string>("icon");
    }

    ///获得类型
    public static int GetType(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<int>("item_type");

        return -1;
    }

    ///获得类型
    public static int GetType(Property item)
    {
        if (item == null)
            return 0;

        return item.Query<int>("item_type");
    }

    ///获得属性描述
    public static string GetPropDesc(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return LocalizationMgr.Get(row.Query<string>("prop_desc"));
        return null;
    }

    ///获得描述
    public static string GetDesc(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row == null)
            return string.Empty;

        // 道具描述脚本
        int scriptNo = row.Query<int>("desc_script");
        if (scriptNo == 0)
            return LocalizationMgr.Get(row.Query<string>("desc"));

        return (string) ScriptMgr.Call(scriptNo, row, ME.user);
    }

    ///获得描述
    public static string GetDesc(Property who, int classId)
    {
        CsvRow row = GetRow(classId);
        if (row == null)
            return string.Empty;

        // 道具描述脚本
        int scriptNo = row.Query<int>("desc_script");
        if (scriptNo == 0)
            return LocalizationMgr.Get(row.Query<string>("desc"));

        return (string) ScriptMgr.Call(scriptNo, row, who);
    }

    ///获得描述
    public static string GetDesc(Property item)
    {
        if (item == null)
            return string.Empty;

        int class_id = item.GetClassID();
        return GetDesc(class_id);
    }

    ///获得使用等级
    public static int GetRequestLevel(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<int>("level_request");
        return -1;
    }

    /// <summary>
    /// 取得作用描述
    /// </summary>
    public static string GetApplyDesc(Property item)
    {
        if (item == null)
            return string.Empty;

        int class_id = item.GetClassID();
        CsvRow row = GetRow(class_id);
        if (row == null)
            return string.Empty;

        // 作用描述脚本
        LPCValue script = row.Query<LPCValue>("apply_desc");
        if (script.IsString)
            return LocalizationMgr.Get(script.AsString);

        if (script.AsInt <= 0)
            return string.Empty;

        return (string)ScriptMgr.Call(script.AsInt, item);
    }

    //// <summary>
    /// 获得作用参数
    /// </summary>
    public static LPCMapping GetApplyArg(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null && row.Query<LPCValue>("apply_arg") != null)
            return row.Query<LPCValue>("apply_arg").AsMapping;
        return null;
    }

    /// <summary>
    /// 获得数据
    /// </summary>
    public static CsvRow GetRow(int classId)
    {
        // 没有配置表
        if (ItemCsv == null)
            return null;

        return ItemCsv.FindByKey(classId);
    }

    /// <summary>
    /// 获取宠物texture
    /// </summary>
    /// <returns>The pet texture.</returns>
    /// <param name="classId">Class identifier.</param>
    /// <param name="rank">Rank.</param>
    public static Texture2D GetTexture(int classId, bool clearIcon = false)
    {
        string textureName;

        if (!clearIcon)
            textureName = GetIcon(classId);
        else
            textureName = GetClearIcon(classId);

        if (string.IsNullOrEmpty(textureName))
            return null;

        return GetTexture(textureName);
    }

    /// <summary>
    /// 获取texture
    /// </summary>
    /// <returns>The texture.</returns>
    /// <param name="icon">Icon.</param>
    public static Texture2D GetTexture(string icon)
    {
        string resPath = string.Format("Assets/Art/UI/Icon/item/{0}.png", icon);
        return ResourceMgr.LoadTexture(resPath);
    }

    /// <summary>
    /// 排序包裹物品
    /// </summary>
    public static List<int> SortItems(List<int> itemList)
    {
        // 根据道具权重排序
        IEnumerable<int> ItemQuery = from id in itemList orderby GetItemSortWeight(id) descending
                                           select id;

        List<int> sortList = new List<int>();

        foreach (int id in ItemQuery)
        {
            sortList.Add(id);
        }

        return sortList;
    }

    /// <summary>
    /// 根据权重排序
    /// </summary>
    /// <returns>The item sort weight.</returns>
    /// <param name="id">Identifier.</param>
    private static string GetItemSortWeight(int classId)
    {
        CsvRow item_data = ItemMgr.GetRow(classId);

        int ksort = item_data.Query<int>("ksort");

        // 返回权重
        return ksort.ToString();
    }

    /// <summary>
    /// 是否是属性道具
    /// </summary>
    /// <returns><c>true</c> if is attrib item the specified classId; otherwise, <c>false</c>.</returns>
    /// <param name="classId">Class identifier.</param>
    public static bool IsAttribItem(int classId)
    {
        if(string.IsNullOrEmpty(FieldsMgr.GetAttribByClassId(classId)))
            return false;

        return true;
    }

    /// <summary>
    /// 是否是双倍经验道具
    /// </summary>
    public static bool IsDoubleExpItem(int classId)
    {
        if (classId.Equals(50317)
            || classId.Equals(50320)
            || classId.Equals(50318))
            return true;
        else
            return false;
    }

    #endregion
}
