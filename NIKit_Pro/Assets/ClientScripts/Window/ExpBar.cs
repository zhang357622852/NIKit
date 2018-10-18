/// <summary>
/// ExpBar.cs
/// Created by fucj 2014-11-27
/// 经验条对象脚本
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class ExpBar : WindowBase<ExpBar>
{
    /// <summary>
    /// 经验条
    /// </summary>
    public UISlider mSlider;

    /// <summary>
    /// 参战宠物等级
    /// </summary>
    public UILabel mLevel;

    /// <summary>
    /// 参战宠物增加的经验
    /// </summary>
    public UILabel mAddExp;

    /// <summary>
    /// 升级提示
    /// </summary>
    public UILabel mUpgradeTips;

    /// <summary>
    /// 达到最大等级提示
    /// </summary>
    public UILabel mMaxLevelTips;

    // 双倍经验提示
    public UILabel mMultipleTips;

    // 原始位置
    private Vector3 pos = Vector3.one;

    private Vector3 curpos = Vector3.one;

    string mRid = string.Empty;

    LPCMapping clearanceMap = new LPCMapping ();

    //宠物奖励经验;
    int petExp = 0;

    //当前等级的标准经验;
    int stdExp = 0;

    /// <summary>
    /// 当前宠物的经验
    /// </summary>
    int mCurrentExp = 0;

    /// <summary>
    /// 宠物对象
    /// </summary>
    Property mPet;

    //没有增加经验时宠物的经验条百分比;
    float mPercent = 0;

    //每一帧value的增量;
    float increment = 0;

    //没有升级时宠物的等级;
    int beforelevel = 0;

    // Use this for initialization
    void Start ()
    {
        InitLabel();
    }

    void Update()
    {
        //设置经验条的位置;
        AdjustWnd();
    }

    /// <summary>
    /// 调整窗口位置
    /// </summary>
    private void AdjustWnd()
    {
        if(mPet == null)
            return;

        CombatActor actor = mPet.Actor;
        if (actor == null)
            return;

        // 计算当前位置
        Vector3 actorPos = actor.GetPosition();
        curpos.x = actorPos.x;
        curpos.y = actorPos.y + actor.GetHpbarOffestY();
        curpos.z = actorPos.z;

        // 判断位置是否需要变化
        if (Game.FloatEqual((pos - curpos).sqrMagnitude, 0f))
            return;

        // 设置位置
        transform.position = Game.WorldToUI(curpos);
        pos = curpos;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        if(string.IsNullOrEmpty(mRid) || clearanceMap == null)
            return;

        //获取奖励数据;
        LPCMapping bonusMap = clearanceMap.GetValue<LPCMapping>("bonus_map");

        //获取经验奖励;
        LPCMapping expMap = bonusMap.GetValue<LPCMapping>("exp");

        petExp = expMap.GetValue<int>("pet_exp");

        // 显示双倍经验提示信息
        if (expMap.GetValue<int>("multiple") == 2)
            mMultipleTips.gameObject.SetActive(true);

        //获取宠物当前等级;
        beforelevel = mPet.GetLevel();

        //获取宠物当前经验;
        mCurrentExp = mPet.Query<int>("exp");

        //获取当前等级+1的标准经验;
        stdExp = StdMgr.GetPetStdExp(beforelevel + 1, mPet.GetStar());

        // 宠物达到最大等级
        if (mPet.Query<int>("level") == MonsterMgr.GetMaxLevel(mPet))
        {
            petExp = 0;

            stdExp = StdMgr.GetPetStdExp(beforelevel, mPet.GetStar());

            mCurrentExp = stdExp;
        }

        //没有升级;
        if((mCurrentExp + petExp) < stdExp)
        {
            mUpgradeTips.gameObject.SetActive(false);
        }
        else
        {
            if(beforelevel >= StdMgr.GetStdAttrib("max_level", mPet.Query<int>("star")))
            {
                mUpgradeTips.gameObject.SetActive(false);
                mMaxLevelTips.gameObject.SetActive(true);
            }
            else
            {
                mUpgradeTips.gameObject.SetActive(true);
                mMaxLevelTips.gameObject.SetActive(false);
            }
        }

        if(stdExp == 0)
            mPercent = 0;
        else
            mPercent = mCurrentExp / (float)stdExp;

        mSlider.value = mPercent;

        mAddExp.text = LocalizationMgr.Get("ExpBar_1") + petExp;

        //添加经验;
        LPCMapping exp = new LPCMapping ();

        exp.Add("exp", petExp);

        mPet.AddAttrib(exp);

        PetMgr.TryLevelUp(mPet);

        mLevel.text = string.Format("{0}{1}", LocalizationMgr.Get("ExpBar_2"), mPet.GetLevel());

        if (mPet.Query<int>("level") == MonsterMgr.GetMaxLevel(mPet))
        {
            mSlider.value = 1;
            return;
        }

        SetSlider();
    }

    /// <summary>
    /// 设置经验滑动条
    /// </summary>
    void SetSlider()
    {
        //没有升级;
        if((mCurrentExp + petExp) < stdExp)
        {

            float value = (mCurrentExp + petExp) / (float) stdExp;

            StartCoroutine(SliderAnimaCoroutine(value));
        }

        else
            StartCoroutine(SliderUpgradeAnimaCoroutine());
    }

    IEnumerator SliderAnimaCoroutine(float varValue)
    {
        yield return new WaitForSeconds(2.0f);

        while(varValue > mSlider.value)
        {
            increment = (petExp / (float) stdExp) / 100f;

            mSlider.value += increment;
            yield return null;
        }

        yield break;
    }

    IEnumerator SliderUpgradeAnimaCoroutine()
    {
        yield return new WaitForSeconds(2.0f);

        int level = beforelevel;

        int initExp = mCurrentExp;

        float sum = 0;

        int addSum = 0;

        while((sum * (float)addSum) < (float)petExp)
        {
            //获取标准经验;
            int exp = StdMgr.GetPetStdExp(level + 1, mPet.GetStar());

            int add = exp - initExp;

            increment = (add / (float) exp) / 100f;

            mSlider.value += increment;

            if(addSum != add)
                addSum = add;

            sum += increment;

            if(mSlider.value > 0.99)
            {
                initExp = 0;

                level ++;

                mSlider.value = 0;
            }

            yield return null;
        }

        yield break;
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLabel()
    {
        mUpgradeTips.text = LocalizationMgr.Get("ExpBar_4");
        mMaxLevelTips.text = LocalizationMgr.Get("ExpBar_3");
        mMultipleTips.text = LocalizationMgr.Get("ExpBar_5");
    }

    /// <summary>
    /// 显示经验条
    /// </summary>
    public void ShowExp()
    {
        // 获取角色的世界缩放
        if (mPet != null)
        {
            float worldScale = mPet.GetWorldScale();
            transform.localScale = new Vector3(worldScale, worldScale, worldScale);
        }

        transform.gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏经验条
    /// </summary>
    public void HideExp()
    {
        transform.gameObject.SetActive(false);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(string rid, LPCMapping map)
    {
        mRid = rid;

        mPet = Rid.FindObjectByRid(mRid);

        //获取宠物当前经验;
        mCurrentExp = mPet.Query<int>("exp");

        // 记录数据
        clearanceMap = map;

        // 获取角色的世界缩放
        if (mPet != null)
        {
            float worldScale = mPet.GetWorldScale();
            transform.localScale = new Vector3(worldScale, worldScale, worldScale);
        }

        InitWnd();
    }

}
