/// <summary>
/// SceneRoot.cs
/// Created by zhaozy 2016/06/15
/// 场景根节点
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SceneRoot : MonoBehaviour
{
    #region 变量

    // 场景列表
    public List<GameObject> mSceneList = new List<GameObject>();

    // 主场景相机
    public Transform mainCityCamera = null;

    // 记录位置信息
    public static Vector3 mMainCityCameraPos = Vector3.zero;

    #endregion

    /// <summary>
    /// Awake this instance.
    /// </summary>
    void Awake()
    {
        // 从副本返回主场景需要重置上一次设置的相机位置
        if (mainCityCamera != null && ME.isLoginOk && ! mMainCityCameraPos.Equals(Vector3.zero))
            mainCityCamera.position = mMainCityCameraPos;
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    void OnDestroy()
    {
        // 移除事件监听回调
        EventMgr.UnregisterEvent("SceneRoot");

        // 记录当前主城相机位置
        mMainCityCameraPos = mainCityCamera.position;
    }

    /// <summary>
    /// Dos the switch scene.
    /// </summary>
    private void DoSwitchScene(int eventId, MixedValue para)
    {
        // 不存在的场景sceneName
        string sceneName = para.GetValue<string>();
        if (string.IsNullOrEmpty(sceneName))
            return;

        // 播放相应的场景背景音效
        GameSoundMgr.PlayBgmMusic(sceneName);
        GameObject curSceneOb = null;

        // 先将个个场景SetActive(false)
        foreach (GameObject sceneOb in mSceneList)
        {
            // 隐藏子场景
            sceneOb.SetActive(false);

            // 记录需要激活的场景
            if (!string.Equals(sceneOb.name, sceneName))
                continue;

            // 记录需要激活的sub scene
            curSceneOb = sceneOb;
        }

        // 没有场景需要激活
        if (curSceneOb == null)
            return;

        // 重新激活需要激活的sub scene
        curSceneOb.SetActive(true);
    }
}
