using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;
using UnityEngine.SceneManagement;

// 任务奖励计算
public class SCRIPT_20000 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20002 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20003 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20004 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20005 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20006 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20007 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务描述
public class SCRIPT_21000 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        int type = data.Query<int>("type");

        if (type.Equals(TaskConst.EASY_TASK))
            return string.Format("[FFF8E1FF]{0}[-][43DFFFFF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format(" ({0}/1)",
                TaskMgr.GetTaskProgress(ME.user, taskId).AsInt));

        return string.Format("[260000FF]{0}[-][FD0806FF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format(" ({0}/1)",
            TaskMgr.GetTaskProgress(ME.user, taskId).AsInt));
    }
}

// 任务奖励计算
public class SCRIPT_20010 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20011 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20012 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args

        LPCArray args = _params[1] as LPCArray;

        LPCArray bonusList = new LPCArray();

        foreach (LPCValue bonusMap in args.Values)
        {
            LPCMapping bonus = bonusMap.AsMapping;
            // 属性、道具奖励
            if (bonus.GetValue<int>("type") == 1 || bonus.GetValue<int>("type") == 2)
                bonusList.Add(bonus.GetValue<LPCMapping>("bonus"));

            // 使魔奖励
            if (bonus.GetValue<int>("type") == 3)
            {
                LPCMapping para = LPCMapping.Empty;
                para.Append(bonus.GetValue<LPCMapping>("bonus"));
                para.Add("amount", bonus.GetValue<int>("amount"));

                bonusList.Add(para);
            }
        }

        return bonusList;
    }
}

// 任务奖励计算
public class SCRIPT_20013 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args

        LPCArray args = _params[1] as LPCArray;

        LPCArray bonusList = new LPCArray();

        foreach (LPCValue bonusMap in args.Values)
        {
            LPCMapping bonus = bonusMap.AsMapping;
            // 属性、道具奖励
            if (bonus.GetValue<int>("type") == 1 || bonus.GetValue<int>("type") == 2)
                bonusList.Add(bonus.GetValue<LPCMapping>("bonus"));

            // 使魔奖励
            if (bonus.GetValue<int>("type") == 3)
            {
                LPCMapping para = LPCMapping.Empty;
                para.Append(bonus.GetValue<LPCMapping>("bonus"));
                para.Add("amount", bonus.GetValue<int>("amount"));

                bonusList.Add(para);
            }
        }

        return bonusList;
    }
}

// 任务奖励计算
public class SCRIPT_20014 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20015 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务奖励计算
public class SCRIPT_20016 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user, 参数2为 args
        return _params[1] as LPCArray;
    }
}

// 任务描述
public class SCRIPT_21003 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        int type = data.Query<int>("type");

        if (type.Equals(TaskConst.EASY_TASK))
            return string.Format("[FFF8E1FF]{0}[-][43DFFFFF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format(" ({0}/{1})",
                    TaskMgr.GetTaskProgress(ME.user, taskId).AsInt, data.Query<LPCMapping>("complete_condition").GetValue<int>("amount")));

        return string.Format("[260000FF]{0}[-][FD0806FF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format(" ({0}/{1})",
                TaskMgr.GetTaskProgress(ME.user, taskId).AsInt, data.Query<LPCMapping>("complete_condition").GetValue<int>("amount")));
    }
}

// 任务描述
public class SCRIPT_21004 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        // 获取进度
        int progress = TaskMgr.GetTaskProgress(ME.user, taskId).AsInt;

        // 获取要求
        int condition_progress = data.Query<LPCMapping>("complete_condition").GetValue<int>("amount");

        // 测试代码
        return string.Format("[260000FF]{0}[-][FD0806FF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format("({0}/1)",
                progress >= condition_progress ? 1 : 0));
    }
}

// 任务描述
public class SCRIPT_21005 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        Property user = (Property)_params[0];

        // 如果玩家对象不存在
        if (user == null)
            return string.Empty;

        // 获取任务id
        int taskId = (int)_params[1];

        // 获取任务配置信息
        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        // 获取进度
        int progress = TaskMgr.GetTaskProgress(user, taskId).AsInt;

        LPCMapping taskMap = user.Query<LPCMapping>("daily_task/task_map");

        // 测试代码
        return string.Format("[260000FF]{0}[-][FD0806FF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format("({0}/1)",
                progress >= taskMap.Count ? 1 : 0));
    }
}

// 任务描述
public class SCRIPT_21006 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        return string.Format(
            LocalizationMgr.Get(data.Query<string>("desc_args")),
            TaskMgr.GetTaskProgress(ME.user, taskId).AsInt,
            complete_condition.GetValue<int>("amount")
        );
    }
}

// 任务描述
public class SCRIPT_21007 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        if (complete_condition.ContainsKey("amount"))
        {
            return string.Format(
                LocalizationMgr.Get(data.Query<string>("desc_args")),
                TaskMgr.GetTaskProgress(ME.user, taskId).AsInt,
                complete_condition.GetValue<int>("amount")
            );
        }
        else if (complete_condition.ContainsKey("pet") && complete_condition.ContainsKey("skill"))
        {
            LPCMapping skillMap = complete_condition.GetValue<LPCMapping>("skill");

            LPCMapping processData = LPCMapping.Empty;

            LPCValue processV = TaskMgr.GetTaskProgress(ME.user, taskId);
            if (processV != null && processV.IsMapping)
                processData = processV.AsMapping;

            return string.Format(
                LocalizationMgr.Get(data.Query<string>("desc_args")),
                processData.GetValue<int>("pet"),
                complete_condition.GetValue<int>("pet"),
                processData.GetValue<int>("skill"),
                skillMap.GetValue<int>("amount")
            );
        }
        else if (complete_condition.ContainsKey("pet"))
        {
            LPCMapping processData = LPCMapping.Empty;

            LPCValue processV = TaskMgr.GetTaskProgress(ME.user, taskId);
            if (processV != null && processV.IsMapping)
                processData = processV.AsMapping;

            return string.Format(
                LocalizationMgr.Get(data.Query<string>("desc_args")),
                processData.GetValue<int>("pet"),
                complete_condition.GetValue<int>("pet")
            );
        }
        else if (complete_condition.ContainsKey("skill"))
        {
            LPCMapping skillMap = complete_condition.GetValue<LPCMapping>("skill");

            LPCMapping processData = LPCMapping.Empty;

            LPCValue processV = TaskMgr.GetTaskProgress(ME.user, taskId);
            if (processV != null && processV.IsMapping)
                processData = processV.AsMapping;

            return string.Format(
                LocalizationMgr.Get(data.Query<string>("desc_args")),
                processData.GetValue<int>("skill"),
                skillMap.GetValue<int>("amount")
            );
        }
        else if (complete_condition.ContainsKey("defence") && complete_condition.ContainsKey("revenge"))
        {
            LPCMapping processData = LPCMapping.Empty;

            LPCValue processV = TaskMgr.GetTaskProgress(ME.user, taskId);
            if (processV != null && processV.IsMapping)
                processData = processV.AsMapping;

            return string.Format(
                LocalizationMgr.Get(data.Query<string>("desc_args")),
                processData.GetValue<int>("defence"),
                complete_condition.GetValue<int>("defence"),
                processData.GetValue<int>("revenge"),
                complete_condition.GetValue<int>("revenge")
            );
        }
        else if (complete_condition.ContainsKey("defence"))
        {
            LPCMapping processData = LPCMapping.Empty;

            LPCValue processV = TaskMgr.GetTaskProgress(ME.user, taskId);
            if (processV != null && processV.IsMapping)
                processData = processV.AsMapping;

            return string.Format(
                LocalizationMgr.Get(data.Query<string>("desc_args")),
                processData.GetValue<int>("defence"),
                complete_condition.GetValue<int>("defence")
            );
        }

        return string.Empty;
    }
}

// 任务描述
public class SCRIPT_21008 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        Property user = (Property)_params[0];

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        List<string> instanceList = InstanceMgr.GetNpcInstanceList(user);

        // 获取任务进度
        LPCArray progress = LPCArray.Empty;

        LPCValue processV = TaskMgr.GetTaskProgress(user, taskId);
        if (processV != null && processV.IsArray)
            progress = processV.AsArray;

        int[] clearance = new int[9];

        for (int i = 0; i < instanceList.Count; i++)
        {
            // 没通关的不处理
            if (!InstanceMgr.IsClearanced(user, instanceList[i]))
                continue;

            LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(instanceList[i]);
            if (instanceInfo == null)
                continue;

            if (progress.IndexOf(instanceInfo.GetValue<int>("flag")) != -1)
                clearance[i] = 1;
        }

        // 拼接任务完成描述
        return string.Format(LocalizationMgr.Get(data.Query<string>("desc_args")),
            clearance[0],
            clearance[1],
            clearance[2],
            clearance[3],
            clearance[4],
            clearance[5],
            clearance[6],
            clearance[7],
            clearance[8]
        );
    }
}

// 任务领取系统提示
public class SCRIPT_21009 : Script
{
    public override object Call(params object[] _params)
    {
        // 给出系统提示
        DialogMgr.Notify(LocalizationMgr.Get(_params[0] as string));

        return true;
    }
}

// 任务领取系统提示
public class SCRIPT_21010 : Script
{
    public override object Call(params object[] _params)
    {
        string tipsArg = _params[0] as string;

        int taskId = (int)_params[1];

        Property user = _params[2] as Property;
        if (user == null)
            return false;

        LPCArray bonus = TaskMgr.GetBonus(user, taskId);
        if (bonus.Count == 0)
            return false;

        LPCMapping bonusData = bonus[0].AsMapping;

        // 给出提示框
        DialogMgr.ShowSimpleSingleBtnDailog(null, string.Format(LocalizationMgr.Get(tipsArg), ItemMgr.GetName(bonusData.GetValue<int>("class_id"))));

        return true;
    }
}

// login类任务描述
public class SCRIPT_21011 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        //int type = data.Query<int>("type");

        return string.Format("[260000FF]{0}[-][FD0806FF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format(" ({0}/{1})",
            TaskMgr.GetTaskProgress(ME.user, taskId).AsMapping.GetValue<int>("login_days"), data.Query<LPCMapping>("complete_condition").GetValue<int>("amount")));
    }
}

// 元素之主任务描述
public class SCRIPT_21012 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        LPCMapping amount = complete_condition.GetValue<LPCMapping>("amount");

        int fNums = 0;
        int sNums = 0;
        int wNums = 0;

        if (TaskMgr.GetTaskProgress(ME.user, taskId).AsMapping != null)
        {
            LPCMapping progress = TaskMgr.GetTaskProgress(ME.user, taskId).AsMapping;
            fNums = progress.GetValue<int>(MonsterConst.ELEMENT_FIRE);
            sNums = progress.GetValue<int>(MonsterConst.ELEMENT_STORM);
            wNums = progress.GetValue<int>(MonsterConst.ELEMENT_WATER);
        }

        return string.Format(
            LocalizationMgr.Get(data.Query<string>("desc_args")),
            fNums,
            amount.GetValue<int>(MonsterConst.ELEMENT_FIRE),
            sNums,
            amount.GetValue<int>(MonsterConst.ELEMENT_STORM),
            wNums,
            amount.GetValue<int>(MonsterConst.ELEMENT_WATER)
        );
    }
}

// 连续N天完成任务类任务描述
public class SCRIPT_21013 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        //int type = data.Query<int>("type");

        // 如果没有数据
        int cTimes = 0;
        if (TaskMgr.GetTaskProgress(ME.user, taskId).AsMapping != null)
            cTimes = TaskMgr.GetTaskProgress(ME.user, taskId).AsMapping.GetValue<int>("complete_times");

        return string.Format("[260000FF]{0}[-][FD0806FF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format(" ({0}/{1})",
            cTimes, data.Query<LPCMapping>("complete_condition").GetValue<int>("amount")));
    }
}

// 光暗之星任务描述
public class SCRIPT_21014 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        LPCMapping amount = complete_condition.GetValue<LPCMapping>("amount");

        int lNums = 0;
        int dNums = 0;

        if (TaskMgr.GetTaskProgress(ME.user, taskId).AsMapping != null)
        {
            LPCMapping progress = TaskMgr.GetTaskProgress(ME.user, taskId).AsMapping;
            lNums = progress.GetValue<int>(MonsterConst.ELEMENT_LIGHT);
            dNums = progress.GetValue<int>(MonsterConst.ELEMENT_DARK);
        }

        return string.Format(
            LocalizationMgr.Get(data.Query<string>("desc_args")),
            lNums,
            amount.GetValue<int>(MonsterConst.ELEMENT_LIGHT),
            dNums,
            amount.GetValue<int>(MonsterConst.ELEMENT_DARK)
        );
    }
}

// 通关几种隐藏圣域任务描述
public class SCRIPT_21015 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        Property user = (Property)_params[0];

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        // 获取任务进度
        LPCArray progress = LPCArray.Empty;
        int num = 0;
        LPCValue processV = TaskMgr.GetTaskProgress(user, taskId);
        if (processV != null && processV.IsArray)
            num = processV.AsArray.Count;

        return string.Format("[260000FF]{0}[-][FD0806FF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format(" ({0}/{1})",
            num, data.Query<LPCMapping>("complete_condition").GetValue<int>("amount")));
    }
}

// 无后顾之忧任务描述
public class SCRIPT_21016 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);
        int defenceTimes = 0;
        LPCValue processV = TaskMgr.GetTaskProgress(ME.user, taskId);
        if (processV != null && processV.IsMapping)
            defenceTimes = processV.AsMapping.GetValue<int>("defence");

        return string.Format("[260000FF]{0}[-][FD0806FF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format(" ({0}/{1})",
            defenceTimes, data.Query<LPCMapping>("complete_condition").GetValue<int>("defence")));
    }
}

// 每日登录任务描述
public class SCRIPT_21017 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1为 user， 参数2为taskid
        // 前面描述要用 260000FF 颜色，后面进度要用 FD0806FF  颜色
        // 获取进度用taskmgr GetTaskProgress接口，总的进度再"complete_condition"取

        int taskId = (int)_params[1];

        CsvRow data = TaskMgr.GetTaskInfo(taskId);

        //int type = data.Query<int>("type");
        int loginDays = 0;
        LPCValue loginMap = TaskMgr.GetTaskProgress(ME.user, taskId);

        if (loginMap != null && loginMap.IsMapping)
            loginDays = loginMap.AsMapping.GetValue<int>("login_days");

        return string.Format("[260000FF]{0}[-][FD0806FF]{1}[-]", LocalizationMgr.Get(data.Query<string>("desc_args")), string.Format(" ({0}/{1})",
            loginDays, data.Query<LPCMapping>("complete_condition").GetValue<int>("amount")));
    }
}

// 获取任务进度
public class SCRIPT_21100 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象 _params[0]
        // 任务配置数据 _params[1]

        Property user = _params[0] as Property;
        if (user == null)
            return LPCValue.Create();

        CsvRow data = _params[1] as CsvRow;
        if (data == null)
            return LPCValue.Create();

        int taskId = data.Query<int>("task_id");

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        // 如果已经完成，直接返回配置的数值
        if (!TaskMgr.IsCompleted(user, taskId))
            return LPCValue.Create();

        return complete_condition.GetValue<LPCValue>("amount");
    }
}

