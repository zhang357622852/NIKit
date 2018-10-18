/// <summary>
/// PetChangeWnd.cs
/// Created by lic 2017/02/16
/// 选择宠物窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PetChangeWnd : WindowBase<PetChangeWnd>
{
    public Transform mPanel;

    public GameObject mCloseBtn;

    public UILabel mTipLb;

    public GameObject mSelectItem;

    #region 私有字段

    List<Property> data = new List<Property>();

    string mSelectRid = string.Empty;

    // 关闭窗口的回调
    CallBack task = null;

    // name与Ob的映射
    Dictionary<string, GameObject> mConfirmObMap = new Dictionary<string, GameObject> ();

    #endregion

    #region 内部函数

    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
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
        UIEventListener.Get(mCloseBtn).onClick += OnCloseBtnClick;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mTipLb.text = LocalizationMgr.Get("PetSelectWnd_1");

        mSelectItem.SetActive (false);
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    void Redraw()
    {
        foreach (Transform item in mPanel)
            item.gameObject.SetActive (false);

        GameObject wnd;

        for (int i = 0; i < data.Count; i++)
        {
            string name = string.Format ("item_{0}", i);

            if (mConfirmObMap.ContainsKey (name))
                wnd = mConfirmObMap [name];
            else
            {
                wnd = Instantiate (mSelectItem) as GameObject;
                wnd.transform.parent = mPanel;
                wnd.name = name;
                wnd.transform.localScale = Vector3.one;
                wnd.transform.localPosition = new Vector3 (0f, -i * 113f, 0f);

                mConfirmObMap.Add (name, wnd);
                wnd.GetComponent<PetSelectItemWnd> ().SetCallBack (new CallBack(OnChangeBtnClick, i));
            }
 
            wnd.SetActive (true);

            wnd.GetComponent<PetSelectItemWnd> ().BindData (data[i]);

            // 设置选中
            wnd.GetComponent<PetSelectItemWnd> ().SetSelect (data [i].GetRid ().Equals (mSelectRid));
        }
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtnClick(GameObject ob)
    {
        if (task != null) 
            task.Go (mSelectRid);

        WindowMgr.DestroyWindow (gameObject.name);
    }

    /// <summary>
    /// 按钮点击回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    void OnChangeBtnClick(object para, params object[] _params)
    {
        GameObject wnd = mConfirmObMap[string.Format("item_{0}", (int) para)];
        wnd.GetComponent<PetSelectItemWnd> ().SetSelect (true);

        Property item = wnd.GetComponent<PetSelectItemWnd> ().item_ob;

        if (string.IsNullOrEmpty (mSelectRid))
        {
            mSelectRid = item.GetRid ();
            return;
        }

        foreach (Transform tf in mPanel)
        {
            if (tf.GetComponent<PetSelectItemWnd> ().item_ob.GetRid ().Equals (mSelectRid))
            {
                tf.GetComponent<PetSelectItemWnd> ().SetSelect (false);
                break;
            }
        }

        mSelectRid = item.GetRid ();
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="pet_ob">Pet ob.</param>
    /// <param name="task">Task.</param>
    public void BindData(List<Property> _data, string _selectRid = "", CallBack _task = null)
    {
        if (_data == null || _data.Count == 0)
            return;

        if (string.IsNullOrEmpty (_selectRid))
            return;

        this.data = _data;
        this.mSelectRid = _selectRid;
        this.task = _task;

        Redraw ();
    }


    #endregion
}
