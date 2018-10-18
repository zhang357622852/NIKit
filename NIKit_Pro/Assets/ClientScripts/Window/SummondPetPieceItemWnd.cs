/// <summary>
/// SummondPetPieceItemWnd.cs
/// Created by zhangwm 2018/07/02
/// 召唤祭坛-使魔碎片召唤界面-itemPrefab
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class SummondPetPieceItemWnd : WindowBase<SummondPetPieceItemWnd>
{
    #region 成员变量
    public PetItemWnd mPetItemWnd;

    public GameObject mItemWnd;
    //背景
    public UISprite mBgSp;
    //图片icon
    public UITexture mIconTex;
    //数目lab
    public UILabel mCountLab;

    private Property mItemInfo;
    #endregion

    private void Start()
    {
        // 注册事件
        RegisterEvent();

        mPetItemWnd.gameObject.SetActive(false);
        mItemWnd.SetActive(false);
    }

    private void OnDestroy()
    {
        if (mItemInfo != null)
            mItemInfo.Destroy();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(gameObject).onClick = OnClickItem;
    }

    public void BindData(LPCMapping mapData)
    {
        if (mapData == null)
            return;

        if (mItemInfo != null)
            mItemInfo.Destroy();

        if (mapData.ContainsKey("class_id")) //道具-万能召唤
        {
            int classId = mapData.GetValue<int>("class_id");

            // 构造参数
            LPCMapping para = LPCMapping.Empty;
            para.Add("class_id", classId);
            para.Add("amount", mapData.GetValue<int>("amount"));
            para.Add("rid", Rid.New());

            // 创建物件
            mItemInfo = PropertyMgr.CreateProperty(para);

            mPetItemWnd.gameObject.SetActive(false);
            mItemWnd.SetActive(true);

            string iconName = ItemMgr.GetClearIcon(classId);
            string path = string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName);
            Texture2D res = ResourceMgr.LoadTexture(path);
            if (res != null)
                mIconTex.mainTexture = res;

            mCountLab.text = mapData.GetValue<int>("amount").ToString();
        }
        else if (mapData.ContainsKey("pet_id")) //宠物
        {
            // 获取召唤碎片关联的宠物id
            int petId = mapData.GetValue<int>("pet_id");
            // 获取宠物的配置表信息
            CsvRow row = MonsterMgr.GetRow(petId);

            if (row == null)
                return;

            LPCMapping para = new LPCMapping();
            para.Add("class_id", petId);
            para.Add("rid", Rid.New());
            para.Add("rank", row.Query<int>("rank"));
            para.Add("star", row.Query<int>("star"));

            // 创建物件
            mItemInfo = PropertyMgr.CreateProperty(para);

            // 创建失败
            if (mItemInfo == null)
                return;

            mPetItemWnd.gameObject.SetActive(true);
            mItemWnd.SetActive(false);

            mPetItemWnd.ShowLeaderSkill(false);
            mPetItemWnd.ShowLevel(false);
            mPetItemWnd.SetBind(mItemInfo);

            UILabel pieceAmount = mPetItemWnd.transform.Find("Amount").GetComponent<UILabel>();
            // 拥有碎片的数量 格式: 2/40
            pieceAmount.text = string.Format("{0}/{1}", mapData.GetValue<int>("amount"), row.Query<int>("piece_amount"));
        }
    }

    /// <summary>
    /// 点击事件
    /// </summary>
    /// <param name="go"></param>
    void OnClickItem(GameObject go)
    {
        if (mItemInfo == null)
            return;

        if (MonsterMgr.IsMonster(mItemInfo))
        {
            // 显示宠物悬浮窗口
            GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

            script.Bind(mItemInfo);
            script.ShowBtn(true, false, false);
        }
        else
        {
            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

            script.SetPropData(mItemInfo, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

            script.SetMask(true);
        }
    }
}