// 获取任务进度
public class SCRIPT_21101 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象 _params[0]
        // 任务配置数据 _params[1]

        Property user = _params[0] as Property;
        if (user == null)
            return LPCValue.Create();

        CsvRow data = _params[1] as CsvRow;
        if (data == null)
            return LPCValue.Create();

        int taskId = data.Query<int>("task_id");

        // 如果已经完成，直接返回配置的数值
        if (!TaskMgr.IsCompleted(user, taskId))
            return LPCValue.Create();

        LPCArray progress = LPCArray.Empty;

        List<string> list = InstanceMgr.GetNpcInstanceList(user);

        for (int i = 0; i < list.Count; i++)
        {
            LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(list[i]);
            if (instanceInfo == null)
                continue;

            progress.Add(instanceInfo.GetValue<int>("flag"));
        }

        return LPCValue.Create(progress);
    }
}

// 获取任务进度
public class SCRIPT_21102 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象 _params[0]
        // 任务配置数据 _params[1]

        Property user = _params[0] as Property;
        if (user == null)
            return LPCValue.Create();

        CsvRow data = _params[1] as CsvRow;
        if (data == null)
            return LPCValue.Create();

        int taskId = data.Query<int>("task_id");

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        // 如果已经完成，直接返回配置的数值
        if (!TaskMgr.IsCompleted(user, taskId))
            return LPCValue.Create();

        LPCMapping map = LPCMapping.Empty;

        map.Add("pet", complete_condition.GetValue<int>("pet"));

        LPCMapping skillMap = complete_condition.GetValue<LPCMapping>("skill");

        map.Add("skill", skillMap.GetValue<int>("amount"));

        return LPCValue.Create(map);
    }
}

// 获取任务进度
public class SCRIPT_21103 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象 _params[0]
        // 任务配置数据 _params[1]

        Property user = _params[0] as Property;
        if (user == null)
            return LPCValue.Create();

        CsvRow data = _params[1] as CsvRow;
        if (data == null)
            return LPCValue.Create();

        int taskId = data.Query<int>("task_id");

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        // 如果已经完成，直接返回配置的数值
        if (!TaskMgr.IsCompleted(user, taskId))
            return LPCValue.Create();

        return LPCValue.Create(complete_condition);
    }
}

// 获取任务进度
public class SCRIPT_21104 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象 _params[0]
        // 任务配置数据 _params[1]

        Property user = _params[0] as Property;
        if (user == null)
            return LPCValue.Create();

        CsvRow data = _params[1] as CsvRow;
        if (data == null)
            return LPCValue.Create();

        int taskId = data.Query<int>("task_id");

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        // 如果已经完成，直接返回配置的数值
        if (!TaskMgr.IsCompleted(user, taskId))
            return LPCValue.Create();

        LPCMapping map = LPCMapping.Empty;

        LPCMapping skillMap = complete_condition.GetValue<LPCMapping>("skill");

        map.Add("skill", skillMap.GetValue<int>("amount"));

        return LPCValue.Create(map);
    }
}

// 获取任务进度
public class SCRIPT_21105 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象 _params[0]
        // 任务配置数据 _params[1]

        Property user = _params[0] as Property;
        if (user == null)
            return LPCValue.Create();

        CsvRow data = _params[1] as CsvRow;
        if (data == null)
            return LPCValue.Create();

        int taskId = data.Query<int>("task_id");

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        // 如果已经完成，直接返回配置的数值
        if (!TaskMgr.IsCompleted(user, taskId))
            return LPCValue.Create();

        LPCMapping map = LPCMapping.Empty;

        map.Add("defence", complete_condition.GetValue<int>("defence"));
        map.Add("revenge", complete_condition.GetValue<int>("revenge"));

        return LPCValue.Create(map);
    }
}

// login类获取任务进度
public class SCRIPT_21106 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象 _params[0]
        // 任务配置数据 _params[1]

        Property user = _params[0] as Property;
        if (user == null)
            return LPCValue.Create(LPCMapping.Empty);

        CsvRow data = _params[1] as CsvRow;
        if (data == null)
            return LPCValue.Create(LPCMapping.Empty);

        int taskId = data.Query<int>("task_id");

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        // 如果已经完成，直接返回配置的数值
        if (!TaskMgr.IsCompleted(user, taskId))
            return LPCValue.Create(LPCMapping.Empty);

        LPCMapping loginMap = LPCMapping.Empty;
        loginMap.Add("login_days", complete_condition.GetValue<int>("amount"));

        return LPCValue.Create(loginMap);
    }
}

// 连续N天完成任务类获取任务进度
public class SCRIPT_21107 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象 _params[0]
        // 任务配置数据 _params[1]

        Property user = _params[0] as Property;
        if (user == null)
            return LPCValue.Create(LPCMapping.Empty);

        CsvRow data = _params[1] as CsvRow;
        if (data == null)
            return LPCValue.Create(LPCMapping.Empty);

        int taskId = data.Query<int>("task_id");

        LPCMapping complete_condition = data.Query<LPCMapping>("complete_condition");

        // 如果已经完成，直接返回配置的数值
        if (!TaskMgr.IsCompleted(user, taskId))
            return LPCValue.Create(LPCMapping.Empty);

        LPCMapping loginMap = LPCMapping.Empty;
        loginMap.Add("complete_times", complete_condition.GetValue<int>("amount"));

        return LPCValue.Create(loginMap);
    }
}

// 前往脚本
public class SCRIPT_22000 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1位user，参数2位arg(lpcvalue)
        // 返回结果为是否关闭任务界面
        LPCMapping valueMap = (_params[1] as LPCValue).AsMapping;

        string loadScene = string.Empty;

        if (valueMap.ContainsKey("load_scene"))
        {
            loadScene = valueMap.GetValue<string>("load_scene");

            // 抛出切换地图事件
            SceneMgr.LoadScene("Main", loadScene, new CallBack(LoadSceneCallBack, valueMap));

            return true;
        }

        return ShowWnd(valueMap) == null ? false : true;
    }

    private GameObject ShowWnd(LPCMapping valueMap)
    {
        string wndName = string.Empty;

        if (valueMap.ContainsKey("wnd_name"))
            wndName = valueMap.GetValue<string>("wnd_name");
        else
            wndName = "MainWnd";

        if (string.IsNullOrEmpty(wndName))
            return null;

        GameObject wnd = WindowMgr.OpenWnd(wndName);

        if (wnd == null)
            return null;

        WindowMgr.ShowWindow(wnd);

        return wnd;
    }

    private void LoadSceneCallBack(object para, params object[] _param)
    {
        LPCMapping valueMap = para as LPCMapping;

        string loadScene = valueMap.GetValue<string>("load_scene");

        string wndName = valueMap.GetValue<string>("wnd_name");

        GameObject wnd = ShowWnd(valueMap);

        if (loadScene.Equals(SceneConst.SCENE_WORLD_MAP))
        {
            Camera camera = SceneMgr.SceneCamera;

            Vector3 pos = Vector3.zero;

            switch (wndName)
            {
                case "MainWnd":

                    wnd.GetComponent<MainWnd>().ShowMainUIBtn(false);

                    pos = new Vector3(0, 0, -20.4f);

                    SceneMgr.SceneCameraFromPos = camera.transform.localPosition;

                    SceneMgr.SceneCameraToPos = pos;

                    // 设置场景相机的位置
                    camera.transform.localPosition = pos;

                    break;
                case "DungeonsWnd":

                    WindowMgr.HideMainWnd();

                    wnd.GetComponent<DungeonsWnd>().Bind(string.Empty, 0, new Vector3(-4.25f, 10.86f, -15f));

                    break;
                default:
                    break;
            }
        }
    }
}

// 前往脚本
public class SCRIPT_22001 : Script
{
    Vector3 mPos = Vector3.zero;

    int mMapId = 0;

    public override object Call(params object[] _params)
    {

        // 参数1位user，参数2位arg(lpcvalue)
        // 返回结果为是否关闭任务界面
        //LPCMapping valueMap = (_params[1] as LPCValue).AsMapping;

        // 参数3为systemfunction数据
        //LPCMapping functionData = (_params[2] as LPCValue).AsMapping;

        // 参数4为botton的gamobject,参数5为是否按下

        Property user = _params[0] as Property;
        if (user == null)
            return false;

        LPCValue args = _params[1] as LPCValue;
        if (args == null || !args.IsMapping)
            return false;

        LPCMapping data = args.AsMapping;

        mMapId = data.GetValue<int>("map_id");

        CsvRow mapConfig = MapMgr.GetMapConfig(mMapId);

        LPCArray pos = mapConfig.Query<LPCArray>("pos");
        if (pos.Count == 0)
            return false;

        mPos = new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat);

        GameObject item = _params[3] as GameObject;
        if (item != null)
            item.GetComponent<SystemFunctionItemWnd>().SetRedPoint(false, SystemFunctionConst.NEW_TIPS);

        LPCMapping functionData = _params[2] as LPCMapping;

        GameObject mainWnd = WindowMgr.GetWindow("MainWnd");
        if (mainWnd == null)
            return false;

        GameObject gotoWnd = mainWnd.GetComponent<MainWnd>().mGotoViewWnd;

        GotoWnd script = gotoWnd.GetComponent<GotoWnd>();
        if (script == null)
            return false;

        gotoWnd.SetActive(true);

        script.ShowView(functionData, _params[3] as GameObject, new CallBack(OnClickCallBack));

        LPCMapping temp = LPCMapping.Empty;

        int sysFuncId = functionData.GetValue<int>("id");

        LPCValue v = user.QueryTemp<LPCValue>("assign_task_sys_func");
        if (v != null && v.IsMapping)
        {
            temp = v.AsMapping;

            temp.Add(sysFuncId, 0);

            user.SetTemp("assign_task_sys_func", LPCValue.Create(temp));
        }

        return true;
    }

    /// <summary>
    /// 立即前往点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnWorldMaskCallBack));

        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OnEnterWorldMapScene));
    }

    void OnWorldMaskCallBack(object para, params object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OnEnterWorldMapScene));
    }

    /// <summary>
    /// 地图场景加载完成会掉
    /// </summary>
    private void OnEnterWorldMapScene(object para, object[] param)
    {
        WindowMgr.HideMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        //获取副本选择界面;
        GameObject selectInstanceWnd = WindowMgr.OpenWnd(SelectInstanceWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (selectInstanceWnd == null)
            return;

        SelectInstanceWnd selectInstanceScript = selectInstanceWnd.GetComponent<SelectInstanceWnd>();

        if (selectInstanceScript == null)
            return;

        // 绑定数据
        selectInstanceScript.Bind(mMapId, mPos);
    }
}

// 前往脚本
public class SCRIPT_22002 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1位user，参数2位arg(lpcvalue)
        // 返回结果为是否关闭任务界面
        //LPCMapping valueMap = (_params[1] as LPCValue).AsMapping;

        // 参数3为systemfunction数据
        //LPCMapping functionData = (_params[2] as LPCValue).AsMapping;

        // 参数4为botton的gamobject,参数5为是否按下

        Property user = _params[0] as Property;
        if (user == null)
            return false;

        LPCValue args = _params[1] as LPCValue;
        if (args == null || !args.IsMapping)
            return false;

        LPCMapping data = args.AsMapping;

        GameObject item = _params[3] as GameObject;
        if (item != null)
            item.GetComponent<SystemFunctionItemWnd>().SetRedPoint(false, SystemFunctionConst.NEW_TIPS);

        LPCMapping functionData = _params[2] as LPCMapping;

        GameObject mainWnd = WindowMgr.GetWindow("MainWnd");
        if (mainWnd == null)
            return false;

        GameObject gotoWnd = mainWnd.GetComponent<MainWnd>().mGotoViewWnd;

        GotoWnd script = gotoWnd.GetComponent<GotoWnd>();
        if (script == null)
            return false;

        gotoWnd.SetActive(true);

        script.ShowView(functionData, _params[3] as GameObject, new CallBack(OnClickCallBack, data));

        LPCMapping temp = LPCMapping.Empty;

        int sysFuncId = functionData.GetValue<int>("id");

        LPCValue v = user.QueryTemp<LPCValue>("assign_task_sys_func");
        if (v != null && v.IsMapping)
        {
            temp = v.AsMapping;

            temp.Add(sysFuncId, 0);

            user.SetTemp("assign_task_sys_func", LPCValue.Create(temp));
        }

        return true;
    }

    /// <summary>
    /// 点击回调
    /// </summary>
    void OnClickCallBack(object para, params object[] param)
    {
        LPCMapping data = para as LPCMapping;
        if (data == null)
            return;

        WindowMgr.HideMainWnd();

        WindowMgr.OpenWnd(data.GetValue<string>("wnd_name"));
    }
}

// 前往脚本
public class SCRIPT_22003 : Script
{
    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;
        if (user == null)
            return false;

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return false;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnWorldMaskCallBack, user));

        return true;
    }

    void OnWorldMaskCallBack(object para, params object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(LoadSceneCallBack, para));
    }

    void LoadSceneCallBack(object para, params object[] param)
    {
        Property user = para as Property;

        int lastMap = MapMgr.GetUnlockLastMapId(user, MapConst.INSTANCE_MAP_1);

        // 地图配置数据
        CsvRow row = MapMgr.GetMapConfig(lastMap);
        if (row == null)
            return;

        // 相机位置
        LPCArray posArray = row.Query<LPCArray>("pos");

        Vector3 pos = new Vector3(posArray[0].AsFloat, posArray[1].AsFloat, posArray[2].AsFloat);

        WindowMgr.HideMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        int difficulty = 0;

        if (InstanceMgr.DifficultyIsUnlock(user, lastMap, InstanceConst.INSTANCE_DIFFICULTY_EASY))
            difficulty = InstanceConst.INSTANCE_DIFFICULTY_EASY;
        if (InstanceMgr.DifficultyIsUnlock(user, lastMap, InstanceConst.INSTANCE_DIFFICULTY_NORMAL))
            difficulty = InstanceConst.INSTANCE_DIFFICULTY_NORMAL;
        if (InstanceMgr.DifficultyIsUnlock(user, lastMap, InstanceConst.INSTANCE_DIFFICULTY_HARD))
            difficulty = InstanceConst.INSTANCE_DIFFICULTY_HARD;

        LPCValue v = OptionMgr.GetOption(ME.user, "instance_difficulty");
        LPCMapping data = LPCMapping.Empty;

        if (v != null && v.IsMapping)
            data = v.AsMapping;

        data.Add(lastMap, difficulty);

        // 缓存副本难度到本地
        OptionMgr.SetOption(ME.user, "instance_difficulty", LPCValue.Create(data));

        //获取副本选择界面;
        GameObject selectInstanceWnd = WindowMgr.OpenWnd(SelectInstanceWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (selectInstanceWnd == null)
            return;

        SelectInstanceWnd selectInstanceScript = selectInstanceWnd.GetComponent<SelectInstanceWnd>();

        if (selectInstanceScript == null)
            return;

        // 绑定数据
        selectInstanceScript.Bind(lastMap, pos);
    }
}

// 前往脚本
public class SCRIPT_22004 : Script
{
    // 地图配置数据
    CsvRow row = null;

    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;
        if (user == null)
            return false;

        LPCValue args = _params[1] as LPCValue;
        if (args == null || !args.IsMapping)
            return false;

        // 地图配置数据
        row = MapMgr.GetMapConfig(args.AsMapping.GetValue<int>("map_id"));
        if (row == null)
            return false;

