using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

public class GUIAtlasMaker : EditorWindow
{
    private static string[] dirs = null;

    private static List<string> dirList = new List<string>();

    private static string ATLAS_PATH
    {
        get
        {
            //图集.png，图集配置文件.txt，材质球.mat 图集预制.prefab
            return ((Application.dataPath + "/Art/").CreateDirIfNotExist() + "Atlas/").CreateDirIfNotExist();
        }
    }

    private static string ATLAS_PREFAB_PATH
    {
        get
        {
            return ((Application.dataPath + "/Resources/").CreateDirIfNotExist() + "Atlas/").CreateDirIfNotExist();
        }
    }

    private static string Z_RES_PATH
    {
        get
        {
            return ((Application.dataPath + "/../" + "Z_RES/").CreateDirIfNotExist() + "Atlas/").CreateDirIfNotExist();
        }
    }

    /// <summary>
    /// TexturePacker安装目录bin
    /// 填写你本地的安装路径
    /// </summary>
    private static string TEXTURE_PACKER_PATH
    {
        get
        {
            return (@"D:\TexturePacker_3.7.1\bin\");
        }
    }

    /// <summary>
    /// 获取待打包图集的文件夹列表
    /// </summary>
    private static void Refresh()
    {
        dirList.Clear();

        string rootPath = Z_RES_PATH;
        dirs = Directory.GetDirectories(rootPath, "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < dirs.Length; i++)
        {
            dirs[i] = dirs[i].Replace("\\", "/");

            dirList.Add(dirs[i]);
        }
    }

    /// <summary>
    /// unity5.3之后,Android平台默认压缩纹理为ETC2,Unity对不支持ETC2的低端机(android4.3),加载图片时解压为RGBA
    /// 这样就会造成极大的内存浪费，也影响加载速度。
    /// 在5.3之后，Unity提供了ETC1+Alpha的支持,无需在用这种就方法了？
    /// </summary>
    //[MenuItem("Tools/AtlasEditor")]
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
        GUILayout.BeginVertical();
        // 绘制作者信息
        NIEditorUtility.DrawAuthorSummary();
        GUILayout.Space(5);
        if (GUILayout.Button("刷新",GUILayout.MaxWidth(70)))
        {
            Refresh();
            return;
        }
        GUILayout.Space(5);
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUI.backgroundColor = new Color32(150, 200, 255, 255);
        //未创建的在前，已创建的在后
        dirList.Sort((string a, string b) =>
        {
            string aPath = a.Replace("Z_RES/Atlas/", "Assets/Art/Atlas/");
            string bPath = b.Replace("Z_RES/Atlas/", "Assets/Art/Atlas/");
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
            string resPath = dirList[i].Replace("Z_RES/Atlas/", "Assets/Art/Atlas/");
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
                MakeAtlas(atlasName);
            }

            if (isNew)
                GUI.color = Color.white;

            //2.文件夹名
            GUILayout.Label(atlasName, EditorStyles.boldLabel);

            //3.配置文件按钮
            if (!isNew)
            {
                if (GUILayout.Button("配置", GUILayout.Width(70f), GUILayout.Height(20)))
                {
                    if (File.Exists(ATLAS_PREFAB_PATH + atlasName + "Atlas" + ".prefab"))
                    {
                        UIAtlas atlas = Resources.Load<UIAtlas>("Atlas/" + atlasName + "Atlas");
                        string textPath = ATLAS_PATH + atlasName + ".txt";
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
    private void MakeAtlas(string fileName)
    {
        string pathPreFix = Application.dataPath + "/../" + "Z_RES/";

        string batFile = "output_images_for_ngui_editor.bat";
        batFile = pathPreFix + batFile;

        // 需要配置TexturePacker.exe的安装路径
        if (!File.Exists(TEXTURE_PACKER_PATH + "TexturePacker.exe"))
        {
            UnityEngine.Debug.LogErrorFormat("TexturePacker安装路径{0}是错误的,请从新填写", TEXTURE_PACKER_PATH);
            return;
        }

        try
        {
            // 确保存放.txt,.png文件夹存在
            (ATLAS_PATH + fileName + "/").CreateDirIfNotExist();

            Process mProcess = new Process();

            // 转换一下路径分割
            //E:/MMO_ARPG_Project/MMO_ARPG_Project/Assets
            string filePath = Application.dataPath.Replace("/","\\");

            //传参数给bat脚本格式(中间由空格隔开): 参数1 参数2 参数n
            ProcessStartInfo startInfo = new ProcessStartInfo(batFile, filePath + " " + fileName);

            // 这里的工作目录必须是TexturePacker的bin目录，要不然无法执行TexturePacker命令行
            // TexturePacker的环境变量已经加入到系统环境变量path中了
            // 如果是在外部直接点击bat文件是可以执行TexturePacker的，但是这里以Process方式打开，目前就需要设置工作目录
            startInfo.WorkingDirectory = TEXTURE_PACKER_PATH;
            mProcess.StartInfo = startInfo;
            mProcess.Start();
            mProcess.WaitForExit();

            AssetDatabase.Refresh();

            SetMeta(fileName);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }

    //resName:文件夹名字->Common
    private void SetMeta(string resName)
    {
        string pngPath = ATLAS_PATH + resName + "/" + resName + ".png";
        string alphaPath = ATLAS_PATH + resName + "/" + resName + "_alpha" + ".png";
        string textPath = ATLAS_PATH + resName + "/" + resName + ".txt";

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

        if (!File.Exists(ATLAS_PREFAB_PATH + resName + "Atlas" + ".prefab"))
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
            AssetDatabase.CreateAsset(mat, GetProjectRelativePath(ATLAS_PATH + resName + "/" + resName + ".mat"));

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

            PrefabUtility.CreatePrefab(ATLAS_PREFAB_PATH + obj.name + ".prefab", obj, ReplacePrefabOptions.ReplaceNameBased);
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
