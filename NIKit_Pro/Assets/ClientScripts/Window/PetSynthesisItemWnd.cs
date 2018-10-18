using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;

public class PetSynthesisItemWnd : MonoBehaviour
{

    public GameObject mPetItem;
    public GameObject[] mMaterialGroup;

    #region 私有字段

    LPCMapping petData = null;

    // 预览界面绑定的id
    int  viewWndBindId = 0;

    // 是否显示可合成（如果要合成的宠物满足条件，其他能合成光效不显示）
    bool isShowCanSyn = true;

    // 宠物property
    Property petItem = null;

    // 材料property列表
    List<Property> materialList = new List<Property>();

    #endregion

    #region 内部函数

    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化显示
        InitWnd();
    }

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mPetItem).onClick = OnPetItemClick;

        // 注册按钮事件
        for (int i = 0; i < mMaterialGroup.Length; i++)
            UIEventListener.Get(mMaterialGroup[i]).onClick = OnMaterialItemClick;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        // 设置petitem显示
        mPetItem.GetComponent<PetItemWnd>().ShowLeaderSkill(false);
        mPetItem.GetComponent<PetItemWnd>().ShowMaxLevel(true);

        // 设置材料item显示
        for (int i = 0; i < mMaterialGroup.Length; i++)
        {
            mMaterialGroup [i].GetComponent<PetItemWnd> ().ShowLevel (false);
            mMaterialGroup [i].GetComponent<PetItemWnd> ().ShowLeaderSkill (false);
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        DestroyProperty ();
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        // 先析构掉已创建property
        DestroyProperty();

        // 先隐藏所有的图标
        mPetItem.SetActive(false);
        for (int i = 0; i < mMaterialGroup.Length; i++) 
            mMaterialGroup[i].SetActive (false);

        // 要合成的宠物数据为空
        if (petData == null || petData.Count == 0)
            return;

        petItem = CreateProperty (petData);

        // 不能生成宠物
        if (petItem == null)
            return;

        mPetItem.SetActive (true);
        mPetItem.GetComponent<PetItemWnd> ().SetBind (petItem);

        // 如果不拥有则将图标置灰
        mPetItem.GetComponent<PetItemWnd> ().ShowCover (! PetsmithMgr.IsOwnMonster (ME.user, petItem.GetClassID ()));

        // 若是浏览窗口，与宠物classid相同显示动画
        if (viewWndBindId > 0)
        {
            mPetItem.GetComponent<PetItemWnd> ().SetAnima (petItem.GetClassID () == viewWndBindId);
        }
        else
            mPetItem.GetComponent<PetItemWnd> ().SetAnima (isShowCanSyn && PetsmithMgr.CanDoSynthe(ME.user, petItem.GetClassID ()));

        CsvRow row = PetsmithMgr.SynthesisCsv.FindByKey (petData.GetValue<int>("class_id"));

        if (row == null)
            return;

        LPCArray materials = row.Query<LPCArray> ("material_cost");

        if (materials == null || materials.Count == 0)
            return;

        for (int i = 0; i < materials.Count; i++) 
        {
            LPCMapping condition = materials [i].AsMapping;

            Property item = CreateProperty (condition);

            if (item == null)
                continue;

            mMaterialGroup [i].SetActive (true);
            mMaterialGroup [i].GetComponent<PetItemWnd> ().SetBind (item);

            if (viewWndBindId > 0)
            {
                mMaterialGroup [i].GetComponent<PetItemWnd> ().SetAnima (item.GetClassID () == viewWndBindId);
            }

            // 如果不拥有则将图标置灰
            mMaterialGroup [i].GetComponent<PetItemWnd> ().ShowCover (! PetsmithMgr.IsOwnMonster (ME.user, item.GetClassID ()));

            materialList.Add (item);
        }

        // 根据数量设置材料的位置
        SetMaterialsPosition (materials.Count);
    }

    /// <summary>
    /// 宠物被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnPetItemClick(GameObject ob)
    {
        PetItemWnd petItemWnd = ob.GetComponent<PetItemWnd> ();

        if (petItemWnd == null)
            return;

        Property item = petItemWnd.item_ob;

        if (item == null)
            return;

        if (viewWndBindId > 0)
        {
            // 打开合成界面
            GameObject wnd = WindowMgr.OpenWnd("PetSimpleInfoWnd_ex", null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            wnd.GetComponent<PetSimpleInfoWnd>().Bind(item, true , true);
            wnd.GetComponent<PetSimpleInfoWnd>().ShowBtn(true);

            return;
        }

        // 显示合成界面
        if (! PetsmithMgr.IsSyntheTarget (item.GetClassID()))
        {
            GameObject InfoWnd = WindowMgr.OpenWnd(PetSynthesisInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (InfoWnd == null)
                return;

            InfoWnd.GetComponent<PetSynthesisInfoWnd>().BindData(item); 
        } 
        else 
        {
            // 显示宠物信息界面
            GameObject petInfoWnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (petInfoWnd == null)
                return;

            PetSimpleInfoWnd petInfo = petInfoWnd.GetComponent<PetSimpleInfoWnd>();

            petInfo.Bind(item);
            petInfo.ShowBtn(true);
           
        }
    }

    /// <summary>
    /// 材料被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnMaterialItemClick(GameObject ob)
    {
        PetItemWnd petItemWnd = ob.GetComponent<PetItemWnd> ();

        if (petItemWnd == null)
            return;

        Property item = petItemWnd.item_ob;

        if (item == null)
            return;

        if (viewWndBindId > 0)
        {
            // 打开合成界面
            GameObject wnd = WindowMgr.OpenWnd("PetSimpleInfoWnd_ex", null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            wnd.GetComponent<PetSimpleInfoWnd>().Bind(item, true , true);
            wnd.GetComponent<PetSimpleInfoWnd>().ShowBtn(true);

            return;
        }
            
        // 显示宠物信息界面
        GameObject petInfoWnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        PetSimpleInfoWnd petInfo = petInfoWnd.GetComponent<PetSimpleInfoWnd>();

        petInfo.Bind(item);
        petInfo.ShowBtn(true);
    }

    /// <summary>
    /// 析构掉property
    /// </summary>
    void DestroyProperty()
    {
        if (petItem != null)
        {
            petItem.Destroy ();
            petItem = null;
        }

        for (int i = 0; i < materialList.Count; i++)
        {
            if (materialList[i] == null)
                continue;

            materialList[i].Destroy ();
            materialList[i] = null;
        }
        
    }

    /// <summary>
    /// 根据属性创建property
    /// </summary>
    /// <param name="classID">Class I.</param>
    Property CreateProperty(LPCMapping condition)
    {
        LPCMapping dbase = new LPCMapping ();

        foreach (string field in condition.Keys)
            dbase.Add (field, condition [field]);

        dbase.Add ("rid", Rid.New ());

        return PropertyMgr.CreateProperty (dbase);
    }

    /// <summary>
    /// 设置材料的位置
    /// </summary>
    /// <param name="materialNum">Material number.</param>
    void SetMaterialsPosition(int materialNum)
    {
        if (materialNum <= 0)
            return;

        if (materialNum == 1)
        {
            mMaterialGroup [0].transform.localPosition = new Vector3 (2.8f, 87f, 0f);
        }
        if (materialNum == 2) 
        {
            mMaterialGroup [0].transform.localPosition = new Vector3 (2.8f, 87f, 0f);
            mMaterialGroup [1].transform.localPosition = new Vector3 (2.8f, -91.2f, 0f);
        } else if (materialNum == 3)
        {
            mMaterialGroup [0].transform.localPosition = new Vector3 (-31f, 87f, 0f);
            mMaterialGroup [1].transform.localPosition = new Vector3 (36.6f, 87f, 0f);
            mMaterialGroup [2].transform.localPosition = new Vector3 (2.8f, -91.2f, 0f);
        } else
        {
            mMaterialGroup [0].transform.localPosition = new Vector3 (-31f, 87f, 0f);
            mMaterialGroup [1].transform.localPosition = new Vector3 (36.6f, 87f, 0f);
            mMaterialGroup [2].transform.localPosition = new Vector3 (-31f, -91.2f, 0f);
            mMaterialGroup [3].transform.localPosition = new Vector3 (36.6f, -91.2f, 0f);
        }
         
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="classId">Class identifier.</param>
    public void BindData(LPCMapping _petData, int _viewWndBindId = 0, bool _isShowCanSyn = true)
    {
        this.petData = _petData;

        this.viewWndBindId = _viewWndBindId;

        this.isShowCanSyn = _isShowCanSyn;

        Redraw ();
    }

    #endregion
}