        if (!MapMgr.IsUnlocked(ME.user, args.AsMapping.GetValue<int>("map_id")))
        {
            // 解锁条件
            LPCMapping unlockArgs = row.Query<LPCMapping>("unlock_args");

            CsvRow config = MapMgr.GetMapConfig(unlockArgs.GetValue<int>("map_id"));

            // 地图未解锁
            DialogMgr.ShowSingleBtnDailog(
                null,
                string.Format(LocalizationMgr.Get("GrowthTaskBonusWnd_9"), LocalizationMgr.Get(config.Query<string>("name"))),
                LocalizationMgr.Get("GrowthTaskBonusWnd_8")
            );

            return false;
        }

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return false;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnWorldMaskCallBack, user));

        return true;
    }


    void OnWorldMaskCallBack(object para, params object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(LoadSceneCallBack, para));
    }

    void LoadSceneCallBack(object para, params object[] param)
    {
        // 相机位置
        LPCArray posArray = row.Query<LPCArray>("pos");

        Vector3 pos = new Vector3(posArray[0].AsFloat, posArray[1].AsFloat, posArray[2].AsFloat);

        WindowMgr.HideMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        //获取副本选择界面;
        GameObject selectInstanceWnd = WindowMgr.OpenWnd(SelectInstanceWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (selectInstanceWnd == null)
            return;

        SelectInstanceWnd selectInstanceScript = selectInstanceWnd.GetComponent<SelectInstanceWnd>();

        if (selectInstanceScript == null)
            return;

        // 绑定数据
        selectInstanceScript.Bind(row.Query<int>("rno"), pos);
    }
}

// 前往脚本
public class SCRIPT_22005 : Script
{
    CsvRow row = null;

    LPCMapping argsData = null;

    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;
        if (user == null)
            return false;

        LPCValue args = _params[1] as LPCValue;
        if (args == null || !args.IsMapping)
            return false;

        argsData = args.AsMapping;

        // 地图配置数据
        row = MapMgr.GetMapConfig(argsData.GetValue<int>("map_id"));
        if (row == null)
            return false;

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return false;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnWorldMaskCallBack, user));

        return true;
    }

    void OnWorldMaskCallBack(object para, params object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(LoadSceneCallBack, para));
    }

    void LoadSceneCallBack(object para, params object[] param)
    {
        // 相机位置
        LPCArray posArray = row.Query<LPCArray>("pos");

        Vector3 pos = new Vector3(posArray[0].AsFloat, posArray[1].AsFloat, posArray[2].AsFloat);

        WindowMgr.HideMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        //获取副本选择界面;
        GameObject dungeonsWndWnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (dungeonsWndWnd == null)
            return;

        DungeonsWnd selectInstanceScript = dungeonsWndWnd.GetComponent<DungeonsWnd>();

        if (selectInstanceScript == null)
            return;

        // 绑定数据
        selectInstanceScript.Bind(string.Empty, argsData.GetValue<int>("soul_map"), pos);
    }
}

// 打界面指定分页
public class SCRIPT_22006 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue args = _params[1] as LPCValue;
        if (args == null || !args.IsMapping)
            return false;

        LPCMapping data = args.AsMapping;

        GameObject wnd = WindowMgr.OpenWnd(BaggageWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return false;

        BaggageWnd script = wnd.GetComponent<BaggageWnd>();
        if (script == null)
            return false;

        PetToolTipWnd petTool = script.mPetToolTipWnd.GetComponent<PetToolTipWnd>();
        if (petTool == null)
            return false;

        petTool.BindPage(data.GetValue<int>("page"));

        return true;
    }
}

// 打界面指定分页
public class SCRIPT_22007 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue args = _params[1] as LPCValue;
        if (args == null || !args.IsMapping)
            return false;

        // 隐藏主窗口
        WindowMgr.HideMainWnd();

        LPCMapping data = args.AsMapping;

        GameObject wnd = WindowMgr.OpenWnd(ArenaWnd.WndType);
        if (wnd == null)
            return false;

        ArenaWnd script = wnd.GetComponent<ArenaWnd>();
        if (script == null)
            return false;

        script.BindPage(data.GetValue<int>("page"));

        return true;
    }
}

// 打界面指定分页
public class SCRIPT_22008 : Script
{
    public override object Call(params object[] _params)
    {
        LPCValue args = _params[1] as LPCValue;
        if (args == null || !args.IsMapping)
            return false;

        LPCMapping data = args.AsMapping;

        GameObject wnd = WindowMgr.OpenWnd(MarketWnd.WndType);
        if (wnd == null)
            return false;

        MarketWnd script = wnd.GetComponent<MarketWnd>();
        if (script == null)
            return false;

        script.Bind(data.GetValue<int>("page"));

        return true;
    }
}

// 前往脚本
public class SCRIPT_22009 : Script
{
    public override object Call(params object[] _params)
    {

        // 参数1位user，参数2位arg(lpcvalue)
        // 返回结果为是否关闭任务界面
        //LPCMapping valueMap = (_params[1] as LPCValue).AsMapping;

        // 参数3为systemfunction数据
        //LPCMapping functionData = (_params[2] as LPCValue).AsMapping;

        // 参数4为botton的gamobject,参数5为是否按下

        Property user = _params[0] as Property;
        if (user == null)
            return false;

        LPCValue args = _params[1] as LPCValue;
        if (args == null || !args.IsMapping)
            return false;

        LPCMapping data = args.AsMapping;

        int mapId = data.GetValue<int>("map_id");

        GameObject item = _params[3] as GameObject;
        if (item != null)
            item.GetComponent<SystemFunctionItemWnd>().SetRedPoint(false, SystemFunctionConst.NEW_TIPS);

        LPCMapping functionData = _params[2] as LPCMapping;

        // 打开任务前往窗口
        GameObject wnd = WindowMgr.OpenWnd(TaskGotoWnd.WndType);
        if (wnd == null)
            return false;

        // 绑定数据
        wnd.GetComponent<TaskGotoWnd>().Bind(mapId, functionData.GetValue<int>("task_id"));

        LPCMapping temp = LPCMapping.Empty;

        int sysFuncId = functionData.GetValue<int>("id");

        LPCValue v = user.QueryTemp<LPCValue>("assign_task_sys_func");
        if (v != null && v.IsMapping)
        {
            temp = v.AsMapping;

            temp.Add(sysFuncId, 0);

            user.SetTemp("assign_task_sys_func", LPCValue.Create(temp));
        }

        return true;
    }
}

// 前往脚本
public class SCRIPT_22010 : Script
{
    public override object Call(params object[] _params)
    {

        // 参数1位user，参数2位arg(lpcvalue)
        // 返回结果为是否关闭任务界面
        //LPCMapping valueMap = (_params[1] as LPCValue).AsMapping;

        // 参数3为systemfunction数据
        //LPCMapping functionData = (_params[2] as LPCValue).AsMapping;

        // 参数4为botton的gamobject,参数5为是否按下

        Property user = _params[0] as Property;
        if (user == null)
            return false;

        LPCValue args = _params[1] as LPCValue;
        if (args == null || !args.IsMapping)
            return false;

        LPCMapping data = args.AsMapping;

        int mapId = data.GetValue<int>("map_id");

        GameObject item = _params[3] as GameObject;
        if (item != null)
            item.GetComponent<SystemFunctionItemWnd>().SetRedPoint(false, SystemFunctionConst.NEW_TIPS);

        LPCMapping functionData = _params[2] as LPCMapping;

        // 打开任务前往窗口
        GameObject wnd = WindowMgr.OpenWnd(LandaTaskGotoWnd.WndType);
        if (wnd == null)
            return false;

        // 绑定数据
        wnd.GetComponent<LandaTaskGotoWnd>().Bind(mapId, functionData.GetValue<int>("task_id"));

        LPCMapping temp = LPCMapping.Empty;

        int sysFuncId = functionData.GetValue<int>("id");

        LPCValue v = user.QueryTemp<LPCValue>("assign_task_sys_func");
        if (v != null && v.IsMapping)
        {
            temp = v.AsMapping;

            temp.Add(sysFuncId, 0);

            user.SetTemp("assign_task_sys_func", LPCValue.Create(temp));
        }

        return true;
    }
}

/// <summary>
/// 打开前往脚本
/// 能够开启多个界面
/// </summary>
public class SCRIPT_22011 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1位user，参数2位arg(lpcvalue)
        // 返回结果为是否关闭任务界面
        LPCMapping valueMap = (_params[1] as LPCValue).AsMapping;

        string loadScene = string.Empty;

        if (valueMap.ContainsKey("load_scene"))
        {
            loadScene = valueMap.GetValue<string>("load_scene");

            // 抛出切换地图事件
            SceneMgr.LoadScene("Main", loadScene, new CallBack(LoadSceneCallBack, valueMap));

            return true;
        }

        if (valueMap.ContainsKey("wnd"))
        {
            LPCArray wndArray = valueMap.GetValue<LPCArray>("wnd");

            foreach (var item in wndArray.Values)
                ShowWnd(item.AsMapping, null, WindowOpenGroup.MULTIPLE_OPEN_WND);
        }

        return false;
    }

    private void LoadSceneCallBack(object para, params object[] _param)
    {
        LPCMapping valueMap = para as LPCMapping;

        if (valueMap.ContainsKey("wnd"))
        {
            LPCArray wndArray = valueMap.GetValue<LPCArray>("wnd");

            foreach (var item in wndArray.Values)
            {
                ShowWnd(item.AsMapping, null, WindowOpenGroup.MULTIPLE_OPEN_WND);
            }
        }
    }

    private GameObject ShowWnd(LPCMapping valueMap, Transform parent = null, string group = "")
    {
        string wndName = string.Empty;

        if (valueMap.ContainsKey("wnd_name"))
            wndName = valueMap.GetValue<string>("wnd_name");
        else
            wndName = "MainWnd";

        if (string.IsNullOrEmpty(wndName))
            return null;

        GameObject wnd = WindowMgr.OpenWnd(wndName, parent, group);

        if (wnd == null)
            return null;

        WindowMgr.ShowWindow(wnd);

        return wnd;
    }
}

// 打开市集界面前往脚本
public class SCRIPT_22012 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1位user，参数2位arg(lpcvalue)
        // 返回结果为是否关闭任务界面
        LPCMapping valueMap = (_params[1] as LPCValue).AsMapping;

        // 获取需要打开窗口名
        GameObject wnd = WindowMgr.OpenWnd(valueMap.GetValue<string>("wnd_name"),
            null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return false;

        // 获取相机当前位置
        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control != null)
        {
            SceneMgr.SceneCameraFromPos = control.transform.localPosition;
            SceneMgr.SceneCameraToPos = new Vector3(3.29f, 0.64f, -16.28f);

            // 移动相机
            control.MoveCamera(control.transform.localPosition, SceneMgr.SceneCameraToPos);
        }

        return true;
    }
}

// 道具描述脚本
public class SCRIPT_23000 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 道具配置数据(CsvRow)

        // _params[1] 玩家对象

        CsvRow row = _params[0] as CsvRow;
        if (row == null)
            return string.Empty;

        Property user = _params[1] as Property;
        if (user == null)
            return string.Empty;

        int classId = row.Query<int>("class_id");

        CsvRow marketData = MarketMgr.GetMarketConfig(classId);

        if (marketData != null)
        {
            // 此钻石道具有开启首充显示
            if (marketData.Query<int>("show_first") == 1)
            {
                // 获取玩家都买次数信息
                LPCValue buyData = ME.user.Query<LPCValue>("limit_buy_data");
                LPCMapping limitBuyData;

                if (buyData != null && buyData.IsMapping)
                    limitBuyData = buyData.AsMapping;
                else
                    limitBuyData = LPCMapping.Empty;

                // 此钻石道具有首充双倍
                if (!limitBuyData.ContainsKey(classId) || limitBuyData[classId].AsMapping.GetValue<int>("amount") < 1)
                {
                    // 基础钻石
                    //LPCMapping applyArg = row.Query<LPCMapping>("apply_arg");

                    // 基础赠送
                    //LPCMapping givingGoods = marketData.Query<LPCMapping>("giving_goods");

                    // 首充赠送
                    //LPCMapping firstGivingGoods = marketData.Query<LPCMapping>("first_giving_goods");

                    //return string.Format(LocalizationMgr.Get(row.Query<string>("desc_args")), (applyArg.GetValue<int>("gold_coin") + givingGoods.GetValue<int>("gold_coin") + firstGivingGoods.GetValue<int>("gold_coin")));
                    return LocalizationMgr.Get(row.Query<string>("desc_args"));
                }
            }
        }

        return LocalizationMgr.Get(row.Query<string>("desc"));
    }
}

// 钻石道具描述脚本
public class SCRIPT_23001 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 道具配置数据(CsvRow)

        // _params[1] 玩家对象

        CsvRow row = _params[0] as CsvRow;
        if (row == null)
            return string.Empty;

        Property user = _params[1] as Property;
        if (user == null)
            return string.Empty;

        int classId = row.Query<int>("class_id");

        CsvRow marketData = MarketMgr.GetMarketConfig(classId);

        if (marketData != null)
        {
            // 此钻石道具有开启首充显示
            if (marketData.Query<int>("show_first") == 1)
            {
                // 获取玩家都买次数信息
                LPCValue buyData = ME.user.Query<LPCValue>("limit_buy_data");
                LPCMapping limitBuyData;

                if (buyData != null && buyData.IsMapping)
                    limitBuyData = buyData.AsMapping;
                else
                    limitBuyData = LPCMapping.Empty;

                // 此钻石道具有首充双倍
                if (!limitBuyData.ContainsKey(classId) || limitBuyData[classId].AsMapping.GetValue<int>("amount") < 1)
                {
                    // 基础钻石
                    //LPCMapping applyArg = row.Query<LPCMapping>("apply_arg");

                    // 基础赠送
                    //LPCMapping givingGoods = marketData.Query<LPCMapping>("giving_goods");

                    // 首充赠送
                    //LPCMapping firstGivingGoods = marketData.Query<LPCMapping>("first_giving_goods");

                    //return string.Format(LocalizationMgr.Get(row.Query<string>("desc_args")), (applyArg.GetValue<int>("gold_coin") + givingGoods.GetValue<int>("gold_coin") + firstGivingGoods.GetValue<int>("gold_coin")));
                    return LocalizationMgr.Get(row.Query<string>("desc_args"));
                }
            }
        }

        LPCMapping first_buy_data = LPCMapping.Empty;

        // 首次购买数据
        LPCValue v = user.Query<LPCValue>("first_buy_data");
        if (v != null && v.IsMapping)
            first_buy_data = v.AsMapping;

        int buyTime = first_buy_data.GetValue<int>(row.Query<int>("class_id"));

        if (Game.IsSameMonth(buyTime, TimeMgr.GetServerTime()))
            return LocalizationMgr.Get(row.Query<string>("desc"));

        return LocalizationMgr.Get("item_desc_50311_0");
    }
}

// 领取奖励检测脚本
public class SCRIPT_24000 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数1位user

        // 此处检测作包裹容量检测
        // 接口:BaggageMgr.TryStoreToBaggage(Property user, int page, int num);返回BOOL
        // page表示包裹页面
        // Num为需要多少个格子。
        // 注意属性道具是不需要存储空间的，只有装备才需要存储空间
        // 检测是否是属性道具ItemMgr.IsAttribItem(int class_id)

        return true;
    }
}


