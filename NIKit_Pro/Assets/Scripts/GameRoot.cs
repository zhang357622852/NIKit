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
    void Start ()
    {
        SceneMgr.Instance.SwitchScene(new StartSceneState(SceneMgr.Instance.CurSceneController), false);
	}

    private void Update()
    {
        SceneMgr.Instance.Update();
    }
}
