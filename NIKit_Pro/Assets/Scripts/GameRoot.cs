/// <summary>
/// GameRoot.cs
/// Created by WinMi 2018/11/05
/// 游戏根对象
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    IEnumerator Start ()
    {
        try
        {
            Init();
        }
        catch (Exception e)
        {
            NIDebug.LogException(e);
        }

        yield break;
	}

    private void Init()
    {
        // 添加组件
        CreateMustComponent();

        // 不休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 逻辑+渲染，Unity编辑器中的state帧率统计不准确
        Application.targetFrameRate = 45;

        // 协程初始化
        Coroutine.DispatchService(DoInit());
    }

    /// <summary>
    /// Dos the init.
    /// </summary>
    /// <returns>The init.</returns>
    static IEnumerator DoInit()
    {
        // 加载最基础的配置Config配置
        // 这个配置信息只能在游戏启动的时候就需要加载
        yield return Coroutine.DispatchService(ConfigMgr.Instance.Init());

        //if (!ConfigMgr.InitSuccessed)
        //{
        //    LogMgr.Trace("ConfigMgr初始化失败");
        //    yield break;
        //}

        //// 战斗客户端初始化
        //if (AuthClientMgr.IsAuthClient)
        //{
        //    // 协程中初始化
        //    Coroutine.DispatchService(DoAuthClientInit());
        //}
        //else
        //{
        //    // 协程中初始化
        //    Coroutine.DispatchService(DoNormalClientInit());
        //}
    }

    /// <summary>
    /// 创建所需的组件
    /// </summary>
    private void CreateMustComponent()
    {
        // 添加音效组件，用于bgm唯一播放
        // 两个音效之间切换需要叠加淡入淡出（所以这个地方增加两个AudioSource）
        gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<AudioListener>();

        // 逻辑驱动Scheduler
        gameObject.AddComponent<Scheduler>();

        // 自定义协程
        Coroutine.Instance = gameObject.AddComponent<Coroutine>();
    }

    //private void Update()
    //{
    //    SceneMgr.Instance.Update();
    //}
}
