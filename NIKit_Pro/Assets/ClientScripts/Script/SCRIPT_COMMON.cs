/// <summary>
/// SCRIPT_COMMON.cs
/// Create by fucj 2014-12-23
/// 通用脚本
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;
using UnityEngine;

/// <summary>
/// 地图解锁判断脚本
/// </summary>
public class SCRIPT_30000 : Script
{
    public override object Call(params object[] _params)
    {
        Property who = _params[0] as Property;
        LPCValue args = _params[1] as LPCValue;

        // 没有脚本参数
        if (!args.IsMapping)
            return true;

        // 转换数据格式
        LPCMapping conditionMap = args.AsMapping;

        // 没有解锁条件
        if (conditionMap.Count == 0)
            return true;

        // 获取地图的解锁条件
        int id = conditionMap.GetValue<int>("map_id");
        List<string> instanceList = InstanceMgr.GetInstanceByMapId(id);
        if (instanceList.Count == 0)
            return true;

        // 获取副本难度
        int difficulty = conditionMap.GetValue<int>("difficulty");
        LPCMapping info = LPCMapping.Empty;

        // 获取玩家的通关数据
        foreach (string instanceId in instanceList)
        {
            //根据副本id获取副本信息;
            info = InstanceMgr.GetInstanceInfo(instanceId);

            // 不是需要匹配的难度;
            if (info.GetValue<int>("difficulty") > difficulty)
                continue;

            // 该副本还没有通关过
            if (! InstanceMgr.IsClearanced(who, instanceId))
                return false;
        }

        // 地图已经解锁
        return true;
    }
}

/// <summary>
/// 地下城地图解锁判断脚本
/// </summary>
public class SCRIPT_30001 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象
        //Property user = _params[0] as Property;
        // 解锁参数
        LPCValue args = _params[1] as LPCValue;

        // (int)_params[2]; 地图id

        if (!args.IsMapping)
            return false;

        LPCMapping unLockArgs = args.AsMapping;

        // 活动解锁
        LPCArray activityList = unLockArgs.GetValue<LPCArray>("activity_list");
        if (activityList.Count != 0)
        {
            foreach (LPCValue activityId in activityList.Values)
            {
                // 如果当前活动已开启，则解锁
                if (ActivityMgr.IsAcitvityValid(activityId.AsString))
                    return true;
            }
        }

        // 获取该地图是星期几解锁
        int wday = unLockArgs.GetValue<int>("wday");

        int weekDay = Game.GetWeekDay(TimeMgr.GetServerTime());

        LPCArray allOpenDate = GameSettingMgr.GetSetting<LPCArray>("dungeons_all_open_date");
        if (allOpenDate.IndexOf(weekDay) != -1)
            return true;

        if (wday.Equals(weekDay))
            return true;
        else
            return false;
    }
}

/// <summary>
/// 副本物件编号通用计算脚本
/// </summary>
public class SCRIPT_30002 : Script
{
    public override object Call(params object[] _params)
    {
        // 获取副本等级
        // LPCValue level = (int) _params[0];

        // 脚本参数
        LPCValue args = _params[1] as LPCValue;

        // 获取资源class_id
        int classId = 0;

        // 获取创建角色class_id
        if (args.IsInt)
            classId = args.AsInt;
        else if (args.IsMapping)
        {
            List<int> classIdList = new List<int>();
            List<int> weightList = new List<int>();
            LPCMapping classIdMap = args.AsMapping;

            // 遍历列表
            foreach (int tempId in classIdMap.Keys)
            {
                // 记录数据
                classIdList.Add(tempId);
                weightList.Add(classIdMap.GetValue<int>(tempId));
            }

            // 根据权重抽取
            int index = RandomMgr.RandomSelect(weightList);
            if (index != -1)
                classId = classIdList[index];
        }
        else if (args.IsArray)
        {
            List<int> classIdList = new List<int>();
            List<int> weightList = new List<int>();
            int index = 0;

            foreach (LPCValue data in args.AsArray.Values)
            {
                // 数据格式不正确
                if (! data.IsArray)
                    continue;

                // 转换数据格式
                LPCArray classIdWeight = data.AsArray;
                int tClassId = classIdWeight[0].AsInt;
                int tWeight = classIdWeight[1].AsInt;

                // 添加抽取列表
                index = classIdList.IndexOf(tClassId);
                if (index == -1)
                {
                    // 记录数据
                    classIdList.Add(tClassId);
                    weightList.Add(tWeight);
                    continue;
                }

                // 增加权重数据
                weightList[index] += tWeight;
            }

            // 根据权重抽取
            index = RandomMgr.RandomSelect(weightList);
            if (index != -1)
                classId = classIdList[index];
        }
        else
        {
            // 未知类型
        }

        // 返回classId
        return classId;
    }
}

/// <summary>
/// 副本怪物初始化参数计算脚本
/// </summary>
public class SCRIPT_30003 : Script
{
    public override object Call(params object[] _params)
    {
        // 获取副本等级
        // LPCValue level = (int) _params[0];

        // 脚本参数
        LPCValue args = _params[1] as LPCValue;

        // 返回参数
        return args.AsMapping;
    }
}

/// <summary>
/// 副本显示怪物初始化参数计算脚本
/// </summary>
public class SCRIPT_30004 : Script
{
    public override object Call(params object[] _params)
    {
        // 获取副本等级
        // LPCValue level = (int) _params[0];

        // 脚本参数
        LPCValue args = _params[1] as LPCValue;

        // 返回参数
        return args.AsInt;
    }
}

/// <summary>
/// 副本显示物初始化参数计算脚本
/// </summary>
public class SCRIPT_30005 : Script
{
    public override object Call(params object[] _params)
    {
        // 获取副本等级
        // LPCValue level = (int) _params[0];

        // 脚本参数
        LPCValue args = _params[1] as LPCValue;

        // 返回参数
        return args.AsMapping;
    }
}

/// <summary>
/// 竞技场怪物初始化参数计算脚本
/// </summary>
public class SCRIPT_30006 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping result = new LPCMapping();

        // 获取副本等级，实际是玩家等级
        int instance_level = (int)_params[0];

        // 脚本参数
        LPCMapping args = (_params[1] as LPCValue).AsMapping;

        result.Append(args);

        // 判断怪物等级，最大35级
        int start_level = args.GetValue<int>("start_level");
        int level = (instance_level <= start_level) ? start_level : instance_level;
        result.Add("level", (level > 35) ? 35 : level);

        // 判断怪物星级
        int star = args.GetValue<int>("star");
        LPCArray star_up = args.GetValue<LPCArray>("star_up");
        foreach (LPCValue mks in star_up.Values)
        {
            // 判断是否需要升星
            if (level >= mks.AsInt)
                star += 1;
        }
        result.Add("star", star);

        // 判断是否觉醒
        int rank_up = args.GetValue<int>("rank_up");
        result.Add("rank", (level >= rank_up) ? 2 : 1);

        // 返回参数
        return result;
    }
}

/// <summary>
/// 竞技场怪物显示物初始化参数计算脚本
/// </summary>
public class SCRIPT_30007 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping result = new LPCMapping();

        // 获取副本等级，实际是玩家等级
        int instance_level = (int)_params[0];

        // 脚本参数
        LPCValue args = _params[1] as LPCValue;

        // 判断怪物等级，最大35级
        int start_level = args.AsMapping.GetValue<int>("start_level");
        int level = (instance_level <= start_level) ? start_level : instance_level;
        result.Add("level", (level > 35) ? 35 : level);

        // 判断怪物星级
        int star = args.AsMapping.GetValue<int>("star");
        LPCArray star_up = args.AsMapping.GetValue<LPCArray>("star_up");
        foreach (LPCValue mks in star_up.Values)
        {
            // 判断是否需要升星
            if (level >= mks.AsInt)
                star += 1;
        }
        result.Add("star", star);

        // 判断是否觉醒
        int rank_up = args.AsMapping.GetValue<int>("rank_up");
        result.Add("rank", (level >= rank_up) ? 2 : 1);

        // 返回参数
        return result;
    }
}

/// <summary>
/// 在游戏中系统tip提示权重计算脚本
/// 在游戏还没有登陆时不能使用ME.user
/// </summary>
public class SCRIPT_30008 : Script
{
    public override object Call(params object[] _params)
    {
        // weight_args
        LPCMapping args = (_params[0] as LPCValue).AsMapping;

        // 可以使用ME.user
        if (ME.user.Query<int>("level") < args.GetValue<int>("level_limit"))
            return 0;

        // 返回参数
        return args.GetValue<int>("weight");
    }
}

/// <summary>
/// 角色还有没有进入游戏系统tip提示权重计算脚本
/// 在游戏还没有登陆时不能使用ME.user
/// </summary>
public class SCRIPT_30009 : Script
{
    public override object Call(params object[] _params)
    {
        // weight_args
        LPCValue args = _params[0] as LPCValue;

        // 返回参数
        return args.AsInt;
    }
}

/// <summary>
/// 普通副本进入副本提示类型计算脚本
/// </summary>
public class SCRIPT_30010 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数： instanceId, mType, mPara, tip_type_args

        LPCValue args = _params[3] as LPCValue;

        // 返回参数
        return args.AsString;
    }
}

/// <summary>
/// 通天塔副本进入提示类型计算脚本
/// </summary>
public class SCRIPT_30011 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数： instanceId, mType, mPara, tip_type_args

        // 后续策划可以根据副本不同的boss关卡提示
        // 策划在脚本中自行实现
        // 通过副本获取通天塔相关信息
        string instanceId = (string) _params[0];
        CsvRow data = TowerMgr.GetTowerInfoByInstance(instanceId);

        // 获取通天塔层级和难度信息
        int layer = data.Query<int>("layer");
        int difficulty = data.Query<int>("difficulty");

        // 根据难度层级获取副本boss关卡资源组编号
        // 根据boss层关卡来提示（需要system_tip_ex配置boss层资源编号group相应的提示类型）
        string groupId = TowerMgr.GetTowerBossResourceGroup(difficulty, layer);

        // 如果有指定资源组提示
        if (SystemTipsMgr.HasTipType(groupId))
            return groupId;

        // 使用默认提示
        LPCValue args = _params[3] as LPCValue;
        return args.AsString;
    }
}

/// <summary>
/// 通天塔解锁脚本
/// </summary>
public class SCRIPT_30012 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象
        Property user = _params[0] as Property;
        if (user == null)
            return false;

        if (user.Query<int>("new_function/tower") != 1)
            return false;

        return true;
    }
}

/// <summary>
/// system_tip选择权重脚本
/// </summary>
public class SCRIPT_30013 : Script
{
    public override object Call(params object[] _params)
    {
        // weight_args
        LPCMapping args = (_params[0] as LPCValue).AsMapping;
        LPCMapping extra_para = _params[1] as LPCMapping;

        // 判断总次数是否满足需求
        int sumTimes = extra_para.GetValue<int>("sum_times");
        LPCArray totalClickTimes = args.GetValue<LPCArray>("total_click_times");
        int minTimes = totalClickTimes[0].AsInt;
        int maxTimes = totalClickTimes[1].AsInt;

        // 不能抽取该提示信息已经超出了抽取范围
        if (sumTimes < minTimes ||
            (maxTimes != -1 && sumTimes > maxTimes))
            return 0;

        // 获取当前点击次数
        int times = extra_para.GetValue<int>("times");
        LPCArray clickTimes = args.GetValue<LPCArray>("click_times");
        minTimes = clickTimes[0].AsInt;

        // 不满足抽取次数范围
        if (times < minTimes)
            return 0;

        // 满足条件返回正常的抽取概率
        return args.GetValue<int>("weight");
    }
}

/// <summary>
/// system_tip选择权重脚本
/// </summary>
public class SCRIPT_30014 : Script
{
    public override object Call(params object[] _params)
    {
        // weight_args
        LPCMapping args = (_params[0] as LPCValue).AsMapping;
        LPCMapping extra_para = _params[1] as LPCMapping;

        // 不符合条件
        if (!extra_para.ContainsKey("show_times"))
            return 0;

        // 显示不符合
        if (args.GetValue<int>("show_times") != extra_para.GetValue<int>("show_times"))
            return 0;

        // 返回参数
        return args.GetValue<int>("weight");
    }
}

/// <summary>
/// 等级奖励界面空白提示抽取规则
/// </summary>
public class SCRIPT_30015 : Script
{
    public override object Call(params object[] _params)
    {
        // weight_args
        LPCMapping args = (_params[0] as LPCValue).AsMapping;
        LPCMapping extra_para = _params[1] as LPCMapping;

        // 判断总次数是否满足需求
        int sumTimes = extra_para.GetValue<int>("sum_times");
        LPCArray totalClickTimes = args.GetValue<LPCArray>("total_click_times");
        int minTimes = totalClickTimes[0].AsInt;
        int maxTimes = totalClickTimes[1].AsInt;

        // 不能抽取该提示信息已经超出了抽取范围
        if (sumTimes < minTimes ||
            (maxTimes != -1 && sumTimes > maxTimes))
            return 0;

        // 获取当前点击次数
        int times = extra_para.GetValue<int>("times");
        LPCArray clickTimes = args.GetValue<LPCArray>("click_times");
        maxTimes = clickTimes[1].AsInt;

        // 不满足抽取次数范围
        if (times >= maxTimes)
            return 0;

        // 满足条件返回正常的抽取概率
        return args.GetValue<int>("weight");
    }
}

/// <summary>
/// 精英圣域副本物件编号通用计算脚本
/// </summary>
public class SCRIPT_30016 : Script
{
    public override object Call(params object[] _params)
    {
        // 获取副本等级
        // LPCValue level = (int) _params[0];

        // 脚本参数
        LPCMapping args = _params[2] as LPCMapping;

        // 返回classId
        return args.GetValue<int>("class_id");
    }
}

/// <summary>
/// 核心圣域副本进入副本提示类型计算脚本
/// </summary>
public class SCRIPT_30017 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数： instanceId, mType, mPara, tip_type_args

        string instanceId = (string) _params[0];
        LPCArray args = (_params[3] as LPCValue).AsArray;

        switch (instanceId)
        {
            case "hx1":
            case "hx6":
                return args[0].AsString;
            case "hx2":
            case "hx7":
                return args[1].AsString;
            case "hx3":
            case "hx8":
                return args[2].AsString;
            case "hx4":
            case "hx5":
            case "hx9":
            case "hx10":
                return args[3].AsString;
            default:
                return args[0].AsString;
        }
    }
}

/// <summary>
/// 通天塔怪物初始化参数计算脚本
/// </summary>
public class SCRIPT_30018 : Script
{
    public override object Call(params object[] _params)
    {
        // 获取副本等级
        // LPCValue level = (int) _params[0];

        // 脚本参数
        LPCMapping args = (_params[1] as LPCValue).AsMapping;
        LPCMapping instancePara = _params[2] as LPCMapping;

        LPCMapping finalMap = new LPCMapping();
        finalMap.Append(args);
         //检查速度影响因子
        if (finalMap.ContainsKey("speed_factor"))
        {
            int layer = instancePara.GetValue<int>("layer");
            if (layer >= finalMap.GetValue<int>("layer_factor"))
                finalMap.Add("add_speed", finalMap.GetValue<int>("speed_factor"));
        }

        // 返回参数
        return finalMap;
    }
}

