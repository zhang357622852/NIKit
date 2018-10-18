/// <summary>
/// ChatGoodsItem.cs
/// Created by fengsc 2016/12/09
/// 聊天栏物品基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class ChatGoodsItem : WindowBase<ChatGoodsItem>
{
    public GameObject mEquipInfo;

    public GameObject mPetInfo;

    public UILabel mPetPrefix;

    public UILabel mPetDesc;

    public UILabel mEquipDesc;

    public UISprite mPetBg;

    public UISprite mEquipBg;

    public UISprite mPetStar;

    public UISprite mElement;

    public GameObject mItemInfo;

    public UITexture mItemIcon;

    public UILabel mItemName;

    public UISprite mItemBg;

    public GameObject mButton;

    public UISprite mButtonBg;

    public UILabel mButtonLb;

    public Vector2 GetBackGroundSize { get; private set;}

    [HideInInspector]
    public LPCMapping mData = new LPCMapping();

    // 物件对象
    Property mOb;

    void OnDestroy()
    {
        if (mOb != null)
            mOb.Destroy();
    }

    /// <summary>
    /// 重绘窗口
    /// </summary>
    void Redraw()
    {
        float width = 0;
        float height = 0;

        int spaceWidth = 0;

        if (mData.ContainsKey("class_id"))
        {
            int classId = mData.GetValue<int>("class_id");

            if (MonsterMgr.IsMonster(classId))
            {
                if (mOb != null)
                    mOb.Destroy();

                LPCMapping para = LPCMapping.Empty;
                if (mData.ContainsKey("is_basic") && mData.GetValue<int>("is_basic") == 1)
                {
                    // 构建参数
                    para.Add("class_id", classId);
                    para.Add("rid", Rid.New());

                    if (mData.ContainsKey("prefix_desc"))
                        para.Add("prefix_desc", mData.GetValue<string>("prefix_desc"));

                    // 创建宠物对象
                    mOb = PropertyMgr.CreateProperty(para);
                }
                else
                {
                    para = LPCValue.Duplicate(mData).AsMapping;

                    para.Add("rid", Rid.New());

                    mOb = PropertyMgr.CreateProperty(para);
                }

                // 根据宠物的是否觉醒获取对应颜色
                string color = PetMgr.GetAwakeColor(mOb.Query<int>("rank"));

                LPCValue v = mOb.Query<LPCValue>("prefix_desc");

                string prefixDesc = string.Empty;

                if (v != null)
                    prefixDesc = LocalizationMgr.GetServerDesc(v);

                // 显示前缀描述
                mPetPrefix.text = string.Format("{0}{1}[-]", color, prefixDesc);

                // 显示星级
                mPetStar.gameObject.SetActive(true);

                mPetStar.spriteName = PetMgr.GetStarName(mOb.Query<int>("rank"));

                // 显示宠物描述
                mPetDesc.text = string.Format("{0}×{1} {2}[-]", color, mOb.Query<int>("star"), MonsterMgr.GetName(mOb.Query<int>("class_id"), mOb.Query<int>("rank")));

                // 显示宠物属性
                mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(mOb.Query<int>("class_id")));

                mPetInfo.SetActive(true);

                spaceWidth = 45;

                NGUIText.dynamicFont = mPetDesc.trueTypeFont;

                if (string.IsNullOrEmpty(prefixDesc))
                    width = mPetStar.width + mElement.width + spaceWidth + NGUIText.CalculatePrintedSize(mPetDesc.text).x;
                else
                    width = mPetStar.width + mElement.width + spaceWidth + NGUIText.CalculatePrintedSize(mPetDesc.text).x + NGUIText.CalculatePrintedSize(prefixDesc).x;

                height = mPetBg.height;
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                mEquipInfo.SetActive(true);

                mEquipDesc.text = string.Format("[{0}]{1}[-]", ColorConfig.GetColor(mData.GetValue<int>("rarity")), EquipMgr.Short(mData));

                spaceWidth = 13 * 2;

                NGUIText.dynamicFont = mEquipDesc.trueTypeFont;

                width = spaceWidth + NGUIText.CalculatePrintedSize(mEquipDesc.text).x;

                height = mEquipBg.height;
            }
            else
            {
                mItemInfo.SetActive(true);

                string path = string.Format("Assets/Art/UI/Icon/item/{0}.png", ItemMgr.GetClearIcon(classId));

                mItemIcon.mainTexture = ResourceMgr.LoadTexture(path);

                mItemName.text = string.Format(" {0} × {1}", ItemMgr.GetName(classId), mData.GetValue<int>("amount"));

                spaceWidth = 34;

                NGUIText.dynamicFont = mItemName.trueTypeFont;

                width = spaceWidth + NGUIText.CalculatePrintedSize(mItemName.text).x + mItemIcon.localSize.x;
                height = mItemBg.height;
            }
        }
        else if (mData.ContainsKey("gang_name"))
        {
            mButton.SetActive(true);

            mButtonLb.text = mData.GetValue<string>("gang_name");

            spaceWidth = 13 * 2;

            NGUIText.dynamicFont = mButtonLb.trueTypeFont;

            width = spaceWidth + NGUIText.CalculatePrintedSize(mButtonLb.text).x;
            height = mButtonBg.height;

            Color color = new Color(173 / 255f, 255 / 255f, 152 / 255f);

            mButtonBg.color = color;

            mButtonLb.color = color;
        }

        GetBackGroundSize = new Vector2(width, height);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        if (mEquipInfo.activeSelf)
            mEquipInfo.SetActive(false);

        if (mPetInfo.activeSelf)
            mPetInfo.SetActive(false);

        if (mItemInfo.activeSelf)
            mItemInfo.SetActive(false);

        if (mButton.activeSelf)
            mButton.SetActive(false);

        if (data == null)
            return;

        mData = data;

        // 重绘窗口
        Redraw();
    }
}