/// <summary>
/// 系统功能双倍活动图标显示判断脚本
/// </summary>
public class SCRIPT_25000 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象
        // _params[1] 显示判断参数
        // _params[2] 附加参数

        // 玩家对象
        Property user = _params[0] as Property;

        LPCMapping show_args = _params[1] as LPCMapping;

        LPCMapping dbase = _params[2] as LPCMapping;

        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        // 判断是不是活动带来的效果
        string activityId = dbase.GetValue<string>("activity_id");
        if (ActivityMgr.IsAcitvityValid(activityId))
            return true;

        // 双倍道具是否使用
        LPCValue v = user.Query<LPCValue>("double_bonus_data");
        if (v == null || !v.IsMapping)
            return false;

        foreach (LPCValue data in v.AsMapping.Values)
        {
            if (!data.IsMapping)
                continue;

            if (!data.AsMapping.ContainsKey("activity_id"))
                continue;

            if (data.AsMapping.GetValue<string>("activity_id").Equals(activityId))
                return true;
        }

        return false;
    }
}

/// <summary>
/// 魂石圣域全开活动显示判断脚本
/// </summary>
public class SCRIPT_25002 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping dbase = _params[2] as LPCMapping;

        LPCMapping show_args = _params[1] as LPCMapping;
        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        string activityId = dbase.GetValue<string>("activity_id");

        // 活动列表
        List<LPCMapping> activityList = ActivityMgr.GetActivityList();
        for (int i = 0; i < activityList.Count; i++)
        {
            // 活动数据
            LPCMapping activityData = activityList[i];
            if (activityData == null)
                continue;

            if (!activityData.ContainsKey("activity_id"))
                continue;

            if (activityId.Equals(activityData.GetValue<string>("activity_id")) && ActivityMgr.IsAcitvityValid(activityId))
                return true;
        }

        return false;
    }
}

/// <summary>
/// 通天之塔免费体力活动
/// </summary>
public class SCRIPT_25003 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping dbase = _params[2] as LPCMapping;

        LPCMapping show_args = _params[1] as LPCMapping;
        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        if (ME.user.Query<int>("new_function/tower") != 1)
            return false;

        string activityId = dbase.GetValue<string>("activity_id");

        // 活动列表
        List<LPCMapping> activityList = ActivityMgr.GetActivityList();
        for (int i = 0; i < activityList.Count; i++)
        {
            // 活动数据
            LPCMapping activityData = activityList[i];
            if (activityData == null)
                continue;

            if (!activityData.ContainsKey("activity_id"))
                continue;

            if (activityId.Equals(activityData.GetValue<string>("activity_id")) && ActivityMgr.IsAcitvityValid(activityId))
                return true;
        }

        return false;
    }
}

/// <summary>
/// 主线任务显示脚本
/// </summary>
public class SCRIPT_25004 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象
        Property user = _params[0] as Property;
        if (user == null)
            return false;

        LPCMapping task = TaskMgr.GetAssignTasks(user);
        if (task == null || task.Count == 0)
            return false;

        LPCMapping show_args = _params[1] as LPCMapping;
        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        return true;
    }
}

public class SCRIPT_25005 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping show_args = _params[1] as LPCMapping;
        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        return true;
    }
}

public class SCRIPT_25006 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象
        // _params[1] 显示判断参数
        // _params[2] 附加参数

        // 玩家对象
        Property user = _params[0] as Property;

        LPCMapping show_args = _params[1] as LPCMapping;

        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        // 活动全部已领取完成，才不显示
        List<CsvRow> bonusList = CommonBonusMgr.GetBonusList(CommonBonusMgr.LEVEL_BONUS);

        if (bonusList == null || bonusList.Count == 0)
            return true;

        for (int i = 0; i < bonusList.Count; i++)
        {
            int level = bonusList[i].Query<int>("id");

            if (!CommonBonusMgr.IsReceivedLevleBonus(user, level))
                return true;
        }

        return false;
    }
}

/// <summary>
/// 系统功能活动图标显示判断脚本
/// </summary>
public class SCRIPT_25001 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象
        // _params[1] 显示判断参数
        // _params[2] 附加参数

        LPCMapping show_args = _params[1] as LPCMapping;
        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        // 活动列表
        List<LPCMapping> activityList = ActivityMgr.GetShowActivityList(ActivityMgr.GetActivityList());

        if (activityList == null || activityList.Count == 0)
            return false;
        else
            return true;
    }
}

/// <summary>
/// 显示商城系统功能显示判断脚本
/// </summary>
public class SCRIPT_25007 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象
        // _params[1] 显示判断参数
        // _params[2] 附加参数

        LPCMapping show_args = _params[1] as LPCMapping;
        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        Property who = _params[0] as Property;
        if (who == null)
            return false;

        LPCArray dynamicGift = MarketMgr.GetLimitMarketList(who);
        if (dynamicGift == null)
            dynamicGift = LPCArray.Empty;

        foreach (LPCValue id in dynamicGift.Values)
        {
            // 过滤掉不需要显示的动态礼包
            if (MarketMgr.IsShow(who, id.AsInt))
                return true;
        }

        // 元素强化限时礼包列表
        LPCArray gift =  MarketMgr.GetLimitStrengthList(who);

        if (gift != null)
        {
            for (int i = 0; i < gift.Count; i++)
            {
                int classId = gift[i].AsMapping["class_id"].AsInt;

                // 过滤掉不需要显示的动态礼包
                if (MarketMgr.IsShow(who, classId))
                    return true;
            }
        }

        return false;
    }
}

/// <summary>
/// 装备卸载活动显示脚本
/// </summary>
public class SCRIPT_25008 : Script
{
    public override object Call(params object[] _params)
    {
        Property who = _params[0] as Property;
        if (who == null)
            return false;

        LPCMapping dbase = _params[2] as LPCMapping;

        LPCMapping show_args = _params[1] as LPCMapping;
        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        string activityId = dbase.GetValue<string>("activity_id");

        // 活动列表
        List<LPCMapping> activityList = ActivityMgr.GetActivityList();
        for (int i = 0; i < activityList.Count; i++)
        {
            // 活动数据
            LPCMapping activityData = activityList[i];
            if (activityData == null)
                continue;

            if (!activityData.ContainsKey("activity_id"))
                continue;

            if (activityId.Equals(activityData.GetValue<string>("activity_id")) && ActivityMgr.IsAcitvityValid(activityId))
                return true;
        }

        // 装备卸载卷数据
        LPCValue unequp_data = who.Query<LPCValue>("free_unequip_data");
        if (unequp_data != null &&
            unequp_data.IsMapping &&
            unequp_data.AsMapping.GetValue<int>("end_time") > TimeMgr.GetServerTime())
        {
            return true;
        }

        return false;
    }
}

/// <summary>
/// 成长手册显示判断脚本
/// </summary>
public class SCRIPT_25009 : Script
{
    static List<int> mTypeList = new List<int>()
    {
        TaskConst.GROWTH_TASK_LEVEL_STAR,
        TaskConst.GROWTH_TASK_SUIT_INTENSIFY,
        TaskConst.GROWTH_TASK_AWAKE,
        TaskConst.GROWTH_TASK_ARENA,
        TaskConst.GROWTH_TASK
    };

    public override object Call(params object[] _params)
    {
        Property who = _params[0] as Property;
        if (who == null)
            return false;

        LPCMapping show_args = _params[1] as LPCMapping;
        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        // 遍历全部成长任务
        foreach (int type in mTypeList)
        {
            // 获取任务
            List<int> taskList = TaskMgr.GetTasksByType(type);
            for (int i = 0; i < taskList.Count; i++)
            {
                // 任务还没有完成
                if (! TaskMgr.IsCompleted(who, taskList[i]))
                    return true;

                // 还有奖励没有领取
                if (! TaskMgr.isBounsReceived(who, taskList[i]))
                    return true;
            }
        }

        // 不需要显示
        return false;
    }
}

/// <summary>
/// 等级礼包显示判断脚本
/// </summary>
public class SCRIPT_25010 : Script
{
    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;
        if (user == null)
            return false;

        LPCMapping show_args = _params[1] as LPCMapping;

        //是否完成指定引导组，没有完成不显示
        if (!GuideMgr.IsGuided(show_args.GetValue<int>("group")))
            return false;

        LPCMapping levelMap = user.Query<LPCMapping>("level_gift");
        if (levelMap == null || !levelMap.ContainsKey("level") || !levelMap.ContainsKey("overdue_time"))
            return false;

        // 判断玩家等级是否达标
        if (levelMap.GetValue<int>("level") != show_args.GetValue<int>("level"))
            return false;

        //超出有效时间
        if (TimeMgr.GetServerTime() > levelMap.GetValue<int>("overdue_time"))
            return false;

        return true;
    }
}

/// <summary>
/// 系统功能获取活动数据
/// </summary>
public class SCRIPT_25050 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象
        // _params[1] 附加参数

        Property user = _params[0] as Property;
        if (user == null)
            return LPCMapping.Empty;

        LPCMapping dbase = _params[1] as LPCMapping;

        // 对应的活动id
        string activityId = dbase.GetValue<string>("activity_id");

        LPCMapping funcData = LPCMapping.Empty;

        // 活动有效，优先显示活动数据，否则显示道具的数据
        if (ActivityMgr.IsAcitvityValid(activityId))
        {
            // 返回活动的有效结束时间
            funcData.Add("end_time", ActivityMgr.GetAcitvityValidEndTime(activityId));
        }
        else
        {
            // 返回道具的结束时间
            LPCValue v = user.Query<LPCValue>("double_bonus_data");
            if (v == null || !v.IsMapping)
                return LPCMapping.Empty;

            LPCMapping data = v.AsMapping;

            foreach (LPCValue value in data.Values)
            {
                if (value == null || !value.IsMapping)
                    continue;

                LPCMapping map = value.AsMapping;

                if (map.GetValue<string>("activity_id") != activityId)
                    continue;

                funcData.Add("end_time", map.GetValue<int>("end_time"));

                // 结束循环
                break;
            }
        }

        return funcData;
    }
}

/// <summary>
/// 系统功能获取活动数据
/// </summary>
public class SCRIPT_25051 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping dbase = _params[1] as LPCMapping;

        LPCMapping funcData = LPCMapping.Empty;

        string activityId = dbase.GetValue<string>("activity_id");

        // 活动生效，获取有效结束时间
        if (ActivityMgr.IsAcitvityValid(activityId))
            funcData.Add("end_time", ActivityMgr.GetAcitvityValidEndTime(activityId));

        return funcData;
    }
}

/// <summary>
/// 系统功能获取活动数据
/// </summary>
public class SCRIPT_25052 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping dbase = _params[1] as LPCMapping;

        string activityId = dbase.GetValue<string>("activity_id");

        LPCMapping funcData = LPCMapping.Empty;

        // 活动生效，获取有效结束时间
        if (ActivityMgr.IsAcitvityValid(activityId))
            funcData.Add("end_time", ActivityMgr.GetAcitvityValidEndTime(activityId));

        return funcData;
    }
}

/// <summary>
/// 系统功能获取活动数据
/// </summary>
public class SCRIPT_25053 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象
        Property user = _params[0] as Property;
        //LPCMapping dbase = _params[1] as LPCMapping;

        LPCArray dataList = new LPCArray();

        // 取得玩家身上的主线任务
        LPCMapping assignTask = TaskMgr.GetAssignTasks(user);

        if (assignTask == null || assignTask.Count == 0)
            return dataList;

        List<int> taskList = new List<int>();

        foreach (int taskId in assignTask.Keys)
        {
            if (TaskMgr.GetTaskInfo(taskId) == null)
            {
                LogMgr.Trace("未找到该主线任务");
                continue;
            }

            taskList.Add(taskId);
        }

        // 对任务进行排序
        taskList.Sort();

        foreach (int taskId in taskList)
        {
            LPCMapping data = new LPCMapping();

            data.Add("task_id", taskId);

            dataList.Add(data);
        }

        return dataList;
    }
}

/// <summary>
/// 免费换装活动数据
/// </summary>
public class SCRIPT_25054 : Script
{
    public override object Call(params object[] _params)
    {
        // 玩家对象
        Property user = _params[0] as Property;
        if (user == null)
            return LPCMapping.Empty;

        LPCMapping dbase = _params[1] as LPCMapping;

        LPCMapping funcData = LPCMapping.Empty;

        string activityId = dbase.GetValue<string>("activity_id");

        // 活动生效，获取有效结束时间
        if (ActivityMgr.IsAcitvityValid(activityId))
            funcData.Add("end_time", ActivityMgr.GetAcitvityValidEndTime(activityId));

        return funcData;
    }
}

/// <summary>
/// 邮件提示信息判断脚本
/// </summary>
public class SCRIPT_25100 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象

        return MailMgr.HasUnReadExpress();
    }
}

/// <summary>
/// 活动提示信息判断脚本
/// </summary>
public class SCRIPT_25101 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象

        // 是否有新活动
        return ActivityMgr.mHasNewActivity;
    }
}

/// <summary>
/// 主线任务
/// </summary>
public class SCRIPT_25102 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象

        Property user = _params[0] as Property;
        if (user == null)
            return false;

        LPCMapping task = TaskMgr.GetAssignTasks(user);
        if (task == null || task.Count == 0)
            return false;

        LPCMapping data = _params[1] as LPCMapping;
        if (data == null)
            return false;

        LPCMapping temp = LPCMapping.Empty;

        int sysFuncId = data.GetValue<int>("id");

        LPCValue v = user.QueryTemp<LPCValue>("assign_task_sys_func");
        if (v != null && v.IsMapping)
        {
            temp = v.AsMapping;

            if (temp.ContainsKey(sysFuncId) && temp.GetValue<int>(sysFuncId) == 0)
                return false;
        }

        temp.Add(sysFuncId, 1);

        user.SetTemp("assign_task_sys_func", LPCValue.Create(temp));

        return true;
    }
}

/// <summary>
/// 等级奖励判断脚本
/// </summary>
public class SCRIPT_25103 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象
        Property user = _params[0] as Property;

        return CommonBonusMgr.HasReceivngLevelBonus(user);
    }
}

/// <summary>
/// 成长手册任务红点提示判断脚本
/// </summary>
public class SCRIPT_25104 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 玩家对象
        Property user = _params[0] as Property;

        if (TaskMgr.CheckHasBonus(user, TaskConst.GROWTH_TASK_LEVEL_STAR) ||
            TaskMgr.CheckHasBonus(user, TaskConst.GROWTH_TASK_SUIT_INTENSIFY) ||
            TaskMgr.CheckHasBonus(user, TaskConst.GROWTH_TASK_AWAKE) ||
            TaskMgr.CheckHasBonus(user, TaskConst.GROWTH_TASK_ARENA) ||
            TaskMgr.CheckHasBonus(user, TaskConst.GROWTH_TASK))
        {
            return true;
        }

        return false;
    }
}

/// <summary>
/// 邮件图标点击事件回调执行脚本
/// </summary>
public class SCRIPT_25150 : Script
{
    public override object Call(params object[] _params)
    {
        // 打开邮件窗口
        WindowMgr.OpenWnd("MailWnd");

        return true;
    }
}

