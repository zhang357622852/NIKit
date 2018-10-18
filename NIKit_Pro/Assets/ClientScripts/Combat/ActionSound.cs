/// <summary>
/// ActionSound.cs
/// Created by wangxw 2015-2-28
/// 音效节点
/// </summary>

using UnityEngine;
using System.Collections;

public class ActionSound : ActionBase
{
    #region 成员变量

    // 音效名
    string mSoundName = string.Empty;

    // 音效后缀
    string mSoundNamePostfix = ".wav";

    // 是否随主节点调整速度
    bool mAdjustSpeed = false;

    // 音效对象
    GameObject mAudioSourceObject = null;

    // 时间长度(-1表示自动时长)
    private float mTime = -1f;

    // 存活时间
    private float mLiveTime = 0f;

    #endregion

    #region 内部函数

    /// <summary>
    /// 创建音效
    /// </summary>
    private bool CreateSound()
    {
        // 声音文件加载失败，不需要播放音效
        if (string.IsNullOrEmpty(mSoundName))
            return false;

        // 创建GameObject对象
        mAudioSourceObject = new GameObject(ActionSet.ActionSetDataName + mSoundName);
        mAudioSourceObject.transform.position = Vector3.zero;

        // 如果没有指定播放时间长度，则获取光效实际长度信息
        if (mTime < 0)
            mTime = GameSoundMgr.GetSoundLength(mSoundName);

        // 如果是验证客户端不需要加载资源
        if (AuthClientMgr.IsAuthClient)
            return true;

        // 加载
        AudioClip clip = ResourceMgr.Load("Assets/Art/Sounds/" + mSoundName + mSoundNamePostfix) as AudioClip;

        // 声音文件加载失败
        if (clip == null)
        {
            LogMgr.Trace("技能{0}的{1}声音文件加载失败。", ActionSet.ActionSetDataName, mSoundName);
            return false;
        }

        // 创建音效对象
        AudioSource audioSource = (AudioSource)mAudioSourceObject.AddComponent(typeof(AudioSource));
        audioSource.clip = clip;
        audioSource.volume = GameSoundMgr.GetSoundVolume();
        audioSource.loop = false;
        audioSource.Play();

        // 随节点缩放时间
        if (mAdjustSpeed)
            audioSource.pitch = ActionSet.TimeScaleFactor;

        // 创建音效成功
        return true;
    }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionSound(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mSoundName = para.GetProperty<string>("sound");
        mSoundNamePostfix = para.GetProperty<string>("sound_postfix", ".wav");
        mAdjustSpeed = para.GetProperty<bool>("adjust_speed", false);
        mTime = para.GetProperty<float>("time", -1f);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 创建音效
        if (! CreateSound())
        {
            IsFinished = true;
            return;
        }
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        // 销毁mAudioSourceObject
        if (mAudioSourceObject)
            GameObject.Destroy(mAudioSourceObject);

        // 结束
        base.End(isCancel);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
        // 判断是否继续播放
        if (mAudioSourceObject == null)
        {
            IsFinished = true;
            return;
        }

        // 记录光效LiveTime
        mLiveTime = mLiveTime + info.DeltaTime * ActionSet.TimeScaleFactor;

        // 如果不是验证客户端需要调整音效效果
        if (! AuthClientMgr.IsAuthClient)
        {
            // 获取音效组件
            AudioSource audioSource = mAudioSourceObject.GetComponent<AudioSource>();
            System.Diagnostics.Debug.Assert(audioSource != null);

            // 更新音效
            audioSource.volume = GameSoundMgr.GetSoundVolume();

            // 更新速度
            if (mAdjustSpeed)
                audioSource.pitch = ActionSet.TimeScaleFactor;
        }

        // 音效还没有播放结束 
        if (mLiveTime < mTime)
            return;

        // 标识音效播放结束
        IsFinished = true;
    }
}
