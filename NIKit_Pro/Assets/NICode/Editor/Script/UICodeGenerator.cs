/// <summary>
/// UICodeGenerator.cs
/// Created by WinMi 2018/11/05
/// UI界面文件代码生成
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Reflection;

/// <summary>
/// Panel信息
/// </summary>
public class PanelCodeData
{
    public string mPanelName;

    public readonly List<MarkObjInfo> mMarkObjInfos = new List<MarkObjInfo>();

    public Dictionary<string, string> mDicNameToFullName = new Dictionary<string, string>();
}

/// <summary>
/// mark信息
/// </summary>
public class MarkObjInfo
{
    public string mName;

    public string mPathToElement;

    public IMark mMarkObj;
}


public class UICodeGenerator : Singleton<UICodeGenerator>
{
    private string mUIScriptDir
    {
        get
        {
            return (Application.dataPath + "/Scripts/").CreateDirIfNotExist() + "UI/";
        }
    }

    private PanelCodeData mPanelCodeData;

    // 变量的命名前缀，驼峰命名
    public static readonly string mPreFormat = "m";

    [MenuItem("Assets/@UI - Create UICode")]
    public static void CreateUICode()
    {
        Object[] objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets | SelectionMode.TopLevel);

        bool displayProgress = objs.Length > 1;

        // 显示进度条
        if (displayProgress)
            EditorUtility.DisplayProgressBar("Create UI Code", "", 0);

        // 确保UI脚本文件夹存在
        UICodeGenerator.Instance.mUIScriptDir.CreateDirIfNotExist();

        for (int i = 0; i < objs.Length; i++)
        {
            UICodeGenerator.Instance.CreateCode(objs[i] as GameObject, AssetDatabase.GetAssetPath(objs[i]));

            if (displayProgress)
                EditorUtility.DisplayProgressBar("Create UI Code", "", (float)(i + 1) / objs.Length);
        }

        // 刷新资源,会清除内存，所以一些信息需要提前序列化保存
        AssetDatabase.Refresh();

        // 清理进度条
        if (displayProgress)
            EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 创建ui模板代码/自动引用
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="uiPrefabPath">Assets/Prefabs/Windows/MainPanel.prefab</param>
    private void CreateCode(GameObject obj, string uiPrefabPath)
    {
        if (obj != null)
        {
            PrefabType type = PrefabUtility.GetPrefabType(obj);

            if (PrefabType.Prefab != type)
                return;

            GameObject clone = PrefabUtility.InstantiatePrefab(obj) as GameObject;

            if (clone == null)
                return;

            mPanelCodeData = new PanelCodeData();
            mPanelCodeData.mPanelName = clone.name.Replace("(clone)", string.Empty);

            // 找到所有mark标记
            FindAllMarkTrans(clone.transform, string.Empty);

            // 创建脚本
            CreateUIPanelCode(clone, uiPrefabPath);

            // 记录预制体路径
            AddSerializeUIPrefab(uiPrefabPath);

            GameObject.DestroyImmediate(clone);
        }
    }

    private static void AddSerializeUIPrefab(string uiPrefabPath)
    {
        if (string.IsNullOrEmpty(uiPrefabPath))
            return;

        var pathStr = EditorPrefs.GetString("AutoGenerateUIPrefabPath");

        if (string.IsNullOrEmpty(pathStr))
            pathStr = uiPrefabPath;
        else
            pathStr += ";" + uiPrefabPath;

        EditorPrefs.SetString("AutoGenerateUIPrefabPath", pathStr);
    }

    /// <summary>
    /// 当调用AssetDatabase.Refresh()后，编辑器会从新编译创建脚本，并且会清理内存
    /// 监听编辑器编译完成
    /// </summary>
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void SerializeUIPrefab()
    {
        string pathStr = EditorPrefs.GetString("AutoGenerateUIPrefabPath");

        if (string.IsNullOrEmpty(pathStr))
            return;

        EditorPrefs.DeleteKey("AutoGenerateUIPrefabPath");

        Assembly assembly = ReflectionExtension.GetAssemblyCSharp();

        string[] paths = pathStr.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);

        bool displayProgress = paths.Length > 3;
        if (displayProgress)
            EditorUtility.DisplayProgressBar("", "Serialize UIPrefab...", 0);

        for (var i = 0; i < paths.Length; i++)
        {
            GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            AttachSerializeObj(uiPrefab, uiPrefab.name, assembly);

            if (displayProgress)
                EditorUtility.DisplayProgressBar("", "Serialize UIPrefab..." + uiPrefab.name, (float)(i + 1) / paths.Length);
        }

