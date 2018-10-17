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

    private Image mLogo;
    private float mSmoothTime = 1f;
    private float mWaitTime = 2f;
    public override void Enter()
    {
        mLogo = GameObject.Find("Logo").GetComponent<Image>();
        mLogo.color = Color.black;
    }

    public override void UpdateState()
    {
        mLogo.color = Color.Lerp(mLogo.color, Color.white, mSmoothTime * Time.deltaTime);

        mWaitTime -= Time.deltaTime;

        if (mWaitTime <= 0)
            mController.SetState(new MainSceneState(mController));
    }
}
