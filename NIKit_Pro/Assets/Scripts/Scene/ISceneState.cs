using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISceneState
{
    private string mSceneName;
    public string SceneName
    {
        get
        {
            return mSceneName;
        }
    }
    protected SceneStateController mController;

    public ISceneState(string sceneName, SceneStateController controller)
    {
        mSceneName = sceneName;
        mController = controller;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void UpdateState() { }
}
