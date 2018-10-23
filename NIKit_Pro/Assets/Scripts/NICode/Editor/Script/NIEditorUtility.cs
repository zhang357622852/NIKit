using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    //参照Windows OPENFILENAME
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public string filter = null;
    public string customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public string file = null;
    public int maxFile = 0;
    public string fileTitle = null;
    public int maxFileTitle = 0;
    public string initialDir = null;
    public string title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public string defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public string templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

/// <summary>
/// Author: WinMi
/// Description: Editor静态工具类
/// </summary>
public static class NIEditorUtility
{
    private static string authorIconPath = "Assets/WinMi/Editor/GUI/authorIcon.png";
    private static Texture2D authorTexture2d = (Texture2D)AssetDatabase.LoadMainAssetAtPath(authorIconPath);

    public static void DrawAuthorSummary()
    {
        GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(65));
        GUILayout.Box(new GUIContent(authorTexture2d, "俺只是一张图片o(*￣︶￣*)o"));
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        GUILayout.Space(12);
        GUILayout.TextArea("Author: WinMi", style);
        GUILayout.Space(12);
        GUILayout.TextArea("Descript:", style);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public static Texture2D GetAuthorImage()
    {
        return authorTexture2d;
    }

    public static void DrawTitle(string text, int fontSize = 20)
    {
        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);

                GUIStyle style = new GUIStyle();
                style.fontSize = fontSize;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.white;
                GUILayout.Label(text, style);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MaxHeight(10f));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    public static bool GetOpenFileName1([In, Out] OpenFileName ofn)
    {
        return GetOpenFileName(ofn);
    }

    public static OpenFileName GetOpenFileInfo()
    {
        OpenFileName info = new OpenFileName();

        info.structSize = Marshal.SizeOf(info);

        info.filter = "Json(.json)|*.json";

        info.file = new string(new char[256]);

        info.maxFile = info.file.Length;

        info.fileTitle = new string(new char[64]);

        info.maxFileTitle = info.fileTitle.Length;

        info.initialDir = UnityEngine.Application.dataPath;//默认路径

        info.title = "Selected File";

        //显示文件的类型
        info.defExt = "Json";//"JPG";

        //注意一下项目不一定要全选 但是0x00000008项不要缺少
        //0x00080000|0x00001000|0x00000800|0x00000200|0x00000008
        //OFN_EXPLORER |OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
        info.flags = 0x00001000 | 0x00000008;



        return info;
    }

    public static string BrowseFolder()
    {
        //FolderBrowserDialog dialog = new FolderBrowserDialog();
        //dialog.Description = "注意: 使用后记得关闭此界面,以免Unity窗口操作受阻";
        ////Environment.CurrentDirectory
        ////dialog.RootFolder = Environment.SpecialFolder.Desktop;
        //if (dialog.ShowDialog() == DialogResult.OK)
        //{
        //    return dialog.SelectedPath;
        //}

        return null;
    }

    public static string OpenFile()
    {
        //OpenFileDialog oplog = new OpenFileDialog();
        //oplog.InitialDirectory = UnityEngine.Application.dataPath + "/../";
        //oplog.Filter = "Json(.json)|*.json"; //筛选格式: |*.json
        //oplog.Title = "Selected File";
        //DialogResult result = oplog.ShowDialog();
        //if (result == DialogResult.OK)
        //{
        //    return oplog.FileName;
        //}

        return null;
    }

    public static void CreateCsConfigFile(string filePath, string fileName, string rootFolderName)
    {
        //string text = File.ReadAllText(filePath);
        //JsonValue jsonRoot = JsonUtility.ToObjectFromJS(text);
        //if (jsonRoot == null)
        //    return;

        //jsonRoot = jsonRoot.Get("root");
        //if (jsonRoot == null || jsonRoot.IsNull() || !jsonRoot.IsArray() || jsonRoot.GetLength() <= 0)
        //    return;

        //JsonValue jsonTemplate = null;

        //if (jsonRoot.GetLength() > 0)
        //    jsonTemplate = jsonRoot.Get(0);

        //for (int i = 0; i < jsonRoot.GetLength(); i++)
        //{
        //    JsonValue item = jsonRoot.Get(i);
        //    foreach (string k in item.GetKeys())
        //    {
        //        if (!jsonTemplate.HasKey(k))
        //            jsonTemplate.Add(k, item.Get(k));
        //    }
        //}

        //if (jsonTemplate != null)
        //{
        //    //类成员
        //    StringBuilder contentStr = new StringBuilder();
        //    contentStr.Append("\n");
        //    foreach (string key in jsonTemplate.GetKeys())
        //    {
        //        string valueName = key;
        //        string valueType = "string";

        //        JsonValue value = jsonTemplate.Get(key);
        //        if (value.IsInt())
        //        {
        //            valueType = "int";
        //        }
        //        else if (value.IsFloat())
        //        {
        //            valueType = "float";
        //        }
        //        else if (value.IsDouble())
        //        {
        //            //valueType = "double"; 目前double类型的也用float
        //            valueType = "float";
        //        }
        //        else if (value.IsString())
        //        {
        //            valueType = "string";
        //        }
        //        else if (value.IsArray())
        //        {
        //            //目前自定义类型NormalObjectClass.cs:AddBattleProp RewardInfo CostInfo TalentLimitConfig
        //            string lowerStr = valueName.ToLower();

        //            if (lowerStr.Contains("rewardinfo") || lowerStr.Contains("reward"))
        //                valueType = "List<RewardInfo>";

        //            else if (lowerStr.Contains("wndcost"))
        //                valueType = "List<int>";

        //            else if (lowerStr.Contains("costinfo") || lowerStr.Contains("cost"))
        //                valueType = "List<CostInfo>";

        //            else if (lowerStr.Contains("prop"))
        //                valueType = "List<AddBattleProp>";

        //            else if (lowerStr.Contains("talentinfo"))
        //                valueType = "List<TalentLimitConfig>";


        //            else
        //                valueType = "List<" + valueName+ ">";
        //        }
        //        //Debug.Log("=========================" + value.GetValueType());

        //        contentStr.Append("\tpublic ").Append(valueType).Append(" ").Append(valueName).Append(" { get; set; }\n");
        //    }
        //    //
        //    string className = fileName.Replace("GameConfig", "") + "Config";

        //    StringBuilder fileStr = new StringBuilder();
        //    fileStr.AppendLine("using System;");
        //    fileStr.AppendLine("using System.Collections.Generic;");
        //    fileStr.AppendLine("using UnityEngine;");
        //    fileStr.AppendLine();
        //    fileStr.Append("public class ").Append(className);
        //    fileStr.Append("\n");
        //    fileStr.AppendLine("{");
        //    fileStr.AppendLine(contentStr.ToString());
        //    fileStr.AppendLine("}");

        //    File.WriteAllText(UnityEngine.Application.dataPath + "/../" + rootFolderName + "/" + className + ".cs", fileStr.ToString(), new System.Text.UTF8Encoding(false));

        //    Debug.Log("==生成文件==" + fileName + ".cs");
        //}

    }
}
