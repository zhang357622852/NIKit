/// <summary>
/// GameSoundMgr.cs
/// Created by fucj 2015-01-05
/// 游戏音效管理
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

// 轨迹方向类型
public enum FadeType
{
    NONE, // 没有渐变
    IN,   // 渐入
    OUT,  // 渐出
}

/// <summary>
/// GameAudio音效基类
/// </summary>
public class GameAudio
{
    #region 变量

    /// <summary>
    /// GameAudio唯一名字
    /// </summary>
    private string mName = string.Empty;

    /// <summary>
    /// 该对象持有音源
    /// </summary>
    private AudioSource mAudioSource = null;

    /// <summary>
    /// 音效资源名
    /// </summary>
    private string mAudioName = string.Empty;

    /// <summary>
    /// 是否是循环音效
    /// </summary>
    private bool mIsLoop = false;

    /// <summary>
    /// 是否是背景音效
    /// </summary>
    private bool mIsBgmAudio = false;

    /// <summary>
    /// 唯一cookie
    /// </summary>
    private string mCookie = string.Empty;

    /// <summary>
    /// 音效渐出渐入
    /// </summary>
    private float mFadeTime = 0f;
    private float mFadeRemainTime = 0f;
    private FadeType mFadeType = FadeType.NONE;

    /// <summary>
    /// 音效声音缩放
    /// </summary>
    private float mVolumeScaleDelta = 0f;
    private float mVolumeScale = 1f;
    private float mScaleFadeRemainTime = 0f;

    #endregion

    #region 属性

    /// <summary>
    /// 音效唯一名
    /// </summary>
    public string Name
    {
        get
        {
            return mName;
        }
    }

    /// <summary>
    /// 音效名
    /// </summary>
    public string AudioName
    {
        get
        {
            return mAudioName;
        }
    }

    /// <summary>
    /// 是否是背景音效
    /// </summary>
    public bool IsBgmAudio
    {
        get
        {
            return mIsBgmAudio;
        }
    }

    /// <summary>
    /// 音效是否已经结束
    /// </summary>
    public bool IsEnd { get; private set; }

    /// <summary>
    /// 音效已经开始播放
    /// </summary>
    public bool IsPlayed { get; private set; }

    #endregion

    #region 内部接口

    /// <summary>
    /// 协程播放音效
    /// </summary>
    private IEnumerator DoPlayMusic(string name, bool isStartMusic)
    {
        string path = string.Format("{0}{1}", 
            isStartMusic ? "Assets/Art/Sounds/Start/" : "Assets/Art/Sounds/", name);

        // 异步载入资源
        yield return Coroutine.DispatchService(ResourceMgr.LoadAsync(path));

        // 载入音效资源
        AudioClip clip = ResourceMgr.Load(path) as AudioClip;

        // 载入音效片段失败
        if (clip == null)
        {
            LogMgr.Trace("{0}声音文件加载失败。", name);
            yield break;
        }

        // 重置音源相关属性
        mAudioSource.enabled = true;
        mAudioSource.volume = (mFadeType == FadeType.NONE) ? GetVolume() * mVolumeScale : 0f;
        mAudioSource.clip = clip;
        mAudioSource.loop = mIsLoop;
        mAudioSource.time = 0f;
        mAudioSource.Play();

        // 标识音效已经开始播放
        IsPlayed = true;
    }

    /// <summary>
    /// Checks the is end.
    /// </summary>
    /// <returns><c>true</c>, if is end was checked, <c>false</c> otherwise.</returns>
    private bool CheckIsEnd()
    {
        // 音效还没有开始播放
        if (! IsPlayed)
            return false;

        // 如果是已经结束的音效
        if (IsEnd)
            return true;

        // 音效已经播放结束
        if (! mAudioSource.isPlaying)
            return true;

        // 如果是FadeOut并且已经渐变结束，则表示需要结束该音效
        if (mFadeType == FadeType.OUT && mFadeRemainTime < 0f)
            return true;

        // 标识还没有结束
        return false;
    }