/// <summary>
/// 竞技场地图解锁判断脚本
/// </summary>
public class SCRIPT_30019 : Script
{
    public override object Call(params object[] _params)
    {
        // 如果竞技场任务已经完成
        if (GuideMgr.IsGuided(GuideConst.ARENA_FINISH))
            return true;

        // 解锁参数
        LPCValue args = _params[1] as LPCValue;

        // 参数格式不正确
        if (!args.IsMapping)
            return true;

        // 获取解锁条件
        LPCArray unlockGuide = args.AsMapping.GetValue<LPCArray>("unlock_guide");
        if (unlockGuide == null)
            return true;

        // 当前分组指引是否完成，显示按钮
        if (GuideMgr.StepUnlock(unlockGuide[0].AsInt, unlockGuide[1].AsInt))
            return true;

        // 还没有解锁
        return false;
    }
}

/// <summary>
/// 地图通关奖励脚本（显示用）
/// </summary>
public class SCRIPT_30050 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0]   通关奖励参数  LPCMapping
        // _params[1]   地图id        int
        // _params[2]   动态地图数据  LPCMapping

        LPCMapping args = _params[0] as LPCMapping;
        if (args == null)
            return null;

        return args;
    }
}

/// <summary>
/// 好友圣域掉落奖励显示脚本（显示用）
/// </summary>
public class SCRIPT_30051 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0]   通关奖励参数  LPCMapping
        // _params[1]   地图id        int
        // _params[2]   动态地图数据  LPCMapping

        LPCMapping args = _params[0] as LPCMapping;
        if (args == null)
            return null;

        LPCArray secretDungeonsList = InstanceMgr.GetSecretDungeonsList(ME.user);

        LPCArray sortList = new LPCArray();
        // 筛选重复pet_id的副本
        foreach (LPCValue dynamicMapData in secretDungeonsList.Values)
        {
            int pet_id = dynamicMapData.AsMapping.GetValue<int>("pet_id");

            if (sortList.IndexOf(pet_id) != -1)
                continue;

            sortList.Add(pet_id);
        }

        args.Add("item_id", sortList);

        return args;
    }
}

/// <summary>
/// 精英圣域掉落奖励显示脚本（显示用）
/// </summary>
public class SCRIPT_30052 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0]   通关奖励参数  LPCMapping
        // _params[1]   地图id        int
        // _params[2]   动态地图数据  LPCMapping

        LPCMapping args = _params[0] as LPCMapping;
        if (args == null)
            return null;

        LPCArray petDungeonsList = InstanceMgr.GetPetDungeonsList(ME.user);

        LPCArray sortList = new LPCArray();
        // 筛选重复pet_id的副本
        foreach (LPCValue dynamicMapData in petDungeonsList.Values)
        {
            int pet_id = dynamicMapData.AsMapping.GetValue<int>("pet_id");

            if (sortList.IndexOf(pet_id) != -1)
                continue;

            sortList.Add(pet_id);
        }

        args.Add("item_id", sortList);

        return args;
    }
}

/// <summary>
/// 指引执行脚本打开某个窗口
/// </summary>
public class SCRIPT_40000 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;

        string wndName = data.GetValue<string>("wnd_name");

        if (data.ContainsKey("stop_bgm"))
        {
            GameSoundMgr.StopBgSound();
        }
        if (data.ContainsKey("bgm_group"))
        {
            GameSoundMgr.SetMusicVolume(GameSettingMgr.GetSetting<LPCValue>("default_music_volume").AsFloat);
            GameSoundMgr.PlayBgmMusic(data.GetValue<string>("bgm_group"));
        }
        if (data.ContainsKey("sound_group"))
        {
            GameSoundMgr.SetSoundVolume(GameSettingMgr.GetSetting<LPCValue>("default_sound_volume").AsFloat);
            GameSoundMgr.PlayGroupSound(data.GetValue<string>("sound_group"));
        }

        // 打开某个窗口
        WindowMgr.OpenWnd(wndName);

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        // 当前阶段完成，执行回调
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }
}

/// <summary>
/// 指引执行脚本关闭某个窗口
/// </summary>
public class SCRIPT_40001 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        if (script_args.AsMapping.Count == 0)
            return false;

        string wndName = script_args.AsMapping.GetValue<string>("wnd_name");

        // 关闭某个窗口
        WindowMgr.DestroyWindow(wndName);

        // 当前阶段完成，执行回调
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

/// <summary>
/// 显示指引对话框信息
/// </summary>
public class SCRIPT_40002 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;

        string wndName = data.GetValue<string>("wnd_name");

        string npcName = data.GetValue<string>("name");

        LPCValue content = data.GetValue<LPCValue>("content");

        // 文字显示效果类型
        int showEffect = data.GetValue<int>("is_show_effect");

        int charsPerSecond = data.GetValue<int>("chars_per_second");

        int fontSize = data.GetValue<int>("font_size");

        CallBack cb = _params[1] as CallBack;

        // 打开对话框窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        // 绑定数据
        wnd.GetComponent<TalkBoxWnd>().Bind(npcName, content, showEffect == 1 , charsPerSecond, fontSize, cb);

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }
}

/// <summary>
/// 显示指引窗口
/// </summary>
public class SCRIPT_40003 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        // 当前阶段完成，执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 触发下一组指引
/// </summary>
public class SCRIPT_40004 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue scriptArgs = _params[0] as LPCValue;
        if (scriptArgs.IsMapping)
        {
            // 开启某个窗口
            string wndName = scriptArgs.AsMapping.GetValue<string>("wnd_name");
            if (! string.IsNullOrEmpty(wndName))
                WindowMgr.OpenWnd(wndName);
        }

        // 执行回调，结束当前分组指引
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 指引执行脚本
public class SCRIPT_40005 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 指引执行脚本
public class SCRIPT_40006 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        // 执行回调，结束当前分组指引
        if (cb != null)
            cb.Go();

        WindowMgr.HideWindow(PetStrengthenWnd.WndType);

        return true;
    }
}

// 指引执行脚本
public class SCRIPT_40007 : Script
{
    string wndName = string.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        LPCArray pos = data.GetValue<LPCArray>("guide_pos");

        Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control != null)
        {
            control.EndSceneCameraAnimation();

            if (!ME.isLoginOk)
                ME.OnLoginOK();

            control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
        }

        // 缓存场景相机的位置
        SceneMgr.SceneCameraToPos = vec;
        SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        CallBack cb = para as CallBack;

        // 打开召唤界面
        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnSummonMaskCallBack, cb));

        // 关闭当前指引窗口
        WindowMgr.DestroyWindow(wndName);
    }

    void OnSummonMaskCallBack(object para, object[] param)
    {
        CallBack cb = para as CallBack;

        // 关闭主界面
        WindowMgr.HideWindow(WindowMgr.GetWindow(MainWnd.WndType));

        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_SUMMON, new CallBack(OnEnterSummonScene, cb));
    }

    /// <summary>
    /// 进入召唤场景回调
    /// </summary>
    private void OnEnterSummonScene(object para, object[] param)
    {
        CallBack cb = para as CallBack;

        // 打开主场景
        WindowMgr.OpenWnd("SummonWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        if (cb != null)
            cb.Go();
    }
}

// 指引执行脚本
public class SCRIPT_40008 : Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        cb = _params[1] as CallBack;

        // 打开指引窗口
        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd, wndName));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        EventMgr.RegisterEvent("SCRIPT_40008", EventMgrEventType.EVENT_SUMMON_SUCCESS, OnEventSummonSuccess);

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 召唤成功事件回调
    /// </summary>
    void OnEventSummonSuccess(int eventId, MixedValue para)
    {
        EventMgr.UnregisterEvent("SCRIPT_40008");

        if (cb != null)
            cb.Go();

        cb = null;
    }

    /// <summary>
    /// 指引按钮点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 关闭当前指引窗口
        string wndName = para as string;

        WindowMgr.DestroyWindow(wndName);

        // 开始召唤
        GameObject summonWnd = WindowMgr.GetWindow(SummonWnd.WndType);
        if (summonWnd != null)
            summonWnd.GetComponent<SummonWnd>().doSummon(1);
    }
}

// 指引执行脚本
public class SCRIPT_40009 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        CallBack cb = para as CallBack;

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        SummonMgr.UnLoadModel();

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnSummonMaskCallBack, cb));

        // 销毁本窗口
        WindowMgr.DestroyWindow(SummonWnd.WndType);
    }

    void OnSummonMaskCallBack(object para, object[] param)
    {
        CallBack cb = para as CallBack;

        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OnEnterMainCityScene, cb));
    }

    /// <summary>
    /// 打开主城回调
    /// </summary>
    private void OnEnterMainCityScene(object para, object[] param)
    {
        // 打开主窗口
        WindowMgr.OpenMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        // 当前阶段完成，执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

// 指引执行脚本,进入副本
public class SCRIPT_40010 : Script
{
    CallBack cb = null;

    LPCMapping data = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        // 执行回调
        cb = _params[1] as CallBack;

        // 注册事件
        EventMgr.RegisterEvent("SCRIPT_40010", EventMgrEventType.EVENT_ROUND_END, OnRoundEnd);

        // 隐藏主窗口
        WindowMgr.HideMainWnd();

        if (data.ContainsKey("stop_bgm"))
        {
            GameSoundMgr.StopBgSound();
        }

        // 构建副本rid
        string rid = Rid.New();

        // 打开副本
        InstanceMgr.OpenInstance(ME.user, "guide_1", rid);

        // 进入副本
        InstanceMgr.DoEnterInstance(ME.user,
            "guide_1",
            rid,
            0,
            LPCMapping.Empty,
            LPCMapping.Empty,
            LPCMapping.Empty,
            LPCMapping.Empty);

        // 延迟到下一帧调用隐藏自己;
        MergeExecuteMgr.DispatchExecute(DoDestroy);

        return true;
    }

    /// <summary>
    /// 回合结束回调
    /// </summary>
    void OnRoundEnd(int eventId, MixedValue para)
    {
        if (cb != null)
            cb.Go();

        EventMgr.UnregisterEvent("SCRIPT_40010");

        cb = null;

        // 暂停回合制战斗
        RoundCombatMgr.PauseRoundCombat();
    }

    void DoDestroy()
    {
        WindowMgr.DestroyWindow(data.GetValue<string>("wnd_name"));
    }
}

// 指引执行脚本,退出副本，返回主城界面
public class SCRIPT_40011 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;

        string wndName = data.GetValue<string>("wnd_name");

        if (InstanceMgr.IsInInstance(ME.user))
        {
            // 执行回调
            if (cb != null)
                cb.Go();

            // 打开主场景
            SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OpenWorldMapWnd, cb));
        }
        else
        {
            // 执行回调
            if (cb != null)
                cb.Go();
        }

        if (!string.IsNullOrEmpty(wndName))
            WindowMgr.DestroyWindow(wndName);

        return true;
    }

    /// <summary>
    /// 打开主场景完成回调
    /// </summary>
    private void OpenWorldMapWnd(object para, object[] param)
    {
        //离开副本;
        InstanceMgr.LeaveInstance(ME.user);

        // 关闭结算界面
        WindowMgr.DestroyWindow(FightSettlementWnd.WndType);

        // 关闭结算界面
        WindowMgr.DestroyWindow(CombatWnd.WndType);

        // 打开主界面
        GameObject mainWnd = WindowMgr.OpenWnd ("MainWnd");
        if (mainWnd == null)
            return;

        // 设置打开方式
        mainWnd.GetComponent<MainWnd>().ShowMainUIBtn(true);
    }
}

/// <summary>
/// 指引执行脚本，冒险指引
/// </summary>
public class SCRIPT_40012 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnWorldMaskCallBack, para));
    }

    void OnWorldMaskCallBack(object para, params object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OnEnterWorldMapScene, para));
    }

    /// <summary>
    /// 地图场景加载完成会掉
    /// </summary>
    private void OnEnterWorldMapScene(object para, object[] param)
    {
        // 隐藏主场景窗口
        GameObject mainWnd = WindowMgr.OpenWnd(MainWnd.WndType);
        if (mainWnd == null)
            return;

        mainWnd.GetComponent<MainWnd>().ShowMainUIBtn(false);

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        Camera camera = SceneMgr.SceneCamera;

        camera.transform.localPosition = new Vector3(0, 0, camera.transform.localPosition.z);

        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引执行脚本，地图点击指引
/// </summary>
public class SCRIPT_40013 : Script
{
    string wndName = string.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        LPCArray mapPos = data.GetValue<LPCArray>("map_pos");

        int mapId = data.GetValue<int>("map_id");

        List<object> list = new List<object>();
        list.Add(cb);
        list.Add(mapPos);
        list.Add(mapId);

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, list));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        List<object> list = para  as List<object>;

        //获取副本选择界面;
        GameObject wnd = WindowMgr.OpenWnd(SelectInstanceWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        SelectInstanceWnd selectInstanceScript = wnd.GetComponent<SelectInstanceWnd>();

        if (selectInstanceScript == null)
            return;

        LPCArray pos = list[1] as LPCArray;
        if (pos == null)
            return;

        // 绑定数据
        selectInstanceScript.Bind((int)list[2], new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat));

        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);
        if (mainWnd != null)
        {
            WindowMgr.HideWindow(mainWnd);

            mainWnd.GetComponent<MainWnd>().mTowerDiffSelectWnd.SetActive(false);
        }

        // 当前阶段完成，执行回调
        CallBack cb = list[0] as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引执行脚本，选择「平原浅滩」副本
/// </summary>
public class SCRIPT_40014 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        string instanceId = data.GetValue<string>("instance_id");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        List<object> list = new List<object>();
        list.Add(cb);
        list.Add(instanceId);

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, list));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        List<object> list = para as List<object>;

        string instanceId = list[1] as string;

        int mapType = InstanceMgr.GetMapTypeByInstanceId(instanceId);

        GameObject instacne = null;

        if (mapType == MapConst.INSTANCE_MAP_1)
        {
            instacne = WindowMgr.GetWindow(SelectInstanceWnd.WndType);
        }
        else if (mapType == MapConst.DUNGEONS_MAP_2)
        {
            instacne = WindowMgr.GetWindow(DungeonsWnd.WndType);
        }
        else
        {
        }

        if (instacne == null)
            return;

        instacne.SetActive(false);

        //获得选择战斗窗口
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        SelectFighterWnd selectFighterScript = wnd.GetComponent<SelectFighterWnd>();

        if (selectFighterScript == null)
            return;

        // 绑定数据
        selectFighterScript.Bind("SelectInstanceWnd", instanceId);

        // 关闭副本选择界面
        WindowMgr.HideWindow(SelectInstanceWnd.WndType);

        // 当前阶段执完成，执行回调
        CallBack cb = list[0] as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引执行脚本,出战使魔选择指引
