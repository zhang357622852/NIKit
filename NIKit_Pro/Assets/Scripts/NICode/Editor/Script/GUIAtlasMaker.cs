using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

public class GUIAtlasMaker : EditorWindow
{
    private static string[] dirs = null;
    private static List<string> dirList = new List<string>();
    private static List<string> grayList = new List<string>() {
        "BuildingIcon"
    };

    private static string CONFIG_PATH = Application.dataPath + "/AtlasRes/"; //图集.png，图集配置文件.txt，材质球.mat
    private static string PREFAB_PATH = Application.dataPath + "/Resources/Atlas/"; //图集预制.prefab

    /// <summary>
    /// 获取待打包图集的文件夹列表
    /// </summary>
    private static void Refresh()
    {
        dirList.Clear();
        string rootPath = Application.dataPath + "/../" + "Z_RES/Atlas";
        dirs = Directory.GetDirectories(rootPath, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++)
        {
            dirs[i] = dirs[i].Replace("\\", "/");
            dirList.Add(dirs[i]);
        }
    }

    [MenuItem("Tools/AtlasEditor")]
    private static void Init()
    {

        Refresh();
        GUIAtlasMaker window = (GUIAtlasMaker)EditorWindow.GetWindow(typeof(GUIAtlasMaker));
        window.minSize = new Vector2(500, 520);
        window.titleContent = new GUIContent("UIAtlasEditor");
    }

