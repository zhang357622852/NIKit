/// <summary>
/// InstanceItemWnd.cs
/// Created by fengsc 2016/07/16
///副本列表格子   
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LPC;

public class InstanceItemWnd : WindowBase<InstanceItemWnd>
{
    /// <summary>
    ///副本名字
    /// </summary>
    public UILabel mInstanceName;

    /// <summary>
    ///副本开销
    /// </summary>
    public UILabel mInstanceSpending;

    public UISprite mCostIcon;

    /// <summary>
    ///列表格子背景
    /// </summary>
    public UISprite mBG;

    public GameObject mPower;

    public UILabel mFightLabel;

    public GameObject mItem;

    public GameObject mFightBtn;

    public GameObject mNormalLock;

    public UIGrid mGrid;

    public GameObject mLevelLimitTips;

    public UILabel mLevelTips;

    public UILabel mTips;

    public UILabel mMonsterInfo;

    // 最大等级宠物头像
    public UITexture mMaxLevelMonster;

    /// <summary>
    /// 副本ID
    /// </summary>
    private string mInstanceId = string.Empty;

    // 默认创建的个数
    int mItemAmount = 6;

    int mLockAmount = 0;

    Dictionary<string, GameObject> mItemList = new Dictionary<string, GameObject>();

    void Awake()
    {
        // 创建一批item
        CreatedGameObject(0, mItemAmount);
    }

