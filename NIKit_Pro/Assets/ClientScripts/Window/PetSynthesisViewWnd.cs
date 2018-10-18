/// <summary>
/// PetSynthesisWnd.cs
/// Created by lic 2017/01/17
/// 宠物合成界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class PetSynthesisViewWnd : WindowBase<PetSynthesisViewWnd>
{
    public GameObject mCloseBtn;

    public Transform mPanel;

    public GameObject mPetSynthesisInfoWnd;

    public GameObject mScrollView;

    public GameObject mPetItem;

    #region 内部字段

    // item超过scrollview后,主动偏移显示的位置
    int overPos = 4;

    int mClassId = 0;

    int mSelect = 0;

    int targetId = 0;

    // 宠物对象数据
    List<Property> mPetsData = new List<Property>();

    List<int> mSyntheData = new List<int>();

    #endregion

    #region 内部函数

    void Awake()
    {
        CreateItem ();
    }

    void Start()
    {
        // 注册事件
        RegisterEvent();

        mPetItem.SetActive(false);

        TweenScale mTweenScale = GetComponent<TweenScale>();

        if(mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
    }

    /// <summary>
    /// 根据合成规则创建格子
    /// </summary>
    void CreateItem()
    {
        // 获取所有二级合成的数据
        mSyntheData = PetsmithMgr.GetSynthesisData(2);

        // 数据为空
        if (mSyntheData == null || mSyntheData.Count == 0)
            return;

        for (int i = 0; i < mSyntheData.Count; i++)
        {
            int classId = mSyntheData[i];

            Property pet = CreateSynthesisPet (classId);

            mPetsData.Add (pet);


            GameObject itemWnd = Instantiate (mPetItem) as GameObject;
            itemWnd.transform.parent = mPanel;
            itemWnd.name = string.Format("PetItemWnd_{0}", i);
            itemWnd.transform.localScale = Vector3.one;
            itemWnd.transform.localPosition = new Vector3(0, -i * 135, 0);

            itemWnd.SetActive(true);

            itemWnd.GetComponent<PetItemWnd>().SetBind(pet);

            UIEventListener.Get(itemWnd).onClick = OnPetItemClick;
                
        }
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
    /// item被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnPetItemClick(GameObject ob)
    {
        PetItemWnd wnd = ob.GetComponent<PetItemWnd>();

        if (wnd == null)
            return;

        Property item = wnd.item_ob;

        if (item == null)
            return;

        mPetSynthesisInfoWnd.GetComponent<PetSynthesisInfoWnd> ().BindData (item, null , mClassId);

        if (wnd.isSelected)
            return;

        wnd.SetSelected (true);

        // 之前没有选中
        if (mSelect <= 0)
        {
            mSelect = item.GetClassID();

            return;
        }

        // 取消掉之前的选中
        foreach (Transform tf in mPanel.transform)
        {
            Property data = tf.GetComponent<PetItemWnd> ().item_ob;

            if (data.GetClassID () == mSelect)
            {
                tf.GetComponent<PetItemWnd> ().SetSelected (false);
                break;
            }
        }

        mSelect = item.GetClassID();
    }

    /// <summary>
    /// 刷新合成目标标示
    /// </summary>
    void RedrawTarget()
    {
        foreach (Transform tf in mPanel.transform)
        {
            Property item = tf.GetComponent<PetItemWnd> ().item_ob;

            if (item.GetClassID () == targetId)
            {
                tf.GetComponent<PetItemWnd> ().SetAnima (true);
                OnPetItemClick (tf.gameObject);
            }
            else
                tf.GetComponent<PetItemWnd> ().SetAnima (false);
        }


        int target = mSyntheData.IndexOf(targetId);

        // 目标对象超出了scrollview的显示范围，做偏移
        if(target + 1> overPos)
            SpringPanel.Begin (mScrollView, new Vector3 (0f, 135f * (target + 1 - overPos), 0f), 8f);
        else
            SpringPanel.Begin (mScrollView, new Vector3 (0f, 0f, 0f), 8f);
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow(gameObject.name);
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
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void BindData(int _classId)
    {
        this.mClassId = _classId;

        this.targetId = PetsmithMgr.GetSyntheTarget (_classId);

        if (targetId <= 0)
        {
            LogMgr.Trace("查找不到classId为{0}的合成对象", _classId);
            return;
        }

        // 刷新合成目标标示
        RedrawTarget ();
    }

    #endregion
}