/// <summary>
/// 双倍道具图标点击事件回调执行脚本
/// </summary>
public class SCRIPT_25151 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 系统功能数据
        // _params[1] 图标实体对象

        LPCMapping data = _params[0] as LPCMapping;

        GameObject go = _params[1] as GameObject;

        GameObject mainWnd = WindowMgr.GetWindow("MainWnd");
        if (mainWnd == null)
            return false;

        GameObject mDescViewWnd = mainWnd.GetComponent<MainWnd>().mRightDescViewWnd;

        DescViewWnd script = mDescViewWnd.GetComponent<DescViewWnd>();
        if (script == null)
            return false;

        script.ShowView(data, go);

        return true;
    }
}

/// <summary>
/// 活动图标点击事件回调执行脚本
/// </summary>
public class SCRIPT_25152 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 系统功能数据
        // _params[1] 图标实体对象

        // 打开活动界面, 关闭红点提示
        GameObject wnd = WindowMgr.OpenWnd(ActivityWnd.WndType);
        if (wnd == null)
            return false;

        GameObject go = _params[1] as GameObject;
        if (go != null)
        {
            SystemFunctionItemWnd script = go.GetComponent<SystemFunctionItemWnd>();

            script.SetEffect(false);
            script.SetRedPoint(false, SystemFunctionConst.RED_POINT);
        }

        // 活动已查看， 取消活动提示信息
        ActivityMgr.mHasNewActivity = false;

        return true;
    }
}

/// <summary>
/// 等级奖励图标点击事件回调执行脚本
/// </summary>
public class SCRIPT_25153 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 系统功能数据
        // _params[1] 图标实体对象

        // 打开等级奖励界面, 关闭红点提示
        GameObject wnd = WindowMgr.OpenWnd(LevelBonusWnd.WndType);
        if (wnd == null)
            return false;

        return true;
    }
}

// 限时商城图标点击执行脚本
public class SCRIPT_25154 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 系统功能数据
        // _params[1] 图标实体对象

        // 打开限时商城物品查看窗口
        GameObject wnd = WindowMgr.OpenWnd(LimitMarketView.WndType);
        if (wnd == null)
            return false;

        GameObject go = _params[1] as GameObject;

        // 绑定数据
        wnd.GetComponent<LimitMarketView>().Bind(go);

        return true;
    }
}

/// <summary>
/// 成长手册图标点击事件回调执行脚本
/// </summary>
public class SCRIPT_25155 : Script
{
    public override object Call(params object[] _params)
    {
        // 打开成长手册窗口
        WindowMgr.OpenWnd(GrowthManualWnd.WndType);

        return true;
    }
}

/// <summary>
/// 等级礼包图标点击事件回调执行脚本
/// </summary>
public class SCRIPT_25156 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 系统功能数据
        // _params[1] 图标实体对象

        LPCMapping csvInfo = (LPCMapping)_params[0];

        if (csvInfo == null)
            return false;

        GameObject wnd = WindowMgr.OpenWnd(LevelGiftWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return false;

        return true;
    }
}

// 元素强化限时礼包是否可购买判断脚本
public class SCRIPT_25198 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 第一个参数 玩家对象
        // _params[1] 第二个参数 商城道具购买参数

        Property who = _params[0] as Property;

        LPCMapping marketData = _params[1] as LPCMapping;

        int classId = marketData["class_id"].AsInt;

        // 强化礼包数据
        LPCArray intensifyGift = MarketMgr.GetLimitStrengthList(who);
        if (intensifyGift == null || intensifyGift.Count == 0)
            return LocalizationMgr.Get("MarketWnd_30");

        LPCMapping giftData = LPCMapping.Empty;

        for (int i = 0; i < intensifyGift.Count; i++)
        {
            LPCMapping giftMap = intensifyGift[i].AsMapping;
            if(giftMap["class_id"].AsInt == classId)
            {
                giftData = giftMap;
                break;
            }
        }

        // 没有该强化礼包的数据
        if(giftData.Count == 0)
            return LocalizationMgr.Get("MarketWnd_30");

        // 强化礼包开始时间
        int startTime = giftData["class_id"].AsInt;

        // 购买检测参数
        LPCMapping buyArgs = marketData["buy_args"].AsMapping;

        if (TimeMgr.GetServerTime() - startTime >= buyArgs["valid_time"].AsInt)
            return true;

        // 获取玩家都买次数信息
        LPCValue limitData = ME.user.Query<LPCValue>("limit_buy_data");
        LPCMapping limitBuyData;

        if (limitData != null && limitData.IsMapping)
            limitBuyData = limitData.AsMapping;
        else
            limitBuyData = LPCMapping.Empty;

        // 没有购买过，可以购买
        if (! limitBuyData.ContainsKey(classId))
            return true;

        // 判断购买买次数是否达到了上限
        LPCMapping buyData = limitBuyData[classId].AsMapping;
        if (buyData["amount"].AsInt < buyArgs["amount_limit"].AsInt)
            return true;

        return LocalizationMgr.Get("MarketWnd_28");
    }
}

// 道具是否可购买判断脚本
public class SCRIPT_25199 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 第一个参数 玩家对象
        // _params[1] 第二个参数 商城道具购买参数
        // _params[2] 第三个参数 商品位置

        Property who = _params[0] as Property;

        LPCMapping marketData = _params[1] as LPCMapping;

        // 无法购买
        if (who == null || marketData == null)
            return LocalizationMgr.Get("MarketWnd_28");

        // 尚未到购买时间，无法购买
        if (marketData.GetValue<string>("group") == ShopConfig.WEEKEND_GIFT_GROUP
            && !Game.IsWeekend(TimeMgr.GetServerTime()))
            return LocalizationMgr.Get("MarketWnd_29");

        return MarketMgr.IsLimitBuy(who, marketData);
    }
}

// 道具是否可购买判断脚本
public class SCRIPT_25200 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 第一个参数 玩家对象
        // _params[1] 第二个参数 商城道具购买参数
        // _params[2] 第三个参数 商品位置

        Property who = _params[0] as Property;

        LPCMapping marketData = _params[1] as LPCMapping;

        // 购买失败
        if (who == null || marketData == null)
            return LocalizationMgr.Get("MarketWnd_28");

        return MarketMgr.IsLimitBuy(who, marketData);
    }
}

/// <summary>
/// 积分活动点击脚本
/// </summary>
public class SCRIPT_25201 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");
        if (string.IsNullOrEmpty(wndName))
            return null;

        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.SendMessage("BindData", activityInfo);

        // 打开积分活动窗口
        return wnd;
    }
}

/// <summary>
/// 通用活动点击脚本
/// </summary>
public class SCRIPT_25202 : Script
{
    public override object Call(params object[] _params)
    {
        // 点击参数
        LPCMapping arg = _params[0] as LPCMapping;

        // 活动数据
        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<GeneralActivityWnd>().Bind(activityInfo);

        // 打开积分活动窗口
        return wnd;
    }
}

// 伽美斯·奎因的试炼活动点击脚本
public class SCRIPT_25203 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<TrialActivityWnd>().Bind(activityInfo);

        // 打开积分活动窗口
        return wnd;
    }
}

// 圣域活动点击脚本
public class SCRIPT_25204 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<SanctuaryActivityWnd>().Bind(activityInfo);

        return wnd;
    }
}

// 伽美斯·奎因的试炼活动点击脚本
public class SCRIPT_25205 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<LesenaDairyWnd>().Bind(activityInfo);

        // 打开积分活动窗口
        return wnd;
    }
}

/// <summary>
/// 进击宝箱怪的呼啦啦代币活动
/// </summary>
public class SCRIPT_25206 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<PetDungeonActivityWnd>().Bind(activityInfo);

        // 打开圣域精英活动窗口
        return wnd;
    }
}

/// <summary>
/// 小魔女的升星大挑战活动
/// </summary>
public class SCRIPT_25207 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<ChestTokenWnd>().Bind(activityInfo);

        // 打开圣域精英活动窗口
        return wnd;
    }
}

// 精英圣域点击脚本
public class SCRIPT_25208 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<StarUpChallengeWnd>().Bind(activityInfo);

        // 打开圣域精英活动窗口
        return wnd;
    }
}

// 月卡增幅点击脚本
public class SCRIPT_25209 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<MonthCardIncreaseWnd>().Bind(activityInfo);

        // 打开月卡增幅活动窗口
        return wnd;
    }
}

// 等级礼包道具是否可购买判断脚本
public class SCRIPT_25210 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 第一个参数 玩家对象
        // _params[1] 第二个参数 商城道具购买参数

        Property who = _params[0] as Property;
        LPCMapping marketData = _params[1] as LPCMapping;

        // 购买失败
        if (who == null || marketData == null)
            return LocalizationMgr.Get("MarketWnd_28");

        // 商品class_id
        int class_id = marketData.GetValue<int>("class_id");

        LPCMapping limitBuyData = LPCMapping.Empty;

        // 获取玩家购买的限购商品数据
        LPCValue v = who.Query<LPCValue>("limit_buy_data");
        if (v != null && v.IsMapping)
            limitBuyData = v.AsMapping;

        // 已经购买的数量
        int haveBuyAmount;

        // 没有该商品的限购数据
        if (!limitBuyData.ContainsKey(class_id))
            haveBuyAmount = 0;
        else
        {
            LPCMapping data = limitBuyData.GetValue<LPCMapping>(class_id);
            haveBuyAmount = data.GetValue<int>("amount");
        }

        LPCMapping buyArgs = marketData.GetValue<LPCMapping>("buy_args");

        // 限购次数
        int limitAmount = 0;
        if (buyArgs.ContainsKey("amount_limit"))
            limitAmount = buyArgs.GetValue<int>("amount_limit");

        if (haveBuyAmount >= limitAmount)
            return LocalizationMgr.Get("MarketWnd_20");

        return true;
    }
}

// 传说之契约任务点击脚本
public class SCRIPT_25211 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<LegendContractWnd>().Bind(activityInfo);

        // 打开传说之契约任务活动窗口
        return wnd;
    }
}

// 国庆七日活动开启点击脚本
public class SCRIPT_25212 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<SummaryActivityWnd>().Bind(activityInfo);

        return wnd;
    }
}

/// <summary>
/// 科学怪人的秘密特训活动
/// </summary>
public class SCRIPT_25213 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping arg = _params[0] as LPCMapping;

        LPCMapping activityInfo = _params[1] as LPCMapping;

        string wndName = arg.GetValue<string>("wnd_name");

        if (string.IsNullOrEmpty(wndName))
            return null;

        // 加载窗口预设
        GameObject wnd = WindowMgr.OpenWnd(wndName);
        if (wnd == null)
            return null;

        // 绑定数据
        wnd.GetComponent<FrankensteinTrainingWnd>().Bind(activityInfo);

        return wnd;
    }
}

/// <summary>
/// 活动是否生效检测脚本
/// </summary>
public class SCRIPT_25250 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;

        if (!args.ContainsKey("week_day"))
            return false;

        LPCArray weekDays = args.GetValue<LPCArray>("week_day");
        // 当前是周几
        int curWeekDay = Game.GetWeekDay(TimeMgr.GetServerTime());

        for (int i = 0; i < weekDays.Count; i++)
        {
            if (curWeekDay != weekDays[i].AsInt)
                continue;
            return true;
        }

        return false;
    }
}

/// <summary>
/// 活动是否生效检测脚本
/// </summary>
public class SCRIPT_25251 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;

        LPCMapping start_time_arg = args.GetValue<LPCMapping>("start_time");

        LPCMapping end_time_arg = args.GetValue<LPCMapping>("end_time");

        System.DateTime time = TimeMgr.ConvertIntDateTime(TimeMgr.GetServerTime());

        int secondTime = time.Hour * 3600 + time.Minute * 60 + time.Second;

        int start_time = start_time_arg["hour"].AsInt * 3600 + start_time_arg["min"].AsInt * 60;

        int end_time = end_time_arg["hour"].AsInt * 3600 + end_time_arg["min"].AsInt * 60;

        return (secondTime >= start_time) && (secondTime <= end_time);
    }
}

/// <summary>
/// 通天塔免体力活动图标是否显示脚本
/// </summary>
public class SCRIPT_25252 : Script
{
    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;
        //LPCValue args = _params[1] as LPCValue;
        LPCMapping activityData = _params[2] as LPCMapping;
        if (user.Query<int>("new_function/tower") != 1)
            return false;

        LPCMapping extraPara = activityData.GetValue<LPCMapping>("extra_para");
        if (extraPara == null)
            return true;

        // 检查是否需要隐藏
        if (extraPara.ContainsKey("hide"))
        {
            if (extraPara.GetValue<int>("hide") == 1)
                return false;
        }

        return true;
    }
}

/// <summary>
/// 通用活动图标是否显示脚本
/// </summary>
public class SCRIPT_25253 : Script
{
    public override object Call(params object[] _params)
    {
        //Property user = _params[0] as Property;
        //LPCValue args = _params[1] as LPCValue;
        LPCMapping activityData = _params[2] as LPCMapping;
        LPCMapping extraPara = activityData.GetValue<LPCMapping>("extra_para");
        if (extraPara == null)
            return true;

        // 检查是否需要隐藏
        if (extraPara.ContainsKey("hide"))
        {
            if (extraPara.GetValue<int>("hide") == 1)
                return false;
        }

        return true;
    }
}

/// <summary>
/// 道具显示判断脚本
/// </summary>
public class SCRIPT_25300 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] : 玩家对象(property)
        // _params[1] : 商品配置数据(LPCMapping)

        Property who = _params[0] as Property;
        if (who == null)
            return false;

        LPCMapping itemData = _params[1] as LPCMapping;
        if (itemData == null)
            return false;

        object ret = MarketMgr.IsLimitBuy(who, itemData);

        // 依赖的道具可以购买则不显示，反之则显示
        return ! (ret is string);
    }
}

/// <summary>
/// 限时礼包显示判断脚本
/// </summary>
public class SCRIPT_25301 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] : 玩家对象(property)
        // _params[1] : 商品配置数据(LPCMapping)
        Property who = _params[0] as Property;
        LPCMapping itemData = _params[1] as LPCMapping;
        System.DateTime time = TimeMgr.ConvertIntDateTime(TimeMgr.GetServerTime());
        LPCMapping buyArgs = itemData.GetValue<LPCMapping>("buy_args");
        LPCMapping itemDbase = itemData.GetValue<LPCMapping>("dbase");

        // 判断是否有时间段
        if (buyArgs.ContainsKey("buy_time_zone"))
        {
            LPCMapping butTimeZone = buyArgs.GetValue<LPCMapping>("buy_time_zone");
            int curHour = time.Hour;
            int startHour = butTimeZone.GetValue<int>("start");
            int endHour = butTimeZone.GetValue<int>("end");
            if (curHour < startHour || curHour >= endHour)
                return false;
        }

        LPCMapping limitBuyData = LPCMapping.Empty;
        LPCValue v = who.Query<LPCValue>("limit_buy_data");
        if (v != null && v.IsMapping)
        {
            int buyTime = 0;
            limitBuyData = v.AsMapping;

            int classId = itemData.GetValue<int>("class_id");

            // 分组类礼包判断
            foreach (int buyId in limitBuyData.Keys)
            {
                CsvRow itemRow = MarketMgr.GetMarketConfig(buyId);
                LPCMapping itemConfig = itemRow.Query<LPCMapping>("dbase");
                if (itemConfig.ContainsKey("type"))
                {
                    if (itemConfig.GetValue<int>("type") == itemDbase.GetValue<int>("type"))
                        return false;
                }
            }

            if (!limitBuyData.ContainsKey(classId))
                buyTime = 0;
            else
            {
                LPCMapping data = limitBuyData.GetValue<LPCMapping>(classId);
                buyTime = data.GetValue<int>("buy_time");
            }
            // 如果同一天买过了，则不显示
            if (Game.IsSameDay(TimeMgr.GetServerTime(), buyTime))
                return false;
        }

        return true;
    }
}

