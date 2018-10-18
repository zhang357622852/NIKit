/// <summary>
/// Hp.cs
/// Created by fucj 2014-11-27
/// 血条脚本
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class Hp : MonoBehaviour
{
    // 血条窗口前缀
    static string HpWndPre = "Hp_";
    static string BossHPWnd = "BossHP_";

    GameObject mHpBar;
    GameObject mBossHpBar;
    string mTragetRid;

    #region 内部接口

    /// <summary>
    /// 创建boss血条
    /// </summary>
    /// <param name="target">Target.</param>
    private void CreateBossHpBar(Property target)
    {
        // 获取角色boss数据
        LPCMapping bossData = target.QueryTemp<LPCMapping>("boss_data");
        if (bossData == null)
            return;

        // 获取BOSS数据
        int bossIndex = bossData.GetValue<int>("index");
        int bossAmount = bossData.GetValue<int>("boss_amount");

        // 血条窗口名
        string wndName = BossHPWnd + mTragetRid;

        // 获取血条窗口对象
        mBossHpBar = WindowMgr.GetWindow(wndName);
        if (mBossHpBar == null)
        {
            string prefebResource = string.Empty;

            // 创建boss血条
            if (bossAmount > 1)
                prefebResource = string.Format("Assets/Prefabs/Window/{0}.prefab",
                    WindowMgr.GetCustomWindowName("BossSmallHPBar"));
            else
                prefebResource = string.Format("Assets/Prefabs/Window/{0}.prefab",
                    WindowMgr.GetCustomWindowName("BossBigHPBar"));

                // 创建血条窗口
            mBossHpBar = WindowMgr.CreateWindow(wndName, prefebResource, null, 1.0f, true);
        }

        // 创建血条窗口失败
        if(mBossHpBar == null)
            return;

        // 调整boss血条的位置
        if(bossAmount > 1)
        {
            // 初始位置
            mBossHpBar.transform.localPosition = new Vector3(471, 314, 0);
            mBossHpBar.transform.localPosition = new Vector3(
                mBossHpBar.transform.localPosition.x - bossIndex * 322,
                mBossHpBar.transform.localPosition.y,
                mBossHpBar.transform.localPosition.z);
        }

        // 绑定数据
        mBossHpBar.GetComponent<BossHPBarWnd>().Bind(mTragetRid);
        mBossHpBar.SetActive(true);
    }

    /// <summary>
    /// 创建普通血条
    /// </summary>
    private void CreateNormalHpBar(Property target)
    {
        // 创建血条
        string wndName = HpWndPre + mTragetRid;
        mHpBar = WindowMgr.GetWindow(wndName);
        if (mHpBar != null)
        {
            if(!mHpBar.activeInHierarchy)
                mHpBar.SetActive(true);

            mHpBar.SendMessage("ShowHp");
            return;
        }

        // 创建血条
        mHpBar = WindowMgr.CreateWindow(wndName, HpBar.PrefebResource, null, 1.0f, true);
        if (mHpBar == null)
        {
            LogMgr.Trace("血条创建失败。");
            return;
        }

        // 绑定对象显示血条
        mHpBar.SendMessage("SetBind", mTragetRid);
    }

    /// <summary>
    /// 窗口析构
    /// </summary>
    private void OnDestroy()
    {
        // 销毁血条窗口
        if (mHpBar != null)
            WindowMgr.DestroyWindow(HpWndPre + mTragetRid);

        if(mBossHpBar != null)
            WindowMgr.DestroyWindow(BossHPWnd + mTragetRid);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 绑定对象
    /// </summary>
    /// <param name="rid">Rid.</param>
    /// <param name="t">持续时间，-1表示永久</param>
    public void ShowHp(string rid)
    {
        // 绑定rid
        mTragetRid = rid;

        // 查找角色对象
        Property target = Rid.FindObjectByRid(mTragetRid);
        if( target == null)
            return;

        // 判断角色是否是boss，如果是boss需要创建boss血条
        if (target.Query<int>("is_boss") == 1)
        {
            CreateBossHpBar(target);
            return;
        }

        // 创建普通血条
        CreateNormalHpBar(target);
    }

    /// <summary>
    /// 关闭血条
    /// </summary>
    public void HideHp()
    {
        // 血条不存在或者已经隐藏了
        if (mHpBar == null ||
            (! mHpBar.activeSelf || ! mHpBar.activeInHierarchy))
            return;

        // 隐藏血条
        mHpBar.SendMessage("HideHp");
    }

    /// <summary>
    /// 设置血条的sortOrder
    /// </summary>
    /// <param name="order">Order.</param>
    public void SetSortOrder(int order)
    {
        if (mHpBar == null || ! mHpBar.activeInHierarchy)
            return;

        // 设置窗口层级
        mHpBar.SendMessage("SetSortOrder", order);
    }

    #endregion
}