    /// <summary>
    /// Fade the specified delta.
    /// </summary>
    /// <param name="delta">Delta.</param>
    private void DoFade(float delta)
    {
        // 音效音量不需要发生变化
        if (mFadeRemainTime < 0f && mScaleFadeRemainTime < 0f)
            return;

        // 计算音效音量变化量
        if (mScaleFadeRemainTime > 0)
        {
            // 计算新的声音音量缩放
            mVolumeScale += (mVolumeScaleDelta * Mathf.Min(delta, mScaleFadeRemainTime));

            // 剩余时间减去delta
            mScaleFadeRemainTime -= delta;
        }

        // 设置音效渐变音量
        float volume = 0;
        if (mFadeRemainTime > 0)
        {
            // 扣除渐出剩余时间
            mFadeRemainTime -= delta;

            if (mFadeType == FadeType.IN)
                volume = GetVolume() * Mathf.Clamp01(1f - mFadeRemainTime / mFadeTime);
            else if (mFadeType == FadeType.OUT)
                volume = GetVolume() * Mathf.Clamp01(mFadeRemainTime / mFadeTime);
            else
                volume = GetVolume();
        }
        else
        {
            volume = GetVolume();
        }

        // 设置音效音量
        mAudioSource.volume = volume * mVolumeScale;
    }

    /// <summary>
    /// Gets the volume.
    /// </summary>
    /// <returns>The volume.</returns>
    private float GetVolume()
    {
        // 背景音效和游戏音效区分
        if (IsBgmAudio)
            return GameSoundMgr.GetMusicVolume();
        else
            return GameSoundMgr.GetSoundVolume();
    }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="audioSource">Audio source.</param>
    /// <param name="audioName">Audio name.</param>
    /// <param name="isLoop">If set to <c>true</c> is loop.</param>
    /// <param name="isBgmAudio">If set to <c>true</c> is bgm audio.</param>
    public GameAudio(string name, AudioSource audioSource, string audioName, bool isLoop, bool isBgmAudio)
    {
        // 重置相关参数
        mName = name;
        mAudioSource = audioSource;
        mAudioName = audioName;
        mIsLoop = isLoop;
        mIsBgmAudio = isBgmAudio;
        mCookie = Game.NewCookie("GameAudio");
    }

    /// <summary>
    /// 开始播放音效
    /// </summary>
    public void Start(FadeType type = FadeType.NONE, float fadeTime = 0f, bool isStartMusic = false)
    {
        // 标识渐变类型
        mFadeType = type;

        // 音效渐入时间
        mFadeTime = fadeTime;
        mFadeRemainTime = mFadeTime;

        // 在协程中播放音效
        Coroutine.DispatchService(DoPlayMusic(mAudioName, isStartMusic), mCookie);
    }

    /// <summary>
    /// 停止播放音效
    /// </summary>
    public void Stop(FadeType type = FadeType.NONE, float fadeTime = 0f)
    {
        // 先结束当前播放背景音乐线程
        Coroutine.StopCoroutine(mCookie);

        // 标识渐变类型
        mFadeType = type;

        // 音效渐入时间
        mFadeTime = fadeTime;
        mFadeRemainTime = mFadeTime;

        // 音效还没有开始播放, 则直接设置音效结束
        // 如果音效不需要渐出，则直接结束音效
        if (! IsPlayed || mFadeType == FadeType.NONE)
            IsEnd = true;
    }

    /// <summary>
    /// 设置音效
    /// </summary>
    /// <param name="volume">Volume.</param>
    public void SetVolume(float volume)
    {
        // 音效还没有开始播放或者已经播放结束
        if (mAudioSource == null || ! mAudioSource.isActiveAndEnabled || ! mAudioSource.isPlaying)
            return;

        // 如果正在渐入渐出中不处理
        if (mFadeType != FadeType.NONE && mFadeRemainTime > 0f)
            return;

        // 设置音效
        mAudioSource.volume = volume;
    }

