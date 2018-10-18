/// <summary>
/// SummonPetPieceWnd.cs
/// Created by fengsc 2016/11/01
/// 宠物召唤碎片显示窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SummonPetPieceWnd : WindowBase<SummonPetPieceWnd>
{
    // 窗口标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 宠物基础格子
    public GameObject mItemWnd;

    // 格子复用组件
    public UIWrapContent mWrapContent;

    // 碎片列表
    List<LPCMapping> mPieceList = new List<LPCMapping>();

    int mColumnAmount = 5;

    int mRowAmount = 6;

    public Vector2 mInitPos = new Vector2(0, 0);

    /// <summary>
    /// 每个格子的间隔
    /// </summary>
    public Vector2 mItemSpace = new Vector2(30, 30);

    /// <summary>
    /// 每个格子的大小;
    /// </summary>
    public Vector2 mItemSize = new Vector2(110, 110);

    Dictionary<string, GameObject> mPetItems = new Dictionary<string, GameObject>();

    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 初始化宠物格子
        InitGrid();

        // 绘制窗口
        Redraw();
    }

    void OnDestroy()
    {
        // 启用协程
        Coroutine.DispatchService(RemoveDoneHook());
    }

    IEnumerator RemoveDoneHook()
    {
        // 等待一帧
        yield return null;

        // 取消消息关注
        MsgMgr.RemoveDoneHook("MSG_SELL_ITEM", "SummonPetPieceWnd_OnMsgSell");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        // 格子复用
        mWrapContent.onInitializeItem = UpdateItem;

        // 关注道具出售下发的消息
        MsgMgr.RegisterDoneHook("MSG_SELL_ITEM", "SummonPetPieceWnd_OnMsgSell", OnSellMsg);
    }

    /// <summary>
    /// 消息回调
    /// </summary>
    void OnSellMsg(string cmd, LPCValue para)
    {
        Redraw();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in mIndexMap)
            FillData(kv.Key, kv.Value);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mItemWnd.SetActive(false);

        mTitle.text = LocalizationMgr.Get("SummonPetPieceWnd_1");

        // 宠物碎片
        LPCValue v = ME.user.Query<LPCValue>("chip_pet");
        LPCMapping pieceData = LPCMapping.Empty;

        if (v != null && v.IsMapping)
            pieceData = v.AsMapping;

        mPieceList.Clear();

        //万能召唤卷 放在第一行第一个
        if (ME.user != null)
        {
            int count = ME.user.Query<int>("chip_all");
            if (count > 0)
            {
                LPCMapping para = LPCMapping.Empty;
                para.Add("amount", count);
                para.Add("class_id", FieldsMgr.GetClassIdByAttrib("chip_all"));

                mPieceList.Add(para);
            }
        }

        //宠物碎片
        foreach (int classId in pieceData.Keys)
        {
            int amount = pieceData.GetValue<int>(classId);

            LPCMapping para = LPCMapping.Empty;
            para.Add("amount", amount);
            para.Add("pet_id", classId);

            mPieceList.Add(para);
        }

        int containerSize = mPieceList.Count;

        mWrapContent.maxIndex = 0;

        //取得需要显示数据的总行数;
        int rowNum = (containerSize % mRowAmount == 0) ? containerSize / mRowAmount : (containerSize / mRowAmount + 1);

        if (rowNum > mColumnAmount && mPieceList.Count > 0)
            mWrapContent.minIndex = -rowNum;
        else
            mWrapContent.minIndex = -mColumnAmount;

        //激活UIWrapContent脚本 复用宠物格子;
        mWrapContent.enabled = true;
    }

    /// <summary>
    /// 初始化固定数量的宠物格子
    /// </summary>
    void InitGrid()
    {
        for (int i = 0; i < mColumnAmount; i++)
        {
            string name = "item_" + i.ToString();
            GameObject rowItemOb;

            if (mWrapContent.transform.Find(name) != null)
                rowItemOb = mWrapContent.transform.Find(name).gameObject;
            else
            {
                //创建空物体;
                rowItemOb = new GameObject();

                rowItemOb.name = name;

                rowItemOb.transform.parent = mWrapContent.transform;

                rowItemOb.transform.localPosition = new Vector3(mInitPos.x, mInitPos.y - i * (mItemSize.y + mItemSpace.y), 0);

                rowItemOb.transform.localScale = Vector3.one;
            }

            for (int j = 0; j < mRowAmount; j++)
            {
                string posName;
                GameObject posWnd;

                posName = string.Format("pet_item_{0}_{1}", i, j);

                // 窗口包裹格
                mPetItems.TryGetValue(posName, out posWnd);

                if (posWnd == null)
                {
                    float x, y;

                    //创建一个宠物格子对象;
                    posWnd = Instantiate(mItemWnd);

                    posWnd.transform.SetParent(rowItemOb.transform);

                    posWnd.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

                    posWnd.name = posName;

                    // 缓存格子对象
                    mPetItems.Add(posName, posWnd);

                    x = (mItemSize.x + mItemSpace.x) * j;

                    y = 0f;

                    posWnd.transform.localPosition = new Vector3(x, y, 0);
                }

                posWnd.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 复用格子更新数据
    /// </summary>
    void UpdateItem(GameObject go, int index, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (!mIndexMap.ContainsKey(index))
            mIndexMap.Add(index, realIndex);
        else
            mIndexMap[index] = realIndex;

        // 填充数据
        FillData(index, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    void FillData(int index, int realIndex)
    {
        for (int i = 0; i < mRowAmount; i++)
        {
            GameObject itemWnd = mPetItems[string.Format("pet_item_{0}_{1}", index, i)];

            SummondPetPieceItemWnd itemCtrl = itemWnd.GetComponent<SummondPetPieceItemWnd>();

            // 计算索引，通过索引拿到对应的宠物数据;
            int dataIndex = System.Math.Abs(realIndex) * mRowAmount + i;

            if (dataIndex >= mPieceList.Count)
            {
                itemWnd.SetActive(false);
                continue;
            }

            itemWnd.SetActive(true);
            // 碎片道具对象
            LPCMapping pieceData = mPieceList[dataIndex];

            if (pieceData == null)
                continue;

            itemCtrl.BindData(pieceData);
        }
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 销毁窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
