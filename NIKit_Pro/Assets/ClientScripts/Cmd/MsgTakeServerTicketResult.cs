using System;
using LPC;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// 获取登录服务器门票结果
/// </summary>
public class MsgTakeServerTicketResult : MsgHandler
{
    public string GetName()
    {
        return "msg_take_server_ticket_result";
    }

    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;
        int result = args["result"].AsInt;

        // 检查获取门票的结果
        if (result == 1)
        {
            // 记录登录信息
            Communicate.AccountInfo.Set("privilege", args["privilege"]);
            Communicate.AccountInfo.Set("seed", args["seed"]);

            // 遍历所有服务器列表
            foreach (LPCValue v in Communicate.AccountInfo.Query("server_list").AsArray.Values)
            {
                LPCMapping info = v.AsMapping;

                // 检查服务器名称
                if (info["server"].AsString != args["server_name"].AsString)
                    continue;

                // 更新服务器信息
                Communicate.AccountInfo.Set("server", args["server_name"]);
                Communicate.AccountInfo.Set("dist", info["dist"]);
                Communicate.AccountInfo.Set("gs_ip", info["ip"]);
                Communicate.AccountInfo.Set("gs_port", info["port"]);
                Communicate.AccountInfo.Set("server_id", info["server_id"]);

                // 登录到GS服务器
                Operation.CmdLogin.Go(Communicate.AccountInfo.Query("account", ""), 
                    info["ip"].AsString, info["port"].AsInt);

                return;
            }

            // 未取得门票对应服务器的信息，返回失败
            LogMgr.Trace("不存在名字叫做({0})的服务器，获取门票失败。", args["server_name"].AsString);
        }
        else
        {
            // 显示提示信息
            DialogMgr.ShowSimpleSingleBtnDailog(new CallBack(OnDialogCallBack), LocalizationMgr.GetServerDesc(args["msg"]));
        }
    }

    /// <summary>
    /// 确认弹框点击回调
    /// </summary>
    void OnDialogCallBack(object para, params object[] param)
    {
        // 未能取得服务器信息，断开与 AAA 的连接
        Communicate.Disconnect();

        // 返回登陆场景
        EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_FAILED, MixedValue.NewMixedValue<bool>(false));
    }
}