/// </summary>
public class SCRIPT_40015 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        string itemName = data.GetValue<string>("item_name");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        List<object> list = new List<object>();
        list.Add(cb);
        list.Add(itemName);

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, list));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        List<object> list = para as List<object>;

        string itemName = list[1] as string;

        // 选择使魔
        GameObject selectWnd = WindowMgr.GetWindow(SelectFighterWnd.WndType);
        if (selectWnd == null)
        {
            LogMgr.Trace("SelectFighterWnd不存在");
            return;
        }

        SelectFighterWnd selectFighterWndScript = selectWnd.GetComponent<SelectFighterWnd>();

        PlayerPetWnd playerPetWndScript = selectFighterWndScript.mPlayerPetWnd;

        GameObject item = null;

        playerPetWndScript.mItems.TryGetValue(itemName, out item);

        if (item == null)
            return;

        // 选择宠物
        playerPetWndScript.ClickSelectPet(item);

        // 指引完成，执行回调
        CallBack cb = list[0] as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引执行脚本,开始战斗
/// </summary>
public class SCRIPT_40016: Script
{
    CallBack cb = null;

    string wndName = string.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        GameObject selectWnd = WindowMgr.GetWindow(SelectFighterWnd.WndType);
        if (selectWnd == null)
        {
            LogMgr.Trace("SelectFighterWnd不存在");
            return;
        }

        SelectFighterWnd selectFighterWndScript = selectWnd.GetComponent<SelectFighterWnd>();

        // 进入战斗
        selectFighterWndScript.GuideClickEnterBattle();

        EventMgr.RegisterEvent("SCRIPT_40016_EVENT_INSTANCE_CLEARANCE", EventMgrEventType.EVENT_INSTANCE_CLEARANCE, OnInstanceClearance);

        MsgMgr.RegisterDoneHook("MSG_ENTER_INSTANCE", "SCRIPT_40016_MSG_ENTER_INSTANCE", OnMsgEnterInstance);

        //注册副事件;
        EventMgr.RegisterEvent("SCRIPT_40016_EVENT_GUIDE_RETUEN_OPERATE", EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, OnReturnGuide);
        EventMgr.RegisterEvent("SCRIPT_40016_EVENT_INSTANCE_LEVEL_START", EventMgrEventType.EVENT_INSTANCE_LEVEL_START, OnStartCombat);
    }

    /// <summary>
    /// MSG_ENTER_INSTANCE消息回调
    /// </summary>
    void OnMsgEnterInstance(string cmd, LPCValue para)
    {
        if (!string.IsNullOrEmpty(wndName))
            WindowMgr.DestroyWindow(wndName);
    }

    /// <summary>
    /// 副本通关回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    void OnInstanceClearance(int eventId, MixedValue para)
    {
        MsgMgr.RemoveDoneHook("MSG_ENTER_INSTANCE", "SCRIPT_40016_MSG_ENTER_INSTANCE");

        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null)
            return;

        // 通关失败清除当前副本缓存阵容
        if (data.GetValue<int>("result") != 1)
        {
            FormationMgr.SetArchiveFormation(data.GetValue<string>("instance_id"), new List<Property>());

            return;
        }

        EventMgr.UnregisterEvent("SCRIPT_40016_EVENT_GUIDE_RETUEN_OPERATE");
        EventMgr.UnregisterEvent("SCRIPT_40016_EVENT_INSTANCE_LEVEL_START");
        EventMgr.UnregisterEvent("SCRIPT_40016_EVENT_INSTANCE_CLEARANCE");

        // 执行回调继续指引
        if (cb != null)
            cb.Go();

        cb = null;
    }

    void OnReturnGuide(int eventId, MixedValue para)
    {
        MsgMgr.RemoveDoneHook("MSG_ENTER_INSTANCE", "SCRIPT_40016_MSG_ENTER_INSTANCE");
        EventMgr.UnregisterEvent("SCRIPT_40016_EVENT_SETTLEMENT_BONUS_SHOW_FINISH");
        EventMgr.UnregisterEvent("SCRIPT_40016_EVENT_GUIDE_RETUEN_OPERATE");
        EventMgr.UnregisterEvent("SCRIPT_40016_EVENT_INSTANCE_LEVEL_START");

        // 指引对象不存在
        if (GuideMgr.GuideOb == null)
            return;

        int type = para.GetValue<int>();
        if (type == GuideConst.RETURN_SELECT_INSTANCE)
        {
            GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 2);
        }
        else if (type == GuideConst.RETURN_RISK)
        {
            GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 4);
        }

        cb = null;
    }

    /// <summary>
    /// 开始战斗回调
    /// </summary>
    void OnStartCombat(int eventId, MixedValue para)
    {
        Instance instance = para.GetValue<Instance>();
        if (instance == null)
            return;

        if (instance.Level != 1)
            return;

        EventMgr.UnregisterEvent("SCRIPT_40016_EVENT_INSTANCE_LEVEL_START");

        WindowMgr.OpenWnd(GuideCombatWnd.WndType);
    }
}

/// <summary>
/// 指引执行脚本,恢复回合制战斗
/// </summary>
public class SCRIPT_40017: Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        RoundCombatMgr.ContinueRoundCombat();

        // 注册事件
        EventMgr.RegisterEvent("SCRIPT_40017", EventMgrEventType.EVENT_ROUND_END, OnRoundEnd);

        return true;
    }

    /// <summary>
    /// 回合结束
    /// </summary>
    void OnRoundEnd(int eventId, MixedValue para)
    {
        // 当前阶段完成，执行回调
        if (cb != null)
            cb.Go();

        EventMgr.UnregisterEvent("SCRIPT_40017");

        cb = null;
    }
}

/// <summary>
/// 战斗指引选择技能
/// </summary>
public class SCRIPT_40018 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        // 恢复战斗
        RoundCombatMgr.ContinueRoundCombat();

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, cb));

        return true;
    }

    /// <summary>
    /// 窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        // 选择技能
        GameObject combatWnd = WindowMgr.GetWindow(CombatWnd.WndType);
        if (combatWnd == null)
            return;

        combatWnd.GetComponent<CombatWnd>().SelectSkill(3);

        // 当前阶段完成，执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 战斗指引释放技能
/// </summary>
public class SCRIPT_40019 : Script
{
    int mClassId = 0;

    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        mClassId = data.GetValue<int>("class_id");

        // 执行回调
        cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        // 选择技能
        GameObject combatWnd = WindowMgr.GetWindow(CombatWnd.WndType);
        if (combatWnd == null)
            return;

        // 攻方列表
        List<Property> list = RoundCombatMgr.GetPropertyList(CampConst.CAMP_TYPE_ATTACK);
        if (list == null)
            return;

        string targetRid = string.Empty;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
                continue;

            if (list[i].GetClassID() == mClassId)
            {
                targetRid = list[i].GetRid();

                break;
            }
        }

        // 注册事件
        EventMgr.RegisterEvent("SCRIPT_40019", EventMgrEventType.EVENT_ROUND_END, OnRoundEnd);

        combatWnd.GetComponent<CombatWnd>().GuideDoSkill(targetRid);
    }

    /// <summary>
    /// 回合结束回调
    /// </summary>
    void OnRoundEnd(int eventId, MixedValue para)
    {
        RoundCombatMgr.PauseRoundCombat();

        EventMgr.UnregisterEvent("SCRIPT_40019");

        if (cb != null)
            cb.Go();
    }
}

// 指引执行脚本
public class SCRIPT_40020 : Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd, wndName));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        EventMgr.RegisterEvent("SCRIPT_40020", EventMgrEventType.EVENT_SUMMON_SUCCESS, OnEventSummonSuccess);

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 召唤成功事件回调
    /// </summary>
    void OnEventSummonSuccess(int eventId, MixedValue para)
    {
        EventMgr.UnregisterEvent("SCRIPT_40020");

        if (cb != null)
            cb.Go();

        cb = null;
    }

    /// <summary>
    /// 指引按钮点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 关闭当前指引窗口
        string wndName = para as string;

        // 开始召唤
        GameObject summonWnd = WindowMgr.GetWindow(SummonWnd.WndType);
        if (summonWnd != null)
            summonWnd.GetComponent<SummonWnd>().doSummon(1);

        WindowMgr.DestroyWindow(wndName);
    }
}

// 指引执行脚本
public class SCRIPT_40021 : Script
{
    CallBack cb = null;

    LPCMapping mData = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        GameObject wnd = WindowMgr.OpenWnd(mData.GetValue<string>("wnd_name"));
        if (wnd == null)
            return false;

        // 暂停回合制战斗
        RoundCombatMgr.PauseRoundCombat();

        wnd.GetComponent<GuideWnd>().Bind(mData.GetValue<string>("guide_desc"), mData, new CallBack(OnClickWnd));

        EventMgr.RegisterEvent("SCRIPT_40021_EVENT_GUIDE_RETUEN_OPERATE", EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, OnGuideReturnOperate);

        EventMgr.RegisterEvent("SCRIPT_40021_EVENT_INSTANCE_CLEARANCE", EventMgrEventType.EVENT_INSTANCE_CLEARANCE, OnInstanceClearance);

        if (mData.ContainsKey("guide_pos"))
        {
            LPCArray pos = mData.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    void OnClickWnd(object para, params object[] param)
    {
        // 恢复回合制战斗
        RoundCombatMgr.ContinueRoundCombat();

        WindowMgr.DestroyWindow(mData.GetValue<string>("wnd_name"));
    }

    /// <summary>
    /// 副本通关回调
    /// </summary>
    void OnInstanceClearance(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data.GetValue<int>("result") != 1)
            return;

        EventMgr.UnregisterEvent("SCRIPT_40021_EVENT_INSTANCE_CLEARANCE");

        EventMgr.UnregisterEvent("SCRIPT_40021_EVENT_GUIDE_RETUEN_OPERATE");

        if (cb != null)
            cb.Go();

        cb = null;
    }

    void OnGuideReturnOperate(int eventId, MixedValue para)
    {
        int type = para.GetValue<int>();
        if (type == GuideConst.RETURN_SELECT_INSTANCE)
        {
            if (GuideMgr.GuideOb != null)
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 3);
        }
        else if (type == GuideConst.RETURN_RISK)
        {
            if (GuideMgr.GuideOb != null)
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 5);
        }

        // 解注册
        EventMgr.UnregisterEvent("SCRIPT_40021_EVENT_GUIDE_RETUEN_OPERATE");
        EventMgr.UnregisterEvent("SCRIPT_40021_EVENT_INSTANCE_CLEARANCE");

        cb = null;
    }
}

/// <summary>
/// 指引执行脚本
/// </summary>
public class SCRIPT_40022 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;

        if (data.ContainsKey("wnd_name"))
            WindowMgr.DestroyWindow(data.GetValue<string>("wnd_name"));

        // 执行回调
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

/// <summary>
/// 指引执行脚本, 打开包裹界面
/// </summary>
public class SCRIPT_40023 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 打开包裹界面
        WindowMgr.OpenWnd(BaggageWnd.WndType);

        // 当前阶段完成，执行回调继续下一步指引
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引执行脚本, 选择使魔
/// </summary>
public class SCRIPT_40024 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 选择装备分页
        GameObject baggageWnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (baggageWnd == null)
            return;

        GameObject petToolTip = baggageWnd.GetComponent<BaggageWnd>().mPetToolTipWnd;
        if (petToolTip == null)
            return;

        petToolTip.GetComponent<PetToolTipWnd>().OnTabBtnClicked((int) BAGGAGE_PAGE.EQUIP_PAGE);

        // 当前阶段完成，执行回调继续下一步指引
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引执行脚本, 穿装备
/// </summary>
public class SCRIPT_40025 : Script
{
    CallBack cb = null;

    bool mIsClick = false;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        mIsClick = false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    GameObject equipViewWnd;

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        if (mIsClick)
            return;

        mIsClick = true;

        // 执行装备按钮点击
        equipViewWnd = WindowMgr.GetWindow(EquipViewWnd.WndType + "_UnEquip");
        if (equipViewWnd == null)
            return;

        equipViewWnd.GetComponent<EquipViewWnd>().GuideOnClickEquipBtn();

        // 注册包裹变化回调
        ME.user.baggage.eventCarryChange += BaggageChange;
    }

    void BaggageChange(string[] pos)
    {
        // 当前阶段完成，执行回调继续下一步指引
        if (cb != null)
        {
            cb.Go();

            WindowMgr.DestroyWindow(equipViewWnd.name);

            cb = null;
        }
    }
}

/// <summary>
/// 指引执行脚本, 选择玩家身上的装备
/// </summary>
public class SCRIPT_40026 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 选择玩家穿戴的装备
        GameObject baggageWnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (baggageWnd == null)
            return;

        GameObject petToolTip = baggageWnd.GetComponent<BaggageWnd>().mPetToolTipWnd;
        if (petToolTip == null)
            return;

        petToolTip.GetComponent<PetToolTipWnd>().GuideOnClickEquipItem(1);

        // 当前阶段完成，执行回调继续下一步指引
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引执行脚本, 选择包裹中的装备
/// </summary>
public class SCRIPT_40027 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 选择装备分页
        GameObject baggageWnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (baggageWnd == null)
            return;

        GameObject petToolTip = baggageWnd.GetComponent<BaggageWnd>().mPetToolTipWnd;
        if (petToolTip == null)
            return;

        // 装备界面
        GameObject equipWnd = petToolTip.GetComponent<PetToolTipWnd>().mGroupWnd[1];
        if (equipWnd == null)
            return;

        // 执行装备点击事件
        equipWnd.GetComponent<EquipWnd>().GuideClickEquipItem("equip_item_0_0");

        // 当前阶段完成，执行回调继续下一步指引
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引脚本点击强化按钮,打开强化界面
/// </summary>
public class SCRIPT_40028 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 执行装备按钮点击
        GameObject equipViewWnd = WindowMgr.GetWindow(EquipViewWnd.WndType + "_Equip");
        if (equipViewWnd == null)
            return;

        equipViewWnd.GetComponent<EquipViewWnd>().GuideOnClickStrengthBtn();

        // 当前阶段完成，执行回调继续下一步指引
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引脚本,点击按钮强化装备
/// </summary>
public class SCRIPT_40029 : Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd, wndName));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        // 注册装备强化事件
        EventMgr.RegisterEvent("SCRIPT_40029", EventMgrEventType.EVENT_EQUIP_STRENGTHEN_FINISH, OnEquipStrengthen);

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 装备强化界面
        GameObject strengthenWnd = WindowMgr.GetWindow(EquipStrengthenWnd.WndType);
        if (strengthenWnd == null)
            return;

        // 指引点击强化装备按钮
        strengthenWnd.GetComponent<EquipStrengthenWnd>().GuideOnClickStrengthenBtn();

        string wndName = para as string;

        WindowMgr.DestroyWindow(wndName);
    }

    /// <summary>
    /// 装备强化事件回调
    /// </summary>
    void OnEquipStrengthen(int eventId, MixedValue para)
    {
        // 解注册事件
        EventMgr.UnregisterEvent("SCRIPT_40029");

        // 当前阶段执行完成
        if (cb != null)
            cb.Go();

        cb = null;
    }
}

