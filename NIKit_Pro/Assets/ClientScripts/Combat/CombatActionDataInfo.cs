/// <summary>
/// CombatActionDataInfo.cs
/// Created by wangxw 2014-11-07
/// 行为节点配置数据
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

// ActionSet类型
public class ActionSetType
{
    public const int AST_ACT = 0;
    // 动作
    public const int AST_STATUS = 1;
    // 状态

    /// <summary>
    /// 根据别名获取ActionSetType
    /// </summary>
    public static int GetTypeByAlias(string typeAlias)
    {
        // 状态
        if (string.Equals(typeAlias, "AST_STATUS"))
            return AST_STATUS;

        // 动作
        return AST_ACT;
    }
}

// 速度控制类型
public class SpeedControlType
{
    // 常量速度
    public const int SCT_CONSTANT = 0;

    // 常量速度
    public const int SCT_MOVE_RATE = 1;

    // 施法速度
    public const int SCT_ATTACK_SPEED = 2;

    // 受创
    public const int SCT_DAMAGE = 3;

    // 过图速度
    public const int SCT_CROSS_MAP = 4;

    /// <summary>
    /// 根据别名获取SpeedControlType
    /// </summary>
    public static int GetTypeByAlias(string typeAlias)
    {
        // 速度因子
        if (string.Equals(typeAlias, "SCT_CONSTANT"))
        {
            // 常量速度
            return SpeedControlType.SCT_CONSTANT;
        }
        else if (string.Equals(typeAlias, "SCT_MOVE_RATE"))
        {
            // 移动缩放
            return SpeedControlType.SCT_MOVE_RATE;
        }
        else if (string.Equals(typeAlias, "SCT_ATTACK_SPEED"))
        {
            // 攻击速度
            return SpeedControlType.SCT_ATTACK_SPEED;
        }
        else if (string.Equals(typeAlias, "SCT_DAMAGE"))
        {
            // 受创
            return SpeedControlType.SCT_DAMAGE;
        }
        else if (string.Equals(typeAlias, "SCT_CROSS_MAP"))
        {
            // 过图速度
            return SpeedControlType.SCT_CROSS_MAP;
        }
        else
        {
            // 默认常量速度
            return SpeedControlType.SCT_CONSTANT;
        }
    }
}

// ActionSet资源数据
public class ActionSetData
{
    // 集合名
    public string Name = string.Empty;

    // 集合类型
    public int ASType = ActionSetType.AST_ACT;

    // 速度控制类型
    public int SCType = SpeedControlType.SCT_CONSTANT;

    // 行为节点数据列表
    public List<ActionData> ActionDataList = new List<ActionData>();

    // 行为节点列表（Event触发）
    public Dictionary<string, List<ActionData>> EventActionDataList = new Dictionary<string, List<ActionData>>();

    // 动态属性编号列表
    public Dictionary<string, KeyValuePair<int, MixedValue>> DynamicIDSet = new Dictionary<string, KeyValuePair<int, MixedValue>>();
}

// Action资源数据
public class ActionData
{
    // 行为节点类型
    public System.Type NodeType = null;

    // 触发时间
    public float StartTime = 0.0f;

    // 触发Event
    public string StartEvent = string.Empty;
    
    // 能否取消
    public bool StopWhenCancel = false;

    // 取消之后是否继续创建
    public bool CreateWhenCanceled = false;

    // 静态属性参数
    public Dictionary<string, MixedValue> StaticProperties = new Dictionary<string, MixedValue>();

    // 动态属性参数
    // 实际使用的时候需要外部给予真实值，int类似于id编号
    public Dictionary<string, string> DynamicProperties = new Dictionary<string, string>();

    // 数据比较函数，按触发时间来区分大小，方便List.Sort()
    public static int Compare(ActionData left, ActionData right)
    {
        return (left.StartTime > right.StartTime) ? 1 : (left.StartTime < right.StartTime) ? -1 : 0;
    }
}

// 属性参数结构
public class PropertiesParameter
{
    #region 成员变量

    /// <summary>
    /// 引用的静态配置数据
    /// </summary>
    /// <value>The static action data.</value>
    public ActionData RefActionData = null;

    /// <summary>
    /// 动态数据
    /// 用于填写ActionData中的动态属性参数
    /// key：数据编号，对应ActionData中动态属性参数的value
    /// value：真实值
    /// </summary>
    public Dictionary<string, MixedValue> Parameter = new Dictionary<string, MixedValue>();

    #endregion

    #region 公共接口

    /// <summary>
    /// 是否有某个name的属性
    /// </summary>
    public bool HasProperty(string name)
    {
        if (RefActionData == null)
            return false;

        if (RefActionData.StaticProperties.ContainsKey(name))
            return true;

        if (RefActionData.DynamicProperties.ContainsKey(name))
            return true;

        return false;
    }

    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <returns>The property.</returns>
    /// <param name="name">Name.</param>
    public T GetProperty<T>(string name, T defaultValue = default(T))
    {
        if (RefActionData == null)
            return defaultValue;

        // 1. 先尝试从静态属性列表中获取
        MixedValue sRet;
        if (RefActionData.StaticProperties.TryGetValue(name, out sRet))
            return sRet.GetValue<T>();

        // 2. 再尝试从动态属性列表中获取
        string dynamicID;
        if (RefActionData.DynamicProperties.TryGetValue(name, out dynamicID))
        {
            // 动态属性表中有记录，看看是否能从动态数据中得到真实值
            MixedValue dRet;
            if (Parameter.TryGetValue(dynamicID, out dRet))
            {
                // 得到了真实值
                return dRet.GetValue<T>();
            }
            else
            {
                // 确实配置了动态属性，但却没得到真实值，
                // 应该是外部逻辑有问题，报错，给默认值
                LogMgr.Trace("动态属性 {0} 没有得到真实输入.", dynamicID);
                return defaultValue;
            }
        }

        // 3. 两边数据都没有，给予默认值
        return defaultValue;
    }

