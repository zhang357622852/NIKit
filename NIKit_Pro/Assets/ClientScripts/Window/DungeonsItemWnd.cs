/// <summary>
/// DungeonsItemWnd.cs
/// Created by fengsc 2016/12/30
/// 地下城列表基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DungeonsItemWnd : WindowBase<DungeonsItemWnd>
{
    // 地下城的名称
    public UILabel mDungeonsName;

    // 地下城图标
    public UITexture mDungeonsIcon;

    // 秘密地下城图标
    public UISprite mSecretIcon;

    public GameObject mRedPointTips;

    public UILabel mTipsAmount;

    public GameObject mTime;

    public UILabel mTimer;

    CsvRow mConfigData;

    int mInterval = 0;

    [HideInInspector]
    public int mMapId = 0;

    [HideInInspector]
    public LPCMapping mExtraPara = LPCMapping.Empty;

    bool mIsCountDown = false;
    float mLastTime = 0;

    void Update()
    {
        if (mIsCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1)
            {
                mLastTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mRedPointTips.SetActive(false);

        // 获取地图id
        mMapId = mConfigData.Query<int>("rno");

        int mapType = mConfigData.Query<int>("map_type");

        // 获取地下城图标
        string iconName = mConfigData.Query<string>("icon");
        if (mapType.Equals(MapConst.PET_DUNGEONS_MAP))
        {
            // 获取使魔id
            int petId = mExtraPara.GetValue<int>("pet_id");

            int rank = 0;
            if (mExtraPara.ContainsKey(rank))
                rank = mExtraPara.GetValue<int>("rank");
            else
                rank = MonsterMgr.GetDefaultRank(petId);

            // 加载头像图片
            Texture2D iconRes = MonsterMgr.GetTexture(petId, rank);

            if (iconRes != null)
                mDungeonsIcon.mainTexture = iconRes;

            mDungeonsIcon.gameObject.SetActive(true);
            mSecretIcon.gameObject.SetActive(false);
        }
        else if (mapType.Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
            mSecretIcon.spriteName = iconName;
            mDungeonsIcon.gameObject.SetActive(false);
            mSecretIcon.gameObject.SetActive(true);
            mRedPointTips.SetActive(true);
            mTipsAmount.text = InstanceMgr.GetSecretDungeonsList(ME.user).Count.ToString();
        }
        else
        {
            // 加载头像图片
            string resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);
            Texture2D iconRes = ResourceMgr.LoadTexture(resPath);

            if (iconRes != null)
                mDungeonsIcon.mainTexture = iconRes;

            mDungeonsIcon.gameObject.SetActive(true);
            mSecretIcon.gameObject.SetActive(false);
        }

        // 获取地下城的名字
        mDungeonsName.text = LocalizationMgr.Get(mConfigData.Query<string>("name"));

        mDungeonsName.transform.localPosition = new Vector3(
            mDungeonsName.transform.localPosition.x,
            3, mDungeonsName.transform.localPosition.z);

        mTime.SetActive(false);

        if (mExtraPara != null && mExtraPara.Count > 0)
        {
            mDungeonsName.transform.localPosition = new Vector3(
                mDungeonsName.transform.localPosition.x,
                14, mDungeonsName.transform.localPosition.z);

            mTime.SetActive(true);

            if (mExtraPara == null)
                return;

            int endTime = mExtraPara.GetValue<int>("end_time");

            mInterval = Mathf.Max(endTime - TimeMgr.GetServerTime(), 0);

            // 开启倒计时
            mIsCountDown = true;
        }
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (mInterval < 1)
        {
            mTime.SetActive(false);

            mDungeonsName.transform.localPosition = new Vector3(
                mDungeonsName.transform.localPosition.x,
                3, mDungeonsName.transform.localPosition.z);

            // 结束调用
            mIsCountDown = false;

            gameObject.SetActive(false);
        }

        mInterval--;
        mTimer.text = TimeMgr.ConvertTimeToChineseTimer(mInterval, false);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int mapId, LPCMapping extraPara)
    {
        // 缓存地图配置信息
        mConfigData = MapMgr.GetMapConfig(mapId);

        // 不存在的地图信息
        if (mConfigData == null)
            return;

        // 缓存附加参数
        mExtraPara = extraPara;

        // 绘制窗口
        Redraw();
    }
}
