using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneState : ISceneState
{
    private const string SCENE_NAME = "BattleScene";

    public BattleSceneState(SceneStateController controller) : base(SCENE_NAME, controller)
    {

    }

    public override void Enter()
    {
        //GameFacade.Instance.Init();
    }

    public override void Exit()
    {
        //GameFacade.Instance.Release();
    }

    public override void UpdateState()
    {
        //if (GameFacade.Instance.IsGameOver)
        //{
        //    mController.SetState(new MainSceneState(mController));

        //    return;
        //}

        //GameFacade.Instance.Update();
    }
}
