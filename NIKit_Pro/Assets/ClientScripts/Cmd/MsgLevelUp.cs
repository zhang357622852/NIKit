using LPC;
using System.Diagnostics;
using System.Collections.Generic;
using QCCommonSDK.Addition;

/// <summary>
/// 服务器通知客户升级事件
/// </summary>
/// <author>weism</author>
public class MsgLevelUp : MsgHandler
{
    public string GetName()
    {
        return "msg_level_up";
    }

    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 查找对象
        Property ob = Rid.FindObjectByRid(args.GetValue<string>("rid"));
        if (ob == null)
            return;

        if (ob.IsMonster())
            MonsterMgr.DoUpgrade(ob);
        else if (ob.IsUser())
        {
            // 执行升级效果
            UserMgr.DoUpgrade(ob);

#if ! UNITY_EDITOR

            // 生成渠道account
            string channelAccount = string.Format("{0}{1}",
                QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL"),
                Communicate.AccountInfo.Query("account", ""));

            // 注册SDK OnLevelUpOK
            QCCommonSDK.Addition.DataAnalyzeSupport.RecordEvent("levelup", new Dictionary<string, string>() {
                {"account", channelAccount},
                {"role", ME.user.GetRid()},
                {"role_name", ME.user.GetName()},
                {"server", Communicate.AccountInfo.Query("server_id", "")},
                {"grade", ME.user.GetLevel().ToString()},
                {"gold_coin", ME.user.Query<int>("gold_coin").ToString()},
                {"money", ME.user.Query<int>("money").ToString()},
                {"create_time",  ME.user.Query<int>("create_time").ToString()}
            });

#endif

            // 显示升级效果窗口
            UserMgr.ShowUpgradeEffectWnd(args.GetValue<int>("pre_level"));
        }
        else
        {
            // 位置类型
        }
    }
}