    Vector2 scrollPos ;
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        NIEditorUtility.DrawAuthorSummary();
        GUILayout.Space(5);
        if (GUILayout.Button("刷新",GUILayout.MaxWidth(70)))
        {
            Refresh();
            return;
        }
        GUILayout.Space(5);
        GUILayout.Label("重新获取待打包图片文件夹", EditorStyles.boldLabel, GUILayout.MaxWidth(350));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10f);
        GUILayout.BeginVertical();
        GUILayout.Space(10f);
        GUI.backgroundColor = new Color32(150, 200, 255, 255);
        //未创建的在前，已创建的在后
        dirList.Sort((string a, string b) =>
        {
            string aPath = a.Replace("Z_RES/Atlas/", "Assets/AtlasRes/");
            string bPath = b.Replace("Z_RES/Atlas/", "Assets/AtlasRes/");
            bool ae = Directory.Exists(aPath);
            bool be = Directory.Exists(bPath);
            if (ae && !be)
                return 1;
            else if (!ae && be)
                return -1;
            else
            {
                //GetFileNameWithoutExtension得到没有扩充名(.txt)的文件名
                //按首字母排序
                string aN = Path.GetFileNameWithoutExtension(a);
                string bN = Path.GetFileNameWithoutExtension(b);
                return aN[0] - bN[0];
            }
        });

        //文件夹列表
        scrollPos = GUILayout.BeginScrollView(scrollPos, "AS TextArea", GUILayout.Width(480), GUILayout.Height(600));
        for (int i = 0; i < dirList.Count; i++)
        {
            string atlasName = Path.GetFileNameWithoutExtension(dirList[i]);

            GUILayout.BeginHorizontal();
            GUILayout.Space(10f);
            string resPath = dirList[i].Replace("Z_RES/Atlas/", "Assets/AtlasRes/");
            string buttonTxt = "";
            bool isNew = false;
            if (Directory.Exists(resPath))
                buttonTxt = "更新";
            else
            {
                buttonTxt = "创建";
                isNew = true;
            }

            if (isNew)
                GUI.color = Color.green;

            //1.创建/更新按钮
            if (GUILayout.Button(buttonTxt, GUILayout.Width(50), GUILayout.Height(20)))
            {
                MakeAtlas(atlasName, atlasName);
            }

            if (isNew)
                GUI.color = Color.white;
            //2.文件夹名
            GUILayout.Label(atlasName, EditorStyles.boldLabel);

            //3.配置文件按钮
            if (!isNew)
            {
                if (GUILayout.Button("配置文件", GUILayout.Width(70f), GUILayout.Height(20)))
                {
                    if (File.Exists(PREFAB_PATH + atlasName + "/" + atlasName + "Atlas" + ".prefab"))
                    {
                        UIAtlas atlas = Resources.Load<UIAtlas>("Atlas/" + atlasName + "/" + atlasName + "Atlas");
                        string textPath = CONFIG_PATH + atlasName + "/" + atlasName + ".txt";
                        textPath = GetProjectRelativePath(textPath);

                        TextAsset configuration = AssetDatabase.LoadAssetAtPath<TextAsset>(textPath);
                        NGUIJson.LoadSpriteData(atlas, configuration); //装载图集配置文件
                        atlas.MarkAsChanged();
                        PrefabUtility.RecordPrefabInstancePropertyModifications(atlas);
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
        }
        GUILayout.EndScrollView();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(10f);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    //arg1,arg2:文件夹名字->Common
    private void MakeAtlas(string arg1, string arg2)
    {
        string pathPreFix = Application.dataPath + "/../" + "Z_RES/";
        string shell = "output_images_for_ngui_editor.sh";
        shell = pathPreFix + shell;

        Process mProcess = new Process();
        //传参数给shell脚本格式(中间由空格隔开): 参数1 参数2 参数n
        ProcessStartInfo startInfo = new ProcessStartInfo(shell, arg1 + " " + arg2 + " " + "alpha");
        startInfo.WorkingDirectory = pathPreFix;
        mProcess.StartInfo = startInfo;
        mProcess.Start();
        mProcess.WaitForExit();
        AssetDatabase.Refresh();

        SetMeta(arg1);
    }

    //resName:文件夹名字->Common
    private void SetMeta(string resName)
    {
        string pngPath = CONFIG_PATH + resName + "/" + resName + ".png";
        string alphaPath = CONFIG_PATH + resName + "/" + resName + "_alpha" + ".png";
        string textPath = CONFIG_PATH + resName + "/" + resName + ".txt";

        pngPath = GetProjectRelativePath(pngPath);
        alphaPath = GetProjectRelativePath(alphaPath);
        textPath = GetProjectRelativePath(textPath);

        //设置图片属性
        TextureImporter texImp = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        texImp.textureType = TextureImporterType.Default;
        //texImp.generateCubemap = TextureImporterGenerateCubemap.None;
        texImp.alphaIsTransparency = true;
        texImp.mipmapEnabled = false;
        texImp.filterMode = FilterMode.Bilinear;
        texImp.maxTextureSize = 2048;
        //texImp.textureFormat = TextureImporterFormat.RGBA32;
        texImp.spriteImportMode = SpriteImportMode.None;
        texImp.SaveAndReimport();

        texImp = AssetImporter.GetAtPath(alphaPath) as TextureImporter;
        texImp.textureType = TextureImporterType.Default;
        //texImp.generateCubemap = TextureImporterGenerateCubemap.None;
        texImp.alphaIsTransparency = false;
        texImp.spriteImportMode = SpriteImportMode.None;
        texImp.filterMode = FilterMode.Bilinear;
        texImp.mipmapEnabled = false;
        texImp.maxTextureSize = 2048;
        //texImp.textureFormat = TextureImporterFormat.Alpha8;
        texImp.SaveAndReimport();

        if (!File.Exists(PREFAB_PATH + resName + "/" + resName + "Atlas" + ".prefab"))
        {
            //创建一个材质球并且配置好
            Shader shader = Shader.Find("MyShader/two_tex_ui 1");
            Material mat = new Material(shader);

            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(pngPath);
            Texture alp_texture = AssetDatabase.LoadAssetAtPath<Texture>(alphaPath);
            mat.name = resName;
            mat.SetTexture("_MainTex", texture);
            texture = AssetDatabase.LoadAssetAtPath<Texture>(alphaPath);
            mat.SetTexture("_FlagTex", texture);
            AssetDatabase.CreateAsset(mat, GetProjectRelativePath(Application.dataPath + "/AtlasRes/" + resName + "/" + resName + ".mat"));

            //创建一个预设体且配置好
            GameObject obj = new GameObject();
            UIAtlas atlas = obj.AddComponent<UIAtlas>();
            obj.name = resName + "Atlas";
            atlas.spriteMaterial = mat;

            if (atlas.texture != null)
                NGUIEditorTools.ImportTexture(atlas.texture, false, false, !atlas.premultipliedAlpha);
            atlas.MarkAsChanged();

            TextAsset ta = AssetDatabase.LoadAssetAtPath<TextAsset>(textPath);
            NGUIJson.LoadSpriteData(atlas, ta);
            atlas.MarkAsChanged();

            if (!Directory.Exists(PREFAB_PATH + resName))
            {
                Directory.CreateDirectory(PREFAB_PATH + resName);
            }
            PrefabUtility.CreatePrefab("Assets/Resources/Atlas/" + resName + "/" + obj.name + ".prefab", obj, ReplacePrefabOptions.ReplaceNameBased);
            GameObject.DestroyImmediate(obj);
        }
    }

    static string GetProjectRelativePath(string AP)
    {
        string newPath = string.Empty;
        if (!string.IsNullOrEmpty(AP))
        {
            newPath = "Assets" + AP.Replace(Application.dataPath, "");
            newPath = newPath.Replace(@"\", "/");
        }

        return newPath;
    }
}