    void CreatedGameObject(int starIndex, int endIndex)
    {
        for (int i = starIndex; i < endIndex; i++)
        {
            mItem.SetActive(false);
            GameObject clone = Instantiate(mItem);

            clone.transform.SetParent(mGrid.transform);

            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = Vector3.zero;

            clone.name = "pet" + i;

            clone.SetActive(false);

            mItemList.Add(clone.name, clone);
        }
        mGrid.Reposition();
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    void Redraw()
    {
        mItem.SetActive(false);

        if (string.IsNullOrEmpty(mInstanceId))
            return;

        // 根据副本ID获取副本配置信息;
        LPCMapping data = InstanceMgr.GetInstanceInfo(mInstanceId);

        // 没有数据;
        if (data == null || data.Count <= 0)
            return;

        // 注册按钮点击事件;
        UIEventListener.Get(mFightBtn).onClick = OnClickFightBtn;

        // 获取副本名字;
        mInstanceName.text = LocalizationMgr.Get(data.GetValue<string>("name"));

        // 根据副本ID获取副本资源数据;
        List<CsvRow> rowList = InstanceMgr.GetInstanceFormation(mInstanceId);

        if (mItemList.Count < mItemAmount)
        {
            CreatedGameObject(mItemList.Count, mItemAmount);
        }

        if (rowList.Count > mItemAmount)
        {
            CreatedGameObject(mItemList.Count, rowList.Count);
        }

        if (rowList != null && rowList.Count > 0)
        {
            // 遍历该副本的副本资源数据;
            for (int i = 0; i < rowList.Count; i++)
            {
                GameObject clone = mItemList["pet" + i];

                clone.SetActive(true);

                if(rowList[i] == null)
                    continue;

                // 调用脚本参数计算怪物class_id;
                int classIdScript = rowList[i].Query<int>("class_id_script");
                int classId = (int) ScriptMgr.Call(classIdScript, ME.user.GetLevel(),
                    rowList[i].Query<LPCValue>("class_id_args"));

                // 获取始化参数;
                int initScript = rowList[i].Query<int>("init_script");
                LPCMapping initArgs = ScriptMgr.Call(initScript, ME.user.GetLevel(),
                    rowList[i].Query<LPCValue>("init_script_args"), LPCMapping.Empty) as LPCMapping;

                if (! mItemList.ContainsKey("pet" + i))
                    continue;

                GameObject iconGo = clone.transform.Find("icon").gameObject;

                if (iconGo == null)
                    return;

                UITexture icon = iconGo.GetComponent<UITexture>();

                // 设置图片
                icon.mainTexture = MonsterMgr.GetTexture(classId, initArgs.GetValue<int>("rank"));
            }
        }

        float rgb = 120 / 255f;

        // 等级解锁条件不满足
        if (! InstanceMgr.IsUnLockLevel(ME.user, mInstanceId))
        {
            mFightLabel.gameObject.SetActive(false);
            mPower.SetActive(false);
            mInstanceSpending.gameObject.SetActive(false);
            mBG.color = new Color(rgb, rgb, rgb);
            mNormalLock.SetActive(false);

            mLevelLimitTips.SetActive(true);

            // 第一个未解锁的副本显示宠物信息
            if (mLockAmount != 1)
            {
                mLevelTips.gameObject.SetActive(false);

                mTips.gameObject.SetActive(false);

                mMonsterInfo.gameObject.SetActive(false);

                mMaxLevelMonster.gameObject.SetActive(false);
            }
            else
            {
                mLevelTips.gameObject.SetActive(true);

                mTips.gameObject.SetActive(true);

                mMonsterInfo.gameObject.SetActive(true);

                mMaxLevelMonster.gameObject.SetActive(true);
            }

            mLevelTips.text = string.Format(LocalizationMgr.Get("SelectInstanceWnd_7"), data.GetValue<int>("unlock_level"));

            mTips.text = LocalizationMgr.Get("SelectInstanceWnd_9");

            // 包裹中最大等级的宠物对象
            Property maxLevelOb = PetMgr.GetMaxLevelPet(ME.user);
            if (maxLevelOb == null)
                return;

            string star = string.Empty;
            for (int i = 0; i < maxLevelOb.Query<int>("star"); i++)
                star += LocalizationMgr.Get("SelectInstanceWnd_6");

            int maxClassId = maxLevelOb.Query<int>("class_id");

            int maxRank = maxLevelOb.Query<int>("rank");

            // 显示宠物星级、名称、等级
            mMonsterInfo.text = string.Format(LocalizationMgr.Get("SelectInstanceWnd_8"), star, MonsterMgr.GetName(maxClassId, maxRank), maxLevelOb.Query<int>("level"));

            // 显示宠物头像
            mMaxLevelMonster.mainTexture = MonsterMgr.GetTexture(maxClassId, maxRank);

            return;
        }

        // 等级解锁隐藏等级限制提示
        mLevelLimitTips.SetActive(false);

        // 判断副本是否解锁;
        if (!InstanceMgr.IsUnlocked(ME.user, mInstanceId))
        {
            mFightLabel.gameObject.SetActive(false);
            mPower.SetActive(false);
            mInstanceSpending.gameObject.SetActive(false);
            mBG.color = new Color(rgb, rgb, rgb);
            mNormalLock.SetActive(true);

            return;
        }

        mNormalLock.SetActive(false);

        rgb = 255 / 255f;
        mInstanceSpending.gameObject.SetActive(true);
        mFightLabel.gameObject.SetActive(true);
        mPower.SetActive(true);

        mBG.color = new Color(rgb, rgb, rgb);

        mFightLabel.text = LocalizationMgr.Get("SelectInstanceWnd_1");

        LPCMapping costData = InstanceMgr.GetInstanceCostMap(ME.user, mInstanceId);

        string fields = FieldsMgr.GetFieldInMapping(costData);

        // 获取副本开销;
        mInstanceSpending.text = costData.GetValue<int>(fields).ToString();

        mCostIcon.spriteName = FieldsMgr.GetFieldIcon(fields);
    }

    /// <summary>
    /// 点击战斗按钮回调
    /// </summary>
    void OnClickFightBtn(GameObject go)
    {
        if (! InstanceMgr.IsUnLockLevel(ME.user, mInstanceId)
            || ! InstanceMgr.IsUnlocked(ME.user, mInstanceId))
            return;

        GameObject instacne = WindowMgr.GetWindow("SelectInstanceWnd");

        if (instacne == null)
            return;

        instacne.SetActive(false);

        //获得选择战斗窗口
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        SelectFighterWnd selectFighterScript = wnd.GetComponent<SelectFighterWnd>();

        if (selectFighterScript == null)
            return;

        // 绑定数据
        selectFighterScript.Bind("SelectInstanceWnd", mInstanceId);

        // 关闭副本选择界面
        WindowMgr.HideWindow(SelectInstanceWnd.WndType);
    }

    /// <summary>
    ///绑定副本列表数据
    /// </summary>
    public void Bind(string instanceId, int lockAmount)
    {
        if (string.IsNullOrEmpty(instanceId))
            return;

        mInstanceId = instanceId;

        mLockAmount = lockAmount;

        Redraw();
    }
}
