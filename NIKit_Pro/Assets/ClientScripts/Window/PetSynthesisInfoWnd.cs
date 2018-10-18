/// <summary>
/// PetSynthesisInfoWnd.cs
/// Created by lic 2017/02/10
/// 合成宠物确认窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LPC;

public class PetSynthesisInfoWnd : WindowBase<PetSynthesisInfoWnd>
{
    public GameObject mSyntheBtn;
    public UILabel mBtnCost;
    public UISprite mBtnIcon;
    public GameObject mBtnCover;
    public UISpriteAnimation mBtnAnima;

    public UISprite mElement;
    public UILabel mNameLb;
    public GameObject[] mStarGroup;
    public GameObject mCloseBtn;

    // 宠物属性参数值
    public UILabel mRaceValueLb;
    public UILabel mTypeValueLb;
    public UILabel mStrengthValueLb;
    public UILabel mAttackValueLb;
    public UILabel mDefenseValueLb;
    public UILabel mSpeedValueLb;

    public GameObject[] mSkillGroup;

    // 本地化文字
    public UILabel mStrengthLb;
    public UILabel mAttackLb;
    public UILabel mDefenseLb;
    public UILabel mSpeedLb;
    public UILabel mSyntheLb;

    //技能悬浮窗口
    public GameObject mSkillViewWnd;

    // 遮盖
    public GameObject mCover;

    // 宠物显示框
    public GameObject mPetItem;

    // 合成材料组
    public GameObject[] materialGroup;

    public TweenScale mTweenScale;

    #region 私有字段

    // 绑定的宠物对象
    Property item_ob = null;

    // 关闭窗口回调
    CallBack task;

    int viewWndBindId = 0;

    bool canSynthe = false;

    #endregion

    #region 内部函数

    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    void RegisterEvent()
    {
        if(mSyntheBtn != null)
            UIEventListener.Get(mSyntheBtn).onClick += OnSynthesisClick;

        if(mCloseBtn != null)
        UIEventListener.Get(mCloseBtn).onClick += OnCloseBtn;

        // 注册按钮事件
        for (int i = 0; i < mSkillGroup.Length; i++)
            UIEventListener.Get(mSkillGroup[i]).onPress = OnSkillItemPress;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// tween动画播放完后回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        // 本地化文本
        mStrengthLb.text = LocalizationMgr.Get("SummonWnd_10");
        mAttackLb.text = LocalizationMgr.Get("SummonWnd_11");
        mDefenseLb.text = LocalizationMgr.Get("SummonWnd_12");
        mSpeedLb.text = LocalizationMgr.Get("SummonWnd_13");

        if(mSyntheLb != null)
            mSyntheLb.text = LocalizationMgr.Get("PetSynthesisInfoWnd_1");

        // 设置petitem显示
        mPetItem.GetComponent<PetItemWnd>().ShowLeaderSkill(false);
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        // 判断能否合成
        int classId = item_ob.GetClassID();
        canSynthe = PetsmithMgr.CanDoSynthe(ME.user, classId); 

        // 宠物元素
        string element = PetMgr.GetElementIconName(MonsterMgr.GetElement(classId));
        mElement.spriteName = element;

        // 宠物名称
        mNameLb.text = string.Format("{0}{1}",
            PetMgr.GetAwakeColor(item_ob.GetRank()), item_ob.Short());

        // 宠物星级
        string StarName = PetMgr.GetStarName(item_ob.GetRank());

        int stars = item_ob.GetStar();

        int count = stars < mStarGroup.Length ? stars : mStarGroup.Length;

        for (int i = 0; i < mStarGroup.Length; i++)
        {
            if (i < count)
            {
                mStarGroup[i].GetComponent<UISprite>().spriteName = StarName;
                mStarGroup[i].SetActive(true);
            }
            else
            {
                mStarGroup[i].SetActive(false);
            }
        }

        // 种族
        int race = MonsterMgr.GetRace(item_ob.GetClassID());
        mRaceValueLb.text = MonsterConst.MonsterRaceTypeMap[race];

        // 类型
        int type = MonsterMgr.GetType(item_ob.GetClassID());
        mTypeValueLb.text = MonsterConst.MonsterStyleTypeMap[type];

        mStrengthValueLb.text = item_ob.Query<int>("max_hp").ToString();

        mAttackValueLb.text = item_ob.Query<int>("attack").ToString();

        mDefenseValueLb.text = item_ob.Query<int>("defense").ToString();

        mSpeedValueLb.text = item_ob.Query<int>("speed").ToString();

        // 获取绑定宠物的技能
        LPCArray skillInfo = item_ob.GetAllSkills();

        // 默认先把所有的技能格子全部置空
        for (int i = 0; i < mSkillGroup.Length; i++)
            mSkillGroup[i].GetComponent<SkillItem>().SetBind(-1);

        // 对字典按key（skillid）进行排序
        foreach (LPCValue mks in skillInfo.Values)
        {
            // 获取技能位置类型
            int skillId = mks.AsArray[0].AsInt;
            int posType = SkillMgr.GetSkillPosType(skillId);

            if (posType <= 0 || posType > mSkillGroup.Length)
                continue;

            SkillItem item = mSkillGroup[posType - 1].GetComponent<SkillItem>();

            item.SetBind(skillId);

            if (SkillMgr.IsLeaderSkill(skillId))
                item.SetLeader(true);
            else
                item.SetLeader(false);
        }

        // 绑定item
        mPetItem.GetComponent<PetItemWnd>().SetBind(item_ob);

        LPCArray material_cost = PetsmithMgr.GetSynthesisMaterials (item_ob.GetClassID());

        for (int i = 0; i < materialGroup.Length; i++)
        {
            if (i < material_cost.Count) 
            {
                materialGroup [i].GetComponent<PetSynthesisItemWnd> ().BindData (material_cost[i].AsMapping, viewWndBindId, ! canSynthe);
                continue;
            }

            materialGroup [i].GetComponent<PetSynthesisItemWnd> ().BindData (null);
        }

        if(mSyntheBtn != null)
            RedrawBtn ();
    }

