using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System;

public static class UIPanelCodeTemplate
{
    public static void Generate(string filePath, string scriptName)
    {
        StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false));

        StringBuilder strBuilder = new StringBuilder();

        strBuilder.AppendLine("/// <summary>");
        strBuilder.AppendFormat("/// {0}.cs", scriptName);
        strBuilder.AppendLine();
        strBuilder.AppendFormat("/// Created by WinMi {0}/{1}/{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        strBuilder.AppendLine();
        strBuilder.AppendLine("///");
        strBuilder.AppendLine("/// </summary>");
        strBuilder.AppendLine();

        strBuilder.AppendLine("using System.Collections;");
        strBuilder.AppendLine("using System.Collections.Generic;");
        strBuilder.AppendLine("using UnityEngine;");
        strBuilder.AppendLine();

        strBuilder.AppendFormat("public partial class {0} : UIBaseForms<{1}>", scriptName, scriptName);
        strBuilder.AppendLine();
        strBuilder.AppendLine("{");

        strBuilder.AppendLine("\t#region 内部函数");
        strBuilder.AppendLine();

        // 重载Init函数
        strBuilder.AppendLine("\tpublic override void Init()");
        strBuilder.AppendLine("\t{");
        strBuilder.AppendLine("\t\tRegisterEvent();");
        strBuilder.AppendLine("\t}");

        strBuilder.AppendLine();

        // 监听事件函数
        strBuilder.AppendLine("\tprivate void RegisterEvent()");
        strBuilder.AppendLine("\t{");
        strBuilder.AppendLine("\t}");

        strBuilder.AppendLine();

        // 重载Show函数
        strBuilder.AppendLine("\tpublic override void Show()");
        strBuilder.AppendLine("\t{");
        strBuilder.AppendLine("\t}");

        strBuilder.AppendLine();

        // 按钮事件回调函数
        strBuilder.AppendLine("\tprivate void OnClickCloseBtn(GameObject go)");
        strBuilder.AppendLine("\t{");
        strBuilder.AppendLine("\t}");

        strBuilder.AppendLine();
        strBuilder.AppendLine("\t#endregion");

        strBuilder.AppendLine();

        strBuilder.AppendLine("\t#region 公共函数");
        strBuilder.AppendLine("\t#endregion");

        strBuilder.AppendLine("}");

        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }
}

public static class UIPanelComponentsCodeTemplate
{
    public static void Generate(string filePath, string scriptName, PanelCodeData panelCodeData)
    {
        StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false));

        StringBuilder strBuilder = new StringBuilder();

        strBuilder.AppendLine("using System.Collections;");
        strBuilder.AppendLine("using System.Collections.Generic;");
        strBuilder.AppendLine("using UnityEngine;");
        strBuilder.AppendLine();

        strBuilder.AppendFormat("public partial class {0}", scriptName);
        strBuilder.AppendLine();
        strBuilder.AppendLine("{");

        strBuilder.AppendLine("\t[Header(\"----------------\")]");

        // mark组件
        for (int i = 0; i < panelCodeData.mMarkObjInfos.Count; i++)
        {
            string strUIType = panelCodeData.mMarkObjInfos[i].mMarkObj.mComponentTypeName;

            strBuilder.AppendFormat("\tpublic {0} {1};\r\n", strUIType, UICodeGenerator.mPreFormat + panelCodeData.mMarkObjInfos[i].mName);

            if (i < (panelCodeData.mMarkObjInfos.Count - 1))
                strBuilder.AppendLine();
        }

        strBuilder.AppendLine("}");

        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }
}