// 指引执行脚本
public class SCRIPT_40030 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        GameObject baggageWnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (baggageWnd == null)
        {
            // 选择装备分页
            baggageWnd = WindowMgr.OpenWnd(BaggageWnd.WndType);
            if (baggageWnd == null)
                return false;

            GameObject petToolTip = baggageWnd.GetComponent<BaggageWnd>().mPetToolTipWnd;
            if (petToolTip == null)
                return false;

            petToolTip.GetComponent<PetToolTipWnd>().OnTabBtnClicked((int) BAGGAGE_PAGE.EQUIP_PAGE);
        }

        string wndName = data.GetValue<string>("wnd_name");

        // 打开指引窗口
        WindowMgr.OpenWnd(wndName);

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        // 执行回调
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        // 等待一帧
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }
}

public class SCRIPT_40031 : Script
{
    CallBack cb = null;

    LPCMapping data = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd, wndName));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        // 注册装备强化事件
        EventMgr.RegisterEvent("SCRIPT_40031", EventMgrEventType.EVENT_EQUIP_STRENGTHEN_FINISH, OnEquipStrengthen);

        GameObject baggageWnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (baggageWnd == null)
        {
            // 选择装备分页
            baggageWnd = WindowMgr.OpenWnd(BaggageWnd.WndType);
            if (baggageWnd == null)
                return false;

            // 等待一帧调用
            Coroutine.DispatchService(WaitInvoke(baggageWnd), "WaitInvoke");
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        // 等待一帧
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    IEnumerator WaitInvoke(GameObject baggageWnd)
    {
        yield return null;

        GameObject petToolTip = baggageWnd.GetComponent<BaggageWnd>().mPetToolTipWnd;
        if (petToolTip == null)
            yield break;

        GameObject showPetWnd = baggageWnd.GetComponent<BaggageWnd>().mShowPetWnd;
        if (showPetWnd == null)
            yield break;

        showPetWnd.GetComponent<ShowPetsWnd>().GuideSelectPet("baggage_pet_item_0_1");

        petToolTip.GetComponent<PetToolTipWnd>().OnTabBtnClicked((int) BAGGAGE_PAGE.EQUIP_PAGE);

        // 打开装备悬浮
        petToolTip.GetComponent<PetToolTipWnd>().GuideOnClickEquipItem(1);

        // 打开强化按钮
        GameObject equipViewWnd = WindowMgr.GetWindow(EquipViewWnd.WndType + "_Equip");
        if (equipViewWnd == null)
            yield break;

        equipViewWnd.GetComponent<EquipViewWnd>().GuideOnClickStrengthBtn();
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 装备强化界面
        GameObject strengthenWnd = WindowMgr.GetWindow(EquipStrengthenWnd.WndType);
        if (strengthenWnd == null)
            return;

        // 指引点击强化装备按钮
        strengthenWnd.GetComponent<EquipStrengthenWnd>().GuideOnClickStrengthenBtn();

        string wndName = para as string;

        WindowMgr.DestroyWindow(wndName);

        // 结束协程
        Coroutine.StopCoroutine("WaitInvoke");
    }

    void OnEquipStrengthen(int eventId, MixedValue para)
    {
        // 解注册事件
        EventMgr.UnregisterEvent("SCRIPT_40031");

        // 当前阶段执行完成
        if (cb != null)
            cb.Go();

        cb = null;
    }
}

public class SCRIPT_40032 : Script
{
    string wndName = string.Empty;

    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        wndName = data.GetValue<string>("wnd_name");

        cb = _params[1] as CallBack;

        // 打开指引窗口
        WindowMgr.OpenWnd(wndName);

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        GameObject baggageWnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (baggageWnd == null)
        {
            // 选择装备分页
            baggageWnd = WindowMgr.OpenWnd(BaggageWnd.WndType);
            if (baggageWnd == null)
                return false;

            // 等待一帧调用
            Coroutine.DispatchService(WaitInvoke(baggageWnd), "WaitInvoke");

            return true;
        }

        if (cb != null)
            cb.Go();

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        // 等待一帧
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    IEnumerator WaitInvoke(GameObject baggageWnd)
    {
        yield return null;

        GameObject petToolTip = baggageWnd.GetComponent<BaggageWnd>().mPetToolTipWnd;
        if (petToolTip == null)
            yield break;

        GameObject showPetWnd = baggageWnd.GetComponent<BaggageWnd>().mShowPetWnd;
        if (showPetWnd == null)
            yield break;

        showPetWnd.GetComponent<ShowPetsWnd>().GuideSelectPet("baggage_pet_item_0_1");

        petToolTip.GetComponent<PetToolTipWnd>().OnTabBtnClicked((int) BAGGAGE_PAGE.EQUIP_PAGE);

        // 打开装备悬浮
        petToolTip.GetComponent<PetToolTipWnd>().GuideOnClickEquipItem(1);

        // 打开强化按钮
        GameObject equipViewWnd = WindowMgr.GetWindow(EquipViewWnd.WndType + "_Equip");
        if (equipViewWnd == null)
            yield break;

        equipViewWnd.GetComponent<EquipViewWnd>().GuideOnClickStrengthBtn();

        if (cb != null)
            cb.Go();

        // 结束协程
        Coroutine.StopCoroutine("WaitInvoke");
    }
}

/// <summary>
/// 关闭包裹界面
/// </summary>
public class SCRIPT_40033 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;

        // 关闭指引窗口
        if (data.ContainsKey("wnd_name"))
            WindowMgr.DestroyWindow(data.GetValue<string>("wnd_name"));

        CallBack cb = _params[1] as CallBack;

        // 关闭强化界面
        WindowMgr.DestroyWindow(EquipStrengthenWnd.WndType);

        // 关闭包裹界面
        WindowMgr.DestroyWindow(BaggageWnd.WndType);

        // 当前阶段执行完成,执行回调继续下一个阶段
        if (cb != null)
            cb.Go();

        return true;
    }
}

/// <summary>
/// 显示指引窗口
/// </summary>
public class SCRIPT_40034 : Script
{
    string wndName = string.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, cb));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        GameObject wnd = WindowMgr.OpenWnd(ShopWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
        {
            LogMgr.Trace("ShopWnd窗口创建失败");
            return;
        }

        // 隐藏主城界面
        WindowMgr.HideMainWnd();

        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();

        if (control != null)
        {
            SceneMgr.SceneCameraFromPos = control.transform.localPosition;

            SceneMgr.SceneCameraToPos = new Vector3(3.29f, 0.64f, -16.28f);

            control.MoveCamera(control.transform.localPosition, SceneMgr.SceneCameraToPos, 0.3f, 0f);
        }

        WindowMgr.DestroyWindow(wndName);
    }
}

// 关闭市集界面
public class SCRIPT_40035 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;

        // 关闭指引窗口
        if (data.ContainsKey("wnd_name"))
            WindowMgr.DestroyWindow(data.GetValue<string>("wnd_name"));

        // 执行关闭操作
        GameObject shopWnd = WindowMgr.GetWindow(ShopWnd.WndType);
        if (shopWnd != null)
            shopWnd.GetComponent<ShopWnd>().DoClose();

        // 打开主界面
        WindowMgr.OpenMainWnd();

        // 执行回调
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

public class SCRIPT_40036 : Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        EventMgr.RegisterEvent("SCRIPT_40036", EventMgrEventType.EVENT_INSTANCE_CLEARANCE, OnInstanceClearance);

        EventMgr.RegisterEvent("SCRIPT_40036_EVENT_GUIDE_RETUEN_OPERATE", EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, OnGuideReturnOperate);

        return true;
    }

    void OnInstanceClearance(int eventID, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();

        if (data == null)
            return;

        if (data.GetValue<int>("result") != 1)
            return;

        EventMgr.UnregisterEvent("SCRIPT_40036");
        EventMgr.UnregisterEvent("SCRIPT_40036_EVENT_GUIDE_RETUEN_OPERATE");

        if (cb != null)
            cb.Go();
    }

    void OnGuideReturnOperate(int eventId, MixedValue para)
    {
        int type = para.GetValue<int>();
        // 返回副本选择指引操作
        if (type == GuideConst.RETURN_SELECT_INSTANCE)
        {
            EventMgr.UnregisterEvent("SCRIPT_40036_EVENT_GUIDE_RETUEN_OPERATE");

            if (GuideMgr.GuideOb != null)
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 2);
        }
        else if (type == GuideConst.RETURN_RISK)
        {
            EventMgr.UnregisterEvent("SCRIPT_40036_EVENT_GUIDE_RETUEN_OPERATE");

            if (GuideMgr.GuideOb != null)
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 3);
        }
    }
}

public class SCRIPT_40037 : Script
{
    CallBack cb = null;

    string wndName = string.Empty;

    LPCMapping mData = null;
    string guideDesc = string.Empty;

    string instanceId = string.Empty;

    bool isInInstance = false;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        LPCArray nameList = data.GetValue<LPCArray>("wnd_name");

        LPCArray guideDescList = data.GetValue<LPCArray>("guide_desc");
        mData = data;

        instanceId = data.GetValue<string>("instance_id");

        isInInstance = InstanceMgr.IsInInstance(ME.user);

        if (isInInstance)
        {
            EventMgr.RegisterEvent("SCRIPT_40037", EventMgrEventType.EVENT_SETTLEMENT_BONUS_SHOW_FINISH, OnFinish);

            wndName = nameList[0].AsString;

            guideDesc = guideDescList[0].AsString;
        }
        else
        {
            // 打开副本选择界面

            wndName = nameList[1].AsString;

            guideDesc = guideDescList[1].AsString;

            // 抛出切换地图事件
            SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OnEnterWorldMapScene));
        }

        EventMgr.RegisterEvent("SCRIPT_40037_EVENT_GUIDE_RETUEN_OPERATE", EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, OnGuideReturnOperate);

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        if (! InstanceMgr.IsInInstance(ME.user))
        {
            // 还原操作，打开选择战斗界面
            GameObject instacne = WindowMgr.GetWindow("SelectInstanceWnd");

            if (instacne == null)
                return;

            instacne.SetActive(false);

            //获得选择战斗窗口
            GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            // 窗口创建失败
            if (wnd == null)
                return;

            SelectFighterWnd selectFighterScript = wnd.GetComponent<SelectFighterWnd>();

            if (selectFighterScript == null)
                return;

            // 绑定数据
            selectFighterScript.Bind("SelectInstanceWnd", instanceId);

            // 关闭副本选择界面
            WindowMgr.HideWindow(SelectInstanceWnd.WndType);
        }
        else
        {
            // 副本结算界面
            GameObject settlementWnd = WindowMgr.GetWindow(FightSettlementWnd.WndType);
            if (settlementWnd == null)
                return;

            // 指引点几下一关操作
            settlementWnd.GetComponent<FightSettlementWnd>().GuideOnClickNextBtn();
        }

        // 当前阶段完成执行回调
        if (cb != null)
            cb.Go();

        cb = null;
    }

    /// <summary>
    /// 地图场景加载完成会掉
    /// </summary>
    private void OnEnterWorldMapScene(object para, object[] param)
    {
        WindowMgr.DestroyWindow(MaskWnd.WndType);

        Camera camera = SceneMgr.SceneCamera;

        camera.transform.localPosition = new Vector3(0, 0, camera.transform.localPosition.z);

        // 打开副本选择界面
        //获取副本选择界面;
        GameObject instanceWnd = WindowMgr.OpenWnd(SelectInstanceWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (instanceWnd == null)
            return;

        SelectInstanceWnd selectInstanceScript = instanceWnd.GetComponent<SelectInstanceWnd>();

        if (selectInstanceScript == null)
            return;

        // 绑定数据
        selectInstanceScript.Bind(1, new Vector3(-0.65f, -0.4f, -15));

        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);
        if (mainWnd != null)
        {
            mainWnd.GetComponent<MainWnd>().ShowMainUIBtn(false);

            WindowMgr.HideWindow(mainWnd);

            mainWnd.GetComponent<MainWnd>().mTowerDiffSelectWnd.SetActive(false);
        }

        // 打开指引窗口
        GameObject guideWnd = WindowMgr.OpenWnd(wndName);
        if (guideWnd == null)
            return;

        guideWnd.GetComponent<GuideWnd>().Bind(guideDesc, mData, new CallBack(OnClickCallBack));
    }

    /// <summary>
    /// 奖励显示完成回调
    /// </summary>
    void OnFinish(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null)
            return;

        int result = data.GetValue<int>("result");
        if (result != 1)
            return;

        // 解注册事件
        EventMgr.UnregisterEvent("SCRIPT_40037");

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return;

        wnd.GetComponent<GuideWnd>().Bind(guideDesc, mData, new CallBack(OnClickCallBack));
    }

    /// <summary>
    /// 指引返回操作
    /// </summary>
    void OnGuideReturnOperate(int eventId, MixedValue para)
    {
        EventMgr.UnregisterEvent("SCRIPT_40037_EVENT_GUIDE_RETUEN_OPERATE");

        // 指引对象不存在
        if (GuideMgr.GuideOb == null)
            return;

        if (isInInstance)
        {
            // 返回副本选择指引操作
            if (para.GetValue<int>() == GuideConst.RETURN_SELECT_INSTANCE)
            {
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 1);
            }
            else if (para.GetValue<int>() == GuideConst.RETURN_RISK)
            {
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 1);
            }
        }
        else
        {
            // 返回副本选择指引操作
            if (para.GetValue<int>() == GuideConst.RETURN_SELECT_INSTANCE)
            {
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 1);
            }
            else if (para.GetValue<int>() == GuideConst.RETURN_RISK)
            {
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 1);
            }
        }
    }
}

// 指引执行脚本
public class SCRIPT_40038 : Script
{
    CallBack cb  = null;
    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        LPCArray nameList = data.GetValue<LPCArray>("wnd_name");

        // 关闭指引窗口
        for (int i = 0; i < nameList.Count; i++)
            WindowMgr.DestroyWindow(nameList[i].AsString);

        EventMgr.RegisterEvent("SCRIPT_40038", EventMgrEventType.EVENT_INSTANCE_CLEARANCE, OnInstanceClearance);

        return true;
    }

    /// <summary>
    /// 副本通关
    /// </summary>
    void OnInstanceClearance(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();

        if (data == null)
            return;

        if (data.GetValue<int>("result") != 1)
            return;

        // 当前阶段完成执行回调
        if (cb != null)
            cb.Go();

        EventMgr.UnregisterEvent("SCRIPT_40038");

        cb = null;
    }
}

// 指引执行窗口
public class SCRIPT_40039 : Script
{
    CallBack cb = null;

    LPCMapping mData = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        if (mData.ContainsKey("guide_pos"))
        {
            LPCArray pos = mData.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        // 监听事件
        EventMgr.RegisterEvent("SCRIPT_40039_EVENT_OPEN_COMBATWND", EventMgrEventType.EVENT_OPEN_COMBATWND, OnOpenCombatwnd);

        return true;
    }

    /// <summary>
    /// 战斗窗口打开回调
    /// </summary>
    private void OnOpenCombatwnd(int eventId, MixedValue para)
    {
        // 打开指引小窗口
        string wndName = mData.GetValue<string>("wnd_name");

        string desc = mData.GetValue<string>("guide_desc");

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return;

        wnd.GetComponent<GuideWnd>().Bind(desc, mData, new CallBack(OnClickWnd, wndName));

        // 暂停回合制战斗
        RoundCombatMgr.PauseRoundCombat();

        // 解注册
        EventMgr.UnregisterEvent("SCRIPT_40039_EVENT_OPEN_COMBATWND");

        if (cb != null)
            cb.Go();

        cb = null;
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 继续回合制战斗
        RoundCombatMgr.ContinueRoundCombat();
    }
}

