using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillLevelUpWnd : WindowBase<SkillLevelUpWnd>
{
    public GameObject mPetItem;
    public GameObject mCloseBtn;
    public UILabel mSkillNameLb;
    public UILabel mSkillLevLb;
    public UILabel mContent;
    public UILabel mTitleLb;
    public GameObject mMask;

    int skill_id;
    float delay;
    int level;

    CallBack task;

    /// <summary>
    /// The m cooke.
    /// </summary>
    string mCooke = string.Empty;

    // Use this for initialization
    void Start ()
    {
        mTitleLb.text = LocalizationMgr.Get("SkillLevelUpWnd_2");

        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mMask).onClick = OnCloseBtn;
    }

    // 刷新窗口
    void Redraw()
    {
        if(skill_id <= 0)
            return;

        // 使用默认延迟
        if(!float.Equals(delay, -1f))
        {
            gameObject.GetComponent<TweenAlpha>().delay = delay;
            gameObject.GetComponent<TweenScale>().delay = delay;
        }

        gameObject.GetComponent<TweenAlpha>().enabled = true;
        gameObject.GetComponent<TweenAlpha>().ResetToBeginning();


        gameObject.GetComponent<TweenScale>().enabled = true;
        gameObject.GetComponent<TweenScale>().ResetToBeginning();

        mPetItem.GetComponent<SkillItem>().SetBind(skill_id);
        mPetItem.GetComponent<SkillItem>().SetLevel(level);

        mSkillNameLb.text = SkillMgr.GetSkillName(skill_id);
        mSkillLevLb.text = SkillMgr.GetSingleLevelDesc(skill_id, level);

        mContent.text = GET_SKILL_SUM_DESC.CALL(skill_id, level);
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow(gameObject.name);

        if (task != null)
            task.Go(true, mCooke, SkillLevelUpWnd.WndType);
    }


    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="args">Arguments.</param>
    public void ShowWindow(Dictionary<string, object> args)
    {
        // 检查数据规范
        if (!args.ContainsKey("skill_id") || !args.ContainsKey("level"))
            return;

        skill_id = (int)args["skill_id"];
        level = (int)args["level"];
        delay = args.ContainsKey("delay") ? (float)args["delay"] : 0f;
        task = args.ContainsKey("call_back") ? (CallBack)args["call_back"] : null;

        // 获取打开窗口的cookie
        if (args.ContainsKey("cookie"))
            mCooke = (string) args["cookie"];

        Redraw();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="_task">Task.</param>
    /// <param name="skillId">Skill identifier.</param>
    /// <param name="_level">Level.</param>
    /// <param name="_delay">Delay.</param>
    public void SetBind(CallBack _task, int skillId, int _level, float _delay = 0f)
    {
        // 重置绑定对象
        skill_id = skillId;
        delay = _delay;
        level = _level;

        task = _task;

        Redraw();
    }

    #endregion
}
