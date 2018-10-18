/// <summary>
/// DragDropPetItem.cs
/// Created by fengsc 2017/02/27
/// 战斗选择界面可拖拽的格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragDropPetItem : UIDragDropItem
{
    SelectFighterWnd mSelectFighterWnd = null;

    PlayerPetWnd mPlayerPetWnd = null;

    FriendPetWnd mFriendPetWnd = null;

    // 当前脚本所挂物体的初始位置
    Vector3 mOriginalPos = Vector3.zero;

    // 格子绑定的物件对象
    Property mItemOb = null;

    UIPanel mPanel = null;

    GameObject mUIDragDropRoot;

    // 最大层级
    int mMaxDepth = 0;

    GameObject mItem = null;

    int mDepth = 0;

    CallBack mCallBack;

    GameObject mCurrentGo;

    UIScrollView mPlayerView;
    UIScrollView mFriendView;

    GameObject mName;

    string mPlayerName = string.Empty;

    protected override void Start()
    {
        mCurrentGo = this.gameObject;

        // 初始化当前格子的变量信息
        mOriginalPos = mCurrentGo.transform.localPosition;

        PetItemWnd script = mCurrentGo.GetComponent<PetItemWnd>();
        if (script == null)
            mItemOb = null;
        else
            mItemOb = script.item_ob;

        mPanel = mCurrentGo.GetComponent<UIPanel>();
        if (mPanel != null)
        {
            mMaxDepth = mPanel.depth + 5;

            // 临时变量，存储当前格子的panel
            mDepth = mPanel.depth;
        }

        GameObject wnd = WindowMgr.GetWindow(SelectFighterWnd.WndType);
        if (wnd != null)
        {
            mSelectFighterWnd = wnd.GetComponent<SelectFighterWnd>();
            mPlayerPetWnd = mSelectFighterWnd.mPlayerPetWnd;
            mFriendPetWnd = mSelectFighterWnd.mFriendPetWnd;

            mUIDragDropRoot = wnd.transform.Find("DragDropRoot").gameObject;

            mPlayerView = mPlayerPetWnd.ScrollView;
            mFriendView = mFriendPetWnd.ScrollView;
        }

        base.Start();
    }

    void OnDestroy()
    {
        mCurrentGo = null;

        // 结束协程
        Coroutine.StopCoroutine("InitPos");
    }

    protected override bool OnDragStart()
    {
        if (mFriendView.isDragging || mPlayerView.isDragging)
            return false;

        PetItemWnd script = mCurrentGo.GetComponent<PetItemWnd>();
        if (script == null)
            mItemOb = null;
        else
            mItemOb = script.item_ob;

        List<string> rids = mSelectFighterWnd.mSelectRidList;

        if (mItemOb == null)
            return false;

        if (mCurrentGo.CompareTag("UserGrid"))
        {
            // 已经选择的宠物不允许拖拽
            if (rids.Contains(mItemOb.GetRid()))
                return false;

            // 玩家自己宠物出战数量限制
            if(mSelectFighterWnd.amount == rids.Count)
                return false;

            if (! InstanceMgr.IsAllowSamePet(mSelectFighterWnd.mInstanceId) &&
                mSelectFighterWnd.mSelectClassIdList.Contains(mItemOb.GetClassID()))
            {
                // 通天之塔不能使用相同的使魔
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("SelectFighterWnd_40"));

                return false;
            }

            UIDragDropRoot.root = mUIDragDropRoot.transform;
        }
        else if (mCurrentGo.CompareTag("FriendGrid"))
        {
            // 玩家自己宠物出战数量限制
            if (mSelectFighterWnd.amount == rids.Count)
                return false;

            if (mFriendPetWnd.mFightAmount >= mFriendPetWnd.mMaxFightAmount)
                return false;

            // 已经使用过的好友共享宠物不允许拖拽
            if (mFriendPetWnd.IsUserSharePet(mItemOb.GetRid()))
                return false;

            if (! InstanceMgr.IsAllowSamePet(mSelectFighterWnd.mInstanceId) &&
                mSelectFighterWnd.mSelectClassIdList.Contains(mItemOb.GetClassID()))
            {
                // 通天之塔不能使用相同的使魔
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("SelectFighterWnd_40"));

                return false;
            }

            mName = mCurrentGo.transform.Find("name").gameObject;
            if (mName != null)
                mName.SetActive(false);

            mPlayerName = mName.GetComponent <UILabel>().text;
        }
        else
        {
        }

        if (mPanel != null)
        {
            mDepth = mPanel.depth;

            // 将拖拽的panel的层级放在最上面
            mPanel.depth = mMaxDepth;
        }

        // 执行回调
        if (mCallBack != null)
            mCallBack.Go(true);

        Transform parent = mCurrentGo.transform.parent;
        if (base.OnDragStart())
        {
            mItem = Instantiate(mCurrentGo);
            mItem.transform.SetParent(parent);
            mItem.transform.localScale = mCurrentGo.transform.localScale;
            mItem.transform.localPosition = mOriginalPos;
            mItem.GetComponent<PetItemWnd>().SetBind(null);

            Transform trans = mItem.transform.Find("name");
            if (trans != null)
            {
                trans.gameObject.SetActive(true);
                trans.GetComponent<UILabel>().text = mPlayerName;
            }

            return true;
        }
        else
        {
            if (mName != null)
                mName.SetActive(true);

            return false;
        }
    }

    protected override void OnDragEnd()
    {
        // 销毁临时的格子对象
        GameObject.DestroyImmediate(mItem);

        mItem = null;

        mPlayerName = string.Empty;

        if (mPanel != null)
        {
            // 还原panel的层级
            mPanel.depth = mDepth;
        }

        // 开启协程
        Coroutine.DispatchService(InitPos(), "InitPos");

        // 执行回调
        if (mCallBack != null)
            mCallBack.Go(false);


        base.OnDragEnd();

        if (mCurrentGo.CompareTag("UserGrid"))
        {
            if (mDragScrollView != null)
                mDragScrollView.scrollView = mPlayerView;
        }
        if (mCurrentGo.CompareTag("FriendGrid"))
        {
            if (mDragScrollView != null)
                mDragScrollView.scrollView = mFriendView;

            if (mName != null)
                mName.SetActive(true);
        }

        // 刷新技能描述
        mSelectFighterWnd.RefreshLeaderDesc();
    }

    IEnumerator InitPos()
    {
        // 等待一帧还原位置
        yield return null;

        if (mCurrentGo == null)
            yield break;

        // 拖拽到其他地方,还原格子的位置
        mCurrentGo.transform.localPosition = mOriginalPos;

        if (!mCurrentGo.activeSelf)
            mCurrentGo.SetActive(true);
    }

    protected override void OnDragDropRelease (GameObject surface)
    {
        if (surface != null)
        {
            if (mPanel == null)
                return;

            if (surface.CompareTag("FormationGrid") || surface.CompareTag("NullGrid"))
            {
                PetItemWnd script = surface.GetComponent<PetItemWnd>();
                if (script == null)
                    return;

                Property surfaceOb = script.item_ob;

                if (mCurrentGo.CompareTag("UserGrid") || mCurrentGo.CompareTag("FriendGrid"))
                {
                    if (mItemOb == null)
                        return;

                    // 绑定数据
                    script.SetBind(mItemOb);
                    if (!mSelectFighterWnd.mSelectRidList.Contains(mItemOb.GetRid()))
                    {
                        mSelectFighterWnd.mSelectRidList.Add(mItemOb.GetRid());

                        mSelectFighterWnd.mSelectClassIdList.Add(mItemOb.GetClassID());
                    }

                    if (surfaceOb != null)
                    {
                        mSelectFighterWnd.mSelectRidList.Remove(surfaceOb.GetRid());

                        mSelectFighterWnd.mSelectClassIdList.Remove(surfaceOb.GetClassID());
                    }

                    // 刷新数据
                    mFriendPetWnd.RefreshData();
                    mPlayerPetWnd.RefreshData();
                }
                else
                {
                    // 交换两个格子数据
                    mCurrentGo.GetComponent<PetItemWnd>().SetBind(surfaceOb);
                    script.SetBind(mItemOb);
                }

                if (mCurrentGo.CompareTag("FriendGrid"))
                    mFriendPetWnd.mFightAmount++;

                mItemOb = mCurrentGo.GetComponent<PetItemWnd>().item_ob;

            }
            else if (surface.CompareTag("UserGrid") && mCurrentGo.CompareTag("FormationGrid"))
            {
                if (mItemOb == null)
                    return;

                // 刷新界面宠物数据
                if (mPlayerPetWnd.mPets.Contains(mItemOb.GetRid()))
                    RefreshPetListData();
            }
            else if (surface.CompareTag("FriendGrid") && mCurrentGo.CompareTag("FormationGrid"))
            {
                if (mItemOb == null)
                    return;

                // 刷新界面宠物数据
                if (mFriendPetWnd.mPets.Contains(mItemOb.GetRid()))
                {
                    RefreshPetListData();
                    mFriendPetWnd.mFightAmount--;
                }
            }

            // 设置宠物格子标签
            for (int i = 0; i < mSelectFighterWnd.mPlayerSelectList.Count; i++)
                mSelectFighterWnd.SetFormationPetItemTag(mSelectFighterWnd.mPlayerSelectList[i].gameObject);

            // 更新阴影效果
            if (mSelectFighterWnd != null)
                mSelectFighterWnd.UpdateShadow();

            base.OnDragDropRelease(surface);
        }
    }

    /// <summary>
    /// 刷新宠物列表数据
    /// </summary>
    private void RefreshPetListData()
    {
        mSelectFighterWnd.mSelectRidList.Remove(mItemOb.GetRid());

        mSelectFighterWnd.mSelectClassIdList.Remove(mItemOb.GetClassID());

        // 绑定数据
        mCurrentGo.GetComponent<PetItemWnd>().SetBind(null);

        mFriendPetWnd.RefreshData();
        mPlayerPetWnd.RefreshData();
    }

    /// <summary>
    /// 设置回调
    /// </summary>
    public void SetCallBack(CallBack callBack)
    {
        mCallBack = callBack;
    }
}
