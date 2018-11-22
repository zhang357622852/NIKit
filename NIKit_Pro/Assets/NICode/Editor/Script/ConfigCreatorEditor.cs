using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using WinMi;
using System.Text;

/// <summary>
/// Author: WinMi
/// Description: 配置表生成工具: 根据.json配置表生成对应的.cs文件
/// </summary>
public class ConfigCreatorEditor : EditorWindow
{
    private const string CONFIG_FOLDER_NAME = "ConfigCS";
    private const string CONFIG_JSON_FOLER_NAME = "Config";
    private static string FILE_ROOT_PATH = null;

    private static List<string> mFilesPathList = new List<string>();

    //[MenuItem("Tools/ConfigCreator(配置表生成)")]
    private static void CreateWindow()
    {
        Refresh();

        if (!Directory.Exists(Application.dataPath + "/../" + CONFIG_FOLDER_NAME))
            Directory.CreateDirectory(Application.dataPath + "/../" + CONFIG_FOLDER_NAME);

        ConfigCreatorEditor wnd = EditorWindow.GetWindow<ConfigCreatorEditor>(true);
        wnd.minSize = new Vector2(500, 780);
        wnd.Show();
    }

    private static void Refresh()
    {
        if (string.IsNullOrEmpty(FILE_ROOT_PATH))
            FILE_ROOT_PATH = Application.dataPath + "/../" + CONFIG_JSON_FOLER_NAME;

        mFilesPathList.Clear();
        string[] filesPath = Directory.GetFiles(FILE_ROOT_PATH, "*.json");

        for (int i = 0; i < filesPath.Length; i++)
        {
            if (filesPath[i].Contains("GameConfigConfigFile"))
                continue;

            filesPath[i] = filesPath[i].Replace("\\", "/");
            if (!filesPath[i].Contains("MD5") && !filesPath[i].Contains("Language"))
                mFilesPathList.Add(filesPath[i]);
        }
    }

    private Vector2 _scrollPos;
    private void OnGUI()
    {
        //头像
        GUILayout.Space(10);
        NIEditorUtility.DrawAuthorSummary();
        GUILayout.Space(10);

        //选择目标目录
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(5);
            GUIStyle style = EditorStyles.textArea;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label(FILE_ROOT_PATH, style, GUILayout.Height(25));

            if (GUILayout.Button("SetPath", GUILayout.MaxWidth(65f)))
            {
                string tempPath = NIEditorUtility.BrowseFolder();
                if (!string.IsNullOrEmpty(tempPath))
                {
                    FILE_ROOT_PATH = tempPath;
                    Refresh();
                }
                return;
            }

            if (GUILayout.Button("ResetPath", GUILayout.MaxWidth(75f)))
            {
                    FILE_ROOT_PATH = Application.dataPath + "/../" + CONFIG_JSON_FOLER_NAME;
                    Refresh();
                return;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        //内容
        GUI.backgroundColor = new Color32(150, 200, 255, 255);
        GUILayout.BeginVertical("AS TextArea", GUILayout.Height(500));
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Config File Name", EditorStyles.toolbarButton);
            GUILayout.Label("Operation", EditorStyles.toolbarButton);
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(470));
            for (int i = 0; i < mFilesPathList.Count; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(mFilesPathList[i]);

                GUILayout.BeginHorizontal();
                {
                    GUIStyle style = EditorStyles.textArea;
                    style.fontStyle = FontStyle.Bold;
                    style.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label(fileName, style, GUILayout.Height(25));

                    DrawCreateBtn(mFilesPathList[i], fileName);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

        //刷新
        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(60f)))
            {
                Refresh();
                return;
            }
            GUILayout.Space(2);
            GUILayout.Label("To refresh Config Files");
        }
        GUILayout.EndHorizontal();

        //创建全部
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Create All", GUILayout.MaxWidth(80f)))
            {
                for (int i = 0; i < mFilesPathList.Count; i++)
                {
                    string fileName = Path.GetFileNameWithoutExtension(mFilesPathList[i]);
                    CreateCsConfigFile(mFilesPathList[i], fileName, CONFIG_FOLDER_NAME);
                }
                return;
            }
            GUILayout.Space(2);
            GUILayout.Label("To Create All Config Files");
        }
        GUILayout.EndHorizontal();

        //生成到目标路径
        GUILayout.BeginVertical();
        {
            GUILayout.Space(10);

            GUILayout.Label("生成到目标路径:");

            GUI.color = Color.white;
            GUIStyle style = EditorStyles.textArea;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label(Application.dataPath+"/../"+CONFIG_FOLDER_NAME, style, GUILayout.Height(25));
        }
        GUILayout.EndVertical();

    }

    //filePath: D:/ TowerGame / Assets /../ Config / GameConfigGoto.json
    //fileName: GameConfigGoto
    private void DrawCreateBtn(string filePath, string fileName)
    {
        bool isNew = false;
        string buttonTxt = "";

        string className = fileName.Replace("GameConfig", "") + "Config";
        if (File.Exists(Application.dataPath + "/../" + CONFIG_FOLDER_NAME + "/" + className + ".cs"))
            buttonTxt = "Update";
        else
        {
            buttonTxt = "Create";
            isNew = true;
        }

        if (isNew)
            GUI.color = Color.green;

        if (GUILayout.Button(buttonTxt, GUILayout.Width(98f)))
        {
            CreateCsConfigFile(filePath, fileName, CONFIG_FOLDER_NAME);
        }

        if (isNew)
            GUI.color = Color.white;
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
