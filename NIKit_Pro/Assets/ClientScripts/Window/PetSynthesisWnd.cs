/// <summary>
/// PetSynthesisWnd.cs
/// Created by lic 2017/01/17
/// 宠物合成界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class PetSynthesisWnd : WindowBase<PetSynthesisWnd>
{
    // 从开始播放光效到播放白色闪光的时间
    public float playWhiteFlashTime = 4f;
    public float waitResultTime = 0.5f;
    public float playSummonAniTime = 2f;

    public UILabel mTitle;
    public GameObject mCloseBtn;

    public GameObject mContainer;

    public GameObject mPetSynthesisInfoWnd;

    public GameObject[] mMaterialModelGroup;
    public GameObject mPetModel;
    public GameObject mPetSyntheAni;
    public GameObject mSkipBtn;
    public GameObject mWhiteMask;
    public GameObject mCycle;
    public GameObject mEffectsWnd;
    public GameObject mModelAndCyle;

    public GameObject mPrefectEfect;
    public GameObject mPrefectEffectLb;
    public GameObject[] mStarEffects;

    public GameObject mPetItem;

    public TweenScale mTweenScale;

    #region 公共字段

    // 预创格子行数
    const int mRowNum = 6;

    // 格子列数
    const int mColumnNum = 3;

    // 宠物数据
    List<Property> mPetsData = new List<Property>();

    // 当前显示数据的index与实际数据的对应关系
    Dictionary<int, int> indexMap = new Dictionary<int, int>();

    // name与Ob的映射
    Dictionary<string, GameObject> mPosObMap = new Dictionary<string, GameObject> ();

    // 当前选中
    string selectRid = string.Empty;

    // 是否是第一次打开
    bool isFirstOpen = true;

    // 能合成列表
    List<int> CanSyntheList = new List<int>();

    // 光效名称
    string[] mEffectNames = new string[]{"Awake_f", "Awake_s", "Awake_w", "Awake_l", "Awake_d"};

    // 觉醒光效
    List<GameObject> mEffects = new List<GameObject>();

    // 当前正在播放合成动画的pet
    Property mCurSynPetOb = null;

    #endregion

    #region 内部函数

    void Start()
    {
        // 注册事件
        RegisterEvent();

        RedrawCanSyntheList ();

        // 加载光效
        mEffects = mEffectsWnd.GetComponent<EffectWnd>().LoadEffects(mEffectNames, Game.UnitToPixelScale);

        mPetSyntheAni.SetActive(false);

        //初始化窗口
        InitWnd();

        // 创建宠物格子
        CreatePos();

        // 初始化数据
        InitData();
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
        // 关闭按钮
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;

        // 跳过按钮
        UIEventListener.Get(mSkipBtn).onClick = OnSkipBtn;

        mContainer.GetComponent<UIWrapContent>().onInitializeItem = OnUpdateItem;

        // 注册宠物合成事件
        EventMgr.RegisterEvent("PetSynthesisiWnd", EventMgrEventType.EVENT_PET_SYNTHESIS, OnPetSynthesis);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mTitle.text = LocalizationMgr.Get("PetSynthesisWnd_1");

        mPetItem.SetActive (false);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 析构掉property
        for (int i = 0; i < mPetsData.Count; i++)
        {
            if (mPetsData [i] == null)
                continue;

            mPetsData [i].Destroy ();
        }

        EventMgr.UnregisterEvent("PetSynthesisiWnd");
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        // 打开主窗口
        WindowMgr.OpenWnd("MainWnd");

        // 隐藏窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 宠物觉醒消息回调
    /// </summary>
    void OnPetSynthesis(int eventId, MixedValue para)
    {
        if (para == null)
            return;

        LPCMapping map = para.GetValue<LPCMapping>();

        if (map == null)
            return;

        string rid = map.GetValue<string>("rid");

        mCurSynPetOb = Rid.FindObjectByRid(rid);
        if (mCurSynPetOb == null)
            return;

        mPetSyntheAni.SetActive(true);
        mPetSyntheAni.GetComponent<TweenAlpha>().PlayForward();
        mSkipBtn.SetActive(false);

        // 获取觉醒类型(根据觉醒类型播放相应的光效)
        int element = mCurSynPetOb.Query<int>("element");

        mEffects[element - 1].SetActive(true);
        mEffects[element - 1].GetComponent<ParticleSystem>().Play();

        mCycle.GetComponent<TweenAlpha>().enabled = true;
        mCycle.GetComponent<TweenAlpha>().ResetToBeginning();
        mCycle.GetComponent<TweenScale>().enabled = true;
        mCycle.GetComponent<TweenScale>().ResetToBeginning();

        mModelAndCyle.GetComponent<TweenPosition> ().ResetToBeginning ();

        // 加载模型
        loadMaterialModel ();

        // 异步预加载合成模型
        Coroutine.DispatchService(SyncPreLoadModel(), "SyncPreLoadModel");

        Coroutine.DispatchService(SyncShowEffect(), "SyncShowEffect");
    }

    /// <summary>
    /// 加载合成材料模型
    /// </summary>
    void loadMaterialModel ()
    {
        LPCArray material_cost = PetsmithMgr.GetSynthesisMaterials (mCurSynPetOb.GetClassID());

        for (int i = 0; i < mMaterialModelGroup.Length; i++)
        {
            if (i >= material_cost.Count)
            {
                mMaterialModelGroup [i].SetActive (false);
                continue;
            }

            mMaterialModelGroup [i].SetActive (true);

            // 异步加载模型
            mMaterialModelGroup [i].GetComponent<ModelWnd> ().LoadModelSync
                (material_cost[i].AsMapping.GetValue<int>("class_id"), material_cost[i].AsMapping.GetValue<int>("rank"), 
                    LayerMask.NameToLayer("UI"));
        }
    }

    /// <summary>
    /// 预加载合成模型
    /// </summary>
    /// <returns>The show effect.</returns>
    IEnumerator SyncPreLoadModel()
    {
        string modelId = MonsterMgr.GetModel(mCurSynPetOb.GetClassID());

        // 异步加载宠物模型
        yield return Coroutine.DispatchService(ResourceMgr.LoadAsync(MonsterMgr.GetModelResPath(modelId)));

        mSkipBtn.SetActive(true);
    }

    /// <summary>
    /// 播放合成光效
    /// </summary>
    /// <returns>The show effect.</returns>
    IEnumerator SyncShowEffect()
    {
        yield return new WaitForSeconds(playWhiteFlashTime);

        ShowNewModel();
    }

    /// <summary>
    /// 显示新的模型
    /// </summary>
    void ShowNewModel()
    {
        // 播放白色闪屏
        mWhiteMask.GetComponent<TweenAlpha>().enabled = true;
        mWhiteMask.GetComponent<TweenAlpha>().ResetToBeginning();

        for (int i = 0; i < mMaterialModelGroup.Length; i++)
            mMaterialModelGroup [i].SetActive (false);

        // 加载新的模型
        mPetModel.GetComponent<ModelWnd>().LoadModel(mCurSynPetOb, LayerMask.NameToLayer("UI"));

        mSkipBtn.SetActive (false);

        Coroutine.DispatchService(SyncWaitResult(), "SyncWaitResult");
    }

    /// <summary>
    /// 跳过按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSkipBtn(GameObject ob)
    {
        Coroutine.StopCoroutine("SyncShowEffect");

        ShowNewModel();
    }

    /// <summary>
    /// 等待显示结果
    /// </summary>
    /// <returns>The wait result.</returns>
    IEnumerator SyncWaitResult()
    {
        yield return new WaitForSeconds(waitResultTime);

        // 播放觉醒动画
        ShowResult();
    }

    /// <summary>
    /// 显示结果
    /// </summary>
    void ShowResult()
    {
        playSummonAni ();

        Coroutine.DispatchService(SyncPetInfo(), "SyncPetInfo");
    }

    /// <summary>
    /// 播放召唤动画
    /// </summary>
    void playSummonAni()
    {
        mPrefectEfect.SetActive(true);
        mPrefectEfect.GetComponent<TweenAlpha> ().ResetToBeginning ();

        // 召唤的星级
        int stars = mCurSynPetOb.GetStar();

        if (stars < 3)
            stars = 3;

        mStarEffects[stars - 3].SetActive(true);

        // 检测宠物觉醒状态
        string starName = PetMgr.GetStarName(mCurSynPetOb.GetRank());

        foreach (Transform star in mStarEffects[stars - 3].transform)
        {
            star.GetComponent<TweenAlpha>().PlayForward();

            star.GetComponent<TweenScale>().PlayForward();

            star.GetComponent<UISprite> ().spriteName = starName;
        }

        mPrefectEffectLb.GetComponent<TweenAlpha>().PlayForward();

        mPrefectEffectLb.GetComponent<TweenScale>().PlayForward();
    }

    /// <summary>
    /// 显示宠物信息界面
    /// </summary>
    /// <returns>The summon.</returns>
    IEnumerator SyncPetInfo()
    {
        yield return new WaitForSeconds(playSummonAniTime);

        mPrefectEfect.GetComponent<TweenAlpha>().PlayForward();

        mModelAndCyle.GetComponent<TweenPosition> ().enabled = true;
 
        // 显示宠物信息界面
        GameObject petInfoWnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (petInfoWnd == null)
            yield break;

        petInfoWnd.transform.localPosition = new Vector3(335f, 0f, 0f);
        PetSimpleInfoWnd petInfo = petInfoWnd.GetComponent<PetSimpleInfoWnd>();

        petInfo.Bind(mCurSynPetOb, false);
        petInfo.ShowBtn(true);
        petInfo.SetCallBack (new CallBack(CloseInfoWndCallBack));
    }

    /// <summary>
    /// 商品信息窗口关闭回调函数
    /// </summary>
    void CloseInfoWndCallBack(object para, params object[] param)
    {
        // 卸载模型
        UnloadModel ();

        mPetSyntheAni.SetActive (false);

        // 刷新合成列表
        Redraw ();
    }

    /// <summary>
    /// 卸载模型
    /// </summary>
    void UnloadModel ()
    {
        mPetModel.GetComponent<ModelWnd> ().UnLoadModel ();

        for (int i = 0; i < mMaterialModelGroup.Length; i++)
            mMaterialModelGroup [i].GetComponent<ModelWnd> ().UnLoadModel ();
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    void InitData()
    {
        // 获取能够合成的宠物数据
        List<int> synthesisList = PetsmithMgr.GetSynthesisData();

        foreach (int classId in synthesisList)
        {
            Property pet = CreateSynthesisPet (classId);

            if (pet == null)
                continue;

            mPetsData.Add (pet);
        }

        int Row = mRowNum;

        // 宠物数量大于预创建的格子
        if (mPetsData.Count > mRowNum * mColumnNum)
            Row = mPetsData.Count % mColumnNum == 0 ?
                mPetsData.Count / mColumnNum : mPetsData.Count / mColumnNum + 1; 

        // 从0开始
        mContainer.GetComponent<UIWrapContent>().maxIndex = 0;

        mContainer.GetComponent<UIWrapContent>().minIndex = -(Row - 1);
    }

    /// <summary>
    /// 生成合成的宠物
    /// </summary>
    /// <param name="classId">Class identifier.</param>
    Property CreateSynthesisPet(int classId)
    {
        LPCMapping dbase = new LPCMapping ();

        dbase.Add ("class_id", classId);
        dbase.Add ("rid", Rid.New());
        dbase.Add ("level", 1);

        return PropertyMgr.CreateProperty (dbase);
    }

    /// <summary>
    ///设置滑动列表时复用宠物格子更改数据
    /// </summary>
    void OnUpdateItem(GameObject go, int index, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (!indexMap.ContainsKey(index))
            indexMap.Add(index, realIndex);
        else
            indexMap[index] = realIndex;

        FillData(index, realIndex);
    }

    /// <summary>
    /// 刷新包裹
    /// </summary>
    void Redraw()
    {
        // 刷新能合成列表
        RedrawCanSyntheList();
 
        mPetSynthesisInfoWnd.GetComponent<PetSynthesisInfoWnd> ().RedrawWnd ();

        GameObject wnd = WindowMgr.GetWindow (PetSynthesisInfoWnd.WndType);

        if(wnd != null)
            wnd.GetComponent<PetSynthesisInfoWnd> ().RedrawWnd ();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in indexMap)
            FillData(kv.Key, kv.Value);
    }

    /// <summary>
    /// 创建宠物格子
    /// </summary>
    private void CreatePos()
    {
        // 生成格子，只生成这么多格子，动态复用
        for (int i = 0; i < mRowNum; i++)
        {

            GameObject rowItemOb = new GameObject();
            rowItemOb.name = string.Format("PetSynthesisWnd_item_", i);
            rowItemOb.transform.parent = mContainer.transform;
            rowItemOb.transform.localPosition = new Vector3(0, - i * 130, 0);
            rowItemOb.transform.localScale = Vector3.one;

            for(int j = 0; j < mColumnNum; j++)
            {
                GameObject posWnd = Instantiate (mPetItem) as GameObject;
                posWnd.transform.parent = rowItemOb.transform;
                posWnd.name = string.Format("PetSynthesisWnd_pet_{0}_{1}", i, j);
                posWnd.transform.localScale = Vector3.one;
                posWnd.transform.localPosition = new Vector3(130 * j, 0, 0);

                mPosObMap.Add (string.Format("PetSynthesisWnd_pet_{0}_{1}", i, j), posWnd);

                posWnd.GetComponent<PetItemWnd>().ShowLevel(false);
                posWnd.GetComponent<PetItemWnd>().ShowLeaderSkill(false);
                posWnd.GetComponent<PetItemWnd>().SetSelectPos (SelectPos.BottonRightCorner);

                posWnd.SetActive(true);

                // 注册点击事件
                UIEventListener.Get(posWnd).onClick = OnItemClick;
            }

        }
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="realIndex">Real index.</param>
    void FillData(int index, int realIndex)
    {
        for (int i = 0; i < mColumnNum; i++)
        {
            GameObject wnd = mPosObMap [string.Format ("PetSynthesisWnd_pet_{0}_{1}", index, i)];

            if (wnd == null)
                continue;

            PetItemWnd item = wnd.GetComponent<PetItemWnd>();

            item.SetSelected(false);

            int dataIndex = System.Math.Abs(realIndex) * mColumnNum + i; 

            if (dataIndex < mPetsData.Count)
            {
                item.SetBind(mPetsData[dataIndex]);

                // 第一次打开默认选中第一个
                if (isFirstOpen && string.IsNullOrEmpty (selectRid))
                {
                    // 最开始默认选中第一个
                    if (dataIndex == 0)
                        OnItemClick (wnd);
                } else
                {
                    // 设置选中
                    if (mPetsData[dataIndex].GetRid().Equals(selectRid))
                        item.SetSelected(true);
                    else
                        item.SetSelected(false);
                }
                    
                // 设置能合成动画
                if (CanSyntheList.Contains(mPetsData [dataIndex].GetClassID ()))
                    item.SetAnima (true);
                else
                    item.SetAnima (false);
            }
            else
            {
                item.SetBind(null);
            }
        }
    }

    /// <summary>
    /// 宠物格子被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnItemClick(GameObject ob)
    {
        // 取得gameobject上绑定的item
        PetItemWnd item = ob.GetComponent<PetItemWnd>();

        // 空格子不处理
        if (item.item_ob == null)
            return;

        // 以选中不处理
        if (item.isSelected)
            return;

        // 如果之前有选中，需要先取消之前选中状态
        if (! string.IsNullOrEmpty(selectRid))
        {
            foreach (Transform child in mContainer.transform)
            {
                foreach (Transform petWnd in child)
                {
                    Property pet_ob = petWnd.GetComponent<PetItemWnd>().item_ob;

                    if (pet_ob == null)
                        continue;

                    if (pet_ob.GetRid().Equals(selectRid))
                        petWnd.GetComponent<PetItemWnd>().SetSelected(false, false);
                }
            }

        }

        // 重新标记选中
        selectRid = item.item_ob.GetRid();

        item.SetSelected(true);

        mPetSynthesisInfoWnd.GetComponent<PetSynthesisInfoWnd>().BindData(item.item_ob);
    }

    /// <summary>
    /// 关闭宠物合成详细信息窗口回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnCloseInfoWnd(object para, params object[] param)
    {
        //取消当前选中
        foreach (Transform child in mContainer.transform)
        {
            foreach (Transform petWnd in child)
            {
                Property pet_ob = petWnd.GetComponent<PetItemWnd>().item_ob;

                if(pet_ob == null)
                    continue;

                if(pet_ob.GetRid().Equals(selectRid))
                {
                    petWnd.GetComponent<PetItemWnd>().SetSelected(false);
                    break;
                }
            }
        }

        isFirstOpen = false;

        // 将选中项置空
        selectRid = string.Empty;
    }

    /// <summary>
    /// 刷新能合成列表
    /// </summary>
    void RedrawCanSyntheList()
    {
        CanSyntheList.Clear();

        List<int> synthesisList = PetsmithMgr.GetSynthesisData();

        for (int i = 0; i < synthesisList.Count; i++) 
        {
            if (PetsmithMgr.CanDoSynthe (ME.user, synthesisList [i]))
                CanSyntheList.Add (synthesisList [i]);
        }
    }

    #endregion
}
