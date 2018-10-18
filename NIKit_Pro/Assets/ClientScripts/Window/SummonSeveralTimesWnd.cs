/// <summary>
/// SummonSeveralTimesWnd.cs
/// Created by lic 2016-8-8
/// 显示多次召唤结果界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SummonSeveralTimesWnd : WindowBase<SummonSeveralTimesWnd>
{

    #region 成员变量

    public Vector2 mInitPos = new Vector2(-171, 172);
    public Vector2 mItemSpace = new Vector2(6, 6);
    public Vector2 mItemSize = new Vector2(108, 108);

    // 每4个item为一行
    public int mRowCount = 4;

    public GameObject mContainer;
    public UIScrollView mScrollView;

    public GameObject mCloseBtn;

    // 宠物属性参数值
    public GameObject mSureBtn;

    //分享按钮
    public GameObject mShareBtn;

    // 本地化文字
    public UILabel mNameLb;
    public UILabel mSureLb;

    public GameObject mPetItem;

    #endregion

    #region 私有变量

    // 绑定的宠物对象
    private List<Property> item_obs = new List<Property>();

    private string selectedRid = "";

    // 关闭按钮点击回调
    private CallBack task;

    // name与OB映射
    private Dictionary<string, GameObject> mPosObMap = new Dictionary<string, GameObject>();

    CallBack mShareCallback;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mSureBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mShareBtn).onClick = OnShareBtn;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 本地化文本
        mSureLb.text = LocalizationMgr.Get("SummonWnd_17");

        mNameLb.text = LocalizationMgr.Get("SummonWnd_24");

        mShareBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("RewardPetInfoWnd_16");

        mPetItem.SetActive (false);
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        // 取消选中状态
        selectedRid = "";

        foreach(Transform item in mContainer.transform)
            item.gameObject.SetActive(false);

        if(item_obs == null || item_obs.Count == 0)
            return;

        GameObject posWnd;

        // 对使魔按照星级进行排序
        item_obs = BaggageMgr.SortPetInBag(item_obs, MonsterConst.SORT_BY_STAR);

        // 生成克隆宠物列表
        for(int i = 0; i < item_obs.Count; i++)
        {
            if (mPosObMap.ContainsKey (string.Format ("equip_item_{0}", i)))
                posWnd = mPosObMap [string.Format ("equip_item_{0}", i)];
            else
            {
                posWnd = Instantiate (mPetItem) as GameObject;
                posWnd.transform.parent = mContainer.transform;
                posWnd.name = string.Format("equip_item_{0}", i);
                posWnd.transform.localScale = new Vector3(0.87f, 0.87f, 0.87f);
                posWnd.transform.localPosition = new Vector3((mItemSize.x + mItemSpace.x)*(i%mRowCount), - (mItemSize.y + mItemSpace.y)*(i/mRowCount), 0f);

                mPosObMap.Add (string.Format("equip_item_{0}", i), posWnd);

                // 注册点击事件
                UIEventListener.Get(posWnd).onClick = OnItemClicked;
            }

            posWnd.GetComponent<PetItemWnd>().SetBind(item_obs[i]);

            posWnd.GetComponent<PetItemWnd>().SetSelected(false);

            posWnd.SetActive(true);
        }

    }

    /// <summary>
    /// 关闭(确认)按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        if(task != null)
            task.Go();

        WindowMgr.DestroyWindow(gameObject.name);

        GameObject infoWnd = WindowMgr.GetWindow("PetSimpleInfoWnd_Summon");

        // 关闭详细信息界面
        if(infoWnd != null)
            WindowMgr.DestroyWindow("PetSimpleInfoWnd_Summon");
    }

    /// <summary>
    /// 分享按钮点击事件
    /// </summary>
    /// <param name="go"></param>
    private void OnShareBtn(GameObject go)
    {
        if (mShareCallback != null)
            mShareCallback.Go();
    }

    /// <summary>
    /// 宠物被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnItemClicked(GameObject ob)
    {
        // 取得gameobject上绑定的item
        PetItemWnd item = ob.GetComponent<PetItemWnd>();

        if(item == null)
            return;

        ItemSelected(ob);
    }

    /// <summary>
    /// 设置包裹格子选中
    /// </summary>
    /// <param name="wnd">Window.</param>
    /// <param name="is_selected">If set to <c>true</c> is_selected.</param>
    private void ItemSelected(GameObject ob)
    {
        PetItemWnd item = ob.GetComponent<PetItemWnd>();

        if(item == null)
            return;

        Property item_ob = item.item_ob;

        if(item_ob == null)
            return;

        // 如果之前有选中，需要先取消之前选中状态
        if(!string.IsNullOrEmpty(selectedRid))
        {
            // 选中同一个不处理
            if (selectedRid.Equals(item_ob.GetRid()))
                return;

            foreach (Transform child in mContainer.transform)
            {
                Property pet_ob = child.GetComponent<PetItemWnd>().item_ob;

                if(pet_ob == null)
                    continue;

                if(pet_ob.GetRid().Equals(selectedRid))
                    child.GetComponent<PetItemWnd>().SetSelected(false);
            }
        }

        GameObject petSimpleInfoWnd = WindowMgr.OpenWnd("PetSimpleInfoWnd_Summon", transform.parent);

        PetSimpleInfoWnd script = petSimpleInfoWnd.GetComponent<PetSimpleInfoWnd>();

        script.Bind(item_ob, false, false);

        script.ShowBtn(true);

        script.SetCallBack(new CallBack(OnCloseInfoWnd));

        petSimpleInfoWnd.transform.localPosition = new Vector3(-336, 0, 0);

        // 重新标记选中
        selectedRid = item_ob.GetRid();

        item.SetSelected(true);
    }

    /// <summary>
    /// 关闭宠物详细信息窗口回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnCloseInfoWnd(object para, params object[] param)
    {
        //取消当前选中
        foreach (Transform child in mContainer.transform)
        {
            Property pet_ob = child.GetComponent<PetItemWnd>().item_ob;

            if(pet_ob == null)
                continue;

            if(pet_ob.GetRid().Equals(selectedRid))
            {
                child.GetComponent<PetItemWnd>().SetSelected(false);
                break;
            }
        }

        // 将选中项置空
        selectedRid = string.Empty;
    }


    #endregion

    #region 外部接口
    /// <summary>
    /// 分享功能入口开关
    /// </summary>
    /// <param name="isActive"></param>
    public void SetShareBtn(bool isActive)
    {
        mShareBtn.SetActive(isActive);
        mSureBtn.transform.localPosition = isActive ? new Vector3(460f, -233.3f, 0) : new Vector3(341f, -233.3f, 0f);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void BindData(List<Property> item_obs, CallBack _callback = null)
    {
        task = _callback;

        this.item_obs = item_obs;

        Redraw();
    }

    /// <summary>
    /// 绑定点击分享按钮回调
    /// </summary>
    /// <param name="callBack"></param>
    public void SetShareCallBack(CallBack callBack)
    {
        mShareCallback = callBack;
    }

    #endregion
}
