/// <summary>
/// DefenceDeployWnd.cs
/// Crested by fengsc 2016/09/08
/// 竞技场防御部署窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DefenceDeployWnd : WindowBase<DefenceDeployWnd>
{
    #region 成员变量

    // 队长技能描述;
    public UILabel mLeaderSkillDesc;
    public UILabel mLeaderSkillDescLb;

    // 防御和排位战说明查看按钮;
    public GameObject mHelpBtn;
    public UILabel mHelpBtnLb;

    // 防御塔攻击力
    public UILabel mAttack;
    public UILabel mAttackLb;

    // 防御塔攻击速度
    public UILabel mAtkSpeed;
    public UILabel mAtkSpeedLb;

    // 商店提示
    public UILabel mShopTips;

    // 防御塔增益效果
    public UILabel mGain;
    public UILabel mGainLb;

    // 确认部署按钮
    public GameObject mConfirmDeployBtn;
    public UILabel mConfirmDeployBtnLb;

    // 阵容变更提示
    public UILabel mChangeTips;

    public UILabel mDefenceTowerLb;

    //    public GameObject mPetItem;
    public UIWrapContent mWrapContent;

    // 宠物模型
    public ModelWnd[] mModels;

    public GameObject[] mSelectEffect;

    public UISpriteAnimation mEffect;

    public UIScrollView mSrollView;

    public GameObject mPetItemWnd;

    Dictionary<string, GameObject> mItems = new Dictionary<string, GameObject>();

    private int mRowAmonut = 10;
    // 每行显示10个宠物格子,实例化十个元素进行复用

    // 存储玩家的所有的宠物数据
    List<Property> mPetObList = new List<Property>();

    List<PetItemWnd> mPetList = new List<PetItemWnd>();

    // 已选玩家宠物rid缓存列表
    List<string> mRidList = new List<string>();

    // 防御宠物数量限制
    private int mDefencePetAmountLimit = 0;

    LPCArray defenceList = LPCArray.Empty;

    // 更新的防守列表
    LPCArray updateDefenceList = LPCArray.Empty;

    Property mOnPressOb = null;

    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    // 起始位置
    private Dictionary<GameObject,Vector3> rePosition = new Dictionary<GameObject,Vector3>();

    #endregion

    #region 内部函数

    // Use this for initialization
    void Awake()
    {
        // 竞技场防御成员限制
        mDefencePetAmountLimit = GameSettingMgr.GetSettingInt("max_defense_member");

        // 初始化本地化文本;
        InitLocalText();

        // 注册事件
        RegisterEvent();

        // 初始化宠物格子
        InitPetGrid();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 监听字段的变化
        if (ME.user != null)
        {
            // 注册回调事件
            ME.user.dbase.RegisterTriggerField("DefenceDeployWnd_DefenceTroop",
                new string[]
                {
                    "defense_troop"
                }, new CallBack(OnChangeDefenceTroop));

            // 注册玩家装备道具事件
            ME.user.baggage.eventCarryChange += BaggageChange;
        }

        // 重置面板位置
        ResetScrollView();

        // 初始数据数据
        InitData();

        // 设置防守宠物显示
        SetDefencePet();

        // 激活复用格子组件
        EnableWrapContent();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in mIndexMap)
        {
            FillData(kv.Key, kv.Value);
        }

        // 显示模型
        GameObject ArenaWnd = WindowMgr.GetWindow("ArenaWnd");
        if (ArenaWnd != null)
        {
            // 获取TweenScale
            TweenScale mTweenScale = ArenaWnd.GetComponent<TweenScale>();

            // 绑定动画结束事件, 再显示模型
            if (mTweenScale != null && mTweenScale.enabled)
                EventDelegate.Add(mTweenScale.onFinished, ShowModel);
            else
                ShowModel();
        }
    }

    /// <summary>
    /// Resets the scroll view.
    /// </summary>
    private void ResetScrollView()
    {
        // 重新设置item的初始位置
        foreach (GameObject item in rePosition.Keys)
        {
            item.transform.localPosition = rePosition[item];
        }

        // 整理位置
        mSrollView.ResetPosition();

        // 重新初始化indexMap
        if (mIndexMap != null)
        {
            mIndexMap.Clear();
            for (int i = 0; i < mRowAmonut; i++)
                mIndexMap.Add(i, -i);
        }
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 隐藏模型
        HideModel();

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 解注册玩家装备道具事件
        ME.user.baggage.eventCarryChange -= BaggageChange;

        // 注销字段监听事件
        ME.user.dbase.RemoveTriggerField("DefenceDeployWnd_DefenceTroop");
    }

    void Update()
    {
        // 选装模型底座的光效
        RotateSelectEffect();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mHelpBtn).onClick = OnClickHelpBtn;

        mWrapContent.onInitializeItem = UpdateItem;
    }

    /// <summary>
    /// 监听字段变化的回调
    /// </summary>
    void OnChangeDefenceTroop(object para, params object[] param)
    {
        // 防御阵容设置成功， 改变确认按钮的状态
        SetConfirmBtnState(120f, false);
    }

    /// <summary>
    /// 包裹变化的回调
    /// </summary>
    void BaggageChange(string[] pos)
    {
        // 窗口没有显示，不处理
        if (gameObject == null ||
            !gameObject.activeSelf ||
            !gameObject.activeInHierarchy)
            return;

        // 获取宠物数据
        mPetObList = UserMgr.GetUserPets(ME.user, false, false, 1);

        // 对宠物按照指定方式排序
        mPetObList = BaggageMgr.SortPetInBag(mPetObList, BaggageMgr.GetMonsterSortType ());

        mWrapContent.maxIndex = mPetObList.Count < mRowAmonut ? mRowAmonut - 1 : mPetObList.Count - 1;
        mWrapContent.minIndex = 0;

        // 填充数据
        foreach (KeyValuePair<int, int> kv in mIndexMap)
            FillData(kv.Key, kv.Value);
    }

    /// <summary>
    /// 激活复用格子组件
    /// </summary>
    void EnableWrapContent()
    {
        mWrapContent.minIndex = 0;
        mWrapContent.maxIndex = mPetObList.Count < mRowAmonut ? mRowAmonut - 1 : mPetObList.Count - 1;

        // 激活格子复用的组件
        mWrapContent.enabled = true;
    }

    /// <summary>
    /// 旋转光效
    /// </summary>
    void RotateSelectEffect()
    {
        for (int i = 0; i < mSelectEffect.Length; i++)
        {
            if (mSelectEffect[i] == null)
                continue;

            mSelectEffect[i].transform.Rotate(Vector3.forward * Time.unscaledDeltaTime * 40);
        }
    }

    /// <summary>
    /// 初始化宠物格子
    /// </summary>
    void InitPetGrid()
    {
        if (mWrapContent.transform.childCount == mRowAmonut)
            return;

        if (mPetItemWnd.activeSelf)
            mPetItemWnd.SetActive(false);

        for (int i = 0; i < mRowAmonut; i++)
        {
            string name = "DefenceDeployWnd_pet_item_" + i;

            if (mItems.ContainsKey(name))
                mItems.Remove(name);

            GameObject petItem = Instantiate(mPetItemWnd);
            if (petItem == null)
                continue;
            petItem.transform.SetParent(mWrapContent.transform);
            petItem.transform.localPosition = Vector3.zero;
            petItem.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            // 设置控件位置
            petItem.transform.localPosition = new Vector3(petItem.transform.localPosition.x + mWrapContent.itemSize * i,
                petItem.transform.localPosition.y,
                petItem.transform.localPosition.z);

            // 添加原始位置映射表
            rePosition.Add(petItem, petItem.transform.localPosition);

            // 显示控件
            petItem.SetActive(true);
            petItem.name = name;

            mItems.Add(name, petItem);

            // 添加缓存列表
            mPetList.Add(petItem.GetComponent<PetItemWnd>());

            // 给宠物格子添加点击事件
            UIEventListener.Get(petItem).onClick = OnClickSelectPet;
            UIEventListener.Get(petItem).onPress = OnPressShowPetInfo;
        }
    }

    void OnPressShowPetInfo(GameObject go, bool isPress)
    {
        //玩家正在滑动宠物列表;
        if (mSrollView.isDragging)
            return;

        //手指抬起时
        if (!isPress)
        {
            mOnPressOb = null;
            return;
        }

        //没有宠物;
        if (go.GetComponent<PetItemWnd>().item_ob == null)
            return;

        mOnPressOb = go.GetComponent<PetItemWnd>().item_ob;
        CancelInvoke("ShowInfo");

        //0.5秒后显示宠物信息界面;
        Invoke("ShowInfo", 0.5f);
    }

    /// <summary>
    /// 显示宠物信息
    /// </summary>
    void ShowInfo()
    {
        // 玩家正在滑动宠物列表;
        if (mSrollView.isDragging)
            return;

        if (mOnPressOb == null)
            return;

        GameObject wnd = WindowMgr.OpenWnd(PetInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<PetInfoWnd>().Bind(mOnPressOb.GetRid(), ME.user.GetName(), ME.user.GetLevel());
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    void InitData()
    {
        // 获取宠物数据
        mPetObList = UserMgr.GetUserPets(ME.user, false, false, 1);

        // 对宠物按照指定方式排序
        mPetObList = BaggageMgr.SortPetInBag(mPetObList, BaggageMgr.GetMonsterSortType ());

        // 清空列表
        mRidList.Clear();

        for (int i = 0; i < mDefencePetAmountLimit; i++)
            mRidList.Add(string.Empty);

        // 获取防守列表
        LPCValue defenseTroop = ME.user.Query("defense_troop");

        if (defenseTroop == null || !defenseTroop.IsArray)
            return;

        defenceList = defenseTroop.AsArray;
        for (int index = 0; index < defenceList.Count; index++)
        {
            // 如果超出了范围不处理
            if ((index + 1) > mDefencePetAmountLimit)
            {
                LogMgr.Trace("防御列表数量大于最大限制数量");
                break;
            }

            // 添加到列表中
            mRidList[index] = defenceList[index].AsString;
        }

        // 刷新队长技能描述
        RefreshLeaderSkillDesc();
    }

    /// <summary>
    /// 滑动列表时的回调函数
    /// </summary>
    void UpdateItem(GameObject go, int wrapIndex, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (!mIndexMap.ContainsKey(wrapIndex))
            mIndexMap.Add(wrapIndex, realIndex);
        else
            mIndexMap[wrapIndex] = realIndex;

        // 填充数据
        FillData(wrapIndex, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    void FillData(int wrapIndex, int realIndex)
    {
        if (mPetObList.Count < 1)
            return;

        int index = Mathf.Abs(realIndex);

        if (index + 1 > mPetObList.Count)
            return;

        Property ob = mPetObList[index];
        if (ob == null)
            return;

        string name = string.Format("DefenceDeployWnd_pet_item_{0}", wrapIndex);

        GameObject wnd = null;
        if (!mItems.ContainsKey(name))
            return;

        wnd = mItems[name];
        if (wnd == null)
            return;

        PetItemWnd script = wnd.GetComponent<PetItemWnd>();
        if (script == null)
            return;

        script.SetBind(ob);

        LPCValue openArena = ME.user.Query<LPCValue>("open_arena");
        if (openArena == null || !openArena.IsInt || openArena.AsInt != 1)
        {
            script.ShowCover(true);
            return;
        }

        if (GetSelectPetAmount() >= mDefencePetAmountLimit)
        {
            script.ShowCover(true);
        }
        else
        {
            script.ShowCover(false);
        }

        if (mRidList.Contains(ob.GetRid()))
        {
            script.SetSelected(true, true);
        }
        else
        {
            script.SetSelected(false, false);
        }
    }


    /// <summary>
    /// 设置玩家防守的阵型
    /// </summary>
    void SetDefencePet()
    {
        // 玩家没有设置防守阵营
        if (defenceList == null)
        {
            SetConfirmBtnState(255f, true);
            return;
        }

        // 设置确认窗口的状态
        SetConfirmBtnState(120f, false);
    }

    int GetSelectPetAmount()
    {
        int index = 0;
        for (int i = 0; i < mRidList.Count; i++)
        {
            if (string.IsNullOrEmpty(mRidList[i]))
                continue;

            index++;
        }
        return index;
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    void ShowModel()
    {
        // 遍历各个防守成员
        for (int i = 0; i < defenceList.Count; i++)
        {
            // 查找宠物对象
            Property ob = Rid.FindObjectByRid(defenceList[i].AsString);

            // 宠物对象不存在
            if (ob == null)
                continue;

            if (i + 1 > mModels.Length)
                continue;

            // 隐藏模型
            mModels[i].gameObject.SetActive(true);

            // 异步载入模型
            mModels[i].LoadModelSync(ob, LayerMask.NameToLayer("UI"), new CallBack(OnClickModel));
        }
    }

    /// <summary>
    /// 隐藏模型
    /// </summary>
    void HideModel()
    {
        // 窗口隐藏是卸载模型
        for (int i = 0; i < mModels.Length; i++)
        {
            // 如果mModels[i]对象不存在
            if (mModels[i] == null)
                continue;

            // 隐藏模型
            mModels[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 点击模型回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    void OnClickModel(object para, params object[] _params)
    {
        GameObject modelOb = (GameObject)_params[0];

        if (modelOb == null)
            return;

        ModelWnd modelWnd = modelOb.transform.parent.GetComponent<ModelWnd>();

        if (modelWnd == null)
            return;

        Property ob = modelWnd.mPetOb;

        if (ob == null)
            return;

        // 获取宠物的rid;
        string rid = ob.GetRid();

        if (string.IsNullOrEmpty(rid))
            return;

        for (int i = 0; i < mPetObList.Count; i++)
        {
            if (!mPetObList[i].GetRid().Equals(rid))
                continue;

            // 卸载模型
            modelWnd.UnLoadModel();

            modelWnd.mPetOb = null;

            break;
        }

        int index = mRidList.IndexOf(rid);

        if (index >= 0 && index + 1 <= mRidList.Count)
            mRidList[index] = string.Empty;

        // 刷新队长技能描述
        RefreshLeaderSkillDesc();

        SetConfirmBtnState(255f, true);

        // 填充数据
        foreach (KeyValuePair<int, int> kv in mIndexMap)
            FillData(kv.Key, kv.Value);
    }

    /// <summary>
    /// 刷新队长技能描述
    /// </summary>
    void RefreshLeaderSkillDesc()
    {
        if (mRidList == null || mRidList.Count == 0)
        {
            // 无buff
            mLeaderSkillDesc.text = LocalizationMgr.Get("DefenceDeployWnd_10");
            return;
        }

        Property ob = Rid.FindObjectByRid(mRidList[0]);

        if (ob == null)
        {
            // 无buff
            mLeaderSkillDesc.text = LocalizationMgr.Get("DefenceDeployWnd_10");
            return;
        }

        string leaderDesc = SkillMgr.GetLeaderSkillDesc(ob);

        mLeaderSkillDesc.text = string.IsNullOrEmpty(leaderDesc) ? LocalizationMgr.Get("DefenceDeployWnd_10") : leaderDesc;
    }

    /// <summary>
    /// 点击选中宠物
    /// </summary>
    void OnClickSelectPet(GameObject go)
    {
        LPCValue openArena = ME.user.Query<LPCValue>("open_arena");
        if (openArena == null || !openArena.IsInt || openArena.AsInt != 1)
        {
            DialogMgr.Notify(LocalizationMgr.Get("DefenceDeployWnd_13"));
            return;
        }

        PetItemWnd petItemWnd = go.GetComponent<PetItemWnd>();
        Property ob = petItemWnd.item_ob;

        if (ob == null)
            return;

        if (mRidList.Contains(ob.GetRid()))
        {
            OnClickCancelSelectPet(go);
            return;
        }

        // 防御宠物数量达到上限
        if (GetSelectPetAmount() >= mDefencePetAmountLimit)
            return;

        int index = 0;

        for (int i = 0; i < mRidList.Count; i++)
        {
            if (string.IsNullOrEmpty(mRidList[i]))
            {
                mRidList[i] = ob.GetRid();

                index = i;

                break;
            }
        }

        // 异步载入模型
        mModels[index].LoadModelSync(ob, LayerMask.NameToLayer("UI"), new CallBack(OnClickModel));

        mModels[index].gameObject.SetActive(true);

        // 刷新队长技能描述
        RefreshLeaderSkillDesc();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in mIndexMap)
            FillData(kv.Key, kv.Value);

        SetConfirmBtnState(255f, true);
    }

    /// <summary>
    /// 点击取消选择宠物
    /// </summary>
    void OnClickCancelSelectPet(GameObject go)
    {
        Property ob = go.GetComponent<PetItemWnd>().item_ob;

        if (ob == null)
            return;

        if (!mRidList.Contains(ob.GetRid()))
            return;

        int index = mRidList.IndexOf(ob.GetRid());

        if (mModels[index].mPetOb == null)
            return;

        if (!mModels[index].mPetOb.GetRid().Equals(ob.GetRid()))
            return;

        // 卸载宠物模型
        mModels[index].UnLoadModel();

        // 移除缓存的模型
        mModels[index].mPetOb = null;

        mRidList[index] = string.Empty;

        // 刷新队长技能描述
        RefreshLeaderSkillDesc();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in mIndexMap)
            FillData(kv.Key, kv.Value);

        SetConfirmBtnState(255f, true);
    }

    /// <summary>
    /// 帮助信息查看点击事件
    /// </summary>
    void OnClickHelpBtn(GameObject go)
    {
        GameObject wnd = WindowMgr.OpenWnd(HelpWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // TODO
        wnd.GetComponent<HelpWnd>().Bind(HelpConst.DUNGEONS_ID);
    }

    /// <summary>
    /// 确认部署按钮点击事件
    /// </summary>
    void OnClickConfirmDeployBtn(GameObject go)
    {
        LPCValue openArena = ME.user.Query<LPCValue>("open_arena");
        if (openArena == null || !openArena.IsInt || openArena.AsInt != 1)
        {
            DialogMgr.Notify(LocalizationMgr.Get("DefenceDeployWnd_13"));
            return;
        }

        // 防守宠物数量
        int amount = 0;

        updateDefenceList = LPCArray.Empty;

        for (int i = 0; i < mDefencePetAmountLimit; i++)
        {
            updateDefenceList.Add(mRidList[i]);

            if (string.IsNullOrEmpty(mRidList[i]))
                amount++;
        }

        if (amount > 0)
        {
            DialogMgr.ShowDailog(
                new CallBack(OnClickCallBack),
                LocalizationMgr.Get("DefenceDeployWnd_11"),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
        }
        else
        {
            // 通知服务器设置防御列表
            Operation.CmdSetArenaDefenseTroop.Go(updateDefenceList);
        }
    }

    void SetConfirmBtnState(float colorValue, bool isActive)
    {
        float rgb = colorValue / 255;
        Color color = new Color(rgb, rgb, rgb);

        mConfirmDeployBtn.GetComponent<UISprite>().color = color;

        mEffect.gameObject.SetActive(isActive);

        mEffect.enabled = isActive;

        if (isActive)
            UIEventListener.Get(mConfirmDeployBtn).onClick = OnClickConfirmDeployBtn;
        else
            UIEventListener.Get(mConfirmDeployBtn).onClick -= OnClickConfirmDeployBtn;
    }

    void OnClickCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        int amount = 0;
        foreach (LPCValue item in updateDefenceList.Values)
        {
            if (string.IsNullOrEmpty(item.AsString))
                amount++;
        }

        // 没有设置防御宠物
        if (amount == mDefencePetAmountLimit)
        {
            DialogMgr.Notify(LocalizationMgr.Get("DefenceDeployWnd_12"));
            return;
        }

        // 通知服务器设置防御列表
        Operation.CmdSetArenaDefenseTroop.Go(updateDefenceList);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mLeaderSkillDescLb.text = LocalizationMgr.Get("DefenceDeployWnd_1");
        mHelpBtnLb.text = LocalizationMgr.Get("DefenceDeployWnd_2");
        mDefenceTowerLb.text = LocalizationMgr.Get("DefenceDeployWnd_3");
//        mShopTips.text = LocalizationMgr.Get("DefenceDeployWnd_4");
        mChangeTips.text = LocalizationMgr.Get("DefenceDeployWnd_5");
        mConfirmDeployBtnLb.text = LocalizationMgr.Get("DefenceDeployWnd_6");
//        mAttackLb.text = LocalizationMgr.Get("DefenceDeployWnd_7");
//        mAtkSpeedLb.text = LocalizationMgr.Get("DefenceDeployWnd_8");
//        mGainLb.text = LocalizationMgr.Get("DefenceDeployWnd_9");
    }

    /// <summary>
    /// 指引选择防御部署宠物
    /// </summary>
    public void GuideSelectPet(string itemName)
    {
        GameObject item = null;

        if (!mItems.TryGetValue(itemName, out item))
            return;

        OnClickSelectPet(item);
    }

    /// <summary>
    /// 指引点击确认部署按钮
    /// </summary>
    public void GuideOnClickConfirmDeploy()
    {
        LPCValue openArena = ME.user.Query<LPCValue>("open_arena");
        if (openArena == null || !openArena.IsInt || openArena.AsInt != 1)
        {
            DialogMgr.Notify(LocalizationMgr.Get("DefenceDeployWnd_13"));
            return;
        }

        updateDefenceList = LPCArray.Empty;

        for (int i = 0; i < mDefencePetAmountLimit; i++)
            updateDefenceList.Add(mRidList[i]);

        // 通知服务器设置防御列表
        Operation.CmdSetArenaDefenseTroop.Go(updateDefenceList);
    }

    #endregion
}
