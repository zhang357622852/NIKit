/// <summary>
/// ISceneState.cs
/// Created by WinMi 2018/10/23
///  场景状态基类,使用状态模式控制场景
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ISceneState
{
    private readonly string mSceneName;

    public string SceneName
    {
        get
        {
            return mSceneName;
        }
    }

    /// <summary>
    /// 状态持有者
    /// </summary>
    protected SceneStateController mController;

    public ISceneState(string sceneName, SceneStateController controller)
    {
        mSceneName = sceneName;

        mController = controller;
    }

    /// <summary>
    /// 进入状态
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// 退出状态
    /// </summary>
    public virtual void Exit() { }

    /// <summary>
    /// 更新状态
    /// </summary>
    public virtual void UpdateState() { }
}