// 指引执行脚本
public class SCRIPT_40040 : Script
{
    CallBack cb = null;

    LPCMapping mData = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        if (InstanceMgr.IsInInstance(ME.user))
        {
            // 监听事件
            EventMgr.RegisterEvent("SCRIPT_40040_EVENT_SETTLEMENT_BONUS_SHOW_FINISH", EventMgrEventType.EVENT_SETTLEMENT_BONUS_SHOW_FINISH, OnSettlementFinish);
        }
        else
        {
            WindowMgr.OpenWnd(mData.GetValue<string>("wnd_name"));

            if (cb != null)
                cb.Go();

            cb = null;
        }

        if (mData.ContainsKey("guide_pos"))
        {
            LPCArray pos = mData.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    /// <summary>
    /// 战斗窗口打开回调
    /// </summary>
    private void OnSettlementFinish(int eventId, MixedValue para)
    {
        WindowMgr.OpenWnd(mData.GetValue<string>("wnd_name"));

        if (cb != null)
            cb.Go();

        cb = null;

        // 解注册事件
        EventMgr.UnregisterEvent("SCRIPT_40040_EVENT_SETTLEMENT_BONUS_SHOW_FINISH");
        EventMgr.UnregisterEvent("SCRIPT_40037_EVENT_GUIDE_RETUEN_OPERATE");
    }
}

/// <summary>
/// 指引执行脚本，地图点击指引
/// </summary>
public class SCRIPT_40041 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        LPCArray mapPos = data.GetValue<LPCArray>("map_pos");

        int mapId = data.GetValue<int>("map_id");

        List<object> list = new List<object>();
        list.Add(cb);
        list.Add(mapPos);
        list.Add(mapId);

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, list));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        List<object> list = para  as List<object>;

        //获取副本选择界面;
        GameObject wnd = WindowMgr.OpenWnd(SelectInstanceWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        SelectInstanceWnd selectInstanceScript = wnd.GetComponent<SelectInstanceWnd>();

        if (selectInstanceScript == null)
            return;

        LPCArray pos = list[1] as LPCArray;
        if (pos == null)
            return;

        // 绑定数据
        selectInstanceScript.Bind((int)list[2], new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat));

        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);
        if (mainWnd != null)
        {
            WindowMgr.HideWindow(mainWnd);

            mainWnd.GetComponent<MainWnd>().mTowerDiffSelectWnd.SetActive(false);
        }

        // 指引对象不存在
        if (GuideMgr.GuideOb == null)
            return;

        int group = GuideMgr.GuideOb.Group;
        if (GuideMgr.StepUnlock(group, GuideMgr.GuideOb.CurStep))
        {
            int step = ME.user.Query<LPCMapping>("guide").GetValue<int>(group) + 1;

            GuideMgr.GuideOb = null;

            // 继续指引
            GuideMgr.DoGuide(group, step);
        }
        else
        {
            CallBack cb = list[0] as CallBack;
            if (cb != null)
                cb.Go();
        }
    }
}

/// <summary>
/// 指引执行脚本
/// </summary>
public class SCRIPT_40042 : Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        // 获取主城界面
        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);
        if (mainWnd == null)
            return false;

        // 玩家的召唤次数
        int summonTimes = ME.user.Query<int>("sum_times");
        if (summonTimes >= 5)
        {
            // 执行回调
            if (cb != null)
                cb.Go();

            cb = null;

            return true;
        }

        GameObject summonWnd = WindowMgr.GetWindow(SummonWnd.WndType);
        if (summonWnd == null)
            return false;

        summonWnd.GetComponent<SummonWnd>().BindGuideCallBack(null, null, new CallBack(OnClickCallBack));

        EventMgr.RegisterEvent("SCRIPT_40042", EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, OnOperate);

        return true;
    }

    /// <summary>
    /// 点击回调
    /// </summary>
    void OnOperate(int eventId, MixedValue para)
    {
        EventMgr.UnregisterEvent("SCRIPT_40042");

        // 指引对象不存在
        if (GuideMgr.GuideOb == null)
            return;

        // 返回上一步指引
        GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 1);
    }

    /// <summary>
    /// 确认按钮点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        // 玩家的召唤次数
        int summonTimes = ME.user.Query<int>("sum_times");
        if (summonTimes >= 5)
        {
            // 执行回调
            if (cb != null)
                cb.Go();

            cb = null;

            // 卸载就模型
            SummonMgr.UnLoadModel();

            EventMgr.UnregisterEvent("SCRIPT_40042");
        }
    }
}

/// <summary>
/// 召唤结束指引返回主城
/// </summary>
public class SCRIPT_40043 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return false;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnSummonMaskCallBack, cb));

        // 销毁本窗口
        WindowMgr.DestroyWindow(SummonWnd.WndType);

        // 卸载就模型
        SummonMgr.UnLoadModel();

        return true;
    }

    void OnSummonMaskCallBack(object para, object[] param)
    {
        CallBack cb = para as CallBack;

        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OnEnterMainCityScene, cb));
    }

    /// <summary>
    /// 打开主城回调
    /// </summary>
    private void OnEnterMainCityScene(object para, object[] param)
    {
        // 打开主窗口
        WindowMgr.OpenMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        // 当前阶段完成，执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 打开使魔强化界面
/// </summary>
public class SCRIPT_40044 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        // 打开使魔强化界面
        WindowMgr.HideMainWnd();

        WindowMgr.OpenWnd("PetStrengthenWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 执行回调，结束当前分组指引
        if (cb != null)
            cb.Go();

        return true;
    }
}

/// <summary>
/// 调整主城按钮位置
/// </summary>
public class SCRIPT_40045 : Script
{
    public override object Call(params object[] _params)
    {
        return true;
    }
}

/// <summary>
/// 选择需要强化的使魔
/// </summary>
public class SCRIPT_40046 : Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        wnd.GetComponent<GuideWnd>().BindMultipleChoice(desc, data,
            new CallBack[]{new CallBack(OnClick1CallBack),
            new CallBack(OnClick2CallBack),
            new CallBack(OnClick3CallBack)}
        );

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 窗口点击回调
    /// </summary>
    void OnClick1CallBack(object para, params object[] param)
    {
        // 选择第一个宠物
        DoCallBack("petStrengthen_item_0_0");
    }

    void OnClick2CallBack(object para, params object[] param)
    {
        DoCallBack("petStrengthen_item_0_1");
    }

    void OnClick3CallBack(object para, params object[] param)
    {
        DoCallBack("petStrengthen_item_0_2");
    }

    // 执行回调,选择需要强化的使魔
    void DoCallBack(string itemName)
    {
        // 宠物强化界面
        GameObject wnd = WindowMgr.GetWindow(PetStrengthenWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<PetStrengthenWnd>().GuideSelectStrengthenPet(itemName);

        if (cb != null)
            cb.Go();
    }
}

// 指引执行脚本，选择材料使魔
public class SCRIPT_40047 : Script
{
    CallBack cb = null;

    string itemName = string.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        itemName = data.GetValue<string>("item_name");

        // 执行回调
        cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);

        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        // 宠物强化界面
        GameObject wnd = WindowMgr.GetWindow(PetStrengthenWnd.WndType);
        if (wnd == null)
            return;

        // 选择材料使魔
        wnd.GetComponent<PetStrengthenWnd>().GuideSelectStrengthenPet(itemName);

        if (cb != null)
            cb.Go();
    }
}

// 指引执行脚本，点击强化使魔按钮
public class SCRIPT_40048 : Script
{
    CallBack cb = null;

    string wndName = string.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        cb = _params[1] as CallBack;
        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        EventMgr.RegisterEvent("EVENT_PET_UPGRADE", EventMgrEventType.EVENT_PET_UPGRADE, OnPetUpgrade);

        EventMgr.RegisterEvent("EVENT_STRENGTHEN_PET_DIALOG_CLICK", EventMgrEventType.EVENT_STRENGTHEN_PET_DIALOG_CLICK, OnDialogClickEvent);

        return true;
    }

    void OnDialogClickEvent(int eventId, MixedValue para)
    {
        bool isOk = para.GetValue<bool>();

        EventMgr.UnregisterEvent("EVENT_STRENGTHEN_PET_DIALOG_CLICK");

        // 返回上一步指引
        if (!isOk && GuideMgr.GuideOb != null)
        {
            GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 1);
        }
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        // 宠物强化界面
        GameObject wnd = WindowMgr.GetWindow(PetStrengthenWnd.WndType);
        if (wnd == null)
            return;

        // 选择材料使魔
        wnd.GetComponent<PetStrengthenWnd>().GuideOnClickUpgradeBtn();

        // 关闭当前指引窗口
        WindowMgr.DestroyWindow(wndName);
    }

    void OnPetUpgrade(int eventId, MixedValue para)
    {
        // 解注册事件
        EventMgr.UnregisterEvent("EVENT_PET_UPGRADE");

        // 当前阶段执行完成
        if (cb != null)
            cb.Go();
    }
}

// 执行脚本，关闭使魔强化界面，返回主城
public class SCRIPT_40049 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        // 关闭使魔研究所
        WindowMgr.HideWindow(PetStrengthenWnd.WndType);

        // 打开主城界面
        GameObject wnd = WindowMgr.OpenMainWnd();
        if (wnd == null)
            wnd.GetComponent<MainWnd>().ShowMainUIBtn(true);

        // 当前阶段完成执行回调
        if (cb != null)
            cb.Go();

        return true;
    }
}

public class SCRIPT_40050 : Script
{
    CallBack cb = null;

    string wndName = string.Empty;

    public override object Call(params object[] _params)
    {
        // 执行回调
        cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        wndName = data.GetValue<string>("wnd_name");

        GameObject petStrengthen = WindowMgr.GetWindow(PetStrengthenWnd.WndType);
        if (petStrengthen == null)
        {
            // 打开强化界面
            WindowMgr.OpenWnd(PetStrengthenWnd.WndType);

            WindowMgr.HideMainWnd();

            // 打开指引窗口
            WindowMgr.OpenWnd(wndName);

            // 当前阶段执行完成
            if (cb != null)
                cb.Go();

            return true;
        }

        // 注册事件
        EventMgr.RegisterEvent("SCRIPT_40050", EventMgrEventType.EVENT_UPGREDE_ANIMATION_FINISH, OnAnimationFinish);

        return true;
    }

    /// <summary>
    /// 动画播放完成事件回调
    /// </summary>
    void OnAnimationFinish(int eventId, MixedValue para)
    {
        // 解注册事件
        EventMgr.UnregisterEvent("SCRIPT_40050");

        // 打开指引窗口
        WindowMgr.OpenWnd(wndName);

        // 当前阶段执行完成
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 调整主城按钮位置
/// </summary>
public class SCRIPT_40051 : Script
{
    CallBack cb = null;
    public override object Call(params object[] _params)
    {
        // 执行回调
        cb = _params[1] as CallBack;

        if (InstanceMgr.IsInInstance(ME.user))
        {
            EventMgr.RegisterEvent("SCRIPT_40051", EventMgrEventType.EVENT_SETTLEMENT_BONUS_SHOW_FINISH, OnFinish);
        }
        else
        {
            if (cb != null)
                cb.Go();

            cb = null;
        }

        return true;
    }

    void OnFinish(int eventId, MixedValue para)
    {
        if (cb != null)
            cb.Go();

        cb = null;

        EventMgr.UnregisterEvent("SCRIPT_40051");
    }
}

// 使用自动战斗提示
public class SCRIPT_40052 : Script
{
    string wndName = string.Empty;

    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        MixedValue para = _params[5] as MixedValue;

        // 副本对象
        Instance instance = para.GetValue<Instance>();
        if (instance == null)
            return false;

        wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        cb = _params[1] as CallBack;

        // 暂停回合制战斗
        RoundCombatMgr.PauseRoundCombat();

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack));

        return true;
    }

    void OnClickCallBack(object para, params object[] param)
    {
        // 关闭指引窗口
        WindowMgr.DestroyWindow(wndName);

        if (cb != null)
            cb.Go();

        // 恢复回合制战斗
        RoundCombatMgr.ContinueRoundCombat();

        cb = null;
    }
}

