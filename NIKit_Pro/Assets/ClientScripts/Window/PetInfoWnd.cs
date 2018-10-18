/// <summary>
/// PetInfoWnd.cs
/// Created by tanzy 2016/05/12
/// 宠物信息栏窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;

public class PetInfoWnd : WindowBase<PetInfoWnd>
{
    #region 成员变量

    /// <summary>
    ///玩家信息
    /// </summary>
    public UILabel playerInfo;

    /// <summary>
    /// 宠物信息
    /// </summary>
    public UILabel petinfo;

    /// <summary>
    ///装备信息
    /// </summary>
    public UILabel equipinfo;

    public UILabel suitName;

    //attrib
    public UILabel leveltips;

    /// <summary>
    /// 等级
    /// </summary>
    public UILabel level;

    public UILabel vigortips;
    /// <summary>
    ///体力
    /// </summary>
    public UILabel vigor;

    public UILabel attacktips;
    /// <summary>
    /// 攻击力
    /// </summary>
    public UILabel attack;

    /// <summary>
    ///攻击力增加值
    /// </summary>
    public UILabel addatk;

    public UILabel defendtips;

    /// <summary>
    ///防御力
    /// </summary>
    public UILabel defend;

    /// <summary>
    ///防御力增加值
    /// </summary>
    public UILabel adddefend;

    public UILabel agilitytips;

    /// <summary>
    ///  敏捷
    /// </summary>
    public UILabel agility;

    public UILabel crittips;
    /// <summary>
    ///  暴击率
    /// </summary>
    public UILabel crit;

    public UILabel critvaltips;
    /// <summary>
    ///  暴击伤害
    /// </summary>
    public UILabel critval;

    public UILabel hittips;
    /// <summary>
    /// 效果命中
    /// </summary>
    public UILabel hit;

    public UILabel resisttips;
    /// <summary>
    /// 效果抵抗
    /// </summary>
    public UILabel resist;

    /// <summary>
    ///体力增加值
    /// </summary>
    public UILabel mAddVigor;

    /// <summary>
    ///攻击速度增加值
    /// </summary>
    public UILabel mAddAgility;

    /// <summary>
    /// 宠物种族
    /// </summary>
    public UILabel type;

    /// <summary>
    /// 宠物的战斗类型
    /// </summary>
    public UILabel racist;

    /// <summary>
    ///合成按钮文本
    /// </summary>
    public UILabel lbcompound;

    /// <summary>
    /// 星星
    /// </summary>
    public GameObject[] stars;

    /// <summary>
    ///合成按钮
    /// </summary>
    public GameObject compundBtn;

    /// <summary>
    ///关闭按钮
    /// </summary>
    public GameObject closeBtn;

    /// <summary>
    ///宠物技能
    /// </summary>
    public GameObject[] skills;

    /// <summary>
    /// 宠物穿戴的装备
    /// </summary>
    public GameObject[] mEquips;

    /// <summary>
    ///宠物模型
    /// </summary>
    public GameObject petModel;

    /// <summary>
    ///技能悬浮窗口
    /// </summary>
    public GameObject mSkillView;

    //宠物元素;
    public UISprite mElement;

    /// <summary>
    /// 装备信息悬浮
    /// </summary>
    public GameObject mEquipView;

    public GameObject mMask;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    // 宠物信息
    private string petRid = string.Empty;

    /// <summary>
    ///宠物数据
    /// </summary>
    private Property mPetData;

    private int classId;

    /// <summary>
    ///宠物等级
    /// </summary>
    private int Level;

    // 玩家名称
    string mUserName;

    // 玩家等级
    int mUserLevel;

    // 第一次打开时待窗口动画完成后再加载模型
    private bool isTweenOver = false;

    CallBack mCallBack;

    List<Property> mEquipData = new List<Property>();

    #endregion

    #region 内部函数