    /// <summary>
    /// 刷新合成按钮
    /// </summary>
    private void RedrawBtn()
    {
        if (item_ob == null)
            return;

        CsvRow data = PetsmithMgr.SynthesisCsv.FindByKey (item_ob.GetClassID ());

        if (data == null)
            return;

        LPCMapping cost = data.Query<LPCMapping> ("attrib_cost");

        if (cost == null || cost.Count == 0)
        {
            mBtnIcon.spriteName = "money";
            mBtnCost.text = "0";
            mBtnCover.SetActive(true);
            return;
        }

        mBtnCover.SetActive (! canSynthe);

        mBtnAnima.gameObject.SetActive (canSynthe);

        string field = FieldsMgr.GetFieldInMapping(cost);

        mBtnIcon.spriteName = FieldsMgr.GetFieldIcon(field);
        mBtnCost.text = cost.GetValue<int>(field).ToString();
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
    }

    /// <summary>
    /// 关闭(确认)按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        if (task != null)
            task.Go();

        WindowMgr.DestroyWindow (gameObject.name);
    }

    /// <summary>
    /// 合成按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSynthesisClick(GameObject ob)
    {
        CsvRow data = PetsmithMgr.SynthesisCsv.FindByKey (item_ob.GetClassID ());

        if (data == null)
            return;

        LPCMapping cost = data.Query<LPCMapping> ("attrib_cost");

        // 检测属性消耗是否足够
        bool result = PetsmithMgr.CheckMoneyEnough(cost);

        if (!result)
            return;

        List<List<Property>> syntheData = new List<List<Property>> ();
        LPCArray materials = data.Query<LPCArray> ("material_cost");

        for (int i = 0; i < materials.Count; i++)
        {
            object check = PetsmithMgr.SlectMaterial (ME.user, materials [i].AsMapping);

            if (check is string)
            {
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    (string)check,
                    string.Empty,
                    string.Empty,
                    true,
                    this.transform
                );
                
                return; 
            }

            List<Property> petList = (List<Property>)check;

            if(petList.Count > 1)
                syntheData.Add (rankMaterialList(petList));
            else
                syntheData.Add (petList);
        }

        GameObject confirmWnd = WindowMgr.OpenWnd ("PetSynthesisConfirmWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (confirmWnd == null)
            return;

        confirmWnd.GetComponent<PetSynthesisConfirmWnd> ().BindData (item_ob.GetClassID (), syntheData);
    }

    /// <summary>
    /// 对宠物排序
    /// </summary>
    /// <param name="petList">Pet list.</param>
    List<Property> rankMaterialList(List<Property> petList)
    {
        // 根据道具权重排序
        IEnumerable<Property> ItemQuery = from ob in petList orderby CALC_PET_SYNTHESIS_SELECT_SORT_RULE.Call(ob) descending
            select ob;

        List<Property> sortItems = new List<Property>();

        foreach (Property item in ItemQuery)
            sortItems.Add(item);

        // 按照权重进行排序
        return sortItems;
    }

    /// <summary>
    /// 技能格子按压事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSkillItemPress(GameObject ob, bool isPress)
    {
        SkillItem item = ob.GetComponent<SkillItem>();
        if (item == null)
            return;

        // 取得技能格子对应的技能id
        int skillId = item.mSkillId;

        if (skillId <= 0)
            return;

        if (mSkillViewWnd == null)
            return;

        SkillViewWnd script = mSkillViewWnd.GetComponent<SkillViewWnd>();
        if (script == null)
            return;

        if (!isPress)
        {
            item.SetSelected (false);

            // 隐藏窗口
            script.HideView();

            return;
        }
        item.SetSelected (true);

        // 显示悬浮窗口 
        script.ShowView(skillId, item_ob);

        BoxCollider box = ob.GetComponent<BoxCollider>();

        Vector3 boxPos = box.transform.localPosition;

        mSkillViewWnd.transform.localPosition = new Vector3(boxPos.x - 120f, boxPos.y - 30f, boxPos.z);

        // 限制悬浮窗口在屏幕范围内
        script.LimitPosInScreen();
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void BindData(Property item_ob, CallBack _callback = null, int _viewWndBindId = 0)
    {
        // item_ob不能为null
        if (item_ob == null)
            return;

        this.task = _callback;

        this.item_ob = item_ob;

        this.viewWndBindId = _viewWndBindId;

        Redraw();
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    public void RedrawWnd()
    {
        if (this.item_ob == null)
            return;

        Redraw ();
    }

    #endregion
}
