/// <summary>
/// MainSceneState.cs
/// Created by WinMi 2018/10/23
///  主场景/登入场景
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneState : ISceneState
{
    private const string SCENE_NAME = "MainScene";

    public MainSceneState(SceneStateController controller) : base(SCENE_NAME, controller)
    {

    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    private void OnClickStartGame()
    {
    }
}