        if (displayProgress)
            EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void AttachSerializeObj(GameObject obj, string behaviourName, System.Reflection.Assembly assembly,
    List<IMark> processedMarks = null)
    {
        if (null == processedMarks)
            processedMarks = new List<IMark>();

        IMark uiMark = obj.GetComponent<IMark>();

        string className = string.Empty;

        className = behaviourName;

        // 得到类名
        System.Type t = assembly.GetType(className);
        // GameObject上没有此组件就添加
        Component com = obj.GetComponent(t) ?? obj.AddComponent(t);
        // 序列化这个组件
        SerializedObject sObj = new SerializedObject(com);

        IMark[] uiMarks = obj.GetComponentsInChildren<IMark>(true);

        foreach (var elementMark in uiMarks)
        {
            if (processedMarks.Contains(elementMark))
                continue;

            processedMarks.Add(elementMark);

            string uiType = elementMark.mComponentTypeName;
            // 成员变量名字
            string propertyName = UICodeGenerator.mPreFormat + elementMark.mTransform.gameObject.name;

            if (sObj.FindProperty(propertyName) == null)
            {
                Debug.LogFormat("sObj is Null:{0} {1} {2}", propertyName, uiType, sObj);
                continue;
            }

            sObj.FindProperty(propertyName).objectReferenceValue = elementMark.mTransform.gameObject;

            //AttachSerializeObj(elementMark.mTransform.gameObject, elementMark.ComponentName, assembly, processedMarks);
        }

        sObj.ApplyModifiedPropertiesWithoutUndo();
    }

    private void FindAllMarkTrans(Transform curTrans, string transFullName)
    {
        foreach (Transform childTrans in curTrans)
        {
            IMark uiMark = childTrans.GetComponent<IMark>();


            if (uiMark != null)
            {
                if (!mPanelCodeData.mMarkObjInfos.Exists(objInfo => objInfo.mName.Equals(uiMark.mTransform.name)))
                {
                    mPanelCodeData.mMarkObjInfos.Add(new MarkObjInfo
                    {
                        mName = uiMark.mTransform.name,
                        mMarkObj = uiMark,
                        mPathToElement = PathToParent(childTrans, mPanelCodeData.mPanelName),
                    });

                    mPanelCodeData.mDicNameToFullName.Add(uiMark.mTransform.name, transFullName + childTrans.name);
                }
                else
                    Debug.LogError("Repeat key: " + childTrans.name);

                FindAllMarkTrans(childTrans, transFullName + childTrans.name + "/");
            }
            else
                FindAllMarkTrans(childTrans, transFullName + childTrans.name + "/");
        }
    }

    /// <summary>
    /// 生成ui界面脚本
    /// </summary>
    /// <param name="uiPrefab"></param>
    /// <param name="uiPrefabPath">Assets/Prefabs/Windows/MainPanel.prefab</param>
    /// <param name="panelData"></param>
    private void CreateUIPanelCode(GameObject uiPrefab, string uiPrefabPath)
    {
        if (uiPrefab == null)
            return;

        // 脚本名/类名
        string scriptFileName = uiPrefab.name;

        // 脚本路径
        string strFilePath = mUIScriptDir + scriptFileName + ".cs";

        // 不存在 创建UI脚本文件(存放逻辑代码)
        if (!File.Exists(strFilePath))
            UIPanelCodeTemplate.Generate(strFilePath, scriptFileName);

        // 控件关联成员变量脚本
        CreateUIPanelComponentsCode(scriptFileName, strFilePath);
    }

    private void CreateUIPanelComponentsCode(string scriptName, string uiUIPanelfilePath)
    {
        string generateFilePath = uiUIPanelfilePath.Replace(".cs", "Components.cs");

        if (File.Exists(generateFilePath))
        {
            // 需要删除.meta文件，才会使编辑器从新编译
            File.Delete(generateFilePath + ".meta");
        }

        UIPanelComponentsCodeTemplate.Generate(generateFilePath, scriptName, mPanelCodeData);
    }

    /// <summary>
    /// 获取到指定父节点的路径
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="parentName"></param>
    /// <returns></returns>
    private string PathToParent(Transform trans, string parentName)
    {
        StringBuilder retValue = new StringBuilder(trans.name);

        while (trans.parent != null)
        {
            if (trans.parent.name.Equals(parentName))
                break;

            retValue = trans.parent.name.Append("/").Append(retValue);

            trans = trans.parent;
        }

        return retValue.ToString();
    }
}
