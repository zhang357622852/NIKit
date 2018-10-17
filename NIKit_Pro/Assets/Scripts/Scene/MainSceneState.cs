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
        GameObject.Find("StartGameBtn").GetComponent<Button>().onClick.AddListener(OnClickStartGame);
    }

    private void OnClickStartGame()
    {
        mController.SetState(new BattleSceneState(mController));
    }
}