    /// <summary>
    /// Sets the volume fade scale.
    /// </summary>
    /// <param name="scale">Scale.</param>
    /// <param name="fadeTime">Fade time.</param>
    public void SetVolumeFadeScale(float scale, float fadeTime)
    {
        // 计算音效音量变化量
        mVolumeScaleDelta = scale - mVolumeScale;

        // 设置变化时间
        mScaleFadeRemainTime = fadeTime;
    }

    /// <summary>
    /// Update the specified delta.
    /// </summary>
    /// <param name="delta">Delta.</param>
    public void Update(float delta)
    {
        // 如果已经停止播放
        if (mAudioSource == null ||
            ! mAudioSource.isActiveAndEnabled)
            return;

        // 判断音效是否需要stop
        if (CheckIsEnd())
        {
            // 停止音效
            mAudioSource.Stop();

            // 释放mAudioSource音源
            if (mAudioSource != null)
                GameSoundMgr.FreeAudioSource(mAudioSource);

            // 删除音效
            mAudioSource = null;

            // 标识IsEnd
            IsEnd = true;

            return;
        }

        // 执行音效渐变处理
        DoFade(delta);
    }
}

/// <summary>
/// 游戏音效管理器
/// </summary>
public static class GameSoundMgr
{
    #region 成员变量

    // 当前播放的背景音乐
    private static string mCurrentMusic = string.Empty;

    // 别名状态映射表
    private static Dictionary<string, List<string>> mBgmMap = new Dictionary<string, List<string>>();

    // 空闲的AudioSource列表
    private static List<AudioSource> mAudioSourceList;

    /// <summary>
    /// 正在播放音效列表
    /// </summary>
    private static List<GameAudio> mGameAudioList = new List<GameAudio>();

    /// <summary>
    /// 正在使用中的音源列表
    /// </summary>
    private static List<AudioSource> mUseAudioSourceList = new List<AudioSource>();

    // 音效详细信息
    private static CsvFile mSoundCsv;

    /// <summary>
    /// 音效渐变时间间隔
    /// </summary>
    private static float mFadeInterval = 1f;

    // 两种音效音量缓存
    private static Dictionary<string, float> mCacheVolumeDict = new Dictionary<string, float>();

    #endregion

    #region 属性

    // 配置表信息
    public static CsvFile BgmMusicCsv { set; get; }

    #endregion

    #region 内部函数

    /// <summary>
    /// 加载bgm配置文件
    /// </summary>UC94ISKD
    private static void LoadMusicConfig()
    {
        mBgmMap.Clear();

        // 载入状态配置表信息
        BgmMusicCsv = CsvFileMgr.Load("music_bgm");

        // 构造状态别名映射表
        foreach (CsvRow row in BgmMusicCsv.rows)
        {
            string group = row.Query<string>("group");
            mBgmMap.Add(group, new List<string>());

            // 添加列表
            LPCArray musicList = row.Query<LPCArray>("music_list");
            foreach (LPCValue musicFile in musicList.Values)
                mBgmMap[group].Add(musicFile.AsString);
        }
    }

