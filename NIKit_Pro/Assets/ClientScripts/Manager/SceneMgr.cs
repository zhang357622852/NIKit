/// <summary>
/// SceneMgr.cs
/// Created by zhaozy 2016/12/10
/// 场景管理模块
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LPC;

public static class SceneMgr
{
    #region 变量声明

    /// <summary>
    /// 场景对象列表
    /// </summary>
    private static Dictionary<string, GameObject> SceneObjectMap = new Dictionary<string, GameObject>();

    // 场景相机
    private static Camera mSceneCamera = null;

    // 剧本场景相机
    private static Camera mScenarioCamera = null;

    #endregion

    #region 属性

    /// <summary>
    /// 当前主场景（世界地图，主城）
    /// </summary>
    /// <value>The main scene.</value>
    public static string MainScene { get; private set; }

    /// <summary>
    /// 场景根节点
    /// </summary>
    public static Transform SceneRoot { get; private set; }

    /// <summary>
    /// 场景中标准相机
    /// </summary>
    public static Camera StdCamera { get; set; }

    // 缓存场景相机起始位置的位置
    public static Vector3 SceneCameraFromPos {get; set;}
    public static Vector3 SceneCameraToPos {get; set;}

    /// <summary>
    /// 剧本场景相机
    /// </summary>
    /// <value>The scenario camera.</value>
    public static Camera ScenarioCamera {
        get
        {
            // 如果当前mScenarioCamera处于有效状态
            if (mScenarioCamera != null && mScenarioCamera.isActiveAndEnabled)
                return mScenarioCamera;

            // 重置mScenarioCamera
            mScenarioCamera = null;

            // 获取相机列表
            GameObject[] cameraList = GameObject.FindGameObjectsWithTag("ScenarioCamera");
            foreach (GameObject cameraOb in cameraList)
            {
                // 对象不存在
                if (cameraOb == null)
                    continue;

                // 获取相机组件
                Camera camera = cameraOb.GetComponent<Camera>();
                if (camera == null || ! camera.isActiveAndEnabled)
                    continue;

                // 设置当前相机
                mScenarioCamera = camera;
                break;
            }

            // 返回当前剧本场景相机
            return mScenarioCamera;
        }
        private set
        {
            mScenarioCamera = value;
        }
    }

    /// <summary>
    /// 场景相机
    /// </summary>
    /// <value>The scene camera.</value>
    public static Camera SceneCamera {
        get
        {
            // 如果当前mSceneCamera处于有效状态
            if (mSceneCamera != null && mSceneCamera.isActiveAndEnabled)
                return mSceneCamera;

            // 重置mSceneCamera
            mSceneCamera = null;

            // 获取相机列表
            GameObject[] cameraList = GameObject.FindGameObjectsWithTag("SceneCamera");
            foreach (GameObject cameraOb in cameraList)
            {
                // 对象不存在
                if (cameraOb == null)
                    continue;

                // 获取相机组件
                Camera camera = cameraOb.GetComponent<Camera>();
                if (camera == null || ! camera.isActiveAndEnabled)
                    continue;

                // 设置当前相机
                mSceneCamera = camera;
                break;
            }

            // 返回当前场景相机
            return mSceneCamera;
        }
        private set
        {
            mSceneCamera = value;
        }
    }