    #endregion
}

// ActionData的property解析器
public class ActionDataPropertyParser
{
    /// <summary>
    /// 解析property数据配置
    /// </summary>
    /// <param name="ad">待写入的ActionData节点引用参数</param>
    /// <param name="asd">待写入的ActionSetData节点引用参数</param>
    /// <param name="name">属性名</param>
    /// <param name="value">属性值</param>
    /// <param name="ctype">类型标识</param>
    public static void Parse(ref ActionData ad, ref ActionSetData asd, string name, string value, string ctype)
    {
        // 动态属性
        if (ctype == "@")
        {
            string[] tempStr = value.Split('@');
            if (tempStr.Length <= 1)
            {
                LogMgr.Trace("动态数据property配置错误, name:{0}, value:{1}.", name, value);
                return;
            }

            // 解析脚本参数，脚本带参数格式 eg : "@100#([])"
            tempStr = tempStr[1].Split('#');

            // 解析脚本
            int outValue;
            int.TryParse(tempStr[0], out outValue);

            // 解析脚本参数
            LPCValue tempArgs = null;
            if (tempStr.Length == 2)
                tempArgs = LPCRestoreString.RestoreFromString(tempStr[1]);
            else
                tempArgs = LPCValue.Create();

            // 记录数据
            ad.DynamicProperties.Add(name, value);

            // 如果已经包含了该动态属性
            if (asd.DynamicIDSet.ContainsKey(value))
                return;

            // 添加数据
            asd.DynamicIDSet.Add(value, new KeyValuePair<int, MixedValue>(outValue, MixedValue.NewMixedValue<LPCValue>(tempArgs)));
        } 
        // bool类型
        else if (ctype == "bool")
        {
            bool outValue;
            bool.TryParse(value, out outValue);

            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<bool>(outValue));
        }
        // int类型
        else if (ctype == "int")
        {
            int outValue;
            int.TryParse(value, out outValue);

            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<int>(outValue));
        }
        // float类型
        else if (ctype == "float")
        {
            float outValue;
            float.TryParse(value, out outValue);

            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<float>(outValue));
        }
        // string类型
        else if (ctype == "string")
        {
            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<string>(value));
        }
        // Vector2
        else if (ctype == "vector2")
        {
            string[] tempStr = value.Split(' ');
            if (tempStr.Length < 2)
                return;

            float x, y;
            float.TryParse(tempStr[0], out x);
            float.TryParse(tempStr[1], out y);

            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<Vector2>(new Vector2(x, y)));
        }
        // Vector3
        else if (ctype == "vector3")
        {
            string[] tempStr = value.Split(' ');
            if (tempStr.Length < 3)
                return;
            
            float x, y, z;
            float.TryParse(tempStr[0], out x);
            float.TryParse(tempStr[1], out y);
            float.TryParse(tempStr[2], out z);

            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<Vector3>(new Vector3(x, y, z)));
        }
        // Quaternion
        else if (ctype == "quaternion")
        {
            string[] tempStr = value.Split(' ');
            if (tempStr.Length < 4)
                return;
            
            float x, y, z, w;
            float.TryParse(tempStr[0], out x);
            float.TryParse(tempStr[1], out y);
            float.TryParse(tempStr[2], out z);
            float.TryParse(tempStr[3], out w);

            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<Quaternion>(new Quaternion(x, y, z, w)));
        }
        // Color
        else if (ctype == "color_rgba")
        {
            string[] tempStr = value.Split(' ');
            if (tempStr.Length < 4)
                return;

            float r, g, b, a;
            float.TryParse(tempStr[0], out r);
            float.TryParse(tempStr[1], out g);
            float.TryParse(tempStr[2], out b);
            float.TryParse(tempStr[3], out a);

            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<Color>(new Color(r, g, b, a)));
        } 
        // LPC Value
        else if (ctype == "lpc")
        {
            LPCValue newValue = LPCRestoreString.RestoreFromString(value);
            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<LPCValue>(newValue));
        }
        // string list
        else if (ctype == "string_array")
        {
            string[] tempStr = value.Split(',');
            if (tempStr.Length > 0)
                ad.StaticProperties.Add(name, MixedValue.NewMixedValue<string[]>(tempStr));
        }
        // ColorChangeType
        else if (ctype == "cct")
        {
            int cct = ColorChangerType.GetTypeByAlias(value);
            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<int>(cct));
        } 
        // EventMgrEventType
        else if (ctype == "emet")
        {
            int emet = EventMgrEventType.GetEventTypeByAlias(value);
            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<int>(emet));
        }
        // ObjectDirection2D
        else if (ctype == "dir2d")
        {
            int dir2d = ObjectDirection2D.GetDirection2DByAlias(value);
            ad.StaticProperties.Add(name, MixedValue.NewMixedValue<int>(dir2d));
        }
        // other to be added
        else
        {
            LogMgr.Trace("property未知属性类型{0}.", ctype);
            return;
        }
    }
}