    /// <summary>
    /// 获取一个空闲的AudioSource
    /// </summary>
    private static AudioSource GetFreeAudioSource()
    {
        // mAudioSourceList还没有初始化
        if (mAudioSourceList == null)
        {
            // 初始化mAudioSourceList
            mAudioSourceList = new List<AudioSource>();

            // 获取AudioSource组件列表
            AudioSource[] audioList = GameRoot.RootGameObject.GetComponents<AudioSource>();
            mAudioSourceList.AddRange(audioList);
        }

        // 现有列表选取空闲的AudioSource，如果选择不到则重新创建一个AudioSource
        AudioSource audioSource = null;
        for (int i = 0; i < mAudioSourceList.Count; i++)
        {
            // 获取AudioSource
            audioSource = mAudioSourceList[i];

            // 如果audioSource对象不存在或者audioSource正在使用中
            if (audioSource == null ||
                mUseAudioSourceList.IndexOf(audioSource) != -1)
                continue;

            // 查找到了空闲AudioSource
            mUseAudioSourceList.Add(audioSource);
            return audioSource;
        }

        if (GameRoot.RootGameObject == null)
            return audioSource;

        // 添加新的音效组件
        audioSource = GameRoot.RootGameObject.AddComponent<AudioSource>();
        mAudioSourceList.Add(audioSource);
        mUseAudioSourceList.Add(audioSource);

        // 返回audioSource
        return audioSource;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 初始化数据
        mCacheVolumeDict = new Dictionary<string, float>();

        // 音效详细信息
        mSoundCsv = CsvFileMgr.Load("sound");

        // 如果是验证客户端不需要载入背景音效
        if (AuthClientMgr.IsAuthClient)
            return;

        // 加载配置文件
        LoadMusicConfig();
    }

