using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class AwakeWnd  : WindowBase<AwakeWnd>
{
#region 成员变量

    public float playWhiteFlashTime = 4f;

    [Header("可跳过等待时间")]
    public float showSkip = 3f;

    public float playOverTime = 20f;

    public GameObject mRichText;

    public GameObject mSkillViewWnd;

    public GameObject mAwakeItem;

    // 宠物觉醒按钮
    public GameObject mAwakeBtn;
    public UILabel mAwakeBtnLb;

    // 觉醒卷按钮
    public UIToggle mAwakeReelBtn;
    public UILabel mAwakeReelBtnLb;

    public GameObject [] mWnds;

    public UILabel mTitle;

    // 不能觉醒
    public UILabel mNotAwakenLb;

    // 觉醒完成
    public UILabel mFinishAwake;

    public GameObject mAwakeMaterialViewWnd;

    public UISprite mBG;

    public GameObject mEffectsWnd;

    public GameObject mPetModel;

    public GameObject mAniCover;

    public GameObject mSkipCover;

    public GameObject mSkipGo;

    public GameObject mAwakeAni;

    public GameObject mWhiteMask;

    public GameObject mPetInfo;

    public GameObject mCycle;

    public UISprite[] mStars;

    public GameObject mStarsPanel;

    public UISprite mElement;

    public UILabel mPetName;

    public GameObject mInfoContent;

#endregion

#region 私有变量

    private string[] mEffectNames = new string[]{"Awake_f", "Awake_s", "Awake_w", "Awake_l", "Awake_d"};

    private Property item_ob;

    // 创建一批觉醒材料格子
    private List<GameObject> mObjectList = new List<GameObject>();

    // 觉醒光效
    private List<GameObject> mEffects = new List<GameObject>();

    private GameObject mUnAwakeModel = null;

    private bool mIsMeBaggage = true;

    private string mAudioName = string.Empty;

    private int mAwakeType = -1;

#endregion

#region 内部函数

    void Start()
    {
        RegisterEvent();

        // 初始化本地化文本
        InitLocalText();

        // 生成光效
        mEffects = mEffectsWnd.GetComponent<EffectWnd>().LoadEffects(mEffectNames, Game.UnitToPixelScale);

        mAwakeAni.SetActive(false);

        if (mSkipCover != null)
            mSkipCover.SetActive(false);

        if (mSkipGo != null)
            mSkipGo.SetActive(false);
    }

    void OnDisable()
    {
        // 重置单选框状态
        if (mAwakeReelBtn != null)
            mAwakeReelBtn.Set(false);

        mAwakeType = -1;
    }

    void OnDestroy()
    {
        EventMgr.UnregisterEvent("AwakeWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        EventMgr.RegisterEvent("AwakeWnd", EventMgrEventType.EVENT_CLICK_PICTURE, WhenClickPicture);

        // 注册宠物觉醒事件
        EventMgr.RegisterEvent("AwakeWnd", EventMgrEventType.EVENT_PET_AWAKE, OnPetAwake);

        // 注册按钮点击事件
        UIEventListener.Get(mAwakeBtn).onClick = OnClickAwakeBtn;

        // 跳过按钮
        if (mSkipCover != null)
            UIEventListener.Get(mSkipCover).onClick = OnSkipBtn;

        // 注册按钮点击事件
        UIEventListener.Get(mTitle.gameObject).onClick = OnClickAwakeTitleBtn;

        if (mAwakeReelBtn != null)
            UIEventListener.Get(mAwakeReelBtn.gameObject).onClick = OnClickAwakeReelBtn;
    }

    /// <summary>
    /// 宠物觉醒消息回调
    /// </summary>
    void OnPetAwake(int eventId, MixedValue para)
    {
        mAwakeType = -1;

        // 重置单选框状态
        if (mAwakeReelBtn != null)
            mAwakeReelBtn.Set(false);

        if (para == null)
            return;

        LPCMapping map = para.GetValue<LPCMapping>();

        if (map == null)
            return;

        string rid = map.GetValue<string>("rid");

        Property pet_ob = Rid.FindObjectByRid(rid);
        if (pet_ob == null)
            return;

        // 构建音效唯一的key
        mAudioName = Game.GetUniqueName("awake");

        // 播放觉醒音效
        GameSoundMgr.PlayGroupSound("awake", mAudioName);

        mAwakeAni.SetActive(true);
        mAwakeAni.GetComponent<TweenAlpha>().PlayForward();
        mAniCover.SetActive(true);
        mUnAwakeModel.SetActive(true);

        // 获取觉醒类型(根据觉醒类型播放相应的光效)
        int element = item_ob.Query<int>("element");

        mEffects[element - 1].SetActive(true);
        mEffects[element - 1].GetComponent<ParticleSystem>().Play();

        mPetInfo.GetComponent<TweenAlpha>().enabled = true;
        mPetInfo.GetComponent<TweenAlpha>().ResetToBeginning();

        mCycle.GetComponent<TweenAlpha>().enabled = true;
        mCycle.GetComponent<TweenAlpha>().ResetToBeginning();
        mCycle.GetComponent<TweenScale>().enabled = true;
        mCycle.GetComponent<TweenScale>().ResetToBeginning();

        UpdateInfo(MonsterConst.RANK_UNAWAKE);

        // 播放光效
        Coroutine.DispatchService(SyncShowResult(), "SyncShowResult");

        Coroutine.DispatchService(SyncShowSkip(), "SyncShowSkip");

        Coroutine.DispatchService(SyncCloseAni(), "SyncCloseAni");
    }

    /// <summary>
    /// 显示可跳过
    /// </summary>
    /// <returns></returns>
    IEnumerator SyncShowSkip()
    {
        yield return new WaitForSeconds(showSkip);

        if (mSkipGo != null)
            mSkipGo.SetActive(true);

        if (mSkipCover != null)
            mSkipCover.SetActive(true);
    }

    // 显示召唤结果
    IEnumerator SyncShowResult()
    {
        yield return new WaitForSeconds(playWhiteFlashTime);

        // 播放白色闪屏
        mWhiteMask.GetComponent<TweenAlpha>().enabled = true;
        mWhiteMask.GetComponent<TweenAlpha>().ResetToBeginning();

        mPetModel.GetComponent<ModelWnd>().UnLoadModel();

        UpdateInfo(MonsterConst.RANK_AWAKED);

        // 加载新的模型
        mPetModel.GetComponent<ModelWnd>().LoadModel(item_ob, LayerMask.NameToLayer("UI"));
    }

    // 关闭界面
    IEnumerator SyncCloseAni()
    {
        yield return new WaitForSeconds(playOverTime);

        // 播放觉醒动画
        CloseAni();
    }

    /// <summary>
    /// 跳过按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSkipBtn(GameObject ob)
    {
        Coroutine.StopCoroutine("SyncShowResult");

        Coroutine.StopCoroutine("SyncCloseAni");

        Coroutine.StopCoroutine("SyncShowSkip");

        CloseAni();
    }

    /// <summary>
    /// 更新宠物描述信息
    /// </summary>
    void UpdateInfo(int rank)
    {
        int element = MonsterMgr.GetElement(item_ob.GetClassID());
        mElement.spriteName = PetMgr.GetElementIconName(element);

        mPetName.text = string.Format("{0}{1}",
            PetMgr.GetAwakeColor(rank), MonsterMgr.GetName(item_ob.GetClassID(), rank));

        // 检测宠物觉醒状态
        string starName = PetMgr.GetStarName(rank);

        // 宠物的星级
        int stars = item_ob.GetStar();

        for (int i = 0; i < mStars.Length; i++)
        {
            if(i >= stars)
            {
                mStars[i].gameObject.SetActive(false);
                continue;
            }

            mStars[i].gameObject.SetActive(true);

            mStars[i].GetComponent<UISprite>().spriteName = starName;
        }

        mInfoContent.transform.localPosition = new Vector3(0, 0, 0);

        int offSet = 0;

        offSet += mPetName.width - 110;

        // 对星级作偏移
        mStarsPanel.transform.localPosition = new Vector3(54 + offSet, 1, 0);

        // 取得星级的偏移
        offSet += (stars - 6)*23;

        mInfoContent.transform.localPosition = new Vector3(- offSet/2, 0, 0);
    }

    /// <summary>
    /// 关闭动画界面
    /// </summary>
    void CloseAni()
    {
        // 卸载模型
        mPetModel.GetComponent<ModelWnd>().UnLoadModel();

        mUnAwakeModel = null;

        mAwakeAni.SetActive(false);

        if (mSkipGo != null)
            mSkipGo.SetActive(false);

        if (mSkipCover != null)
            mSkipCover.SetActive(false);

        // 结束觉醒音效
        GameSoundMgr.StopSound(mAudioName);
    }

    /// <summary>
    /// 图片点击回调
    /// </summary>
    void WhenClickPicture(int eventId, MixedValue para)
    {
        List<object> args = para.GetValue<List<object>>();
        if (args == null)
            return;

        int skillID = (int)args[1];
        if (skillID <= 0)
            return;

        bool isPress = (bool)args[0];

        if (mSkillViewWnd == null)
            return;

        SkillViewWnd viewWnd = mSkillViewWnd.GetComponent<SkillViewWnd>();
        if (viewWnd == null)
            return;

        if (isPress)
        {
            // 显示悬浮窗口
            viewWnd.ShowView(skillID, item_ob, true);

            mSkillViewWnd.transform.position = (Vector3)args[2];

            // 限制悬浮窗口在屏幕范围内
            viewWnd.LimitPosInScreen();
        }
        else
        {
            // 隐藏悬浮窗口
            viewWnd.HideView();
        }
    }

    /// <summary>
    /// 觉醒卷按钮点击事件回调
    /// </summary>
    void OnClickAwakeReelBtn(GameObject go)
    {
        if (mAwakeType == AwakeConst.ITEM_AWAKE)
        {
            mAwakeType = -1;

            return;
        }

        mAwakeType = AwakeConst.ITEM_AWAKE;

        if (mAwakeReelBtn != null)
            mAwakeReelBtn.Set(false);

        if (!CheckAwakeMaterialAmount(mAwakeType))
        {
            // 您尚未拥有觉醒卷
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("AwakeWnd_7"),
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(BaggageWnd.WndType).transform
            );

            mAwakeType = -1;

            return;
        }

        // 勾选后，觉醒使魔将优先使用觉醒卷觉醒。
        DialogMgr.ShowDailog(
            new CallBack(AwakenCardApplyCallback),
            LocalizationMgr.Get("AwakeWnd_8"),
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            WindowMgr.GetWindow(BaggageWnd.WndType).transform
        );
    }

    /// <summary>
    /// 觉醒绝使用确认回调
    /// </summary>
    void AwakenCardApplyCallback(object para, params object[] param)
    {
        if (!(bool)param[0])
        {
            mAwakeType = -1;

            return;
        }

        mAwakeReelBtn.Set(true);
    }

    /// <summary>
    /// 宠物觉醒按钮点击事件
    /// </summary>
    void OnClickAwakeTitleBtn(GameObject go)
    {
        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 显示觉醒需要的材料
        // 获取配置表信息
        CsvRow row = MonsterMgr.GetRow(item_ob.Query<int>("class_id"));

        if (row == null)
            return;

        // 获取需要的觉醒材料
        LPCArray awakeMaterial = row.Query<LPCArray>("awake_material");

        GameObject wnd = WindowMgr.OpenWnd(DropSoulWnd.WndType);

        wnd.GetComponent<DropSoulWnd>().BindData(awakeMaterial);
    }

    /// <summary>
    /// 宠物觉醒按钮点击事件
    /// </summary>
    void OnClickAwakeBtn(GameObject go)
    {
        if (mAwakeReelBtn.value)
        {
            // 使用觉醒卷觉醒

            // 弹框提示
            DialogMgr.ShowDailog(
                new CallBack(AwakenCardCallBack),
                string.Format(LocalizationMgr.Get("AwakeWnd_9"), ME.user.Query<int>("awaken_card"), LocalizationMgr.Get(item_ob.GetName())),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(BaggageWnd.WndType).transform
            );
        }
        else
        {
            // 使用材料觉醒
            // 弹框提示
            DialogMgr.ShowDailog(
                new CallBack(ConfirmAwakeDialogCallBack),
                LocalizationMgr.Get("AwakeWnd_4"),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(BaggageWnd.WndType).transform
            );
        }
    }

    /// <summary>
    /// 觉醒卷觉醒确认弹框
    /// </summary>
    void AwakenCardCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 执行觉醒操作
        DoAction();
    }

    void ConfirmAwakeDialogCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        mAwakeType = AwakeConst.MATERIAL_AWAKE;

        if (CheckAwakeMaterialAmount(mAwakeType))
        {
            DoAction();
        }
        else
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("AwakeWnd_5"),
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(BaggageWnd.WndType).transform
            );
        }
    }

    /// <summary>
    /// 执行操作
    /// </summary>
    void DoAction()
    {
        LPCMapping extraPara = new LPCMapping();
        extraPara.Add("rid", item_ob.GetRid());
        extraPara.Add("type", mAwakeType);

        // 先克隆一个未觉醒之前的模型
        mUnAwakeModel = mPetModel.GetComponent<ModelWnd>().LoadModel(item_ob, LayerMask.NameToLayer("UI"));

        mUnAwakeModel.SetActive(false);

        // 执行工坊操作
        PetsmithMgr.DoAction(ME.user, "awake", extraPara);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mAwakeBtn.SetActive(mIsMeBaggage);

        mFinishAwake.gameObject.SetActive(false);

        if (item_ob == null)
            return;

        // 判断宠物能否觉醒
        if (! MonsterMgr.IsCanAwaken(item_ob.Query<int>("class_id")))
        {
            SetWndsShowState(false);
            return;
        }

        SetWndsShowState(true);

        string desc = MonsterMgr.GetEvolutionDesc(item_ob);

        RichTextContent content = mRichText.GetComponent<RichTextContent>();

        content.clearContent();

        content.ParseValue(desc);

        // 宠物已觉醒
        if (MonsterMgr.IsAwaken(item_ob))
        {
            mWnds[2].SetActive(false);

            mFinishAwake.gameObject.SetActive(true);

            return;
        }

        // 显示觉醒需要的材料
        // 获取配置表信息
        CsvRow row = MonsterMgr.GetRow(item_ob.Query<int>("class_id"));

        if (row == null)
            return;

        // 获取需要的觉醒材料
        LPCArray awakeMaterial = row.Query<LPCArray>("awake_material");

        mAwakeItem.SetActive(false);

        float center_x = 0f;

        if (mIsMeBaggage)
            center_x = -63;

        float starX = center_x - 74*(awakeMaterial.Count - 1)/2;

        foreach (GameObject item in mObjectList)
            item.SetActive(false);

        int index = 0;
        foreach (LPCValue material in awakeMaterial.Values)
        {
            if (material == null || !material.IsArray)
                continue;

            GameObject go = mObjectList[index];

            if (go == null)
                continue;

            go.transform.SetParent(mWnds[2].transform);

            go.transform.localScale = Vector3.one;

            go.transform.localPosition = new Vector3 (starX + index * 74,
                mAwakeItem.transform.localPosition.y,
                mAwakeItem.transform.localPosition.z);

            index++;

            // 构建参数
            LPCMapping data = new LPCMapping();
            data.Add("icon", material.AsArray[0].AsInt);

            // 材料需要的总数
            data.Add("total_amount", material.AsArray[1].AsInt);

            // 玩家拥有的材料数量
            data.Add("amount", UserMgr.GetAttribItemAmount(ME.user, material.AsArray[0].AsInt));

            go.GetComponent<AwakeMaterialItemWnd>().Bind(data, material.AsArray[0].AsInt);

            go.SetActive(true);

            UIEventListener.Get(go).onPress = OnClickMaterialItem;
        }
    }

    /// <summary>
    /// 创建觉醒材料格子
    /// </summary>
    void CreateGrid()
    {
        if (mObjectList.Count >= 4)
            return;

        for (int i = 0; i < 4; i++)
        {
            GameObject go = Instantiate(mAwakeItem);

            if (go == null)
                continue;

            go.transform.SetParent(mWnds[2].transform);

            go.transform.localScale = Vector3.one;

            mObjectList.Add(go);
        }
    }

    /// <summary>
    /// 材料格子点击事件
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="isPress">If set to <c>true</c> is press.</param>
    void OnClickMaterialItem(GameObject go, bool isPress)
    {
        AwakeMaterialViewWnd viewWnd = mAwakeMaterialViewWnd.GetComponent<AwakeMaterialViewWnd>();
        if(isPress)
            viewWnd.ShowView(go.GetComponent<AwakeMaterialItemWnd>().mClassId);
        else
            viewWnd.HideView();
    }

    /// <summary>
    /// 检测玩家的觉醒材料是否足够
    /// </summary>
    bool CheckAwakeMaterialAmount(int type)
    {
        CsvRow row = MonsterMgr.GetRow(item_ob.Query<int>("class_id"));

        if (row == null)
            return false;

        if (type == AwakeConst.MATERIAL_AWAKE)
        {
            // 获取需要的觉醒材料
            LPCArray awakeMaterial = row.Query<LPCArray>("awake_material");

            if (awakeMaterial.Count < 1)
                return false;

            foreach (LPCValue item in awakeMaterial.Values)
            {
                // 拥有该材料的数量
                int amount = UserMgr.GetAttribItemAmount(ME.user, item.AsArray[0].AsInt);
                if (item.AsArray[1].AsInt > amount)
                    return false;
            }
        }
        else
        {
            // 觉醒卷数量
            LPCValue v = ME.user.Query<LPCValue>("awaken_card");
            if (v == null || !v.IsInt || v.AsInt <= 0)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 设置窗口的显示状态
    /// </summary>
    void SetWndsShowState(bool isEnable)
    {
        foreach (GameObject go in mWnds)
            go.SetActive(isEnable);

        mNotAwakenLb.gameObject.SetActive(!isEnable);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mNotAwakenLb.text = LocalizationMgr.Get("AwakeWnd_3");
        mTitle.text = LocalizationMgr.Get("AwakeWnd_1");
        mAwakeBtnLb.text = LocalizationMgr.Get("PetToolTipWnd_5");
        mFinishAwake.text = LocalizationMgr.Get("AwakeWnd_2");
        if (mSkipGo != null)
            mSkipGo.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("BaggageWnd_1");

        if (mAwakeReelBtnLb != null)
            mAwakeReelBtnLb.text = LocalizationMgr.Get("AwakeWnd_6");
    }

#endregion

#region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="item">Item.</param>
    public void BindData(Property item, bool isMeBaggage = true)
    {
        item_ob = item;

        mIsMeBaggage = isMeBaggage;

        // 创建一批格子
        CreateGrid();

        // 宠物切换时如果技能悬浮没有关闭，隐藏悬浮
        if(mSkillViewWnd.GetComponent<UIPanel>().alpha > 0f)
            mSkillViewWnd.GetComponent<SkillViewWnd>().HideView();

        Redraw();
    }

    /// <summary>
    /// 指引点击觉醒按钮
    /// </summary>
    public void GuideClickiAwakeBtn()
    {
        mAwakeType = AwakeConst.MATERIAL_AWAKE;

        // 执行觉醒操作
        if (CheckAwakeMaterialAmount(mAwakeType))
            DoAction();
    }

#endregion
}
