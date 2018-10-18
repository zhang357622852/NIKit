/// <summary>
/// PetSynthesisConfirmWnd.cs
/// Created by lic 2017/02/16
/// 合成宠物确认窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class PetSynthesisConfirmWnd : WindowBase<PetSynthesisConfirmWnd>
{
    public Transform mItemPanel;

    public GameObject mContinueBtn;
    public UILabel mContinueBtnLb;
    public GameObject mCancelBtn;
    public UILabel mCancelBtnLb;

    public UILabel mTipLb;

    public GameObject mConfirmItem;

    #region 私有字段

    List<List<Property>> data = new List<List<Property>>();

    Dictionary<string, GameObject> mConfirmObMap = new Dictionary<string, GameObject> ();

    LPCArray selectList = new LPCArray();

    int rule = -1;

    #endregion

    #region 内部函数

    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        float scale = Game.CalcWndScale();
        transform.localScale = new Vector3(scale, scale, scale);
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
        UIEventListener.Get(mContinueBtn).onClick += OnContinueBtnClick;
        UIEventListener.Get(mCancelBtn).onClick += OnCancelBtnClick;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mTipLb.text = LocalizationMgr.Get("PetSynthesisConfirmWnd_1");
        mContinueBtnLb.text = LocalizationMgr.Get("PetSynthesisConfirmWnd_2");
        mCancelBtnLb.text = LocalizationMgr.Get("PetSynthesisConfirmWnd_3");

        mConfirmItem.SetActive (false);
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    void Redraw()
    {
        foreach (Transform item in mItemPanel)
            item.gameObject.SetActive (false);

        for (int i = 0; i < data.Count; i++)
        {
            GameObject wnd;
            string name = string.Format ("ConfirmWnd_Item_{0}", i);

            if (mConfirmObMap.ContainsKey (name))
                wnd = mConfirmObMap [name];
            else
            {
                wnd = Instantiate (mConfirmItem) as GameObject;
                wnd.transform.parent = mItemPanel;
                wnd.name = name;
                wnd.transform.localScale = Vector3.one;
                wnd.transform.localPosition = new Vector3 (0f, -i * 113f, 0f);

                mConfirmObMap.Add (name, wnd);
            }

            wnd.SetActive (true);

            Property item;

            if (data [i].Count > 1)
            {
                // 多个宠物满足条件需要进行筛选
                item = PET_SYNTHESIS_AUTO_SELECT_MATERIAL.Call(ME.user, data[i]);
                wnd.GetComponent<PetSelectItemWnd> ().SetChangeState (true);
            }
            else
            {
                item = data [i] [0];
                wnd.GetComponent<PetSelectItemWnd> ().SetChangeState (false);
            }

            selectList.Add (item.GetRid());

            wnd.GetComponent<PetSelectItemWnd> ().BindData (item);
            wnd.GetComponent<PetSelectItemWnd> ().SetCallBack (new CallBack(ChangeBtnCallBack, i));
        }
    }

    /// <summary>
    /// 继续按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnContinueBtnClick(GameObject ob)
    {
        // 执行工坊操作
        LPCMapping extraPara = new LPCMapping();
        extraPara.Add("material_rids", selectList);
        extraPara.Add ("rule", rule);

        PetsmithMgr.DoAction (ME.user, "petsynthesis", extraPara);

        WindowMgr.DestroyWindow (gameObject.name);
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCancelBtnClick(GameObject ob)
    {
        WindowMgr.DestroyWindow (gameObject.name);
    }

    /// <summary>
    /// 按钮点击回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    void ChangeBtnCallBack(object para, params object[] _params)
    {
        int index = (int)para;

        GameObject changeWnd = WindowMgr.OpenWnd ("PetChangeWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (changeWnd == null)
            return;

        changeWnd.GetComponent<PetChangeWnd> ().BindData (data[index], selectList[index].AsString, new CallBack(CloseChangeWndCallBack, index));
    }

    /// <summary>
    /// 关闭窗口回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    void CloseChangeWndCallBack(object para, params object[] _params)
    {
        int index = (int)para;
        string rid = (string)_params [0];

        // 选择没有变化
        if (selectList [index].AsString.Equals (rid))
            return;

        selectList [index] = LPCValue.Create(rid);

        Property item = Rid.FindObjectByRid (rid);

        GameObject wnd = mConfirmObMap [string.Format ("ConfirmWnd_Item_{0}", index)];

        wnd.GetComponent<PetSelectItemWnd> ().BindData (item);

    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="pet_ob">Pet ob.</param>
    /// <param name="task">Task.</param>
    public void BindData(int rule, List<List<Property>> _data)
    {
        if (_data == null || _data.Count == 0)
            return;

        // 需要保证数据的正确性
        for (int i = 0; i < _data.Count; i++)
        {
            if (_data [i].Count == 0)
                return;
        }

        if (PetsmithMgr.SynthesisCsv.FindByKey (rule) == null)
            return;

        this.data = _data;
        this.selectList = new LPCArray();
        this.rule = rule;
 
        Redraw ();
    }
        

    #endregion
}