    /// <summary>
    /// 获取音效时长
    /// </summary>
    public static float GetSoundLength(string sound)
    {
        // 没有配置信息
        if (mSoundCsv == null)
            return 0f;

        // 音效数据不存在
        CsvRow data = mSoundCsv.FindByKey(sound);
        if (data == null)
            return 0f;

        // 音效时间长度
        LPCValue length = data.Query<LPCValue>("length");
        if (length == null)
            return 0f;

        // 返回音效时间长度
        return length.AsFloat;
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    public static void Update()
    {
        // 目前使用的单音源
        // current淡出完成后，next才接入
        // 如有其他混合需求，另议
        if (GameRoot.RootGameObject == null)
            return;

        // 获取delta time
        float delta = Time.unscaledDeltaTime;
        int index = 0;

        do
        {
            // 已经越界
            if (index >= mGameAudioList.Count)
                break;

            // 获取GameAudio
            GameAudio gameAudio = mGameAudioList[index];

            // 更新音效
            gameAudio.Update(delta);

            // 如果音效需要不需要结束
            if (gameAudio.IsEnd)
            {
                // 移除数据
                mGameAudioList.Remove(gameAudio);
                continue;
            }

            // index++
            index++;

        } while(true);
    }

    /// <summary>
    /// 释放audioSource
    /// </summary>
    public static void FreeAudioSource(AudioSource audioSource)
    {
        // 将audioSource从使用列表中移除
        mUseAudioSourceList.Remove(audioSource);

        // 重置音源相关属性
        audioSource.Stop();
        audioSource.enabled = false;
        audioSource.volume = 0f;
        audioSource.clip = null;
        audioSource.loop = false;
        audioSource.time = 0f;
    }

    /// <summary>
    /// UI 点击事件
    /// </summary>
    public static void OnUIClicked()
    {
        // 播放click组音效
        PlayGroupSound("click", string.Empty);
    }

    /// <summary>
    /// 播放音源(单独的音源)
    /// </summary>
    public static void PlayGroupSound(string group)
    {
        // 播放游戏音效
        PlayGroupSound(group, string.Empty, FadeType.NONE, 0f);
    }

    /// <summary>
    /// Plaies the group sound.
    /// </summary>
    /// <param name="group">Group.</param>
    /// <param name="name">Name.</param>
    public static void PlayGroupSound(string group, string name, FadeType type = FadeType.NONE, float fadeTime = 0f)
    {
        List<string> musicList;
        if (!mBgmMap.TryGetValue(group, out musicList) || musicList.Count <= 0)
        {
            LogMgr.Trace("指定的bgm组{0}不存在或没有配置音乐。", group);
            return;
        }

        // 随机一个播放音效
        int index = Random.Range(0, musicList.Count);

        // new一个GameAudio
        // (string name, AudioSource audioSource, string audioName, bool isLoop, bool isBgmAudio)
        GameAudio gameAudio = new GameAudio(
            string.IsNullOrEmpty(name) ? Game.GetUniqueName(group) : name,
            GetFreeAudioSource(),
            musicList[index],
            (BgmMusicCsv.FindByKey(group).Query<int>("is_loop") == 1),
            false
        );

        // 添加到列表中统一处理
        mGameAudioList.Add(gameAudio);

        // 开始播放音效
        gameAudio.Start(type, fadeTime);
    }

    /// <summary>
    /// 设置音效声音(目前只有两种音效（背景音效和游戏音效）)
    /// </summary>
    public static void SetAudioVolumeFadeScale(float scale, float fadeTime, bool isBgmAudio = true)
    {
        // 如果是背景音效
        if (isBgmAudio)
        {
            // 重置正在播放的背景音效音量
            foreach (GameAudio ob in mGameAudioList)
            {
                // 不是背景音效不处理
                if (! ob.IsBgmAudio)
                    continue;

                // 设置音效音量
                ob.SetVolumeFadeScale(scale, fadeTime);
            }
        }
        else
        {
            // 重置正在播放的背景音效音量
            foreach (GameAudio ob in mGameAudioList)
            {
                // 是背景音效不处理
                if (ob.IsBgmAudio)
                    continue;

                // 设置音效音量
                ob.SetVolumeFadeScale(scale, fadeTime);
            }
        }
    }

    /// <summary>
    /// 播放指定音乐方案组
    /// </summary>
    /// <param name="group">Group.</param>
    public static void PlayBgmMusic(string group)
    {
        List<string> musicList;
        if (! mBgmMap.TryGetValue(group, out musicList) || musicList.Count <= 0)
            return;

        // 随机一个播放音效
        int index = Random.Range(0, musicList.Count);
        string audioName = musicList[index];

        // 记录当前正在播放背景音效
        if (string.Equals(mCurrentMusic, audioName))
            return;

        // 记录当前正在播放背景音效
        mCurrentMusic = audioName;

        // new一个GameAudio
        // (string name, AudioSource audioSource, string audioName, bool isLoop, bool isBgmAudio, bool isMainAudio)
        GameAudio gameAudio = new GameAudio(
            Game.GetUniqueName(group),
            GetFreeAudioSource(),
            musicList[index],
            (BgmMusicCsv.FindByKey(group).Query<int>("is_loop") == 1),
            true
        );

        // 让原来的背景音效淡出
        foreach (GameAudio ob in mGameAudioList)
        {
            // 不是背景音效不处理
            if (! ob.IsBgmAudio)
                continue;

            // 原来的音效淡出
            ob.Stop(FadeType.OUT, mFadeInterval);
        }

        // 添加到列表中统一处理
        mGameAudioList.Add(gameAudio);

        // 开始播放音效
        gameAudio.Start(FadeType.IN, mFadeInterval);
    }

    /// <summary>
    /// 播放初始指定音乐
    /// </summary>
    /// <param name="group">Group.</param>
    public static void PlayStartBgmMusic(string name, bool isLoop)
    {
        // 记录当前正在播放背景音效
        if (string.Equals(mCurrentMusic, name))
            return;

        // 记录当前正在播放背景音效
        mCurrentMusic = name;

        // new一个GameAudio
        // (string name, AudioSource audioSource, string audioName, bool isLoop, bool isBgmAudio, bool isMainAudio)
        GameAudio gameAudio = new GameAudio(
            Game.GetUniqueName(name),
            GetFreeAudioSource(),
            name,
            isLoop,
            true
        );

        // 让原来的背景音效淡出
        foreach (GameAudio ob in mGameAudioList)
        {
            // 不是背景音效不处理
            if (! ob.IsBgmAudio)
                continue;

            // 原来的音效淡出
            ob.Stop(FadeType.OUT, mFadeInterval);
        }

        // 添加到列表中统一处理
        mGameAudioList.Add(gameAudio);

        // 开始播放音效
        gameAudio.Start(FadeType.IN, mFadeInterval, true);
    }

    /// <summary>
    /// 停止播放全部背景音效
    /// </summary>
    /// <param name="name">Name.</param>
    public static void StopBgSound()
    {
        int index = 0;

        do
        {
            if (index >= mGameAudioList.Count)
                break;

            // 获取GameAudio
            GameAudio ob = mGameAudioList[index];

            index++;

            // 不是背景音效不处理
            if (! ob.IsBgmAudio)
                continue;

            // 停止音效
            ob.Stop(FadeType.NONE, 0f);

        } while(true);

        // 清除当前播放背景音效
        mCurrentMusic = string.Empty;
    }

    /// <summary>
    /// 停止播放音效
    /// </summary>
    /// <param name="name">Name.</param>
    public static void StopSound(string name, FadeType type = FadeType.OUT, float fadeTime = 0f)
    {
        int index = 0;

        do
        {
            if (index >= mGameAudioList.Count)
                break;

            // 获取GameAudio
            GameAudio ob = mGameAudioList[index];

            index++;

            // 不是需要查找的音效
            if (! string.Equals(ob.Name, name))
                continue;

            // 停止音效
            ob.Stop(type, fadeTime);

        } while(true);
    }

    /// <summary>
    /// 设置背景音乐的大小
    /// </summary>
    /// <param name="value">Value.</param>
    public static void SetMusicVolume(float value)
    {
        // 重置正在播放的背景音效音量
        foreach (GameAudio ob in mGameAudioList)
        {
            // 不是背景音效不处理
            if (! ob.IsBgmAudio)
                continue;

            // 设置音效音量
            ob.SetVolume(value);
        }

        // 缓存临时数据
        mCacheVolumeDict["music_volume"] = value;

        // 保存背景音乐声音设置
        OptionMgr.SetPublicOption("music_volume", LPCValue.Create(value));
    }

    /// <summary>
    /// 设置音效大小
    /// </summary>
    /// <param name="value">Value.</param>
    public static void SetSoundVolume(float value)
    {
        // 重置正在播放的游戏音效音量
        foreach (GameAudio ob in mGameAudioList)
        {
            // 不是背景音效不处理
            if (ob.IsBgmAudio)
                continue;

            // 设置音效音量
            ob.SetVolume(value);
        }

        // 缓存临时数据
        mCacheVolumeDict["sound_volume"] = value;

        // 保存设置
        OptionMgr.SetPublicOption("sound_volume", LPCValue.Create(value));
    }

    /// <summary>
    /// 取得背景音乐声音的大小
    /// </summary>
    public static float GetMusicVolume()
    {
        float volume;

        // 判断是否有缓存数据
        if (mCacheVolumeDict.TryGetValue("music_volume", out volume))
            return volume;

        // 本地有缓存音量大小使用本地缓存，反之使用默认配置
        if(PlayerPrefs.HasKey("music_volume"))
            volume = LPCRestoreString.RestoreFromString(PlayerPrefs.GetString("music_volume")).AsFloat;
        else
            volume = GameSettingMgr.GetSetting<LPCValue>("default_music_volume", LPCValue.Create(1f)).AsFloat;

        // 缓存临时数据
        mCacheVolumeDict["music_volume"] = volume;

        // 返回音量
        return volume;
    }

    /// <summary>
    /// 取得游戏音效的大小
    /// </summary>
    public static float GetSoundVolume()
    {
        float volume;

        // 判断是否有缓存数据
        if (mCacheVolumeDict.TryGetValue("sound_volume", out volume))
            return volume;

        // 本地有缓存音量大小使用本地缓存，反之使用默认配置
        if (PlayerPrefs.HasKey("sound_volume"))
            volume = LPCRestoreString.RestoreFromString(PlayerPrefs.GetString("sound_volume")).AsFloat;
        else
            volume = GameSettingMgr.GetSetting<LPCValue>("default_sound_volume", LPCValue.Create(1f)).AsFloat;

        // 缓存临时数据
        mCacheVolumeDict["sound_volume"] = volume;

        // 返回音量
        return volume;
    }

    #endregion
}