// 执行指引，继续下一关
public class SCRIPT_40053 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        CallBack cb = _params[1] as CallBack;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (InstanceMgr.IsInInstance(ME.user))
        {
            GameObject wnd = WindowMgr.OpenWnd(wndName);
            if (wnd == null)
                return false;

            wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, cb));
        }
        else
        {
            if (cb != null)
                cb.Go();
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    void OnClickCallBack(object para, params object[] param)
    {
        // 战斗结算界面
        GameObject wnd = WindowMgr.GetWindow(FightSettlementWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<FightSettlementWnd>().GuideOnClickNextBtn();

        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

// 
public class SCRIPT_40054 : Script
{
    public override object Call(params object[] _params)
    {

        return true;
    }
}

// 战斗结束后指引
public class SCRIPT_40055 : Script
{
    CallBack cb = null;

    LPCMapping mData = null;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        // 执行回调
        cb = _params[1] as CallBack;

        string instanceId = mData.GetValue<string>("instance_id");

        // 在副本中
        if (InstanceMgr.IsInInstance(ME.user))
        {
            EventMgr.RegisterEvent("SCRIPT_40055", EventMgrEventType.EVENT_SETTLEMENT_BONUS_SHOW_FINISH, OnInstanceClearance);
        }
        else
        {
            if (InstanceMgr.IsClearanced(ME.user, instanceId))
            {
                if (cb != null)
                    cb.Go();

                cb = null;
            }
            else
            {
                EventMgr.RegisterEvent("SCRIPT_40055", EventMgrEventType.EVENT_SETTLEMENT_BONUS_SHOW_FINISH, OnInstanceClearance);
            }
        }

        return true;
    }

    void OnInstanceClearance(int eventId, MixedValue para)
    {
        LPCMapping instance = ME.user.Query<LPCMapping>("instance");
        if (instance == null)
            return;

        if (mData.GetValue<string>("instance_id") != instance.GetValue<string>("id"))
            return;

        EventMgr.UnregisterEvent("SCRIPT_40055");

        if (cb != null)
            cb.Go();

        cb = null;
    }
}

// 打开防御部署界面
public class SCRIPT_40056 : Script
{
    public override object Call(params object[] _params)
    {
        GameObject wnd = WindowMgr.OpenWnd(ArenaWnd.WndType);
        if (wnd == null)
            return false;

        // 指引打开防御部署界面
        wnd.GetComponent<ArenaWnd>().GuideOpenDefenceWnd();

        // 执行回调
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 选择防御部署宠物，第四只
public class SCRIPT_40057 : Script
{
    public override object Call(params object[] _params)
    {
        GameObject wnd = WindowMgr.GetWindow(ArenaWnd.WndType);
        if (wnd == null)
            return false;

        // 指引打开防御部署界面
        wnd.GetComponent<ArenaWnd>().GuideSelectDefencePet("DefenceDeployWnd_pet_item_3");

        // 执行回调
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 执行确认部署
public class SCRIPT_40058 : Script
{
    public override object Call(params object[] _params)
    {
        GameObject wnd = WindowMgr.GetWindow(ArenaWnd.WndType);
        if (wnd == null)
            return false;

        wnd.GetComponent<ArenaWnd>().GuideConfirmDeploy();

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 监听字段的变化
        ME.user.dbase.RemoveTriggerField("SCRIPT_40058");
        ME.user.dbase.RegisterTriggerField("SCRIPT_40058",
            new string[]
            {
                "defense_troop"
            }, new CallBack(OnChangeDefenceTroop, cb));

        return true;
    }

    /// <summary>
    /// 防御阵容变化回调
    /// </summary>
    void OnChangeDefenceTroop(object para, params object[] param)
    {
        // 执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();

        // 移除字段监听变化
        ME.user.dbase.RemoveTriggerField("SCRIPT_40058");
    }
}

// 指引执行脚本, 点击练习对战
public class SCRIPT_40059 : Script
{
    public override object Call(params object[] _params)
    {
        GameObject wnd = WindowMgr.GetWindow(ArenaWnd.WndType);
        if (wnd == null)
            wnd = WindowMgr.OpenWnd(ArenaWnd.WndType);

        if (wnd == null)
            return false;

        // 指引打开防御部署界面
        wnd.GetComponent<ArenaWnd>().GuideClickPracticeBattle();

        // 执行回调
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 执行脚本，选择npc对战
public class SCRIPT_40060 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string instanceId = data.GetValue<string>("instance_id");

        int classId = data.GetValue<int>("class_id");

        // 获取副本配置信息
        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return false;

        List<Property> mPetData = UserMgr.GetUserPets(ME.user, InstanceMgr.IsSelectStorePet(instanceId), false, instanceInfo.GetValue<int>("limit_level"));

        // 对宠物按指定方式进行排序
        mPetData = BaggageMgr.SortPetInBag(mPetData, MonsterConst.SORT_BY_STAR);

        List<Property> mCacheList = new List<Property>();

        Property ob = null;

        for (int i = 0; i < mPetData.Count; i++)
        {
            if (mCacheList.Count >= 5)
                break;

            if (mPetData[i].GetClassID() == classId)
            {
                ob = mPetData[i];

                continue;
            }

            mCacheList.Add(mPetData[i]);
        }

        mCacheList.Insert(0, ob);

        // 设置阵容缓存信息
        FormationMgr.SetArchiveFormation(instanceId, mCacheList);

        GameObject wnd = WindowMgr.GetWindow(ArenaWnd.WndType);
        if (wnd == null)
            return false;

        wnd.GetComponent<ArenaWnd>().GuideSelectNpcBattle(0);

        // 执行回调
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 指引玩家进入战斗
public class SCRIPT_40061 : Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        GameObject wnd = WindowMgr.GetWindow(SelectFighterWnd.WndType);
        if (wnd == null)
            return false;

        WindowMgr.HideMainWnd();

        wnd.GetComponent<SelectFighterWnd>().GuideClickEnterBattle();

        cb = _params[1] as CallBack;

        EventMgr.RegisterEvent("SCRIPT_40061", EventMgrEventType.EVENT_ENTER_INSTANCE_OK, OnMsgEnterInstance);

        return true;
    }

    /// <summary>
    /// 进入副本成功消息回调
    /// </summary>
    void OnMsgEnterInstance(int eventId, MixedValue par)
    {
        EventMgr.UnregisterEvent("SCRIPT_40061");

        Coroutine.DispatchService(WaitInvoke(), "WaitInvoke");
    }

    IEnumerator WaitInvoke()
    {
        yield return null;

        if (cb != null)
            cb.Go();

        cb = null;

        Coroutine.StopCoroutine("WaitInvoke");
    }
}

// 监听副本通关事件
public class SCRIPT_40062 : Script
{
    CallBack cb = null;

    int result = 0;

    LPCMapping mData = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        cb = _params[1] as CallBack;

        EventMgr.RegisterEvent("SCRIPT_40062_EVENT_INSTANCE_CLEARANCE", EventMgrEventType.EVENT_INSTANCE_CLEARANCE, OnEventCallBack);

        EventMgr.RegisterEvent("SCRIPT_40062_EVENT_GUIDE_RETUEN_OPERATE", EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, OnRetrunGuideEventCallBack);

        return true;
    }

    void OnEventCallBack(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null)
            return;

        result = data.GetValue<int>("result");

        if (result != 1)
            return;

        if (cb != null)
        {
            cb.Go();

            cb = null;
        }

        EventMgr.UnregisterEvent("SCRIPT_40062_EVENT_GUIDE_RETUEN_OPERATE");
        EventMgr.UnregisterEvent("SCRIPT_40062_EVENT_INSTANCE_CLEARANCE");
    }

    void OnRetrunGuideEventCallBack(int eventId, MixedValue para)
    {
        if (result != 1)
        {
            EventMgr.UnregisterEvent("SCRIPT_40062_EVENT_INSTANCE_CLEARANCE");
            EventMgr.UnregisterEvent("SCRIPT_40062_EVENT_GUIDE_RETUEN_OPERATE");

            // 指引对象不存在
            if (GuideMgr.GuideOb == null)
                return;

            if (para.GetValue<int>() == GuideConst.AGAIN_ARENA)
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 3);
            else
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 6);
        }
    }
}

// 第七关第三波开始
public class SCRIPT_40063 : Script
{
    public override object Call(params object[] _params)
    {
        // 暂停战斗
        RoundCombatMgr.PauseRoundCombat();

        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 竞技场返回主城
public class SCRIPT_40064 : Script
{
    public override object Call(params object[] _params)
    {
        // 关闭竞技场界面
        WindowMgr.DestroyWindow(ArenaWnd.WndType);

        // 打开主城界面
        GameObject wnd = WindowMgr.OpenMainWnd();
        if (wnd == null)
            return false;

        wnd.GetComponent<MainWnd>().ShowMainUIBtn(true);

        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 恢复回合制战斗
public class SCRIPT_40065 : Script
{
    public override object Call(params object[] _params)
    {
        // 恢复回合制战斗
        RoundCombatMgr.ContinueRoundCombat();

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        WindowMgr.DestroyWindow(data.GetValue<string>("wnd_name"));

        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 副本通关失败指引
public class SCRIPT_40066 : Script
{
    CallBack cb= null;
    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        if (InstanceMgr.IsInInstance(ME.user))
        {
            EventMgr.RegisterEvent("SCRIPT_40066", EventMgrEventType.EVENT_FAILED_ANIMATION_FINISH, OnFinish);
        }
        else
        {
            if (cb != null)
                cb.Go();

            cb = null;
        }

        return true;
    }

    void OnFinish(int evetnId, MixedValue para)
    {
        EventMgr.UnregisterEvent("SCRIPT_40066");

        if (cb != null)
            cb.Go();

        cb = null;
    }
}

// 首次通关失败返回主城，打开邮件窗口
public class SCRIPT_40067 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        if (InstanceMgr.IsInInstance(ME.user))
        {
            // 打开主场景
            SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY,
                new CallBack(OpenWorldMapWnd, cb));
        }
        else
        {
            // 执行回调
            if (cb != null)
                cb.Go();
        }

        return true;
    }

    /// <summary>
    /// 打开主场景完成回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    private void OpenWorldMapWnd(object para, object[] param)
    {
        //离开副本;
        InstanceMgr.LeaveInstance(ME.user);

        // 关闭结算界面
        WindowMgr.DestroyWindow(FightSettlementWnd.WndType);

        WindowMgr.DestroyWindow(ArenaFightSettlementWnd.WndType);

        // 打开主界面
        GameObject mainWnd = WindowMgr.OpenWnd ("MainWnd");
        if (mainWnd == null)
            return;

        mainWnd.GetComponent<MainWnd>().ShowMainUIBtn(true);

        // 打开邮件窗口
        WindowMgr.OpenWnd("MailWnd");

        // 执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

// 
public class SCRIPT_40068 : Script
{
    public override object Call(params object[] _params)
    {
        return true;
    }
}

// 指引脚本，打开世界地图
public class SCRIPT_40069 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        // 打开主场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP,
            new CallBack(OpenWorldMapWnd, cb));

        return true;
    }

    /// <summary>
    /// 打开主场景完成回调
    /// </summary>
    private void OpenWorldMapWnd(object para, object[] param)
    {
        //离开副本;
        InstanceMgr.LeaveInstance(ME.user);

        // 关闭结算界面
        WindowMgr.DestroyWindow(FightSettlementWnd.WndType);

        // 打开主界面
        GameObject mainWnd = WindowMgr.OpenWnd ("MainWnd");
        if (mainWnd == null)
            return;

        mainWnd.GetComponent<MainWnd>().ShowMainUIBtn(false);

        // 执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

// 指引执行脚本, 打开北镜圣域
public class SCRIPT_40070 : Script
{
    string wndName = string.Empty;

    LPCMapping mData = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        wndName = mData.GetValue<string>("wnd_name");

        string desc = mData.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        WindowMgr.HideWindow(SelectInstanceWnd.WndType);

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, mData, new CallBack(OnClickCallBack, cb));

        // 销毁指定窗口
        if (mData.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(mData.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (mData.ContainsKey("guide_pos"))
        {
            LPCArray pos = mData.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    void OnClickCallBack(object para, object[] param)
    {
        LPCArray pos = mData.GetValue<LPCArray>("map_pos");

        // 相机移动的目标位置
        Vector3 targetPos = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

        // 创建地下城窗口
        GameObject wnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<DungeonsWnd>().Bind(string.Empty, 0, targetPos);

        // 隐藏主界面
        WindowMgr.HideMainWnd();

        // 执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

// 打开通天塔难度选择界面
public class SCRIPT_40072 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        // 打开主场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP,
            new CallBack(OpenWorldMapWnd, cb));

        return true;
    }

    void OpenWorldMapWnd(object para, object[] param)
    {
        WindowMgr.DestroyWindow(TaskWnd.WndType);

        // 打开主界面
        GameObject mainWnd = WindowMgr.OpenWnd ("MainWnd");
        if (mainWnd == null)
            return;

        mainWnd.GetComponent<MainWnd>().ShowMainUIBtn(false);

        mainWnd.GetComponent<MainWnd>().GuideClickTower();

        // 执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

public class SCRIPT_40073 : Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        EventMgr.RegisterEvent("SCRIPT_40073_EVENT_OPEN_COMBATWND", EventMgrEventType.EVENT_OPEN_COMBATWND, OnOpenCombat);

        EventMgr.RegisterEvent("SCRIPT_40073_EVENT_GUIDE_RETUEN_OPERATE", EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, OnReturnGuide);

        return true;
    }

    void OnOpenCombat(int eventId, MixedValue para)
    {
        EventMgr.UnregisterEvent("SCRIPT_40073_EVENT_OPEN_COMBATWND");
        EventMgr.UnregisterEvent("SCRIPT_40073_EVENT_GUIDE_RETUEN_OPERATE");

        if (cb != null)
            cb.Go();

        cb = null;
    }

    void OnReturnGuide(int eventId, MixedValue para)
    {
        EventMgr.UnregisterEvent("SCRIPT_40073_EVENT_OPEN_COMBATWND");
        EventMgr.UnregisterEvent("SCRIPT_40073_EVENT_GUIDE_RETUEN_OPERATE");

        if (GuideMgr.GuideOb != null)
            GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 2);

        cb = null;
    }
}

public class SCRIPT_40074 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        CallBack cb = _params[1] as CallBack;

        wnd.GetComponent<GuideElementRelationWnd>().Bind(new CallBack(OnClickCallBack, cb));

        return true;
    }

    void OnClickCallBack(object para, params object[] param)
    {
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

// 指引执行脚本
public class SCRIPT_40075 : Script
{
    CallBack cb = null;
    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        if (InstanceMgr.IsInInstance(ME.user))
        {
            EventMgr.RegisterEvent("SCRIPT_40075_EVENT_SETTLEMENT_BONUS_SHOW_FINISH", EventMgrEventType.EVENT_SETTLEMENT_BONUS_SHOW_FINISH, OnSettlementBonusFinish);
        }
        else
        {
            if (cb != null)
                cb.Go();
        }

        return true;
    }

    /// <summary>
    /// 副本通关回调
    /// </summary>
    private void OnSettlementBonusFinish(int eventId, MixedValue para)
    {
        // 执行回调继续指引
        if (cb != null)
            cb.Go();

        // 解注册
        EventMgr.UnregisterEvent("SCRIPT_40075_EVENT_SETTLEMENT_BONUS_SHOW_FINISH");

        cb = null;
    }
}

public class SCRIPT_40076 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        FormationMgr.SetArchiveFormation(data.GetValue<string>("instance_id"), new List<Property>());

        if (cb != null)
            cb.Go();

        return true;
    }
}

// 指引执行脚本
public class SCRIPT_40077 : Script
{
    CallBack cb = null;

    LPCMapping mData = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        // 不在副本中直接执行下一步
        if (!InstanceMgr.IsInInstance(ME.user))
        {
            if (cb != null)
                cb.Go();

            cb = null;
        }

        EventMgr.RegisterEvent("SCRIPT_40077_EVENT_SETTLEMENT_BONUS_SHOW_FINISH", EventMgrEventType.EVENT_SETTLEMENT_BONUS_SHOW_FINISH, OnSettlementFinish);

        EventMgr.RegisterEvent("SCRIPT_40077_EVENT_SHOW_EQUIP", EventMgrEventType.EVENT_SHOW_EQUIP, OnShowEquipCallBack);

        return true;
    }

    void OnShowEquipCallBack(int eventId, MixedValue para)
    {
        EventMgr.UnregisterEvent("SCRIPT_40077_EVENT_SHOW_EQUIP");

        GameObject wnd = WindowMgr.OpenWnd(mData.GetValue<string>("wnd_name"));
        if (wnd == null)
            return;

        wnd.GetComponent<GuideWnd>().Bind(mData.GetValue<string>("guide_desc"), mData, new CallBack(OnClickCallBack));

        if (mData.ContainsKey("guide_pos"))
        {
            LPCArray pos = mData.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }
    }

    void OnClickCallBack(object para, params object[] param)
    {
        GameObject wnd = WindowMgr.GetWindow(RewardItemInfoWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<RewardItemInfoWnd>().GuideCloseWnd();
    }

    /// <summary>
    /// 副本奖励显示完成回调
    /// </summary>
    private void OnSettlementFinish(int eventId, MixedValue para)
    {
        // 解注册
        EventMgr.UnregisterEvent("SCRIPT_40077_EVENT_SETTLEMENT_BONUS_SHOW_FINISH");

        // 当前阶段完成执行回调
        if (cb != null)
            cb.Go();

        cb = null;
    }
}

// 指引执行脚本
public class SCRIPT_40078 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        GameObject summonWnd = WindowMgr.GetWindow(SummonWnd.WndType);
        if (summonWnd == null && GuideMgr.GuideOb != null)
        {
            int group = GuideMgr.GuideOb.Group;

            int step = GuideMgr.GuideOb.CurStep;

            GuideMgr.ResetGuide();

            GuideMgr.DoGuide(group, step + 3);

            return false;
        }

        summonWnd.GetComponent<SummonWnd>().BindGuideCallBack(
                null,
            new CallBack(OnSummonFinishCallBack, cb), null);

        return true;
    }

    /// <summary>
    /// 召唤完成回调
    /// </summary>
    void OnSummonFinishCallBack(object para, object[] param)
    {
        CallBack cb = para as CallBack;

        // 当前阶段完成执行回调
        if (cb != null)
            cb.Go();
    }
}