    /// <summary>
    /// 重绘显示图标及文本的相对位置
    /// </summary>
    void RedrawPosWnd()
    {
        // 隐藏星级窗口
        foreach (GameObject ob in stars)
            ob.SetActive(false);

        // 获取星级
        int star = mPetData.GetStar();
        int count = star < stars.Length ? star : stars.Length;
        string IconName = PetMgr.GetStarName(mPetData.GetRank());

        //x轴方向的偏移量;
        int x_offset = 8;

        int multiple = stars.Length - count;

        // 显示星级
        for (int i = 0; i < count; i++)
        {
            stars[i].GetComponent<UISprite>().spriteName = IconName;
            stars[i].SetActive(true);

            //计算宠物星星的位置，相对于宠物背景居中;
            stars[i].transform.localPosition = new Vector3(stars[i].transform.localPosition.x + (x_offset * multiple),
                stars[i].transform.localPosition.y,
                stars[i].transform.localPosition.z);
        }

        //计算宠物元素类型图标的位置;
        mElement.transform.localPosition = new Vector3(mElement.transform.localPosition.x + multiple * x_offset,
            mElement.transform.localPosition.y,
            mElement.transform.localPosition.z);

        //设置玩家的名字和等级相对居中;
        petinfo.transform.localPosition = new Vector3(petinfo.transform.localPosition.x + multiple * 2, 
            petinfo.transform.localPosition.y, 
            petinfo.transform.localPosition.z);
    }

    void Redraw()
    {
        mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(classId));
        mElement.gameObject.SetActive(true);

        // 非合成材料不显示合成按钮
//        compundBtn.SetActive(MonsterMgr.IsSyntheMaterial(classId));

        RedrawPosWnd();

        // 初始化技能
        RedrawSkill();

        // 设置文本
        SetText();

        // 设置装备信息
        SetEquip();

        // 显示宠物模型
        if(isTweenOver)
            ShowModel();