/// <summary>
/// 周末特惠礼包显示判断脚本
/// </summary>
public class SCRIPT_25302 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] : 玩家对象(property)
        // _params[1] : 商品配置数据(LPCMapping)
        Property who = _params[0] as Property;
        LPCMapping itemData = _params[1] as LPCMapping;

        LPCMapping limitBuyData = LPCMapping.Empty;
        LPCValue v = who.Query<LPCValue>("limit_buy_data");
        if (v != null && v.IsMapping)
        {
            int buyTime = 0;
            limitBuyData = v.AsMapping;

            int classId = itemData.GetValue<int>("class_id");

            if (!limitBuyData.ContainsKey(classId))
                buyTime = 0;
            else
            {
                LPCMapping data = limitBuyData.GetValue<LPCMapping>(classId);
                buyTime = data.GetValue<int>("buy_time");
            }
            // 如果同一天买过了，则不显示
            if (Game.IsSameWeek(TimeMgr.GetServerTime(), buyTime))
                return false;
        }

        return true;
    }
}

/// <summary>
/// 历程小标题显示脚本（通用）
/// </summary>
public class SCRIPT_25400 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];
        // 第一个参数为达成时间
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);
        // 如果没有信息，则显示未解锁问号
        if (courseData.Count == 0)
            return LocalizationMgr.Get("sub_title_arg_0");
        // 有记录则将时间显示出来
        return string.Format(LocalizationMgr.Get(descArg), TimeMgr.GetTimeFormat(courseData[0].AsInt, "yyyy-MM-dd HH:mm"));
    }
}

/// <summary>
/// 历程分页标题描述脚本（区分性别）
/// </summary>
public class SCRIPT_25401 : Script
{
    public override object Call(params object[] _params)
    {
        // who, descArg
        Property who = _params[0] as Property;
        string descArg = (string)_params[1];

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString);
    }
}

/// <summary>
/// 历程描述显示脚本（第一次登录）
/// </summary>
public class SCRIPT_25500 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);
        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");
        // 有记录则将描述显示出来
        int time = TimeMgr.GetServerTime() - courseData[0].AsInt;

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, time / 86400);
    }
}

/// <summary>
/// 历程描述显示脚本（满级）
/// </summary>
public class SCRIPT_25501 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 第一个参数为达到满级时间，第二参数总共花费时间
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 有记录则将描述显示出来
        return string.Format(LocalizationMgr.Get(descArg),
            GameSettingMgr.GetSettingInt("max_user_level"),
            courseData[1].AsInt / 86400);
    }
}

/// <summary>
/// 历程描述显示脚本（使魔升星）
/// </summary>
public class SCRIPT_25502 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 第一个参数为达到满级时间，元素为升星时间，第二个元素为使魔id, 第三个元素为使魔星级
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        int monsterId = courseData[1].AsInt;

        CsvRow monsterInfo = MonsterMgr.GetRow(monsterId);
        string elementName = MonsterConst.elementTypeMap[monsterInfo.Query<int>("element")];
        string monsterName = LocalizationMgr.Get(monsterInfo.Query<string>("name"));

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        // 有记录则将描述显示出来
        return string.Format(LocalizationMgr.Get(descArg), genderString, elementName, monsterName);
    }
}

/// <summary>
/// 历程描述显示脚本（召唤使魔）
/// </summary>
public class SCRIPT_25503 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 第一个参数为达到满级时间，元素为召唤时间，第二个元素为使魔id, 第三个元素为使魔星级，召唤道具的class_id
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 召唤道具
        int classId = courseData[3].AsInt;

        int monsterId = courseData[1].AsInt;

        CsvRow monsterInfo = MonsterMgr.GetRow(monsterId);
        string elementName = MonsterConst.elementTypeMap[monsterInfo.Query<int>("element")];
        string monsterName = LocalizationMgr.Get(monsterInfo.Query<string>("name"));

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        // 有记录则将描述显示出来
        return string.Format(LocalizationMgr.Get(descArg), genderString, ItemMgr.GetName(classId), elementName, monsterName, genderString);
    }
}

/// <summary>
/// 历程描述显示脚本（获得和消耗属性）
/// </summary>
public class SCRIPT_25504 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 数据格式： ({ 属性名, 获得属性 , 消耗属性 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        if (courseData.Count == 0)
            return string.Format(LocalizationMgr.Get(descArg),genderString, 0, 0);

        return string.Format(LocalizationMgr.Get(descArg),genderString, Game.BigNumFormat(courseData[1].AsString), Game.BigNumFormat(courseData[2].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（统计使魔图鉴）
/// </summary>
public class SCRIPT_25505 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 数据结构 ({完成图鉴的使魔数量， 使魔星级， 指定星级完成图鉴的使魔数量})
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        if (courseData.Count == 0)
            return string.Format(LocalizationMgr.Get(descArg), 0, 0);

        // 获得当前完成图鉴的使魔数量百分比
        int getPerNum = courseData[0].AsInt * 100 / ManualMgr.GetManualAmount();

        string perString = string.Empty;
        if (getPerNum == 0)
            perString = LocalizationMgr.Get("desc_arg_6_0");
        else
            perString = getPerNum.ToString();

        return string.Format(LocalizationMgr.Get(descArg), perString, courseData[2].AsInt);
    }
}

/// <summary>
/// 历程描述显示脚本（统计使魔升星）
/// </summary>
public class SCRIPT_25506 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 数据格式 ({ 使魔星级, 升星至指定星级的数量, 升星总次数 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        if (courseData.Count == 0)
            return string.Format(LocalizationMgr.Get(descArg), 0, 0);

        return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(courseData[1].AsString), Game.BigNumFormat(courseData[2].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（副本通关和升级）
/// </summary>
public class SCRIPT_25507 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 数据格式 ({ 副本通关次数, 使魔提升等级次数 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        if (courseData.Count == 0)
            return string.Format(LocalizationMgr.Get(descArg), 0, 0);

        return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(courseData[0].AsString), Game.BigNumFormat(courseData[1].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（指定圣域通关）
/// </summary>
public class SCRIPT_25508 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 1、record_condition 为 (["instance_id":"id"]) 时 ({ 解锁时间, 副本id, 通关最高记录, 对战列表 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 副本层数
        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(courseData[1].AsString);
        int layer = instanceInfo.GetValue<int>("layer");
        // 最速通关记录
        int fastRe = courseData[2].AsInt;
        string timeF = string.Empty;
        string timeM = string.Empty;
        string timeString = string.Empty;

        if (fastRe < 3600)
        {
            // 剩余多少分钟
            timeString = string.Format(LocalizationMgr.Get("DungeonsWnd_21"), fastRe / 60);
            int leftTime = fastRe - 60 * fastRe / 60;
            if (leftTime > 0)
            {
                timeM = string.Format(LocalizationMgr.Get("DungeonsWnd_22"), leftTime);
                timeString += timeM;
            }
        }
        // 剩余多少秒
        if (fastRe < 60)
            timeString = string.Format(LocalizationMgr.Get("DungeonsWnd_22"), fastRe);
        else
        {
            // 剩余多少小时
            timeString = string.Format(LocalizationMgr.Get("DungeonsWnd_20"), fastRe / 3600);
            int leftTimeS = fastRe - 3600 * fastRe / 3600;
            if (leftTimeS > 0)
            {
                timeF = string.Format(LocalizationMgr.Get("DungeonsWnd_21"), leftTimeS);
                timeString += timeF;
            }
            int leftTimeM = leftTimeS - 60 * leftTimeS / 60;
            if (leftTimeM > 0)
            {
                timeM = string.Format(LocalizationMgr.Get("DungeonsWnd_22"), leftTimeM);
                timeString += timeM;
            }
        }

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, layer, timeString);
    }
}

/// <summary>
/// 历程描述显示脚本（战斗次数）
/// </summary>
public class SCRIPT_25509 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 2、record_condition 为 (["map_id":({"id"})]) 时  ({ ([地图id: 战斗次数]) })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 获取所有战斗次数
        LPCMapping fightMap = courseData[0].AsMapping;
        string fightNum = string.Empty;
        foreach (int key in fightMap.Keys)
            fightNum = Game.BigNumAdd(fightNum, fightMap[key].AsString);

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, Game.BigNumFormat(fightNum));
    }
}

/// <summary>
/// 历程描述显示脚本（获得装备数量和道具数量）
/// </summary>
public class SCRIPT_25510 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 3、record_condition 为 ([""map_id_list"":({ 11, 18, 26 }), ""attrib"":""stone_mystery""]) 时 ({ 获得装备数量, 属性名称, 属性数量 });
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(courseData[0].AsString), Game.BigNumFormat(courseData[2].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（战斗次数和发现隐藏圣域）
/// </summary>
public class SCRIPT_25511 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 4、record_condition 为 ([""map_id_list"":({ 13, 14, 15, 16, 17 }), ""pet_id"":1]) 时
        // ({ 战斗次数, 发现隐藏圣域 });
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, Game.BigNumFormat(courseData[0].AsString), Game.BigNumFormat(courseData[1].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（指定类型道具数量和指定道具数量）
/// </summary>
public class SCRIPT_25512 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 5、record_condition 为 ([""map_id_list"":({ 13, 14, 15, 16, 17 }), ""item_type"":8, ""attrib"":""stone_mystery""]) 时
        // ({ 指定类型道具数量, 属性名称, 属性数量 });
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(courseData[0].AsString), Game.BigNumFormat(courseData[2].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（获得装备完成历程、强化装备完成历程）
/// </summary>
public class SCRIPT_25513 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 进度数据结构({ 达成条件时间 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString);
    }
}

/// <summary>
/// 历程描述显示脚本（获得最大属性通过什么途径）
/// </summary>
public class SCRIPT_25514 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 取最大值方式 : 进度数据结构({ 达成时间, 属性最大值 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, Game.BigNumFormat(courseData[1].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（装备满足条件装备达成历程）
/// </summary>
public class SCRIPT_25515 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        //({ 达成时间, 装备使魔class_id })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        int monsterId = courseData[1].AsInt;

        CsvRow monsterInfo = MonsterMgr.GetRow(monsterId);
        string elementName = MonsterConst.elementTypeMap[monsterInfo.Query<int>("element")];
        string monsterName = LocalizationMgr.Get(monsterInfo.Query<string>("name"));

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        // 有记录则将描述显示出来
        return string.Format(LocalizationMgr.Get(descArg), genderString, elementName, monsterName, genderString);
    }
}

/// <summary>
/// 历程描述显示脚本（获得总属性通过什么途径、消耗属性记录途径、强化装备失败次数记录）
/// </summary>
public class SCRIPT_25516 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];
        // 累加方式 : 进度数据结构({ 获得属性总数量 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, Game.BigNumFormat(courseData[0].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（获得总属性通过什么途径、消耗属性记录途径、强化装备失败次数记录）(没有性别选择查看版本)
/// </summary>
public class SCRIPT_25517 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];
        // 累加方式 : 进度数据结构({ 获得属性总数量 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(courseData[0].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（强化装备到指定等级完成历程、累加计算）
/// </summary>
public class SCRIPT_25518 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 累加次数强化次数方式 : 进度数据结构({ 累加次数强化次数 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(courseData[0].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（通天塔通关记录）
/// </summary>
public class SCRIPT_25519 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // 进度数据结构({ 达成条件时间, 通关层数, 花费时间 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 最速通关记录
        int fastRe = courseData[2].AsInt;
        string timeString = string.Empty;

        if (fastRe < 3600)
            timeString = LocalizationMgr.Get("desc_arg_33_1");

        // 花费时间
        timeString = string.Format(LocalizationMgr.Get("desc_arg_33_2"), fastRe / 3600);

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, courseData[1].AsInt + 1, timeString);
    }
}

/// <summary>
/// 历程描述显示脚本（通天塔胜利次数记录）
/// </summary>
public class SCRIPT_25520 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // ({ 普通通关次数, 困难通关次数 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, Game.BigNumFormat(Game.BigNumAdd(courseData[0].AsString, courseData[1].AsString)));
    }
}

/// <summary>
/// 历程描述显示脚本（通天塔指定层数以上通关次数记录）
/// </summary>
public class SCRIPT_25521 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // ({ 普通通关次数, 困难通关次数 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(courseData[0].AsString), Game.BigNumFormat(courseData[1].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（结算时达到竞技场段位）
/// </summary>
public class SCRIPT_25522 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // ({ 达成条件时间, 排名(或者段位) })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        string stepName = ArenaConst.arenaTopNameMap[courseData[1].AsInt];

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, stepName);
    }
}

/// <summary>
/// 历程描述显示脚本（通关全部npc副本）
/// </summary>
public class SCRIPT_25523 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // ({ 普通通关次数, 困难通关次数 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString);
    }
}

/// <summary>
/// 历程描述显示脚本（结算时达到竞技场排名）
/// </summary>
public class SCRIPT_25524 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // ({ 达成条件时间, 排名(或者段位) })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, Game.BigNumFormat(courseData[1].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（竞技场连胜类型）
/// </summary>
public class SCRIPT_25525 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // ({ 达成条件时间, 连胜次数 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(courseData[1].AsString), genderString);
    }
}

/// <summary>
/// 历程描述显示脚本（竞技场战斗，功勋统计）
/// </summary>
public class SCRIPT_25526 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // ({ 胜利次数, 功勋点数 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        // 1 男 2女
        int gender = who.Query<int>("gender");
        string genderString = LocalizationMgr.Get("gender_default");
        if (! who.GetRid().Equals(ME.user.GetRid()))
            genderString = LocalizationMgr.Get(CharConst.genderStringMap[gender]);

        return string.Format(LocalizationMgr.Get(descArg), genderString, Game.BigNumFormat(courseData[0].AsString), Game.BigNumFormat(courseData[1].AsString));
    }
}

/// <summary>
/// 历程描述显示脚本（增加竞技场防守统计）
/// </summary>
public class SCRIPT_25527 : Script
{
    public override object Call(params object[] _params)
    {
        // who, courseId, descArg
        Property who = _params[0] as Property;
        int courseId = (int)_params[1];
        string descArg = (string)_params[2];

        // ({ 总防守次数, 总防守成功次数 })
        LPCArray courseData = GameCourseMgr.GetCourseData(who, courseId);

        // 如果没有信息，则未解锁记录
        if (courseData.Count == 0)
            return LocalizationMgr.Get("desc_arg_0");

        long winTimes = 0;
        if (! long.TryParse(courseData[1].AsString, out winTimes))
            winTimes = 0;

        long totalWinTimes = 0;
        if (! long.TryParse(courseData[0].AsString, out totalWinTimes))
            totalWinTimes = 0;

        if (totalWinTimes == 0)
            return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(winTimes.ToString()), 0);
        else
        {
            if (100 * winTimes / totalWinTimes == 0)
                return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(winTimes.ToString()), LocalizationMgr.Get("desc_arg_6_0"));