public class SCRIPT_40079 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        GameObject summonWnd = WindowMgr.GetWindow(SummonWnd.WndType);
        if (summonWnd == null)
        {
            if (cb != null)
                cb.Go();

            return false;
        }

        summonWnd.GetComponent<SummonWnd>().BindGuideCallBack(
            null,
            null, new CallBack(OnClickCallBack, cb));

        return true;
    }

    /// <summary>
    /// 召唤点击回调
    /// </summary>
    void OnClickCallBack(object para, object[] param)
    {
        // 卸载就模型
        SummonMgr.UnLoadModel();

        CallBack cb = para as CallBack;

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnSummonMaskCallBack, cb));

        // 销毁本窗口
        WindowMgr.DestroyWindow(SummonWnd.WndType);
    }

    void OnSummonMaskCallBack(object para, object[] param)
    {
        // 当前阶段完成，执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();

        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OnEnterMainCityScene));
    }

    /// <summary>
    /// 打开主城回调
    /// </summary>
    private void OnEnterMainCityScene(object para, object[] param)
    {
        // 打开主窗口
        WindowMgr.OpenMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();
    }
}

/// <summary>
/// 打开竞技场界面
/// </summary>
public class SCRIPT_40080 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        WindowMgr.OpenWnd(ArenaWnd.WndType);

        WindowMgr.HideMainWnd();

        if (cb != null)
            cb.Go();

        return true;
    }
}

/// <summary>
/// 选择使魔
/// </summary>
public class SCRIPT_40081 : Script
{
    public override object Call(params object[] _params)
    {
        // 选择装备分页
        GameObject baggageWnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (baggageWnd == null)
            return false;

        GameObject showPetWnd = baggageWnd.GetComponent<BaggageWnd>().mShowPetWnd;
        if (showPetWnd == null)
            return false;

        showPetWnd.GetComponent<ShowPetsWnd>().GuideSelectPet("baggage_pet_item_0_1");

        // 当前阶段完成，执行回调继续下一步指引
        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 选择防御部署宠物，第四只
public class SCRIPT_40082 : Script
{
    public override object Call(params object[] _params)
    {
        // 执行回调
        CallBack cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;

        GameObject wnd = WindowMgr.GetWindow(ArenaWnd.WndType);
        if (wnd != null)
        {
            if (cb != null)
                cb.Go();

            if (data.ContainsKey("destroy_wnd"))
                Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

            return true;
        }

        string wndName = data.GetValue<string>("wnd_name");

        // 打开某个窗口
        WindowMgr.OpenWnd(wndName);

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        wnd = WindowMgr.OpenWnd(ArenaWnd.WndType);
        if (wnd == null)
            return false;

        // 指引打开防御部署界面
        wnd.GetComponent<ArenaWnd>().GuideSelectDefencePet("DefenceDeployWnd_pet_item_3");

        if (cb != null)
            cb.Go();

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }
}

// 暂停回合制战斗
public class SCRIPT_40083 : Script
{
    public override object Call(params object[] _params)
    {
        // 暂停回合制战斗
        RoundCombatMgr.PauseRoundCombat();

        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 恢复回合制战斗
public class SCRIPT_40084 : Script
{
    public override object Call(params object[] _params)
    {
        // 恢复回合制战斗
        RoundCombatMgr.ContinueRoundCombat();

        CallBack cb = _params[1] as CallBack;
        if (cb != null)
            cb.Go();

        return true;
    }
}

// 显示弹框，返回世界地图
public class SCRIPT_40085 : Script
{
    CallBack cb = null;

    LPCMapping data = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        int mapId = data.GetValue<int>("map_id");

        CsvRow row = MapMgr.GetMapConfig(mapId);
        if (row == null)
            return false;

        // 提示弹框
        DialogMgr.ShowSingleBtnDailog(new CallBack(DialogCallBack),
            string.Format(LocalizationMgr.Get("GuideDialog_1"), LocalizationMgr.Get(row.Query<string>("name"))),
            LocalizationMgr.Get("GuideDialog_0"));

        return true;
    }

    /// <summary>
    /// 弹框回调
    /// </summary>
    void DialogCallBack(object para, params object[] param)
    {
        // 返回地图界面
        // 打开世界地图场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP,
            new CallBack(OnLoadSceneAfter));
    }

    /// <summary>
    /// 打开战斗场景回调
    /// </summary>
    private void OnLoadSceneAfter(object para, object[] param)
    {
        //离开副本;
        InstanceMgr.LeaveInstance(ME.user);

        // 销毁自己
        WindowMgr.DestroyWindow(FightSettlementWnd.WndType);
        WindowMgr.DestroyWindow(ArenaFightSettlementWnd.WndType);

        // 显示主界面;
        GameObject wnd = WindowMgr.OpenMainWnd();
        if (wnd == null)
            return;

        // 设置为世界地图显示方式
        wnd.GetComponent<MainWnd>().ShowMainUIBtn(false);

        LPCArray posArray = data.GetValue<LPCArray>("map_pos");

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control != null)
            control.MoveCamera(SceneMgr.SceneCamera.transform.position, new Vector3(posArray[0].AsFloat, posArray[1].AsFloat, posArray[2].AsFloat));

        // 执行回调
        if (cb != null)
            cb.Go();

        cb = null;
    }
}

/// <summary>
/// 选择使魔
/// </summary>
public class SCRIPT_40086 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        CallBack cb = _params[1] as CallBack;

        string wndName = data.GetValue<string>("wnd_name");

        string npcName = data.GetValue<string>("name");

        LPCValue content = data.GetValue<LPCValue>("content");

        // 文字显示效果类型
        int showEffect = data.GetValue<int>("is_show_effect");

        int charsPerSecond = data.GetValue<int>("chars_per_second");

        int fontSize = data.GetValue<int>("font_size");

        // 打开对话框窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        // 绑定数据
        wnd.GetComponent<TalkBoxWnd>().Bind(npcName, content, showEffect == 1 , charsPerSecond, fontSize, cb);

        if (WindowMgr.GetWindow(BaggageWnd.WndType) == null)
        {
            WindowMgr.OpenWnd(data.GetValue<string>("guide_bg"));

            // 选择装备分页
            GameObject baggageWnd = WindowMgr.OpenWnd(BaggageWnd.WndType);
            if (baggageWnd == null)
                return false;

            GameObject petToolTip = baggageWnd.GetComponent<BaggageWnd>().mPetToolTipWnd;
            if (petToolTip == null)
                return false;

            petToolTip.GetComponent<PetToolTipWnd>().OnTabBtnClicked((int) BAGGAGE_PAGE.EQUIP_PAGE);

            Coroutine.DispatchService(SelectPet());
        }

        return true;
    }

    IEnumerator SelectPet()
    {
        yield return null;

        GameObject baggageWnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (baggageWnd == null)
            yield break;

        GameObject showPetWnd = baggageWnd.GetComponent<BaggageWnd>().mShowPetWnd;
        if (showPetWnd == null)
            yield break;

        showPetWnd.GetComponent<ShowPetsWnd>().GuideSelectPet("baggage_pet_item_0_1");
    }
}

// 竞技场指引脚本
public class SCRIPT_40087 : Script
{
    public override object Call(params object[] _params)
    {
        // 执行回调
        CallBack cb = _params[1] as CallBack;

        GameObject wnd = WindowMgr.GetWindow(ArenaWnd.WndType);
        if (wnd != null)
        {
            if (cb != null)
                cb.Go();

            return true;
        }

        wnd = WindowMgr.OpenWnd(ArenaWnd.WndType);
        if (wnd == null)
            return false;

        if (cb != null)
            cb.Go();

        return true;
    }
}

// 点击选择魔力圣域
public class SCRIPT_40088 : Script
{
    LPCMapping mData = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        // 执行回调
        CallBack cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        // 获取圣域界面
        GameObject wnd = WindowMgr.GetWindow(DungeonsWnd.WndType);
        if (wnd == null)
            return false;

        LPCArray mapPos = mData.GetValue<LPCArray>("map_pos");

        // 绑定数据
        wnd.GetComponent<DungeonsWnd>().Bind(
            mData.GetValue<string>("instance_id"),
            mData.GetValue<int>("map_id"),
            new Vector3(mapPos[0].AsFloat, mapPos[1].AsFloat, mapPos[2].AsFloat));

        // 销毁指定窗口
        if (mData.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(mData.GetValue<string>("destroy_wnd")), "DoDestroy");

        // 执行回调
        if (cb != null)
            cb.Go();

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }
}

// 魔力圣域指引脚本
public class SCRIPT_40089 : Script
{
    LPCMapping mData = LPCMapping.Empty;

    CallBack mCb = null;

    public override object Call(params object[] _params)
    {
        // 回调
        mCb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        // 销毁指定窗口
        if (mData.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(mData.GetValue<string>("destroy_wnd")), "DoDestroy");

        EventMgr.RegisterEvent("SCRIPT_40089", EventMgrEventType.EVENT_INSTANCE_CLEARANCE, OnInstanceClearance);

        // 注册指引返回事件
        EventMgr.RegisterEvent("SCRIPT_40089_EVENT_GUIDE_RETUEN_OPERATE", EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, OnGuideReturnOperate);

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 副本通关事件回调
    /// </summary>
    void OnInstanceClearance(int eventID, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();

        if (data == null)
            return;

        if (data.GetValue<int>("result") != 1)
            return;

        EventMgr.UnregisterEvent("SCRIPT_40089");
        EventMgr.UnregisterEvent("SCRIPT_40089_EVENT_GUIDE_RETUEN_OPERATE");

        if (mCb != null)
            mCb.Go();
    }

    /// <summary>
    /// 指引返回事件回调
    /// </summary>
    void OnGuideReturnOperate(int eventId, MixedValue para)
    {
        int type = para.GetValue<int>();

        // 返回副本选择指引操作
        if (type == GuideConst.RETURN_SELECT_INSTANCE)
        {
            // 解注册事件
            EventMgr.UnregisterEvent("SCRIPT_40089_EVENT_GUIDE_RETUEN_OPERATE");

            // 返回圣域窗口指引
            if (GuideMgr.GuideOb != null)
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 1);
        }
        else if (type == GuideConst.RETURN_RISK)
        {
            // 解注册事件
            EventMgr.UnregisterEvent("SCRIPT_40089_EVENT_GUIDE_RETUEN_OPERATE");

            // 返回世界地图指引
            if (GuideMgr.GuideOb != null)
                GuideMgr.ReturnGuide(GuideMgr.GuideOb.Group, GuideMgr.GuideOb.CurStep - 2);
        }
    }
}

// 打开包裹觉醒界面
public class SCRIPT_40090 : Script
{
    LPCMapping mData = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        string wndName = mData.GetValue<string>("wnd_name");

        string desc = mData.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, mData, new CallBack(OnClickWnd, cb));

        // 打开包裹窗口
        WindowMgr.OpenWnd(BaggageWnd.WndType);

        if (mData.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(mData.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (mData.ContainsKey("guide_pos"))
        {
            LPCArray pos = mData.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        GameObject wnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (wnd == null)
            return;

        // 脚本对象获取失败
        BaggageWnd bw = wnd.GetComponent<BaggageWnd>();
        if (bw == null)
            return;

        // 选择指定宠物
        bw.mShowPetWnd.GetComponent<ShowPetsWnd>().SelectPet(mData.GetValue<int>("class_id"));

        GameObject petToolTip = bw.mPetToolTipWnd;
        if (petToolTip == null)
            return;

        // 选择觉醒分页
        petToolTip.GetComponent<PetToolTipWnd>().OnTabBtnClicked((int) BAGGAGE_PAGE.AWAKE_PAGE);

        // 当前阶段完成，执行回调继续下一步指引
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

// 指引点击觉醒按钮
public class SCRIPT_40091 : Script
{
    CallBack cb = null;

    AwakeWnd mAwakeWnd;
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        // 执行回调
        cb = _params[1] as CallBack;

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickWnd));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (data.ContainsKey("guide_pos"))
        {
            LPCArray pos = data.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }

            // 缓存场景相机的位置
            SceneMgr.SceneCameraToPos = vec;
            SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引窗口点击回调
    /// </summary>
    void OnClickWnd(object para, object[] param)
    {
        // 选择装备分页
        GameObject baggageWnd = WindowMgr.GetWindow(BaggageWnd.WndType);
        if (baggageWnd == null)
            return;

        GameObject petToolTip = baggageWnd.GetComponent<BaggageWnd>().mPetToolTipWnd;
        if (petToolTip == null)
            return;

        EventMgr.RegisterEvent("SCRIPT_40091", EventMgrEventType.EVENT_PET_AWAKE, OnPetAwake);

        mAwakeWnd = petToolTip.GetComponent<PetToolTipWnd>().mGroupWnd[(int)BAGGAGE_PAGE.AWAKE_PAGE].GetComponent<AwakeWnd>();

        // 觉醒宠物操作
        mAwakeWnd.GuideClickiAwakeBtn();
    }

    // 宠物觉醒消息回调
    void OnPetAwake(int eventId, MixedValue para)
    {
        // 解注册事件
        EventMgr.UnregisterEvent("SCRIPT_40091");

        // 执行回调
        if (cb != null)
        {
            cb.Go();

            cb = null;
        }
    }
}

// 魔力圣域指引完成返回主城
public class SCRIPT_40092 : Script
{
    LPCMapping mData = LPCMapping.Empty;

    CallBack mCb = null;

    public override object Call(params object[] _params)
    {
        // 回调
        mCb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        if(script_args.IsMapping)
            mData = script_args.AsMapping;

        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OnEnterMainCityScene));

        return true;
    }

    /// <summary>
    /// 打开主城回调
    /// </summary>
    private void OnEnterMainCityScene(object para, object[] param)
    {
        // 离开副本
        if (InstanceMgr.IsInInstance(ME.user))
            InstanceMgr.LeaveInstance(ME.user);

        // 析构结算界面
        WindowMgr.DestroyWindow("FightSettlementWnd");

        WindowMgr.HideAllWnd();

        GameObject mainWnd = WindowMgr.OpenMainWnd();

        mainWnd.GetComponent<MainWnd>().ShowMainUIBtn(true);

        // 执行回调
        if (mCb != null)
        {
            mCb.Go();

            mCb = null;
        }

        // 销毁指定窗口
        if (mData.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(mData.GetValue<string>("destroy_wnd")), "DoDestroy");
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }
}

// 指引执行脚本, 打开北镜圣域
public class SCRIPT_40093 : Script
{
    string wndName = string.Empty;

    LPCMapping mData = LPCMapping.Empty;

    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        mData = script_args.AsMapping;
        if (mData.Count == 0)
            return false;

        wndName = mData.GetValue<string>("wnd_name");