        //设置宠物属性;
        SetPetAttribute();
    }

    /// <summary>
    /// 设置文本内容
    /// </summary>
    void SetText()
    {
        lbcompound.text = LocalizationMgr.Get("PetInfoWnd_1");
        leveltips.text = LocalizationMgr.Get("PetInfoWnd_2");
        vigortips.text = LocalizationMgr.Get("PetInfoWnd_3");
        attacktips.text = LocalizationMgr.Get("PetInfoWnd_4");
        defendtips.text = LocalizationMgr.Get("PetInfoWnd_5");
        agilitytips.text = LocalizationMgr.Get("PetInfoWnd_6");
        crittips.text = LocalizationMgr.Get("PetInfoWnd_8");
        critvaltips.text = LocalizationMgr.Get("PetInfoWnd_13");
        hittips.text = LocalizationMgr.Get("PetInfoWnd_9");
        resisttips.text = LocalizationMgr.Get("PetInfoWnd_10");
    }

    /// <summary>
    ///设置宠物属性数据
    /// </summary>
    void SetPetAttribute()
    {
        //设置宠物的等级和名称;
        petinfo.text = string.Format(LocalizationMgr.Get("PetInfoWnd_12"), PetMgr.GetAwakeColor(mPetData.GetRank()), Level, mPetData.Short());

        //设置宠物种族;
        type.text = MonsterConst.MonsterRaceTypeMap[MonsterMgr.GetRace(classId)];

        //获取宠物类型;
        racist.text = MonsterConst.MonsterStyleTypeMap[MonsterMgr.GetType(classId)];

        //设置玩家等级和昵称;
        playerInfo.text = string.Format(LocalizationMgr.Get("PetInfoWnd_12"), string.Empty, mUserLevel, mUserName);

        //宠物等级;
        level.text = Level.ToString();

        //基础体力
        vigor.text = mPetData.Query<int>("max_hp").ToString();

        //体力加成;
        int virgorAdd = mPetData.QueryAttrib("max_hp") - mPetData.Query<int>("max_hp");

        //没有体力加成;
        if (virgorAdd <= 0)
            mAddVigor.text = string.Empty;
        else
            //体力增加值;
            mAddVigor.text = string.Format("{0}{1}", "+", virgorAdd);

        //基础攻击力
        attack.text = mPetData.Query<int>("attack").ToString();

        //攻击力加成;
        int attackAdd = mPetData.QueryAttrib("attack") - mPetData.Query<int>("attack");

        //没有攻击力加成;
        if (attackAdd <= 0)
            addatk.text = string.Empty;
        else
            addatk.text = string.Format("{0}{1}", "+", attackAdd);

        //基础防御力
        defend.text = mPetData.Query<int>("defense").ToString();

        //防御力加成;
        int defendAdd = mPetData.QueryAttrib("defense") - mPetData.Query<int>("defense");

        if (defendAdd <= 0)
            adddefend.text = string.Empty;
        else
            adddefend.text = string.Format("{0}{1}", "+", defendAdd);

        // 敏捷
        agility.text = mPetData.Query<int>("agility").ToString();

        // 敏捷加成;
        int attSpeedAdd = mPetData.QueryAttrib("agility") - mPetData.Query<int>("agility");

        if (attSpeedAdd <= 0)
            mAddAgility.text = string.Empty;
        else
            mAddAgility.text = string.Format("{0}{1}", "+", attSpeedAdd);

        //暴击率;
        crit.text = mPetData.QueryAttrib("crt_rate") / 10 + "%";

        //暴击伤害;
        critval.text = mPetData.QueryAttrib("crt_dmg_rate") / 10 + "%";

        //命中效果;
        hit.text = mPetData.QueryAttrib("accuracy_rate") / 10 + "%";

        //效果抵抗;
        resist.text = mPetData.QueryAttrib("resist_rate") / 10 + "%";

        //套装属性
        equipinfo.text = LocalizationMgr.Get("PetInfoWnd_11");

        suitName.text = PetMgr.GetPetSuitName(mPetData);
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        for (int i = 0; i < mEquipData.Count; i++)
        {
            if (mEquipData[i] != null)
                mEquipData[i].Destroy();
        }
    }
        
    /// <summary>
    ///  初始化窗口
    /// </summary>
    void InitWnd()
    {
        // 注册事件
        RegisterEvent();

        if (mTweenAlpha != null && mTweenScale != null)
        {
            // 播放动画
            mTweenScale.PlayForward();

            mTweenAlpha.PlayForward();
        }

        //绘制窗口;
        Redraw();
    }

    /// <summary>
    /// 初始化技能显示
    /// </summary>
    void RedrawSkill()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].GetComponent<SkillItem>().SetBind(-1);
            skills[i].GetComponent<SkillItem>().SetSelected(false);

            skills[i].SetActive(true);
        }

        // 获取绑定宠物的技能
        LPCArray skillInfo = mPetData.GetAllSkills();

        // 遍历技能
        foreach (LPCValue mks in skillInfo.Values)
        {
            // 获取技能类型
            int skillId = mks.AsArray[0].AsInt;
            int type = SkillMgr.GetSkillPosType(skillId);

            if (type <= 0 || type > skills.Length)
                continue;

            SkillItem item = skills[type - 1].GetComponent<SkillItem>();

            //获取技能等级;
            int level = mPetData.GetSkillLevel(skillId);

            item.SetBind(skillId);

            if (! SkillMgr.IsLeaderSkill(skillId))
                item.SetMaxLevel(level);
            else
                item.SetLeader(true);

            item.SetSelected(false);

            //添加点击事件;
            UIEventListener.Get(skills[type - 1]).onPress = ClickShowHoverWnd;
        }
    }

    /// <summary>
    /// 设置英雄装备信息
    /// </summary>
    void SetEquip()
    {
        //设置装备的装备部位类型;
        mEquips[0].GetComponent<EquipItemWnd>().SetType(EquipConst.WEAPON);
        mEquips[1].GetComponent<EquipItemWnd>().SetType(EquipConst.ARMOR);
        mEquips[2].GetComponent<EquipItemWnd>().SetType(EquipConst.SHOES);
        mEquips[3].GetComponent<EquipItemWnd>().SetType(EquipConst.AMULET);
        mEquips[4].GetComponent<EquipItemWnd>().SetType(EquipConst.NECKLACE);
        mEquips[5].GetComponent<EquipItemWnd>().SetType(EquipConst.RING);

        for (int i = 0; i < mEquips.Length; i++)
        {
            int type = mEquips[i].GetComponent<EquipItemWnd>().equipType;

            // 获取宠物身上该位置的装备
            Property petData = GetPetEquip(mPetData, type);

            if (petData == null)
                continue;

            mEquips[i].GetComponent<EquipItemWnd>().SetBind(petData);

            UIEventListener.Get(mEquips[i]).onPress = ClickEquipShowHoverWnd;
        }
    }

    /// <summary>
    /// 获取宠物装备
    /// </summary>
    Property GetPetEquip(Property petOb, int type)
    {
        if (petOb == null)
            return null;

        // 返回角色相应装备位置装备
        return (mPetData as Container).baggage.GetCarryByPos(EquipMgr.GetEquipPos(type));
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    void ShowModel()
    {
        // 获取窗口绑定的模型窗口组件
        ModelWnd pmc = petModel.GetComponent<ModelWnd>();
        if (pmc == null)
            return;

        // 异步载入模型
        pmc.LoadModelSync(mPetData, LayerMask.NameToLayer("UI"));
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(compundBtn).onClick = OnCompound;
        UIEventListener.Get(closeBtn).onClick = OnClose;
        UIEventListener.Get(mMask).onClick = OnClose;

        if (mTweenScale == null)
            return;

        mTweenScale.AddOnFinished(OnTweenFinished);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// TweenAlpha结束的回调
    /// </summary>
    private void OnTweenFinished()
    {
        isTweenOver = true;

        ShowModel();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    ///按下显示悬浮窗口
    /// </summary>
    void ClickShowHoverWnd(GameObject go, bool isPress)
    {
        SkillItem data = go.GetComponent<SkillItem>();
        if (data == null || mSkillView == null)
            return;

        SkillViewWnd script = mSkillView.GetComponent<SkillViewWnd>();
        if (script == null)
            return;

        //按下
        if (isPress)
        {
            if (data.mSkillId <= 0)
                return;

            data.SetSelected(true);

            BoxCollider box = go.GetComponent<BoxCollider>();

            Vector3 boxPos = box.transform.localPosition;

            mSkillView.transform.localPosition = new Vector3(boxPos.x, boxPos.y + box.size.y / 2 + box.size.y / 3, boxPos.z);

            // 限制悬浮在屏幕内
            script.LimitPosInScreen();

            // 显示悬浮窗口
            script.ShowView(data.mSkillId, mPetData,true);
        }
        else
        {
            data.SetSelected(false);

            // 隐藏悬浮窗口
            script.HideView();
        }
    }

    /// <summary>
    ///点击装备显示悬浮窗口
    /// </summary>
    void ClickEquipShowHoverWnd(GameObject go, bool isPress)
    {
        if (isPress)
        {
            //获取装备对象
            Property equipOb = go.GetComponent<EquipItemWnd>().ItemOb;

            if (equipOb == null)
                return;

            BoxCollider box = go.GetComponent<BoxCollider>();

            if (box == null || mEquipView == null)
                return;

            Vector3 boxPos = box.transform.localPosition;

            mEquipView.transform.localPosition = new Vector3(boxPos.x, boxPos.y + box.size.y, boxPos.z);

            mEquipView.GetComponent<EquipViewWnd>().ShowView(equipOb.GetRid(), mPetData.GetRid());
        }
        else
        {
            if (mEquipView == null)
                return;
            mEquipView.GetComponent<EquipViewWnd>().HideView();
        }
    }

    /// <summary>
    /// 点击合成按钮的回调
    /// </summary>
    void OnCompound(GameObject ob)
    {
        GameObject wnd = WindowMgr.OpenWnd ("PetSynthesisViewWnd");

        wnd.GetComponent<PetSynthesisViewWnd> ().BindData (mPetData.GetClassID());
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    void OnClose(GameObject ob)
    {
        if (mCallBack != null)
            mCallBack.Go(false);

        // 关闭本窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定宠物
    /// </summary>
    public void Bind(string rid, string userName, int userLevel)
    {
        // 绑定宠物
        this.petRid = rid;
        this.mPetData = Rid.FindObjectByRid(petRid);
        if (mPetData == null)
            return;

        this.classId = mPetData.GetClassID();
        this.Level = mPetData.GetLevel();

        mUserName = userName;
        mUserLevel = userLevel;

        // 绘制窗口
        InitWnd();
    }

    public void SetCallBack(CallBack callBack)
    {
        mCallBack = callBack;
    }

    #endregion
}
