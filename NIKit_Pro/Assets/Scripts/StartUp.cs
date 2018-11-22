using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUp : MonoBehaviour
{

	// Use this for initialization
	void Awake ()
    {
        CreateGameRoot();

        GameObject.Destroy(gameObject);
    }

    private void CreateGameRoot()
    {
        GameObject go = new GameObject("GameRoot");

        GameObject.DontDestroyOnLoad(go);

        // 添加音效组件，用于bgm唯一播放
        // 两个音效之间切换需要叠加淡入淡出（所以这个地方增加两个AudioSource）
        go.AddComponent<AudioSource>();
        go.AddComponent<AudioSource>();
        go.AddComponent<AudioSource>();
        go.AddComponent<AudioSource>();
        go.AddComponent<AudioSource>();
        go.AddComponent<AudioListener>();

        // 逻辑驱动Scheduler
        go.AddComponent<Scheduler>();
    }
}