        string desc = mData.GetValue<string>("guide_desc");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        WindowMgr.HideWindow(SelectInstanceWnd.WndType);

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, mData, new CallBack(OnClickCallBack, cb));

        // 销毁指定窗口
        if (mData.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(mData.GetValue<string>("destroy_wnd")), "DoDestroy");

        if (mData.ContainsKey("guide_pos"))
        {
            LPCArray pos = mData.GetValue<LPCArray>("guide_pos");

            Vector3 vec = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

            SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
            if (control != null)
            {
                control.EndSceneCameraAnimation();

                if (!ME.isLoginOk)
                    ME.OnLoginOK();

                control.MoveCamera(SceneMgr.SceneCamera.transform.position, vec, 0.3f, 0f);
            }
        }

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    void OnClickCallBack(object para, object[] param)
    {
        int mapId = mData.GetValue<int>("map_id");

        CsvRow row = MapMgr.GetMapConfig(mapId);

        LPCArray pos = row.Query<LPCArray>("pos");

        // 相机移动的目标位置
        Vector3 targetPos = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

        // 创建地下城窗口
        GameObject wnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<DungeonsWnd>().Bind(mData.GetValue<string>("instance_id"), mapId, targetPos);

        // 隐藏主界面
        WindowMgr.HideMainWnd();

        // 执行回调
        CallBack cb = para as CallBack;
        if (cb != null)
            cb.Go();
    }
}

/// <summary>
/// 指引执行脚本，选择「平原浅滩」副本
/// </summary>
public class SCRIPT_40094 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        string wndName = data.GetValue<string>("wnd_name");

        string desc = data.GetValue<string>("guide_desc");

        string instanceId = data.GetValue<string>("instance_id");

        // 执行回调
        CallBack cb = _params[1] as CallBack;

        List<object> list = new List<object>();
        list.Add(cb);
        list.Add(instanceId);

        // 打开指引窗口
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return false;

        wnd.GetComponent<GuideWnd>().Bind(desc, data, new CallBack(OnClickCallBack, list));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    /// <summary>
    /// 指引点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        List<object> list = para as List<object>;

        string instanceId = list[1] as string;

        int mapType = InstanceMgr.GetMapTypeByInstanceId(instanceId);

        GameObject instacne = null;

        if (mapType == MapConst.INSTANCE_MAP_1)
        {
            instacne = WindowMgr.GetWindow(SelectInstanceWnd.WndType);
        }
        else if (mapType == MapConst.DUNGEONS_MAP_2)
        {
            instacne = WindowMgr.GetWindow(DungeonsWnd.WndType);
        }
        else
        {
        }

        if (instacne == null)
            return;

        instacne.SetActive(false);

        //获得选择战斗窗口
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        SelectFighterWnd selectFighterScript = wnd.GetComponent<SelectFighterWnd>();

        if (selectFighterScript == null)
            return;

        // 绑定数据
        selectFighterScript.Bind("SelectInstanceWnd", instanceId);

        // 关闭副本选择界面
        WindowMgr.HideWindow(SelectInstanceWnd.WndType);

        // 当前阶段执完成，执行回调
        CallBack cb = list[0] as CallBack;
        if (cb != null)
            cb.Go();
    }
}

public class SCRIPT_40095 : Script
{
    CallBack cb = null;

    public override object Call(params object[] _params)
    {
        cb = _params[1] as CallBack;

        if(InstanceMgr.IsInInstance(ME.user))
            EventMgr.RegisterEvent("SCRIPT_40095", EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, OnRetrunGuideEventCallBack);
        else
            cb.Go();

        return true;
    }

    void OnRetrunGuideEventCallBack(int eventId, MixedValue para)
    {
        if (cb != null)
            cb.Go();

        EventMgr.UnregisterEvent("SCRIPT_40095");
    }
}

public class SCRIPT_40096 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        GameObject wnd = WindowMgr.OpenWnd(LandaTaskGotoWnd.WndType);
        if (wnd == null)
            return false;

        // 绑定数据
        wnd.GetComponent<LandaTaskGotoWnd>().Bind(data.GetValue<int>("map_id"), data.GetValue<int>("task_id"));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        // 开启协程
        Coroutine.DispatchService(WaitInvoke(cb), "WaitInvoke");

        return true;
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }

    IEnumerator WaitInvoke(CallBack cb)
    {
        // 等待一帧
        yield return null;

        if (cb != null)
            cb.Go();

        Coroutine.StopCoroutine("WaitInvoke");
    }
}

public class SCRIPT_40097 : Script
{
    public override object Call(params object[] _params)
    {
        CallBack cb = _params[1] as CallBack;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        GameObject wnd = WindowMgr.OpenWnd(TaskGotoWnd.WndType);
        if (wnd == null)
            return false;

        // 绑定数据
        wnd.GetComponent<TaskGotoWnd>().Bind(data.GetValue<int>("map_id"), data.GetValue<int>("task_id"));

        if (data.ContainsKey("destroy_wnd"))
            Coroutine.DispatchService(DoDestroy(data.GetValue<string>("destroy_wnd")), "DoDestroy");

        // 开启协程
        Coroutine.DispatchService(WaitInvoke(cb), "WaitInvoke");

        return true;
    }

    IEnumerator WaitInvoke(CallBack cb)
    {
        // 等待一帧
        yield return null;

        if (cb != null)
            cb.Go();

        Coroutine.StopCoroutine("WaitInvoke");
    }

    IEnumerator DoDestroy(string wndName)
    {
        yield return null;
        yield return null;

        WindowMgr.DestroyWindow(wndName);

        Coroutine.StopCoroutine("DoDestroy");
    }
}

/// <summary>
/// 指引检测脚本
/// </summary>
public class SCRIPT_41000 : Script
{
    public override object Call(params object[] _params)
    {
        // 没有角色时才指引
        List<LPCValue> list = LoginMgr.GetAllUser();
        if (list == null || list.Count == 0)
            return true;

        return false;
    }
}

/// <summary>
/// 指引检测脚本
/// </summary>
public class SCRIPT_41001 : Script
{
    public override object Call(params object[] _params)
    {
        if (!InstanceMgr.IsInInstance(ME.user))
            return false;

        MixedValue para = _params[1] as MixedValue;
        if (para == null)
            return false;

        if (para.GetValue<LPCMapping>().GetValue<int>("result") == 1)
            return false;

        return true;
    }
}

/// <summary>
/// 指引检测脚本
/// </summary>
public class SCRIPT_41002 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        LPCArray instanceIds = data.GetValue<LPCArray>("instance_id");

        foreach (LPCValue instanceId in instanceIds.Values)
        {
            if (instanceId == null || !instanceId.IsString)
                continue;

            // 副本是否通关，列表中只有一个副本通关即可
            if (InstanceMgr.IsClearanced(ME.user, instanceId.AsString))
                return true;
        }

        return false;
    }
}

// 指引检测脚本
public class SCRIPT_41003 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        if (ME.user == null)
            return false;

        if (ME.user.GetLevel() != data.GetValue<int>("level"))
            return false;

        // 副本中不显示等级指引
        if (InstanceMgr.IsInInstance(ME.user))
            return false;

        return true;
    }
}

// 指引检测脚本
public class SCRIPT_41004 : Script
{
    public override object Call(params object[] _params)
    {
        if (!InstanceMgr.IsInInstance(ME.user))
            return false;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        // 副本id
        LPCArray instanceIds = data.GetValue<LPCArray>("instance_id");

        // 批次
        int batch = data.GetValue<int>("batch");

        // 触发该指引的事件参数
        MixedValue eventPara = _params[1] as MixedValue;
        if (eventPara == null)
            return false;

        // 当前副本对象
        Instance instance = eventPara.GetValue<Instance>();

        // 指定副本和当前通关副本不一致
        if (instanceIds.IndexOf(instance.InstanceId) == -1)
            return false;

        if (batch != 0 && batch != instance.Level)
            return false;

        return true;
    }
}

/// <summary>
/// 指引检测脚本
/// </summary>
public class SCRIPT_41005 : Script
{
    public override object Call(params object[] _params)
    {
        if (! InstanceMgr.IsInInstance(ME.user))
            return false;

        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        // 触发该指引的事件参数
        MixedValue eventPara = _params[1] as MixedValue;
        if (eventPara == null)
            return false;

        // 当前副本对象
        Instance instance = eventPara.GetValue<Instance>();

        if (data.GetValue<int>("batch") != instance.Level)
            return false;

        return true;
    }
}

// 指引检测脚本
public class SCRIPT_41006 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue script_args = _params[0] as LPCValue;
        if (script_args == null || ! script_args.IsMapping)
            return false;

        LPCMapping data = script_args.AsMapping;
        if (data.Count == 0)
            return false;

        if (!TaskMgr.IsCompleted(ME.user, data.GetValue<int>("task_id")))
            return false;

        return true;
    }
}

/// <summary>
/// 服务器文本拼接通用脚本1
/// 支持格式1 ： ({ "SCRIPT_42000", 文本id, 拼接参数1...拼接参数n })
/// 支持格式2 ： ({ "SCRIPT_42000", 文本id, 拼接参数1...({"SCRIPT_42000", 文本id, 拼接参数1 })...拼接参数n })
/// 效果 ： 后面的参数不做转义直接拼接，string.Format("{0}xxxx", 参数)；
/// </summary>
public class SCRIPT_42000 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 获取第一个参数
        string localText = LocalizationMgr.Get(msgList[1].AsString);

        // 如果只是一个参数
        if (msgList.Count == 2)
            return localText;

        // 计算后续参数
        string[] value = new string[msgList.Count - 2];
        for (int i = 2; i < msgList.Count; i++)
        {
            if (msgList[i].IsArray)
                value[i - 2] = LocalizationMgr.GetServerDesc(msgList[i]);
            else
                value[i - 2] = msgList[i].AsString;
        }

        // 组装字符串
        return string.Format(localText, value);
    }
}

/// <summary>
/// 服务器文本拼接脚本2
/// 支持格式1 ： ({ "SCRIPT_42000", 文本id, 拼接参数1...拼接参数n })
/// 效果 ： 后面的参数转义拼接，string.Format("{0}xxxx", 转义本地化资源参数)；
/// </summary>
public class SCRIPT_42001 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 获取第一个参数
        string localText = LocalizationMgr.Get(msgList[1].AsString);

        // 如果只是一个参数
        if (msgList.Count == 2)
            return localText;

        // 计算后续参数
        string[] value = new string[msgList.Count - 2];
        for (int i = 2; i < msgList.Count; i++)
            value[i - 2] = LocalizationMgr.GetServerDesc(msgList[i]);

        // 组装字符串
        return string.Format(localText, value);
    }
}

/// <summary>
/// 服务器文本拼接脚本3
/// 支持格式1 ： ({ "SCRIPT_42000", 文本id, 拼接参数1, 拼接参数2 })
/// 效果 ： 参数1本地化转义，参数2不转义；
/// </summary>
public class SCRIPT_42002 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 获取第一个参数
        string localText = LocalizationMgr.Get(msgList[1].AsString);

        // 组装字符串
        return string.Format(localText, msgList[2].AsString, LocalizationMgr.Get(msgList[3].AsString), msgList[4].AsString);
    }
}

/// <summary>
/// 服务器文本拼接脚本4
/// 支持格式1 ： ({ "SCRIPT_42000", 文本id, 拼接参数1, 拼接参数2 })
/// 效果 ： 参数1不转义，参数2本地化转义；
/// </summary>
public class SCRIPT_42003 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 获取第一个参数
        string localText = LocalizationMgr.Get(msgList[1].AsString);

        // 组装字符串
        return string.Format(localText, msgList[2].AsString, LocalizationMgr.Get(msgList[3].AsString));
    }
}

/// <summary>
/// 服务器文本拼接脚本5
/// 支持格式1 ： ({ "SCRIPT_42000", 文本id, 拼接参数1，拼接参数2 })
/// 效果 ： 参数1本地化转义，参数2不转义；
/// </summary>
public class SCRIPT_42004 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 获取第一个参数
        string localText = LocalizationMgr.Get(msgList[1].AsString);

        // 组装字符串
        return string.Format(localText, LocalizationMgr.Get(msgList[2].AsString), msgList[3].AsString);
    }
}

/// <summary>
/// 服务器文本拼接脚本6
/// 支持格式1 ： ({ "SCRIPT_42000", 文本id, 拼接参数1, 拼接参数2, 拼接参数3, 拼接参数4 })
/// 效果 ： 参数1本地化转义，参数2不转义，参数3本地化转义，参数4不转义；
/// </summary>
public class SCRIPT_42005 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 获取第一个参数
        string localText = LocalizationMgr.Get(msgList[1].AsString);

        // 组装字符串
        return string.Format(localText,
            LocalizationMgr.Get(msgList[2].AsString),
            msgList[3].AsString,
            LocalizationMgr.Get(msgList[4].AsString),
            msgList[5].AsString);
    }
}

/// <summary>
/// 服务器文本拼接脚本7
/// 支持格式1 ： ({ "SCRIPT_42000", 文本id, 拼接参数1, 拼接参数2, 拼接参数3 })
/// 效果 ： 参数1本地化转义，参数2本地化转义，参数3不转义；
/// </summary>
public class SCRIPT_42006 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 获取第一个参数
        string localText = LocalizationMgr.Get(msgList[1].AsString);

        // 组装字符串
        return string.Format(localText,
            LocalizationMgr.Get(msgList[2].AsString),
            LocalizationMgr.Get(msgList[3].AsString),
            msgList[4].AsString);
    }
}

/// <summary>
/// 服务器文本拼接脚本8
/// 支持格式1 ： ({ "SCRIPT_42000", 文本id, 拼接参数1, 拼接参数2, 拼接参数3, 拼接参数4 })
/// 效果 ： 参数1不转义，参数2本地化转义，参数3本地化转义，参数4不转义；
/// </summary>
public class SCRIPT_42007 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 您在成长积分兑换活动中花费了 {0} {1}兑换了「{2}」× {3}\n请及时收取，否则在保存时间结束后道具将会被删除。
        return string.Format(LocalizationMgr.Get(msgList[1].AsString),
            msgList[2].AsString,
            LocalizationMgr.Get(msgList[3].AsString),
            LocalizationMgr.Get(msgList[4].AsString),
            msgList[5].AsString);
    }
}

/// <summary>
/// 服务器文本拼接脚本9
/// GD 复制功能专用
/// </summary>
public class SCRIPT_42008 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 获取第一个参数
        string localText = LocalizationMgr.Get(msgList[1].AsString);

        // 组装字符串
        return string.Format(localText,
            msgList[2].AsString,
            LocalizationMgr.GetServerDesc(LPCRestoreString.SafeRestoreFromString(msgList[3].AsString)));
    }
}

/// <summary>
/// 服务器文本拼接脚本10
/// 支持格式1 ： ({ "SCRIPT_42009", 参数1...参数n })
/// 效果 ： 参数都做本地化转义，参数1 + 参数2 ....+ 参数n；
/// </summary>
public class SCRIPT_42009 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray msgList = _params[0] as LPCArray;

        // 获取第一个参数
        string localText = string.Empty;

        // 计算后续参数
        for (int i = 1; i < msgList.Count; i++)
            localText = localText + LocalizationMgr.Get(msgList[i].AsString);

        // 组装字符串
        return localText;
    }
}