            return string.Format(LocalizationMgr.Get(descArg), Game.BigNumFormat(winTimes.ToString()), 100 * winTimes / totalWinTimes);
        }
    }
}

/// <summary>
/// 占用包裹格子计算脚本
/// </summary>
public class SCRIPT_26000 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 道具class_id
        // _params[1] 邮件数据
        // _params[2] 脚本参数

        LPCMapping mailData = _params[1] as LPCMapping;

        LPCArray property_list = mailData.GetValue<LPCArray>("property_list");

        // TODO
        return property_list.Count;
    }
}

/// <summary>
/// 占用使魔包裹格子计算脚本
/// </summary>
public class SCRIPT_26001 : Script
{
    public override object Call(params object[] _params)
    {
        // _params[0] 道具class_id
        // _params[1] 邮件数据
        // _params[2] 脚本参数

        //LPCMapping mailData = _params[1] as LPCMapping;

        LPCMapping args = _params[2] as LPCMapping;

        // TODO
        return args.GetValue<int>("amount");
    }
}

/// <summary>
/// 商品格子点击脚本
/// </summary>
public class SCRIPT_27000 : Script
{
    public override object Call(params object[] _params)
    {
        // 商品数据
        LPCMapping itemData = _params[0] as LPCMapping;

        // 道具对象
        Property itemOb = _params[1] as Property;

        GameObject wnd = null;

        // 显示弹框
        if (!MonsterMgr.IsMonster(itemOb))
        {
            if (itemOb == null)
                return wnd;

            // 创建窗口
            wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return wnd;

            RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

            if (ItemMgr.IsItem(itemOb.GetClassID()))
            {
                script.SetPropData(itemOb, false, true, string.Empty, true);
                script.SetCallBack(new CallBack(OnClickConfirmBtn, itemData));
            }
            else
            {
                script.SetEquipData(itemOb, false, true, string.Empty, true);
                script.SetCallBack(new CallBack(OnClickConfirmBtn, itemData));
            }

            script.SetMask(true);
        }
        else
        {
            if (itemOb == null)
                return wnd;

            // 创建窗口
            wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            // 创建窗口失败
            if (wnd == null)
                return wnd;

            PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

            if (script == null)
                return wnd;

            script.ShowBtn(false, true, true, LocalizationMgr.Get("RewardPetInfoWnd_15"));
            script.Bind(itemOb);
            script.SetCallBack(new CallBack(OnClickConfirmBtn, itemData));
        }

        return wnd;
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(object para, params object[] _params)
    {
        if (!(bool)_params[0])
            return;

        LPCMapping itemData = para as LPCMapping;

        object result = MarketMgr.IsBuy(ME.user, itemData.GetValue<int>("class_id"));
        if (result is string)
        {
            // 无法购买
            DialogMgr.Notify((string) result);

            return;
        }

        // 购买商品
        MarketMgr.Buy(ME.user, itemData, 1);
    }
}

/// <summary>
/// 商品格子点击脚本
/// </summary>
public class SCRIPT_27001 : Script
{
    public override object Call(params object[] _params)
    {
//        // 商品数据
//        LPCMapping marketData = _params[0] as LPCMapping;

        // 道具对象
        Property itemOb = _params[1] as Property;

        GameObject wnd = null;

        if (itemOb == null)
            return wnd;

        // 创建窗口
        wnd = WindowMgr.OpenWnd(GiftInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return wnd;

        GiftInfoWnd script = wnd.GetComponent<GiftInfoWnd>();

        script.Bind(itemOb);

        return wnd;
    }
}

/// <summary>
/// 新手礼包点击脚本
/// </summary>
public class SCRIPT_27002 : Script
{
    public override object Call(params object[] _params)
    {
        // 商品数据
        LPCMapping itemData = _params[0] as LPCMapping;

        // 道具对象
        Property itemOb = _params[1] as Property;

        GameObject wnd = null;

        if (itemOb == null)
            return wnd;

        // 创建窗口
        wnd = WindowMgr.OpenWnd(NovicesGiftBagWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return wnd;

        NovicesGiftBagWnd script = wnd.GetComponent<NovicesGiftBagWnd>();

        string pos = string.Empty;
        if (itemData.ContainsKey("pos"))
            pos = itemData.GetValue<string>("pos");

        script.Bind(itemOb, pos);

        return wnd;
    }
}

/// <summary>
/// 召唤礼包点击脚本
/// </summary>
public class SCRIPT_27003 : Script
{
    public override object Call(params object[] _params)
    {
        // 商品数据
        LPCMapping itemData = _params[0] as LPCMapping;

        // 道具对象
        Property itemOb = _params[1] as Property;

        GameObject wnd = null;

        if (itemOb == null)
            return wnd;

        // 创建窗口
        wnd = WindowMgr.OpenWnd(SummonGiftBagWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return wnd;

        SummonGiftBagWnd script = wnd.GetComponent<SummonGiftBagWnd>();

        string pos = string.Empty;
        if (itemData.ContainsKey("pos"))
            pos = itemData.GetValue<string>("pos");

        script.Bind(itemOb, pos);

        return wnd;
    }
}

/// <summary>
/// 月卡礼包点击脚本
/// </summary>
public class SCRIPT_27004 : Script
{
    public override object Call(params object[] _params)
    {
        // 商品数据
       LPCMapping itemData = _params[0] as LPCMapping;

        // 道具对象
        Property itemOb = _params[1] as Property;

        GameObject wnd = null;

        if (itemOb == null)
            return wnd;

        // 创建窗口
        wnd = WindowMgr.OpenWnd(MonthCardWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return wnd;

        MonthCardWnd script = wnd.GetComponent<MonthCardWnd>();

        script.Bind(itemData.GetValue<int>("class_id"));

        return wnd;
    }
}

/// <summary>
/// 通用礼包点击脚本（人民币购买）
/// </summary>
public class SCRIPT_27005 : Script
{
    public override object Call(params object[] _params)
    {
        // 商品数据
        LPCMapping itemData = _params[0] as LPCMapping;

        // 道具对象
        Property itemOb = _params[1] as Property;

        GameObject wnd = null;

        if (itemOb == null)
            return wnd;

        // 创建窗口
        wnd = WindowMgr.OpenWnd(LimitGiftBagWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return wnd;

        LimitGiftBagWnd script = wnd.GetComponent<LimitGiftBagWnd>();

        // 绑定数据
        script.Bind(itemData.GetValue<int>("class_id"));

        return wnd;
    }
}

/// <summary>
/// 通用等级礼包点击脚本（人民币购买）
/// </summary>
public class SCRIPT_27006 : Script
{
    public override object Call(params object[] _params)
    {
        // 商品数据
        LPCMapping itemData = _params[0] as LPCMapping;

        GameObject wnd = null;

        if (itemData == null)
            return wnd;

        // 创建窗口
        wnd = WindowMgr.OpenWnd(LevelGiftWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return wnd;

        return wnd;
    }
}

/// <summary>
/// 通用周末特惠礼包点击脚本（人民币购买）
/// </summary>
public class SCRIPT_27007 : Script
{
    public override object Call(params object[] _params)
    {
        // 商品数据
        LPCMapping itemData = _params[0] as LPCMapping;

        GameObject wnd = null;

        if (itemData == null)
            return wnd;

        // 创建窗口
        wnd = WindowMgr.OpenWnd(WeekendGiftWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return wnd;

        WeekendGiftWnd script = wnd.GetComponent<WeekendGiftWnd>();

        // 绑定数据
        script.BindData(itemData.GetValue<int>("class_id"));

        return wnd;
    }
}

/// <summary>
/// 通用新手装备礼包点击脚本（人民币购买）
/// </summary>
public class SCRIPT_27008 : Script
{
    public override object Call(params object[] _params)
    {
        // 商品数据
        LPCMapping itemData = _params[0] as LPCMapping;

        GameObject wnd = null;

        if (itemData == null)
            return wnd;

        // 创建窗口
        wnd = WindowMgr.OpenWnd(NoobEquipGiftWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return wnd;

        NoobEquipGiftWnd script = wnd.GetComponent<NoobEquipGiftWnd>();

        // 绑定数据
        script.BindData(itemData.GetValue<int>("class_id"));

        return wnd;
    }
}

/// <summary>
/// 通用新手特惠礼包点击脚本（人民币购买）
/// </summary>
public class SCRIPT_27009 : Script
{
    public override object Call(params object[] _params)
    {
        // 商品数据
        LPCMapping itemData = _params[0] as LPCMapping;

        // 道具对象
        Property itemOb = _params[1] as Property;

        GameObject wnd = null;

        if (itemOb == null)
            return wnd;

        // 创建窗口
        wnd = WindowMgr.OpenWnd(NoobGiftBagWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return wnd;

        NoobGiftBagWnd script = wnd.GetComponent<NoobGiftBagWnd>();

        string pos = string.Empty;
        if (itemData.ContainsKey("pos"))
            pos = itemData.GetValue<string>("pos");

        script.BindData(itemOb, pos);

        return wnd;
    }
}

/// <summary>
/// 活动时间描述脚本
/// </summary>
public class SCRIPT_27500 : Script
{
    public override object Call(params object[] _params)
    {
        // 描述参数
        string arg = (_params[0] as LPCValue).AsString;
        if (string.IsNullOrEmpty(arg))
            return arg;

        // 有效时间列表
        LPCArray validList = _params[1] as LPCArray;
        if (validList == null)
            validList = LPCArray.Empty;

        List<string> valueList = new List<string>();

        for (int i = 0; i < validList.Count; i++)
        {
            LPCMapping data = validList[i].AsMapping;

            // 起始时间
            valueList.Add(TimeMgr.GetTimeFormat(data.GetValue<int>("start"), "HH:mm"));
            // 结束时间
            LPCMapping detailTime = TimeMgr.GetTimeDetail(data.GetValue<int>("end"), true);
            string endTimeString = string.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}",
                detailTime["year"].AsInt,
                detailTime["month"].AsInt,
                detailTime["day"].AsInt,
                detailTime["hour"].AsInt,
                detailTime["minute"].AsInt,
                detailTime["Second"].AsInt
            );
            valueList.Add(endTimeString);
        }

        // 分配数组长度
        string[] value = new string[valueList.Count];

        for (int i = 0; i < valueList.Count; i++)
            value[i] = valueList[i];

        try
        {
            // 返回时间描述
            return string.Format(LocalizationMgr.Get(arg), value);
        }
        catch (System.Exception ex)
        {
            LogMgr.Trace(ex.Message);
        }

        return string.Empty;
    }
}

/// <summary>
/// 活动时间描述脚本
/// </summary>
public class SCRIPT_27501 : Script
{
    public override object Call(params object[] _params)
    {
        // 描述参数
        string arg = (_params[0] as LPCValue).AsString;
        if (string.IsNullOrEmpty(arg))
            return arg;

        // 有效时间列表
        LPCArray validList = _params[1] as LPCArray;
        if (validList == null)
            validList = LPCArray.Empty;

        List<string> valueList = new List<string>();

        for (int i = 0; i < validList.Count; i++)
        {
            LPCMapping data = validList[i].AsMapping;

            // 开始时间
            int start = data.GetValue<int>("start");

            // 结束时间
            int end = data.GetValue<int>("end");

            valueList.Add(Game.GetWeekDayToChinese(start));
            valueList.Add(TimeMgr.GetTimeFormat(start, "HH:mm"));
            valueList.Add(Game.GetWeekDayToChinese(end));
            valueList.Add(TimeMgr.GetTimeFormat(end, "HH:mm"));
        }

        // 分配数组长度
        string[] value = new string[valueList.Count];

        for (int i = 0; i < valueList.Count; i++)
            value[i] = valueList[i];

        try
        {
            // 返回时间描述
            return string.Format(LocalizationMgr.Get(arg), value);
        }
        catch (System.Exception ex)
        {
            LogMgr.Trace(ex.Message);
        }

        return string.Empty;
    }
}

/// <summary>
/// 活动时间描述脚本
/// </summary>
public class SCRIPT_27502 : Script
{
    public override object Call(params object[] _params)
    {
        // 描述参数
        string arg = (_params[0] as LPCValue).AsString;
        if (string.IsNullOrEmpty(arg))
            return arg;

        // 有效时间列表
        LPCArray validList = _params[1] as LPCArray;
        if (validList == null)
            validList = LPCArray.Empty;

        List<string> valueList = new List<string>();

        for (int i = 0; i < validList.Count; i++)
        {
            LPCMapping data = validList[i].AsMapping;

            // 开始时间
            int start = data.GetValue<int>("start");

            // 结束时间
            int end = data.GetValue<int>("end");

            valueList.Add(Game.GetWeekDayToChinese(start));
            valueList.Add(TimeMgr.GetTimeFormat(start, "HH:mm"));
            valueList.Add(TimeMgr.GetTimeFormat(end, "HH:mm"));
        }

        // 分配数组长度
        string[] value = new string[valueList.Count];

        for (int i = 0; i < valueList.Count; i++)
            value[i] = valueList[i];

        try
        {
            // 返回时间描述
            return string.Format(LocalizationMgr.Get(arg), value);
        }
        catch (System.Exception ex)
        {
            LogMgr.Trace(ex.Message);
        }

        return string.Empty;
    }
}

/// <summary>
/// 活动时间描述脚本
/// </summary>
public class SCRIPT_27503 : Script
{
    public override object Call(params object[] _params)
    {
        // 描述参数
        string arg = (_params[0] as LPCValue).AsString;
        if (string.IsNullOrEmpty(arg))
            return arg;

        // 有效时间列表
        LPCArray validList = _params[1] as LPCArray;
        if (validList == null)
            validList = LPCArray.Empty;

        List<string> valueList = new List<string>();

        for (int i = 0; i < validList.Count; i++)
        {
            LPCMapping data = validList[i].AsMapping;

            // 开始时间
            int start = data.GetValue<int>("start");

            // 结束时间
            int end = data.GetValue<int>("end");

            // 起始时间
            valueList.Add(TimeMgr.GetTimeFormat(start, "yyyy-MM-dd HH:mm:ss"));
            // 结束时间
            LPCMapping detailTime = TimeMgr.GetTimeDetail(end, true);
            string endTimeString = string.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}",
                detailTime["year"].AsInt,
                detailTime["month"].AsInt,
                detailTime["day"].AsInt,
                detailTime["hour"].AsInt,
                detailTime["minute"].AsInt,
                detailTime["Second"].AsInt
            );
            valueList.Add(endTimeString);
        }

        // 分配数组长度
        string[] value = new string[valueList.Count];

        for (int i = 0; i < valueList.Count; i++)
            value[i] = valueList[i];

        try
        {
            // 返回时间描述
            return string.Format(LocalizationMgr.Get(arg), value);
        }
        catch (System.Exception ex)
        {
            LogMgr.Trace(ex.Message);
        }

