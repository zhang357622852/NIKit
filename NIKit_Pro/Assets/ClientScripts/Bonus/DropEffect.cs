/// <summary>
/// DropEffect.cs
/// Created by lic 2016-8-31
/// 掉落物品
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public partial class DropEffect : MonoBehaviour
{

    public GameObject mPickUpEffect;

    public GameObject[] mNumbers;

    public GameObject mDropOb;

    public GameObject mShadow;

    #region 成员变量

    private float timeScale = 1.0f;

    private float delay = 0f;

    // 缓存物品信息
    private string mWndType = "";

    private int mNum = 0;

    // 是否在下落动画完后立即回收
    private bool PickUpImmediately {get; set;}

    // 自动拾取时间
    private float mAutoPickTime = CALC_DROP_ITEM_AUTO_PICK_TIME.Call();

    // 状态
    private enum EFFECT_STATE
    {
        STATE_NULL,    // 空
        STATE_DROP,    // 正在掉落状态
        STATE_IDLE,    // 等待状态
        STATE_PICK,    // 拾取状态
        STATE_END,     // 结束
    };

    private EFFECT_STATE mCurrentStage = EFFECT_STATE.STATE_NULL;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Awake()
    {
        mPickUpEffect.GetComponent<SpriteRenderer>().enabled = false;

        mDropOb.GetComponent<SpriteRenderer>().enabled = false;

        foreach (GameObject item in mNumbers)
        {
            item.GetComponent<SpriteRenderer>().enabled = false;
        }

        mShadow.GetComponent<SpriteRenderer>().enabled = false;
    }

    /// <summary>
    /// 协程播放掉落动画
    /// </summary>
    private IEnumerator PlayDrop(float delay)
    {
        yield return new WaitForSeconds(delay);

        mDropOb.GetComponent<SpriteRenderer>().enabled = true;
        mDropOb.GetComponent<Animator>().Play("luoxia", 0, 0f);

        mShadow.GetComponent<SpriteRenderer>().enabled = true;
        mShadow.GetComponent<Animator>().Play("luoxia", 0, 0f);
    }

    /// <summary>
    /// 播放拾取动画
    /// </summary>
    private void PickDrop()
    {
        // 设置数量
        SetNum(mNum);

        for (int i = 0; i < mNumbers.Length; i++)
        {
            mNumbers[i].GetComponent<TweenAlpha>().enabled = true;
            mNumbers[i].GetComponent<TweenAlpha>().ResetToBeginning();
        }

        mPickUpEffect.GetComponent<SpriteRenderer>().enabled = true;
        mPickUpEffect.GetComponent<Animator>().Play("idle", 0, 0f);

        mDropOb.GetComponent<SpriteRenderer>().enabled = true;
        mDropOb.GetComponent<Animator>().Play("shiqu", 0, 0f);
        mShadow.GetComponent<Animator>().Play("shiqu", 0, 0f);
    }

    /// <summary>
    /// 设置数量
    /// </summary>
    /// <param name="num">Number.</param>
    private void SetNum(int num)
    {
        if (num <= 0 || num >= 1000)
            return;

        char[] nums = num.ToString().ToCharArray();

        for (int i = 0; i < mNumbers.Length; i++)
        {
            SpriteRenderer spr = mNumbers[i].GetComponent<SpriteRenderer>();

            if (i >= nums.Length)
            {
                spr.enabled = false;
                continue;
            }

            spr.enabled = true;
            spr.sprite = LoadSpriteRes(spr, nums[i]);
            
        }
    }

    /// <summary>
    /// 加载SpriteRenderer图片
    /// </summary>
    /// <param name="sr">Sr.</param>
    /// <param name="num">Number.</param>
    private Sprite LoadSpriteRes(SpriteRenderer spr, char num)
    {
        string resPath = string.Format("Assets/Art/UI/Numbers/DropNum/white_{0}.png", num);
        Texture2D spriteRes = ResourceMgr.LoadTexture(resPath);

        if (spriteRes == null)
            return null;

        return Sprite.Create(spriteRes, spr.sprite.textureRect, new Vector2(0.5f, 0.5f));
    }

    #endregion

    #region 公共函数

    /// <summary>
    /// 绑定物品信息
    /// </summary>
    /// <param name="item">Item.</param>
    /// <param name="num">Number.</param>
    /// <param name="entityInfo">Entity info.</param>
    public void BindItemInfo(string wndType, int num, LPCMapping entityInfo, float zOffset)
    { 
        if (num == 0)
            return;

        // 绑定数据
        mWndType = wndType;

        mNum = num;

        // 计算掉落起点位置
        LPCArray posArr = entityInfo.GetValue<LPCArray>("position");
        if (posArr != null)
            transform.position = new Vector3(posArr[0].AsFloat, posArr[1].AsFloat, posArr[2].AsFloat);

        // 随机偏移量(x,y)
        Vector2 offset = CALC_DROP_ITEM_DROP_VELOCITY.Call();
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x + offset.x, pos.y + offset.y, pos.z + zOffset);

        PickUpImmediately = false;

        // 随机延迟掉落
        delay = CALC_DROP_ITEM_DROP_DELAY_TIME.Call();

        // 切换掉落状态
        mCurrentStage = EFFECT_STATE.STATE_DROP;

        // 延迟掉落
        Coroutine.DispatchService(PlayDrop(delay));
    }

    /// <summary>
    /// 执行拾取
    /// </summary>
    public void DoPick()
    {
        if (mCurrentStage == EFFECT_STATE.STATE_IDLE)
        {
            mCurrentStage = EFFECT_STATE.STATE_PICK;
            CancelInvoke ("PickDrop");
            PickDrop ();
            return;
        }
        else if (mCurrentStage == EFFECT_STATE.STATE_DROP)
        {
            PickUpImmediately = true;
        }
    }

    /// <summary>
    /// 拾取动画播放完毕
    /// </summary>
    public void OnPickFinish()
    {
        mCurrentStage = EFFECT_STATE.STATE_END;

        mPickUpEffect.GetComponent<SpriteRenderer>().enabled = false;

        mDropOb.GetComponent<SpriteRenderer>().enabled = false;

        mShadow.GetComponent<SpriteRenderer>().enabled = false;

        // 释放光效资源
        DropEffectMgr.ReleaseUsedEffectItem(mWndType, gameObject);
    }

    /// <summary>
    /// 播放掉落idle动画
    /// </summary>
    public void OnDropFinish()
    {
        if (PickUpImmediately)
        {
            mCurrentStage = EFFECT_STATE.STATE_PICK;
            PickDrop();
            return;
        }

        mCurrentStage = EFFECT_STATE.STATE_IDLE;

        mDropOb.GetComponent<Animator>().Play("idle", 0, 0f);

        mShadow.GetComponent<Animator>().Play("idle", 0, 0f);

        Invoke("PickDrop", mAutoPickTime / timeScale);
    }

    #endregion
}
