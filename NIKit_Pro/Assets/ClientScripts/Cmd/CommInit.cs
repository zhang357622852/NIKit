/// <summary>
/// 通讯文件初始化
/// </summary>
using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Text;

public class CommInit
{
    static public void Init()
    {
        // 加载通讯文件
        string text = ResourceMgr.LoadText(ConfigMgr.ETC_PATH + "/send.txt");
        if (! string.IsNullOrEmpty(text))
            MsgMgr.LoadMessageFile(Game.Explode(text, "\n"));

        // 加载通讯描述文件
        text = ResourceMgr.LoadText(ConfigMgr.ETC_PATH + "/communicate.txt");
        if (! string.IsNullOrEmpty(text))
            PktAnalyser.LoadCommDescFile(text, PktAnalyser.EncapType.ET_MAPPING);

        // 登记所有的消息处理器
        RegisterCmds.Init();
    }

#if UNITY_EDITOR
    // 更新配置文件
    public static void UpdateCfg()
    {
        // 处理 Etc/communicate.txt
        string basePath = Application.dataPath + "/../../server/server_scripts/etc/";
        string [] path = new string[] { basePath + "comm/game_comm.txt",
                                        basePath + "comm/combat_comm.txt",
                                        basePath + "comm/global_comm.txt",
                                        basePath + "comm/gm_comm.txt",
                                        basePath + "native_comm/native_gm_comm.txt",
                                        basePath + "native_comm/native_game_comm.txt",
                                        };

        // 收集文件内容
        string communicate_txt = "";
        foreach (string filePath in path)
        {
            foreach (string line in FileMgr.ReadLines(filePath))
            {
                string s = line.Trim();
                if (s.Length < 1 || s [0] == '#' || s [0] == '/')
                    continue;

                if (s.Length >= 2 && s [0] == '[' && s [s.Length - 1] == ']')
                    continue;

                communicate_txt += s + "\n";
            }
        }

        Directory.CreateDirectory(ConfigMgr.ETC_PATH);

        // 写文件
        var fs = new FileStream(ConfigMgr.ETC_PATH + "/communicate.txt", FileMode.Create, FileAccess.Write);
        var data = Encoding.UTF8.GetBytes(communicate_txt);
        fs.Write(data, 0, data.Length);
        fs.Close();

        // 处理 Etc/send.txt
        basePath = Application.dataPath + "/../../server/server_scripts/";
        path = new string[] {  basePath + "extend/round_combat/global/include/send.h",
                               basePath + "global/include/global_send.h",
                               basePath + "native/global/include/native_send.h",
        };

        // 收集文件内容
        string send_txt = "";
        foreach (string filePath in path)
        {
            foreach (string line in FileMgr.ReadLines(filePath))
            {
                string s = line.Trim();
                if (s.Length < "#define".Length ||
                    s [0] == '/' ||
                    s.Substring(0, "#define".Length) != "#define")
                    continue;
                if (s [0] == '[' &&
                    s [s.Length - 1] == ']')
                    continue;
                send_txt += s + "\n";
            }
        }

        // 写文件
        fs = new FileStream(ConfigMgr.ETC_PATH + "/send.txt", FileMode.Create, FileAccess.Write);
        data = Encoding.UTF8.GetBytes(send_txt);
        fs.Write(data, 0, data.Length);
        fs.Close();
    }
#endif
}