        return string.Empty;
    }
}

/// <summary>
/// 活动奖励领取提示脚本
/// </summary>
public class SCRIPT_27504 : Script
{
    public override object Call(params object[] _params)
    {
        // 描述参数
        LPCMapping activityData = _params[1] as LPCMapping;

        int closeTime = activityData.GetValue<int>("close_time");

        //如果没有关闭时间
        if (closeTime == -1)
            return string.Empty;

        // 构建奖励领取提示信息
        return string.Format(LocalizationMgr.Get("TrialActivityWnd_25"),
                TimeMgr.GetTimeFormat(closeTime, "yyyy.MM.dd HH:mm:ss"));
    }
}

/// <summary>
/// 提示列表活动时间描述脚本
/// </summary>
public class SCRIPT_27505 : Script
{
    public override object Call(params object[] _params)
    {
        // 描述参数
        //LPCMapping activityInfo = _params[0] as LPCMapping;
        LPCMapping item = _params[1] as LPCMapping;

        // 有效时间列表
        int startTime = item.GetValue<int>("start");
        int endTime = item.GetValue<int>("end");

        string finalString = string.Format("20\n{0}~{1}",
            TimeMgr.GetTimeFormat(startTime, "HH:mm"),
            TimeMgr.GetTimeFormat(endTime, "HH:mm"));

        return finalString;
    }
}

/// <summary>
/// 活动时间描述脚本（通用多时段）
/// </summary>
public class SCRIPT_27506 : Script
{
    public override object Call(params object[] _params)
    {
        // 描述参数
        LPCArray arg = (_params[0] as LPCValue).AsArray;
        string dayDescArg = arg[0].AsString;
        string timeDescArg = arg[1].AsString;
        // 有效时间列表
        LPCArray validList = _params[1] as LPCArray;
        if (validList == null)
            validList = LPCArray.Empty;

        LPCMapping timeData = validList[0].AsMapping;
        int startDay = timeData.GetValue<int>("start");
        int endDay = timeData.GetValue<int>("end");
        string dayDesc = string.Empty;

        // 起始、结束天描述
        string dayStartDesc = TimeMgr.GetTimeFormat(startDay, "yyyy-MM-dd");
        string dayEndDesc = TimeMgr.GetTimeFormat(endDay, "yyyy-MM-dd");

        // 获取活动时间天数描述
        if (startDay == endDay)
            dayDesc = string.Format(LocalizationMgr.Get("single_day_desc"), dayStartDesc);
        else
            dayDesc = string.Format(LocalizationMgr.Get(dayDescArg), dayStartDesc, dayEndDesc);

        string timeDesc = string.Empty;
        // 获取每天具体时间段描述
        for (int i = 0; i < validList.Count; i++)
        {
            LPCMapping data = validList[i].AsMapping;
            // 开始、结束时间
            int start = data.GetValue<int>("start");
            int end = data.GetValue<int>("end");
            // 开始、结束时间描述
            string timeStartDesc = TimeMgr.GetTimeFormat(start, "HH:mm");
            LPCMapping detailTime = TimeMgr.GetTimeDetail(end, true);
            string timeEndDesc = string.Format("{0:D2}:{1:D2}",
                detailTime["hour"].AsInt,
                detailTime["minute"].AsInt
            );

            timeDesc += string.Format(LocalizationMgr.Get(timeDescArg), timeStartDesc, timeEndDesc);
        }

        // 返回时间描述
        return dayDesc + timeDesc;
    }
}

/// <summary>
/// 副标题描述脚本
/// </summary>
public class SCRIPT_27600 : Script
{
    public override object Call(params object[] _params)
    {
        // sub_title_arg
        string arg = (string) _params[0];

        // 附加参数
        LPCMapping extraPara = _params[1] as LPCMapping;

        // 使魔id
        int classId = extraPara.GetValue<int>("pet_id");

        int element = MonsterMgr.GetElement(classId);

        string color = MonsterConst.MonsterElementColorMap[element];

        // 没有该使魔的配置数据
        CsvRow row = MonsterMgr.GetRow(classId);
        if (row == null)
            return string.Empty;

        // 返回描述信息
        return string.Format(arg,
            color,
            PetMgr.GetElementName(element),
            row.Query<int>("star"),
            color,
            MonsterMgr.GetName(classId, row.Query<int>("rank"))
        );
    }
}

/// <summary>
/// 标题描述脚本
/// </summary>
public class SCRIPT_27700 : Script
{
    public override object Call(params object[] _params)
    {
        // title_arg
        string arg = (string) _params[0];

        // 附加参数
        LPCMapping extraPara = _params[1] as LPCMapping;

        // 使魔id
        int classId = extraPara.GetValue<int>("pet_id");

        // 没有该使魔的配置数据
        CsvRow row = MonsterMgr.GetRow(classId);
        if (row == null)
            return string.Empty;

        // 返回描述信息
        return string.Format(arg, MonsterConst.MonsterElementColorMap[MonsterMgr.GetElement(classId)]);
    }
}

/// <summary>
/// 雷丝那研究员任务描述脚本
/// </summary>
public class SCRIPT_28001 : Script
{
    public override object Call(params object[] _params)
    {
        // 当前进度值
        Property user = _params[0] as Property;
        string activityCookie = (string) _params[1];
        int taskId = (int) _params[2];
        string desc = _params[3] as string;
        LPCMapping completeCondition = _params[4] as LPCMapping;

        // 限制当前值为最大值
        int maxValue = completeCondition.GetValue<int>("amount");
        int curValue = ActivityMgr.GetActivityTaskProgress(user, activityCookie, taskId);
        curValue = Mathf.Clamp(curValue, curValue, maxValue);

        // 显示绿色
        if (curValue >= maxValue)
        {
            desc = desc.Insert(desc.IndexOf("{"), "[33FF00FF]");
            desc = desc.Insert(desc.LastIndexOf("}")  + 1, "[-]");
        }
        // 显示红色
        else
        {
            desc = desc.Insert(desc.IndexOf("{"), "[FF0000FF]");
            desc = desc.Insert(desc.LastIndexOf("}")  + 1, "[-]");
        }

        // 替换当前值
        desc = desc.Replace("{0}", curValue.ToString());

        // 替换最大值
        desc = desc.Replace("{1}", maxValue.ToString());

        return  desc;
    }
}

/// <summary>
/// 呼啦啦代币活动的子任务描述脚本（有完成进度显示的类型）
/// </summary>
public class SCRIPT_28002 : Script
{
    public override object Call(params object[] _params)
    {
        // 当前进度值
        Property user = _params[0] as Property;
        string activityCookie = (string) _params[1];
        int taskId = (int) _params[2];
        string desc = _params[3] as string;
        //LPCMapping completeCondition = _params[4] as LPCMapping;

        CsvRow taskInfo = ActivityMgr.GetActivityTaskInfo(taskId);

        // 限制当前值为最大值
        int maxValue = taskInfo.Query<int>("complate_times_limit");
        int curValue = ActivityMgr.GetCompletedActivityTaskTimes(user, activityCookie, taskId);
        curValue = Mathf.Clamp(curValue, curValue, maxValue);

        // 显示红色
        if (curValue >= maxValue)
        {
            desc = desc.Insert(desc.IndexOf("{"), "[FF0000FF]");
            desc = desc.Insert(desc.LastIndexOf("}")  + 1, "[-]");
        }
        // 显示绿色
        else
        {
            desc = desc.Insert(desc.IndexOf("{"), "[33FF00FF]");
            desc = desc.Insert(desc.LastIndexOf("}")  + 1, "[-]");
        }

        // 替换当前值
        desc = desc.Replace("{0}", curValue.ToString());

        // 替换最大值
        desc = desc.Replace("{1}", maxValue.ToString());

        return  desc;
    }
}

/// <summary>
/// 新功能icon显示
/// </summary>
public class SCRIPT_28500 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping map = _params[1] as LPCMapping;

        return map.GetValue<string>("show_icon_args");
    }
}

/// <summary>
/// 登入礼包提示排序脚本
/// </summary>
public class SCRIPT_28600 : Script
{
    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;

        LPCMapping row = _params[1] as LPCMapping;

        LPCMapping loginTipsSortArgs = row.GetValue<LPCMapping>("tips_sort_args");

        LPCMapping result = LPCMapping.Empty;

        // 是否需要进行等级限制判断
        if (loginTipsSortArgs.ContainsKey("limit_lv"))
        {
            // 玩家等级超过限定等级则不添加排序显示
            if (user.Query<int>("level") > loginTipsSortArgs.GetValue<int>("limit_lv"))
                return LPCMapping.Empty;
        }

        result.Add("sort_id", loginTipsSortArgs.GetValue<int>("sort_id"));

        return result;
    }
}

/// <summary>
/// 登入等级礼包提示排序脚本
/// </summary>
public class SCRIPT_28601 : Script
{
    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;

        LPCMapping row = _params[1] as LPCMapping;

        LPCMapping loginTipsSortArgs = row.GetValue<LPCMapping>("tips_sort_args");

        LPCMapping dbase = row.GetValue<LPCMapping>("dbase");

        LPCMapping levelMap = user.Query<LPCMapping>("level_gift");

        if (levelMap == null || !levelMap.ContainsKey("level") || !levelMap.ContainsKey("overdue_time"))
            return LPCMapping.Empty;

        // 判断玩家等级是否达标
        if (levelMap.GetValue<int>("level") != dbase.GetValue<int>("level"))
            return LPCMapping.Empty;

        int startValidTime = loginTipsSortArgs.GetValue<int>("start_valid_time");
        int endValidTime = loginTipsSortArgs.GetValue<int>("end_valid_time");

        int overdueTime = levelMap.GetValue<int>("overdue_time");
        int startTime = overdueTime - dbase.GetValue<int>("valid_time");

        int curTime = TimeMgr.GetServerTime();

        LPCMapping result = LPCMapping.Empty;

        //有效时间
        if ((curTime > startTime && curTime < (startTime + startValidTime)) || (curTime > (overdueTime - endValidTime) && curTime < overdueTime))
            result.Add("extra_sort_id", loginTipsSortArgs.GetValue<int>("extra_sort_id"));
        else
            result.Add("sort_id", loginTipsSortArgs.GetValue<int>("sort_id"));

        return result;
    }
}

/// <summary>
/// 登入提示筛选脚本
/// </summary>
public class SCRIPT_28698 : Script
{
    public override object Call(params object[] _params)
    {
        //Property user = _params[0] as Property;

        //LPCMapping row = _params[1] as LPCMapping;

        return true;
    }
}

/// <summary>
/// 登入提示筛选脚本
/// </summary>
public class SCRIPT_28699 : Script
{
    public override object Call(params object[] _params)
    {
        //Property user = _params[0] as Property;

        //LPCMapping row = _params[1] as LPCMapping;

        return false;
    }
}

/// <summary>
/// 登入提示筛选脚本
/// 每日礼包、周末特惠礼包
/// </summary>
public class SCRIPT_28700 : Script
{
    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;

        LPCMapping row = _params[1] as LPCMapping;

        // 是否可以购买
        if (MarketMgr.IsBuy(user, row.GetValue<int>("class_id")) is string)
            return false;

        LPCMapping dbase = row.GetValue<LPCMapping>("dbase");

        // 引导筛选
        if (!GuideMgr.IsGuided(dbase.GetValue<int>("group")))
            return false;

        // 是否显示
        if (!MarketMgr.IsShow(user, row.GetValue<int>("class_id")))
            return false;

        // 动态礼包不存在
        LPCArray dynamicGift = MarketMgr.GetLimitMarketList(user);
        if (dynamicGift == null)
            return false;

        // 没有礼包购买数据
        if (dynamicGift.IndexOf(row.GetValue<int>("class_id")) < 0)
            return false;

        return true;
    }
}

/// <summary>
/// 登入提示筛选脚本
/// 新手超惠礼包、特惠召唤礼包
/// </summary>
public class SCRIPT_28701 : Script
{
    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;

        LPCMapping row = _params[1] as LPCMapping;

        // 是否可以购买
        if (MarketMgr.IsBuy(user, row.GetValue<int>("class_id")) is string)
            return false;

        // 是否显示
        if (!MarketMgr.IsShow(user, row.GetValue<int>("class_id")))
            return false;

        return true;
    }
}

/// <summary>
/// 登入提示筛选脚本
/// 等级礼包
/// </summary>
public class SCRIPT_28702 : Script
{
    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;

        LPCMapping row = _params[1] as LPCMapping;

        LPCMapping levelMap = user.Query<LPCMapping>("level_gift");

        if (levelMap == null || !levelMap.ContainsKey("level") || !levelMap.ContainsKey("overdue_time"))
            return false;

        LPCMapping dbase = row.GetValue<LPCMapping>("dbase");

        // 判断玩家等级是否达标
        if (levelMap.GetValue<int>("level") != dbase.GetValue<int>("level"))
            return false;

        // 引导筛选
        if (!GuideMgr.IsGuided(dbase.GetValue<int>("group")))
            return false;

        // 同一种等级礼包，只记录一个，可以进入同一个界面
        if (dbase.GetValue<int>("can_show") == 0)
            return false;

        //超出有效时间
        if (TimeMgr.GetServerTime() > levelMap.GetValue<int>("overdue_time"))
            return false;

        // 是否显示
        if (!MarketMgr.IsShow(user, row.GetValue<int>("class_id")))
            return false;

        return true;
    }
}

/// <summary>
/// 登入提示筛选脚本
/// 新手装备礼包
/// </summary>
public class SCRIPT_28703 : Script
{
    public override object Call(params object[] _params)
    {
        Property user = _params[0] as Property;

        LPCMapping row = _params[1] as LPCMapping;

        // 是否可以购买
        if (MarketMgr.IsBuy(user, row.GetValue<int>("class_id")) is string)
            return false;

        // 是否显示
        if (!MarketMgr.IsShow(user, row.GetValue<int>("class_id")))
            return false;

        // 玩家等级检测
        if (user.Query<int>("level") >= 30)
            return false;

        return true;
    }
}

/// <summary>
/// 登入提示获取动画位置脚本
/// 主界面右边菜单位置（限时商城/）
/// </summary>
public class SCRIPT_28750 : Script
{
    public override object Call(params object[] _params)
    {
        //Property user = _params[0] as Property;

        LPCMapping row = _params[1] as LPCMapping;

        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);

        if (mainWnd == null)
            return null;

        LPCMapping tipsPosArgs = row.GetValue<LPCMapping>("tips_menu_args");

        Transform tran = mainWnd.GetComponent<MainWnd>().GetRightFunctionMenu(tipsPosArgs.GetValue<int>("function_id"));

        return tran;
    }
}

/// <summary>
/// 登入提示获取动画位置脚本
/// 主界面商店位置
/// </summary>
public class SCRIPT_28751 : Script
{
    public override object Call(params object[] _params)
    {
        //Property user = _params[0] as Property;

        //LPCMapping row = _params[1] as LPCMapping;

        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);

        if (mainWnd == null)
            return null;

        Transform tran = mainWnd.GetComponent<MainWnd>().GetShopMenu();

        return tran;
    }
}

/// <summary>
/// 登入提示获取动画位置脚本
/// 主界面右边菜单位置（等级礼包）
/// </summary>
public class SCRIPT_28752 : Script
{
    public override object Call(params object[] _params)
    {
        //Property user = _params[0] as Property;

        LPCMapping row = _params[1] as LPCMapping;

        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);

        if (mainWnd == null)
            return null;

        LPCMapping dbase = row.GetValue<LPCMapping>("dbase");

        LPCMapping functionConfig = SystemFunctionMgr.GetLevelGiftConfigByLevel(dbase.GetValue<int>("level"));

        if (functionConfig == null)
            return null;

        Transform tran = mainWnd.GetComponent<MainWnd>().GetRightFunctionMenu(functionConfig.GetValue<int>("id"));

        return tran;
    }
}




