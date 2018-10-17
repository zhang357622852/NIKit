using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateController
{
    private ISceneState mState;
    private AsyncOperation mAO = null;
    private bool mIsRunEnter = false;

    public void SetState(ISceneState state, bool isLoadScene = true)
    {
        if (state == null)
            return;

        if (mState != null)
            mState.Exit();

        mState = state;
        if (isLoadScene)
        {
            mAO = SceneManager.LoadSceneAsync(mState.SceneName);
            mIsRunEnter = false;
        }
        else
        {
            //本身就在这个场景就无需加载场景了，例如: StartScene
            mState.Enter();
            mIsRunEnter = true;
            mAO = null;
        }
    }

    public void UpdateState()
    {
        if (mState == null)
            return;

        if (mAO != null && !mAO.isDone)
            return;

        if (mAO != null && mAO.isDone && !mIsRunEnter)
        {
            mState.Enter();
            mIsRunEnter = true;
            mAO = null;
        }

        mState.UpdateState();
    }
}
