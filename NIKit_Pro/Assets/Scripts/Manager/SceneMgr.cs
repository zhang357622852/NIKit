using System.Collections;
using System.Collections.Generic;

public class SceneMgr : Singleton<SceneMgr>, IInit, IRelease, IUpdate
{

    private SceneStateController mCurSceneController;

    /// <summary>
    /// 场景控制器
    /// </summary>
    public SceneStateController CurSceneController { get { return mCurSceneController; } }

    #region 公共接口

    public void Init()
    {
        mCurSceneController = new SceneStateController();
    }

    public void Release()
    {
    }

    /// <summary>
    /// 切换场景
    /// </summary>
    /// <param name="state"></param>
    /// <param name="isLoadScene"></param>
    public void SwitchScene(ISceneState state, bool isLoadScene = true)
    {
        if (mCurSceneController == null)
            return;

        mCurSceneController.SetState(state, isLoadScene);
    }

    public void Update()
    {
        if (mCurSceneController == null)
            return;

        mCurSceneController.UpdateState();
    }

    #endregion
}