    /// <summary>
    /// UI相机
    /// </summary>
    /// <value>The user interface camera.</value>
    public static Camera UiCamera
    {
        get
        {
            return UICamera.mainCamera;
        }
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// Syncs the load scene.
    /// </summary>
    /// <param name="mainScene">Main scene.</param>
    /// <param name="subScene">Sub scene.</param>
    /// <param name="afterCb">After cb.</param>
    /// <param name="beforeCb">Before cb.</param>
    private static void SyncLoadScene(string mainScene, string subScene, CallBack cb)
    {
        // 如果当前场景已经是需要加载场景不处理，否则需要加载
        string oldScene = SceneManager.GetActiveScene().name;
        if(!string.Equals(oldScene, mainScene))
        {
            // 载入场景
            SceneManager.LoadScene(mainScene);

            // 卸载当前主场景
            UnLoadMainScene(oldScene);

            // 查找场景节点
            foreach (GameObject GameOb in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                // 不是场景节点
                if(!string.Equals(GameOb.name, "SceneRoot"))
                    continue;

                // 重置SceneRoot
                SceneRoot = GameOb.transform;

                break;
            }
        }

        // 场景载入失败
        if (SceneRoot == null)
            return;

        // 获取子场景对象
        GameObject scene = null;

        // 如果SceneObjectMap没有数据则需要重新创建
        if(!string.IsNullOrEmpty(subScene))
        {
            if(! SceneObjectMap.ContainsKey(subScene))
            {
                // 载入资源
                GameObject sceneOb = ResourceMgr.Load(string.Format("Assets/Prefabs/Scene/{0}.prefab", subScene)) as GameObject;

                // 载入场景失败
                if(sceneOb == null)
                {
                    LogMgr.Trace("场景预设{0}不存在, 载入场景失败！", subScene);
                    return;
                }

                // Instantiate一个对象
                scene = GameObject.Instantiate(sceneOb, sceneOb.transform.localPosition, Quaternion.identity) as GameObject;
                scene.name = subScene;
                scene.transform.parent = SceneRoot;

                // 添加到缓存列表中
                SceneObjectMap.Add(subScene, scene);
            } else
            {
                scene = SceneObjectMap[subScene];
            }

            // 载入子场景失败
            if(scene == null)
                return;

            // 激活场景节点
            scene.SetActive(true);

            // 如果是主城或者是世界地图需要记录一下
            // 目前只是需要记录这两个场景
            if (string.Equals(subScene, SceneConst.SCENE_MAIN_CITY) ||
                string.Equals(subScene, SceneConst.SCENE_WORLD_MAP))
                MainScene = subScene;

            // 销毁场景对象
            foreach (GameObject ob in SceneObjectMap.Values)
            {
                // 对象不存在
                if(ob == null)
                    continue;

                // 如果是当前场景节点不处理
                if(ob == scene)
                    continue;

                // 设置节点为非激活状态
                ob.SetActive(false);
            }

            // 执行回调，执行场景载入前相关处理
            // 比如场景加载需要一个闪屏过度表现
            if(cb != null)
                cb.Go();

            // 播放相应的场景背景音效
            GameSoundMgr.PlayBgmMusic(subScene);
        } else
        {
            // 执行回调
            if(cb != null)
                cb.Go();
        }
    }

    /// <summary>
    /// 异步fire事件实际处理函数
    /// </summary>
    private static IEnumerator NonSyncLoadScene(string mainScene, string subScene, CallBack cb)
    {
        // 异步载入场景资源， 首先必须保证场景资源存在
        string path = string.Empty;
        if(! string.IsNullOrEmpty(subScene))
        {
            path = string.Format("Assets/Prefabs/Scene/{0}.prefab", subScene);
            yield return Coroutine.DispatchService(ResourceMgr.LoadAsync(path));
        }

        // 如果当前场景已经是需要加载场景不处理，否则需要加载
        string oldScene = SceneManager.GetActiveScene().name;
        if(!string.Equals(oldScene, mainScene))
        {
            // 载入场景
            AsyncOperation async = SceneManager.LoadSceneAsync(mainScene);

            // 等待Scene加载结束
            yield return async;

            // 卸载当前主场景
            UnLoadMainScene(oldScene);

            // 查找场景节点
            foreach (GameObject GameOb in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                // 不是场景节点
                if(!string.Equals(GameOb.name, "SceneRoot"))
                    continue;

                // 重置SceneRoot
                SceneRoot = GameOb.transform;

                break;
            }
        }

        // 场景载入失败
        if (SceneRoot == null)
            yield break;

        // 获取子场景对象
        GameObject scene = null;

        // 如果SceneObjectMap没有数据则需要重新创建
        if(!string.IsNullOrEmpty(subScene))
        {
            if(!SceneObjectMap.ContainsKey(subScene))
            {
                // 载入资源
                GameObject sceneOb = ResourceMgr.Load(path) as GameObject;

                // 载入场景失败
                if(sceneOb == null)
                {
                    LogMgr.Trace("场景预设{0}不存在, 载入场景失败！", path);
                    yield break;
                }

                // Instantiate一个对象
                scene = GameObject.Instantiate(sceneOb, sceneOb.transform.localPosition, Quaternion.identity) as GameObject;
                scene.name = subScene;
                scene.transform.parent = SceneRoot;

                // 添加到缓存列表中
                SceneObjectMap.Add(subScene, scene);
            } else
            {
                scene = SceneObjectMap[subScene];
            }

            // 载入子场景失败
            if(scene == null)
                yield break;

            // 激活场景节点
            scene.SetActive(true);

            // 如果是主城或者是世界地图需要记录一下
            // 目前只是需要记录这两个场景
            if (string.Equals(subScene, SceneConst.SCENE_MAIN_CITY) ||
                string.Equals(subScene, SceneConst.SCENE_WORLD_MAP))
                MainScene = subScene;

            // 销毁场景对象
            foreach (GameObject ob in SceneObjectMap.Values)
            {
                // 对象不存在
                if(ob == null)
                    continue;

                // 如果是当前场景节点不处理
                if(ob == scene)
                    continue;

                // 设置节点为非激活状态
                ob.SetActive(false);
            }

            // 执行回调，执行场景载入前相关处理
            // 比如场景加载需要一个闪屏过度表现
            if(cb != null)
            {
                cb.Go();
                yield return null;
            }

            // 播放相应的场景背景音效
            GameSoundMgr.PlayBgmMusic(subScene);
        } else
        {
            // 执行回调
            if(cb != null)
            {
                cb.Go();
                yield return null;
            }
        }
    }

    /// <summary>
    /// 卸载主场景
    /// </summary>
    private static void UnLoadMainScene(string mainScene)
    {
        // 重置场景相关数据
        SceneObjectMap.Clear();
        SceneRoot = null;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 载入场景
    /// </summary>
    /// <param name="mainScene">主场景</param>
    /// <param name="cb">场景载入后回调</param>
    /// <param name="isSync">是否同步载入</param>
    public static void LoadScene(string mainScene, CallBack cb, bool isSync = false)
    {
        // 载入场景
        LoadScene(mainScene, null, cb, isSync);
    }

    /// <summary>
    /// 载入场景
    /// </summary>
    /// <param name="mainScene">主场景</param>
    /// <param name="subScene">子场景</param>
    /// <param name="cb">场景载入后回调</param>
    /// <param name="isSync">是否同步处理</param>
    public static void LoadScene(string mainScene, string subScene, CallBack cb, bool isSync = false)
    {
        // 先停止原来的协程
        Coroutine.StopCoroutine("SceneMgrLoadScene");

        if (isSync)
        {
            SyncLoadScene(mainScene, subScene, cb);
        }
        else
        {
            // 异步调用
            Coroutine.DispatchService(NonSyncLoadScene(mainScene, subScene, cb), "SceneMgrLoadScene");
        }
    }

    /// <summary>
    /// 判断执行场景是否激活
    /// </summary>
    public static bool IsActiveScene(string subScene)
    {
        // 没有该子场景
        if (! SceneObjectMap.ContainsKey(subScene))
            return false;

        // 获取场景对象
        GameObject sceneOb = SceneObjectMap[subScene];

        // 场景节点对象不存在
        if (sceneOb == null)
            return false;

        // 返回窗口是否已经激活
        return sceneOb.activeSelf;
    }

    /// <summary>
    /// 获取当前激活场景
    /// </summary>
    public static GameObject GetActiveScene()
    {
        GameObject activeScene = null;

        // 销毁场景对象
        foreach (GameObject ob in SceneObjectMap.Values)
        {
            // 对象不存在
            // 或者场景节点没有激活
            if (ob == null || ! ob.activeSelf)
                continue;

            // 销毁对象
            activeScene = ob;
            break;
        }

        // 返回当前激活场景
        return activeScene;
    }

    /// <summary>
    /// 卸载子场景
    /// </summary>
    /// <param name="mainScene">Main scene.</param>
    /// <param name="subScene">Sub scene.</param>
    public static void UnLoadSubScene(string subScene)
    {
        // 没有该子场景
        if (! SceneObjectMap.ContainsKey(subScene))
            return;

        // 获取场景对象
        GameObject sceneOb = SceneObjectMap[subScene];

        // 将场景节点从缓存列表中移除
        SceneObjectMap.Remove(subScene);

        // 场景节点对象不存在
        if (sceneOb == null)
            return;

        // 设置节点为非激活状态
        sceneOb.SetActive(false);

        // 销毁场景对象
        GameObject.DestroyObject(sceneOb);
    }

    #endregion
}
