/// <summary>
/// StartSceneState.cs
/// Created by WinMi 2018/10/23
///  开始场景,游戏从这个场景开始
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneState : ISceneState
{
    private const string SCENE_NAME = "StartScene";

    public StartSceneState(SceneStateController controller) : base(SCENE_NAME, controller)
    {

    }

    public override void Enter()
    {

    }

    public override void Exit()
    {
    }

    public override void UpdateState()
    {
    }
}
