/// <summary>
/// TowerBossInfoWnd.cs
/// Created by fengsc 2017/08/22
/// 通天之塔首领信息窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TowerBossInfoWnd : WindowBase<TowerBossInfoWnd>
{
    #region 成员变量

    // 首领信息标题
    public UILabel mBossTitle;

    // 附加技能提示
    public UILabel mSkillTips;

    // 技能基础格子
    public GameObject mSkillItem;

    // 使魔基础格子
    public GameObject mPetItemWnd;

    // 技能查看悬浮
    public GameObject mSkillViewWnd;

    // 当前选择的难度
    int mDifficulty = 0;

    // 当前的boss层
    int mBossLayer = 0;

    // 副本阵容使魔列表
    List<Property> mPets = new List<Property>();

    // 附加技能
    List<int> mAppendSkill = new List<int>();

    Dictionary<int, Property> mDic = new Dictionary<int, Property>();

    List<GameObject> mPetItems = new List<GameObject>();

    // 技能格子缓存列表
    List<GameObject> mSkillItems = new List<GameObject>();

    #endregion

    #region 内部接口

    void Awake()
    {
        // 创建缓存的基础格子
        CreatedGameObject();
    }

    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化文本信息
        InitLabel();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("TowerBossInfoWnd");

        // 清除临时数据
        CleanData();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 监听通天塔滑动事件
        EventMgr.RegisterEvent("TowerBossInfoWnd", EventMgrEventType.EVENT_TOWER_SLIDE, OnTowerSlideEvent);
    }

    /// <summary>
    /// 通天塔滑动事件
    /// </summary>
    void OnTowerSlideEvent(int eventID, MixedValue para)
    {
        int bossLayer = para.GetValue<int>();
        if (bossLayer == mBossLayer)
            return;

        mBossLayer = bossLayer;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 初始化文本信息
    /// </summary>
    void InitLabel()
    {
        mSkillTips.text = LocalizationMgr.Get("TowerBossInfoWnd_2");
    }

    /// <summary>
    /// 创建一批缓存的基础格子
    /// </summary>
    void CreatedGameObject()
    {
        mSkillItem.SetActive(false);
        mPetItemWnd.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            GameObject skillItem = Instantiate(mSkillItem);
            if (skillItem != null)
            {
                skillItem.transform.SetParent(transform);
                skillItem.transform.localPosition = Vector3.zero;
                skillItem.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                mSkillItems.Add(skillItem);
            }
        }

        for (int i = 0; i < 12; i++)
        {
            GameObject go = Instantiate(mPetItemWnd);
            if (go != null)
            {
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;

                mPetItems.Add(go);
            }
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mBossTitle.text = string.Format(LocalizationMgr.Get("TowerBossInfoWnd_1"), mBossLayer + 1);

        // 收集首领信息
        DoGatherBossInfo(mBossLayer);

        // 绘制附加技能
        RedrawAppendSkill();

        // 绘制首领层阵容信息
        RedrawBossFormation();
    }

    /// <summary>
    /// Cleans the data.
    /// </summary>
    void CleanData()
    {
        // 析构临时对象
        foreach (Property ob in mPets)
        {
            if (ob == null)
                continue;

            // 析构临时对象
            ob.Destroy();
        }

        // 清除数据
        mPets.Clear();

        // 清除附加技能
        mAppendSkill.Clear();

        // 清除窗口列表
        mDic.Clear();
    }

    /// <summary>
    /// 收集首领信息
    /// </summary>
    void DoGatherBossInfo(int bossLayers)
    {
        int batch = 0;

        // 获取boss关卡资源数据
        List<CsvRow> data = TowerMgr.GetTowerBossLevelResources(mDifficulty, bossLayers, out batch);
        if (data == null)
            return;

        // 清除临时数据
        CleanData();

        // 收集阵容使魔数据以及附加技能数据
        for (int i = 0; i < data.Count; i++)
        {
            CsvRow row = data[i];
            if (row == null)
                continue;

            LPCMapping para = new LPCMapping();

            // 调用脚本参数计算怪物class_id;
            int classIdScript = row.Query<int>("class_id_script");
            int classId = (int) ScriptMgr.Call(classIdScript, ME.user.GetLevel(),
                row.Query<LPCValue>("class_id_args"));

            para.Add("rid", Rid.New());
            para.Add("class_id", classId);
            para.Add("difficulty", mDifficulty);
            para.Add("layer", mBossLayer);
            para.Add("batch", batch);

            // 获取始化参数;
            int initScript = row.Query<int>("init_script");
            LPCMapping initArgs = ScriptMgr.Call(initScript, ME.user.GetLevel(),
                row.Query<LPCValue>("init_script_args"), para) as LPCMapping;

            // 获取始化参数
            para.Append(initArgs);

            // 替换掉初始化规则
            para.Add("init_rule", "tower_monster_show");

            // 创建宠物对象
            Property ob = PropertyMgr.CreateProperty(para);

            mPets.Add(ob);

            if (ob.Query<int>("is_boss") != 1)
                continue;

            // 收集附加技能
            LPCArray array = SkillMgr.GetAppendSkill(ob);
            if (array == null || array.Count == 0)
                continue;

            foreach (LPCValue v in array.Values)
            {
                if (v == null || ! v.IsInt)
                    continue;

                int skillId = v.AsInt;

                if (mAppendSkill.Contains(skillId))
                    continue;

                mAppendSkill.Add(skillId);

                if (mDic.ContainsKey(skillId))
                    mDic.Remove(skillId);

                mDic.Add(skillId, ob);
            }
        }
    }

    /// <summary>
    /// 绘制附加技能
    /// </summary>
    void RedrawAppendSkill()
    {
        for (int i = 0; i < mSkillItems.Count; i++)
            mSkillItems[i].SetActive(false);

        if (mAppendSkill == null || mAppendSkill.Count == 0)
        {
            mSkillTips.gameObject.SetActive(false);
            return;
        }

        mSkillTips.gameObject.SetActive(true);

        float startX = 0;

        for (int i = 0; i < mAppendSkill.Count; i++)
        {
            int skillId = mAppendSkill[i];

            GameObject go = mSkillItems[i];
            if (go == null)
                continue;

            if (mAppendSkill.Count % 2 == 1)
                startX = (i - (mAppendSkill.Count / 2)) * 65;
            else
                startX = (i - ((mAppendSkill.Count - 1) / 2f)) * 65;

            go.transform.localPosition = new Vector3(startX, mSkillItem.transform.localPosition.y, mSkillItem.transform.localPosition.z);

            go.SetActive(true);

            if (!mSkillItems.Contains(go))
                mSkillItems.Add(go);

            SkillItem script = go.GetComponent<SkillItem>();
            if (script == null)
                continue;

            // 绑定数据
            script.SetBind(skillId);

            // 注册按钮点击事件
            UIEventListener.Get(go).onPress = OnPressSkillItem;
        }
    }

    /// <summary>
    /// 绘制首领层阵容信息
    /// </summary>
    void RedrawBossFormation()
    {
        mPetItemWnd.SetActive(false);

        for (int i = 0; i < mPetItems.Count; i++)
            mPetItems[i].SetActive(false);

        if (mPets == null)
            return;

        List<Property> bossList = new List<Property>();

        List<Property> petList = new List<Property>();

        for (int i = 0; i < mPets.Count; i++)
        {
            if (mPets[i] == null)
                continue;

            if (mPets[i].Query<int>("is_boss") == 1)
                bossList.Add(mPets[i]);
            else
                petList.Add(mPets[i]);
        }

        float startX = 0;

        for (int i = 0; i < bossList.Count; i++)
        {
            if (bossList[i] == null)
                continue;

            GameObject go = mPetItems[i];
            if (go == null)
                continue;

            go.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            if (bossList.Count % 2 == 1)
                startX = (i - (bossList.Count / 2)) * 110;
            else
                startX = (i - ((bossList.Count - 1) / 2f)) * 110;

            go.transform.localPosition = new Vector3(startX, -31, mPetItemWnd.transform.localPosition.z);

            go.SetActive(true);

            PetItemWnd script = go.GetComponent<PetItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.SetBind(bossList[i]);

            // 注册点击事件
            UIEventListener.Get(go).onClick = OnClickPetItem;
        }

        for (int i = 0; i < petList.Count; i++)
        {
            if (petList[i] == null)
                continue;

            GameObject go = mPetItems[i + bossList.Count];
            if (go == null)
                return;

            go.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            if (petList.Count % 2 == 1)
                startX = (i - (petList.Count / 2)) * 75;
            else
                startX = (i - ((petList.Count - 1) / 2f)) * 75;

            go.transform.localPosition = new Vector3(startX, -148, mPetItemWnd.transform.localPosition.z);

            go.SetActive(true);

            if (!mPetItems.Contains(go))
                mPetItems.Add(go);

            PetItemWnd script = go.GetComponent<PetItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.SetBind(petList[i]);

            // 注册点击事件
            UIEventListener.Get(go).onClick = OnClickPetItem;
        }
    }

    /// <summary>
    /// 技能格子点击事件
    /// </summary>
    void OnPressSkillItem(GameObject go, bool isPress)
    {
        SkillItem skillItem = go.GetComponent<SkillItem>();
        if (skillItem == null)
            return;

        SkillViewWnd script = mSkillViewWnd.GetComponent<SkillViewWnd>();
        if (script == null)
            return;

        //按下
        if (isPress)
        {
            if (skillItem.mSkillId <= 0)
                return;

            skillItem.SetSelected(true);

            // 显示悬浮窗口
            script.ShowView(skillItem.mSkillId, mDic[skillItem.mSkillId], true);

            BoxCollider box = go.GetComponent<BoxCollider>();

            Vector3 boxPos= box.transform.localPosition;

            mSkillViewWnd.transform.localPosition = new Vector3 (boxPos.x, boxPos.y + box.size.y / 2, boxPos.z);

            // 限制悬浮窗口在屏幕范围内
            script.LimitPosInScreen();
        }
        else
        {
            skillItem.SetSelected(false);

            // 隐藏悬浮窗口
            script.HideView();
        }
    }

    /// <summary>
    /// 宠物格子点击事件
    /// </summary>
    void OnClickPetItem(GameObject go)
    {
        PetItemWnd petItemWnd = go.GetComponent<PetItemWnd>();
        if (petItemWnd == null)
            return;

        // 获得宠物信息窗口
        GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();
        if (script == null)
            return;

        script.Bind(petItemWnd.item_ob);

        script.ShowBtn(true);

        wnd.transform.localPosition = Vector3.zero;
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int difficulty, int bossLayer)
    {
        if (mBossLayer == bossLayer && mDifficulty == difficulty)
            return;

        mDifficulty = difficulty;

        // 当前位置的boss层数
        mBossLayer = bossLayer;

        // 重绘窗口
        Redraw();
    }

    #endregion
}
