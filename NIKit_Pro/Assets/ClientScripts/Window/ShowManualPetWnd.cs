/// <summary>
/// ShowManualPetWnd.cs
/// Created by fengsc 2017/12/28
/// 显示图鉴使魔
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ShowManualPetWnd : WindowBase<ShowManualPetWnd>
{
    #region 成员变量

    public UIScrollView ScrollView;
    public UIWrapContent petcontent;

    public GameObject mManualPetItem;

    public ManualAttribWnd mManualAttribWnd;

    public UIScrollView mUIScrollView;

    // name与OB映射
    public Dictionary<string, GameObject> mPosObMap = new Dictionary<string, GameObject>();

    #endregion

    #region 私有变量

    // 每5个item为一行
    const int mColumnNum = 5;

    // 此处是指预先创建多少行
    const int mRowNum = 5;

    private List<LPCMapping> petData = new List<LPCMapping>();

    // 当前显示数据的index与实际数据的对应关系
    private Dictionary<int, int> indexMap = new Dictionary<int, int>();

    private Vector3 mPanelPos = Vector3.zero;

    string mSelectRid = string.Empty;

    int mCurRows = 1;

    #endregion

    #region 内部函数

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        petcontent.onInitializeItem = OnUpdateItem;

        MsgMgr.RegisterDoneHook("MSG_RECEIVE_MANUAL_BONUS", "ShowManualPetWnd", OnMsgReceiveManulaBonus);
    }

    void OnMsgReceiveManulaBonus(string cmd, LPCValue para)
    {
        // 填充数据
        foreach (KeyValuePair<int, int> kv in indexMap)
            FillPetData(kv.Key, kv.Value);

        if (para == null || !para.IsMapping)
            return;

        LPCMapping data = para.AsMapping;

        // 定位领取的行数
        for (int i = 0; i < petData.Count; i++)
        {
            LPCMapping pet = petData[i];
            if (pet == null)
                continue;

            if (pet.GetValue<int>("class_id") != data.GetValue<int>("class_id"))
                continue;

            if (pet.GetValue<int>("rank") != data.GetValue<int>("rank"))
                continue;

            // 计算当前行数
            int rows = (i + 1) / mRowNum;
            if ((i + 1) % mRowNum != 0)
                rows++;

            // 更新当前行数
            if (mCurRows < rows)
                mCurRows = rows;

            // 跳出循环
            break;
        }

        // 移动panel的位置
        MovePanelPos();
    }

    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void Awake()
    {
        // 注册事件
        RegisterEvent();

        // 创建格子
        CreatePos();

        mPanelPos = mUIScrollView.transform.localPosition;
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 移除消息关注
        MsgMgr.RemoveDoneHook("MSG_RECEIVE_MANUAL_BONUS", "ShowManualPetWnd");
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
            rowItemOb.name = string.Format("item_{0}", i);
            rowItemOb.transform.parent = petcontent.transform;
            rowItemOb.transform.localPosition = new Vector3(0, - i * 140, 0);
            rowItemOb.transform.localScale = Vector3.one;

            mManualPetItem.SetActive(false);

            for(int j = 0; j < mColumnNum;j++)
            {
                GameObject posWnd = Instantiate (mManualPetItem) as GameObject;
                posWnd.transform.parent = rowItemOb.transform;
                posWnd.name = string.Format("manual_pet_item_{0}_{1}", i, j);
                posWnd.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                posWnd.transform.localPosition = new Vector3(114 * j, 0f, 0f);

                mPosObMap.Add (string.Format("manual_pet_item_{0}_{1}", i, j), posWnd);

                posWnd.SetActive(true);

                // 注册点击事件
                UIEventListener.Get(posWnd).onClick = OnBaggageItemClicked;
            }
        }
    }

    /// <summary>
    /// 刷新宠物数据.
    /// </summary>
    private void InitPetData()
    {
        int Row = mRowNum;

        int containerSize = petData.Count;

        int containerRow = containerSize % mColumnNum == 0 ?
            containerSize / mColumnNum : containerSize / mColumnNum + 1;

        // 多显示一行用来显示添加格子按钮
        if (containerRow > mRowNum)
            Row = containerRow;

        // 从0开始
        petcontent.maxIndex = 0;

        petcontent.minIndex = -(Row - 1);
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        // 刷新宠物数据数据
        InitPetData();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in indexMap)
            FillPetData(kv.Key, kv.Value);

        // 移动panel的位置
        MovePanelPos();
    }

    /// <summary>
    /// 移动Panel位置
    /// </summary>
    void MovePanelPos()
    {
        // 新使魔
        int index = -1;

        for (int i = 0; i < petData.Count; i++)
        {
            LPCMapping data = petData[i];

            if (data == null)
                continue;

            if (ManualMgr.IsNewManual(ME.user, data.GetValue<int>("rank"), data.GetValue<int>("class_id")))
            {
                index = i + 1;
                break;
            }
        }

        // 没有新图鉴
        if (index == -1)
            return;

        // 计算行数
        int row = index / mRowNum;

        // 修正行数
        if (index % mRowNum != 0)
            row++;

         // 玩家主动操作滑动至最后面领取奖励时，需要滑动至最上面的奖励处
        float dis = (row - 1) * petcontent.itemSize + mPanelPos.y - mUIScrollView.transform.localPosition.y;

        // 计算总行数
        int sumRows = petData.Count / mRowNum;
        if (petData.Count % mRowNum != 0)
            sumRows++;

        // 处理临界情况, petcontent.itemSize / 2 是因为一页中的最后一行只能显示一半
        dis = Mathf.Min((sumRows - mColumnNum) * petcontent.itemSize + petcontent.itemSize / 2, dis);

        // 滑动panel
        SpringPanel.Begin(
            mUIScrollView.panel.cachedGameObject,
            new Vector3(
                mUIScrollView.panel.transform.localPosition.x,
                mUIScrollView.panel.transform.localPosition.y + dis,
                mUIScrollView.panel.transform.localPosition.z),
            10f
        );

        // 缓存行数
        mCurRows = row;
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

        FillPetData(index, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="realIndex">Real index.</param>
    private void FillPetData(int index, int realIndex)
    {
        for (int i = 0; i < mColumnNum; i++)
        {
            GameObject petWnd = mPosObMap [string.Format ("manual_pet_item_{0}_{1}", index, i)];

            if (petWnd == null)
                continue;

            ManualPetItemWnd item = petWnd.GetComponent<ManualPetItemWnd>();

            int dataIndex = System.Math.Abs(realIndex) * mColumnNum + i;

            if (dataIndex < petData.Count)
            {
                petWnd.SetActive(true);

                item.Bind(petData[dataIndex]);

                if (string.IsNullOrEmpty(mSelectRid))
                {
                    // 最开始默认选中第一个
                    if (dataIndex == 0)
                    {
                        ItemSelected(petWnd);
                    }
                }
                else
                {
                    if (petData[dataIndex].GetValue<string>("rid").Equals(mSelectRid))
                        item.Select(true);
                    else
                        item.Select(false);
                }
            }
            else
            {
                petWnd.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 设置包裹格子选中
    /// </summary>
    private void ItemSelected(GameObject ob)
    {
        // 当前包裹无宠物
        if (petData.Count == 0 && ob == null)
            return;

        // ManualPetItemWnd组件不存在
        ManualPetItemWnd item = ob.GetComponent<ManualPetItemWnd>();
        if (item == null)
            return;

        // 不能重复选择
        if (string.Equals(mSelectRid, item.mPetData.GetValue<string>("rid")))
            return;

        foreach (Transform child in petcontent.transform)
        {
            foreach (Transform petWnd in child)
            {
                if (petWnd == null)
                    continue;

                petWnd.GetComponent<ManualPetItemWnd>().Select(false);
            }
        }

        // 设置item的选择状态
        item.Select(true);

        // 设置选择id
        mSelectRid = item.mPetData.GetValue<string>("rid");

        // 刷新使魔属性面板
        mManualAttribWnd.Bind(item.mPetData);
    }


    /// <summary>
    /// 包裹格子被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnBaggageItemClicked(GameObject go)
    {
        // 取得gameobject上绑定的item
        ManualPetItemWnd item = go.GetComponent<ManualPetItemWnd>();

        if (item == null)
            return;

        if (item.mPetData == null)
            return;

        LPCMapping data = item.mPetData;

        // 选择物件
        ItemSelected(go);

        int classId = data.GetValue<int>("class_id");

        int rank = data.GetValue<int>("rank");

        if (!item.mIsBonus)
            return;

        if (! ManualMgr.IsNewManual(ME.user, rank, classId))
            return;

        CsvRow row = MonsterMgr.GetRow(classId);
        if (row == null)
            return;

        LPCMapping manual_bonus = row.Query<LPCMapping>("manual_bonus");

        LPCMapping bonus = manual_bonus.GetValue<LPCMapping>(rank);

        string fields = FieldsMgr.GetFieldInMapping(bonus);

        DialogMgr.ShowSingleBtnDailog(new CallBack(OnCallBack, data),
            string.Format(LocalizationMgr.Get("PetManualWnd_26"), bonus.GetValue<int>(fields), FieldsMgr.GetFieldIcon(fields)),
            LocalizationMgr.Get("PetManualWnd_25")
        );
    }

    void OnCallBack(object para, params object[] param)
    {
        LPCMapping data = para as LPCMapping;
        if (data == null || data.Count == 0)
            return;

        // 领取奖励
        Operation.CmdReceiveManualBonus.Go(data.GetValue<int>("class_id"), data.GetValue<int>("rank"));
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int element, List<LPCMapping> petList)
    {
        petData = petList;

        mSelectRid = string.Empty;

        mCurRows = 1;

        Redraw();
    }

    public void ResetPosition()
    {
        petcontent.SortAlphabetically();

        mUIScrollView.ResetPosition();
    }

    #endregion
}
