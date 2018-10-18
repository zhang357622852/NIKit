/// <summary>
/// TalkBoxWnd.cs
/// Created by fengsc 2017/10/24
/// 对话框窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TalkBoxWnd : WindowBase<TalkBoxWnd>
{
    // 遮罩
    public GameObject mMask;

    // npc名称
    public UILabel mNameLb;

    // 对话内容
    public UILabel mContentLb;

    // 打字机特效控件
    public TypewriterEffectEx mTypewriterEffect;

    string mName;

    LPCValue mContent;

    // 每秒显示的字数
    int mCharsPerSecond = 0;

    string mCacheStr = string.Empty;

    int mIndex = 0;

    CallBack mCallBack;

    bool mLabelEffect = false;

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mMask).onClick = OnClickMask;

        // 注册打字机效果执行完成事件
        EventDelegate.Add(mTypewriterEffect.onFinished, TypewriterEffectOnFinish);
    }

    /// <summary>
    /// 打字机效果执行完成回调
    /// </summary>
    void TypewriterEffectOnFinish()
    {
        if (mContent == null)
            return;

        if (mContent.IsArray)
        {
            if (mIndex + 1 <= mContent.AsArray.Count)
                return;
        }
        else
        {
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (! mLabelEffect)
        {
            mTypewriterEffect.enabled = false;
        }
        else
        {
            mTypewriterEffect.enabled = true;

            // 默认每秒显示字数
            if (mCharsPerSecond > 0)
                mTypewriterEffect.charsPerSecond = mCharsPerSecond;
            else
                mTypewriterEffect.charsPerSecond = GameSettingMgr.GetSettingInt("default_chars_per_second");
        }

        // 显示npc名称
        mNameLb.text = LocalizationMgr.Get(mName);

        if (mContent == null)
            return;

        string userName = string.Empty;

        if (ME.user != null)
            userName = ME.user.GetName();

        if (mContent.IsArray)
        {
            mTypewriterEffect.sectionFill = true;

            if (mIndex + 1 <= mContent.AsArray.Count)
                mCacheStr += string.Format(LocalizationMgr.Get(mContent.AsArray[mIndex].AsString), userName);

            mIndex++;
        }
        else
        {
            mTypewriterEffect.sectionFill = false;

            // 显示对话内容
            mCacheStr = string.Format(LocalizationMgr.Get(mContent.AsString), userName);
        }

        mContentLb.text = mCacheStr;

        // 重置控件
        if (mTypewriterEffect.enabled)
            mTypewriterEffect.Play();
    }

    /// <summary>
    /// mask点击事件
    /// </summary>
    void OnClickMask(GameObject go)
    {
        // 当前没有播放完成,直接显示完成某一段
        if (mTypewriterEffect.isActive)
        {
            mTypewriterEffect.Finish();

            return;
        }

        // 分段显示
        if (mContent.IsArray)
        {
            if (mIndex + 1 <= mContent.AsArray.Count)
            {
                // 重新绘制窗口
                Redraw();

                return;
            }
        }

        // 关闭窗口
        WindowMgr.DestroyWindow(gameObject.name);

        if (mCallBack == null)
            return;

        // 当前阶段指引完成，执行回调
        mCallBack.Go();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(string name, LPCValue content, bool effect, int charsPerSecond, int fontSize, CallBack cb)
    {
        mName = name;

        mContent = content;

        mLabelEffect = effect;

        mIndex = 0;

        mCacheStr = string.Empty;

        mCharsPerSecond = charsPerSecond;

        if (fontSize > 0)
            mContentLb.fontSize = fontSize;
        else
            mContentLb.fontSize = GameSettingMgr.GetSettingInt("default_font_size");

        mTypewriterEffect.ResetToBeginning();

        mCallBack = cb;

        // 绘制窗口
        Redraw();
    }
}
