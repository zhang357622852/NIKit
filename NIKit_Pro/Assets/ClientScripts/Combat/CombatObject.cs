/// <summary>
/// CombatObject.cs
/// Created by wangxw 2014-11-14
/// 战场对象
/// 目前是面向2D角色
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using LPC;

public class CombatObject
{
    #region 成员变量

    // Unity渲染对象
    private GameObject mGameObject = null;

    // 阴影对象
    private Transform mShadow = null;

    // 渲染角色资源名
    private string mPrefebResource = string.Empty;

    // 动画控制器对象
    private Animator mAnimator = null;

    // 模型原始体型半径
    private float mBodyRang = 0.0f;

    // 模型模型碰撞盒中心位置
    private Vector3 mModelCenter = Vector3.zero;

    // 血条偏移量
    private float mHpbarOffestY = 0.0f;

    // 当前速度控制类型
    private int mCurrentSpeedType = SpeedControlType.SCT_CONSTANT;

    // 时间缩放列表
    private Dictionary<int, float> mTimeScaleMap = new Dictionary<int, float>()
    {
        { SpeedControlType.SCT_ATTACK_SPEED, 1.0f },
        { SpeedControlType.SCT_CONSTANT,     1.0f },
        { SpeedControlType.SCT_DAMAGE,       1.0f },
        { SpeedControlType.SCT_MOVE_RATE,    1.0f },
        { SpeedControlType.SCT_CROSS_MAP,    1.0f },
    };

    // 颜色列表
    private List<ColorChanger> mColorList = new List<ColorChanger>();

    // 角色方向
    private int mDirection2D = ObjectDirection2D.RIGHT;

    // 角色透明度
    private float mAlpha = 1f;

    // 模型渐变效果
    private float mAlphaTweenTime = 0f;
    private float mStartAlpha = 1f;
    private float mEndAlpha = 1f;
    private float mAlphaElapseTime = 0f;
    private bool mAlphaTweenEnd = true;

    // 暂停动画的申请计数
    private int mPauseAnimation = 0;

    // 获取当前播放动画名
    private string mAniName = string.Empty;

    #endregion

    #region 属性

    // 是否已经加载
    public bool IsLoaded { get; private set; }

    // 当前速度基准量（默认1空间距离/秒）
    // 最终速度 = 基准逻辑速度 * 速度缩放 * 逻辑坐标转空间坐标
    public float MoveSpeed { get; set; }

    // 时间缩放表
    public Dictionary<int, float> TimeScaleMap
    {
        get { return mTimeScaleMap; }
        private set { TimeScaleMap = value; }
    }

    // 战斗对象的gameObject
    public GameObject gameObject
    {
        get { return mGameObject; }
        private set { mGameObject = value; }
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 设置角色整体透明度
    /// </summary>
    /// <param name="alpha">整体透明比例\n[0 - 1]区间，会影响所有变色层</param>
    private void SetAlpha(float alpha)
    {
        mAlpha = alpha;

        // 获取阴影SpriteRenderer
        SetShadowAlpha(mAlpha);

        // 修改并应用一下顶层颜色
        mColorList[mColorList.Count - 1].DoChange(mGameObject, mAlpha);
    }

    /// <summary>
    /// 设置阴影位置
    /// </summary>
    private void SetShadowAlpha(float alpha)
    {
        // 没有阴影
        if (mShadow == null)
            return;

        // 获取阴影SpriteRenderer
        SpriteRenderer renderer = mShadow.GetComponent<SpriteRenderer>();

        // 没有SpriteRenderer
        if (renderer == null)
            return;

        // 重新设置alpha
        Color curColor = renderer.color;
        renderer.color = new Color(curColor.r, curColor.g, curColor.b, alpha);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    protected CombatObject(string prefeb)
    {
        IsLoaded = false;
        mPrefebResource = prefeb;
        mColorList.Add(ColorChanger.BaseColor);
        MoveSpeed = 1.0f;
    }

    public string ModelId = string.Empty;

    /// <summary>
    /// 加载角色对象
    /// 内部加载prefeb，创建GameObject
    /// </summary>
    protected void Load(string name)
    {
        // 加载资源，目前直接使用阻塞的方式
        IsLoaded = true;

        // 获取模型详细信息
        CsvRow modelInfo = MonsterMgr.GetModelInfo(ModelId);

        // 获取模型的mBodyRang
        if (modelInfo != null)
        {
            // 获取模型体型半径
            LPCValue bodyRange = modelInfo.Query<LPCValue>("body_range");
            if (bodyRange != null)
                mBodyRang = bodyRange.AsFloat;
            else
                mBodyRang = 0f;

            // 获取模型center信息
            LPCArray center = modelInfo.Query<LPCArray>("center");
            mModelCenter = new Vector3(center[0].AsFloat, center[1].AsFloat, center[2].AsFloat);
        }

        // 如果是战斗验证客户端不需要实例化mGameObject
        if (AuthClientMgr.IsAuthClient)
        {
            // 战斗验证客户端只能创建一个模拟对象
            mGameObject = new GameObject(name);
            GameObject.DontDestroyOnLoad(mGameObject);

            // 逻辑层负责激活角色
            mGameObject.SetActive(false);

            return;
        }

        GameObject preObj = ResourceMgr.Load(mPrefebResource) as GameObject;
        mGameObject = GameObject.Instantiate(preObj, preObj.transform.localPosition, preObj.transform.localRotation) as GameObject;
        mGameObject.name = name;
        UnityEngine.GameObject.DontDestroyOnLoad(mGameObject);

        // 逻辑层负责激活角色
        mGameObject.SetActive(false);

        // 获取动画组件
        mAnimator = mGameObject.GetComponent<Animator>();
        if (mAnimator == null)
            LogMgr.Trace("战斗对象({0})无法获取动画组件", mPrefebResource);

        // 获取碰撞
        BoxCollider boxCollider = mGameObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
            mHpbarOffestY = boxCollider.size.y * 0.5f + boxCollider.center.y;

        // 获取阴影组件
        mShadow = mGameObject.transform.Find("EntityShadow");
    }

    /// <summary>
    /// 设置角色对象的Active状态
    /// </summary>
    public void SetActive(bool flag)
    {
        mGameObject.SetActive(flag);
    }

    /// <summary>
    /// 设置角色对象的Active状态
    /// </summary>
    public bool isActive()
    {
        return mGameObject.activeSelf;
    }

    /// <summary>
    /// 卸载角色对象
    /// </summary>
    public void Unload()
    {
        // 如果角色还没有Loaded不处理
        if (! IsLoaded)
            return;

        // 重置IsLoaded标识
        IsLoaded = false;

        // 析构对象
        UnityEngine.Object.Destroy(mGameObject);
    }

    /// <summary>
    /// 获取tag名
    /// </summary>
    /// <returns>The tag.</returns>
    public string GetTag()
    {
        return mGameObject.tag;
    }

    /// <summary>
    /// 比较tag
    /// </summary>
    /// <returns><c>true</c>, if tag was compared, <c>false</c> otherwise.</returns>
    /// <param name="name">Name.</param>
    public bool CompareTag(string name)
    {
        return mGameObject.CompareTag(name);
    }

    /// <summary>
    /// 获取X坐标位置
    /// </summary>
    /// <returns>X轴坐标</returns>
    public float GetPositionX()
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        return mGameObject.transform.position.x;
    }

    /// <summary>
    /// 获取Z坐标位置
    /// </summary>
    /// <returns>Z轴坐标</returns>
    public float GetPositionZ()
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        return mGameObject.transform.position.z;
    }

    /// <summary>
    /// 获取XY坐标位置
    /// </summary>
    /// <returns>XY平面坐标</returns>
    public Vector2 GetPositionXY()
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        Vector3 pos = mGameObject.transform.position;

        // 返回角色位置信息
        return new Vector2(pos.x, pos.y);
    }

    /// <summary>
    /// 获取位置
    /// </summary>
    public Vector3 GetPosition()
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        return mGameObject.transform.position;
    }

    /// <summary>
    /// 获取旋转
    /// </summary>
    public Quaternion GetRotation()
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        return mGameObject.transform.rotation;
    }

    /// <summary>
    /// 设置X轴坐标位置
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    public void SetPositionX(float x)
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        Vector3 cur = mGameObject.transform.position;
        mGameObject.transform.position = new Vector3(x, cur.y, cur.z);
    }

    /// <summary>
    /// 设置XY坐标
    /// Z轴会自动计算，无需处理
    /// </summary>
    /// <param name="xyPos">XY position.</param>
    public void SetPositionXY(Vector2 xyPos)
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        Vector3 cur = mGameObject.transform.position;
        mGameObject.transform.position = new Vector3(xyPos.x, xyPos.y, cur.z);
    }

    /// <summary>
    /// 获取位置
    /// </summary>
    public void SetPosition(Vector3 xyzPos)
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        mGameObject.transform.position = xyzPos;
    }

    /// <summary>
    /// 设置阴影位置
    /// </summary>
    public void SetShadowPosition(Vector3 xyzPos)
    {
        // 没有阴影
        if (mShadow == null)
            return;

        // 设置阴影位置
        mShadow.position = xyzPos;
    }

    /// <summary>
    /// Gets the directoion2 d.
    /// </summary>
    /// <returns>The directoion2 d.</returns>
    public int GetDirectoion2D()
    {
        return mDirection2D;
    }

    /// <summary>
    /// 模型方向
    /// </summary>
    /// <param name="dir">Dir.</param>
    public void SetDirection2D(int dir)
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);

        // 方向没有发生变化
        if (mDirection2D == dir)
            return;

        // 设置方向
        mDirection2D = dir;

        // 设置模型的方向
        if (mDirection2D == ObjectDirection2D.RIGHT)
            mGameObject.transform.eulerAngles = Vector3.zero;
        else
            mGameObject.transform.eulerAngles = new Vector3(0, 180, 0);
    }

    /// <summary>
    /// 指定的动画是否正在播放
    /// </summary>
    /// <returns>播放中的动画</returns>
    public bool IsPlayingAnimation(string aniName)
    {
        // 没有mAnimator组件
        return string.Equals(mAniName, aniName);
    }

    /// <summary>
    /// 设置角色动画
    /// </summary>
    /// <param name="aniName">动画名</param>
    /// <param name="speedType">速度控制类型</param>
    /// <param name="t">开播时间点</param>
    public void SetAnimation(string aniName, int speedType, float t = 0.0f)
    {
        // 记录当前播放动画名
        mAniName = aniName;

        // 使用动画控制器的全局速度来控制播放速度
        mCurrentSpeedType = speedType;

        // 没有mAnimator组件
        if (mAnimator == null)
            return;

        // 直接播放，无需融合
        mAnimator.Play(CombatConfig.ANIMATION_BASE_LAYER + aniName,
            CombatConfig.ANIMATION_BASE_LAYER_INEDX,
            t);

        // 设置动画速度
        mAnimator.speed = IsAnimationPaused() ? 0f : TimeScaleMap[mCurrentSpeedType];
    }

    /// <summary>
    /// 获取当前动画已经播放的时长
    /// 该接口屏蔽不能使用
    /// </summary>
    public float GetCurrentNormalizedTime()
    {
        // 没有mAnimator组件
        if (mAnimator == null)
            return 0f;

        System.Diagnostics.Debug.Assert(mAnimator != null);
        return mAnimator.GetCurrentAnimatorStateInfo(CombatConfig.ANIMATION_BASE_LAYER_INEDX).normalizedTime;
    }

    /// <summary>
    /// 设置角色速度参数
    /// </summary>
    /// <param name="type">速度类型</param>
    /// <param name="speed">速度比例</param>
    public void SetSpeedFactor(int type, float speedFactor)
    {
        TimeScaleMap[type] = speedFactor;

        // 没有mAnimator组件
        if (mAnimator == null)
            return;

        // 如果是当前起效的动画速度，则控制动画变速
        if (mCurrentSpeedType == type)
            mAnimator.speed = IsAnimationPaused() ? 0f : speedFactor;
    }

    /// <summary>
    /// 获取角色速度参数
    /// </summary>
    public float GetSpeedFactor(int type)
    {
        return IsAnimationPaused() ? 0f : TimeScaleMap[type];
    }

    /// <summary>
    /// 是否动画暂停中
    /// </summary>
    /// <returns><c>true</c> if this instance is animation paused; otherwise, <c>false</c>.</returns>
    public bool IsAnimationPaused()
    {
        return mPauseAnimation > 0;
    }

    /// <summary>
    /// 申请动画暂停
    /// </summary>
    public void SubmitAnimationPause(bool enable)
    {
        // 添加引用计数
        if (enable)
            mPauseAnimation++;
        else
            mPauseAnimation = Mathf.Max(0, mPauseAnimation - 1);

        // 主动回到idle
        if (! IsAnimationPaused())
        {
            // 回到默认DEFAULT_PLAY
            SetAnimation(CombatConfig.DEFAULT_PLAY, SpeedControlType.SCT_CONSTANT);
            return;
        }

        // 没有mAnimator
        if (!mAnimator)
            return;

        // 设置动画速度
        mAnimator.speed = 0f;
    }

    /// <summary>
    /// 设置角色变色
    /// </summary>
    /// <returns>颜色id，用于pop操作</returns>
    /// <param name="newColor">New color</param>
    /// <param name="cct">变色类型</param>
    /// <param name="setSource">If set to <c>true</c> 设置为原始颜色</param>
    public string PushColor(Color newColor, int cct, bool setSource = false)
    {
        // 生成唯一编号
        string id = Game.GetUniqueName(CombatConfig.CCID_PRE);

        // 设置为原始色，修改最底层
        if (setSource)
            // 原始色只能为CCT_BASE类型
            mColorList[0] = new ColorChanger(CombatConfig.CCID_BASE, newColor, ColorChangerType.CCT_BASE);
        else
            // 顶层添加颜色变化器
            mColorList.Add(new ColorChanger(id, newColor, cct));

        // 应用一下顶层的颜色
        mColorList[mColorList.Count - 1].DoChange(mGameObject, mAlpha);

        return id;
    }

    /// <summary>
    /// 移除指定id的模型变色
    /// </summary>
    /// <param name="colorId">Color identifier.</param>
    public void PopColor(string colorId)
    {
        // 查找节点
        int index = mColorList.FindIndex((c) =>
            {
                return c.CCid == colorId;
            });

        // 如果是最后一个节点被变动，需要更新一下
        bool needUpdate = (index == mColorList.Count - 1);

        if (mColorList[index].CCid == CombatConfig.CCID_BASE)
            // 原始色不能被移除，替换为基础颜色即可
            mColorList[0] = ColorChanger.BaseColor;
        else
            // 其他节点的移除，直接删
            mColorList.RemoveAt(index);

        // 应用一下顶层的颜色
        if (needUpdate)
            mColorList[mColorList.Count - 1].DoChange(mGameObject, mAlpha);
    }

    /// <summary>
    /// 角色整体透明度参数
    /// </summary>
    /// <returns>The alpha.</returns>
    public float GetAlpha()
    {
        return mAlpha;
    }

    /// <summary>
    /// 角色体形半径
    /// </summary>
    public float GetBodyRang()
    {
        // 体型半径碰撞盒x * 模型的缩放
        return mBodyRang * mGameObject.transform.localScale.x;
    }

    /// <summary>
    /// 角色血条Y偏移
    /// </summary>
    public float GetHpbarOffestY()
    {
        // 体型半径碰撞盒x * 模型的缩放
        return mHpbarOffestY * mGameObject.transform.localScale.y;
    }

    /// <summary>
    /// 获取boxcolider中心点位置
    /// </summary>
    /// <returns>The box colider position.</returns>
    public Vector3 GetBoxColiderPos()
    {
        // 获取碰撞盒实际位置（这个地方只是Y轴需要缩放）
        Vector3 pos = new Vector3(
            mModelCenter.x,
            mModelCenter.y * mGameObject.transform.localScale.y,
            mModelCenter.z
        );

        // 返回碰撞盒中心位置
        return pos + mGameObject.transform.position;
    }

    /// <summary>
    /// 设置Alpha渐变
    /// </summary>
    public void SetTweenAlpha(float endAlpha)
    {
        SetTweenAlpha(GetAlpha(), endAlpha, 0f);
    }

    /// <summary>
    /// 设置Alpha渐变
    /// </summary>
    public void SetTweenAlpha(float endAlpha, float time)
    {
        SetTweenAlpha(GetAlpha(), endAlpha, time);
    }

    /// <summary>
    /// 设置Alpha渐变
    /// </summary>
    public void SetTweenAlpha(float startAlpha, float endAlpha, float time)
    {
        // 如果是瞬间变化
        if (Game.FloatEqual(time, 0f))
        {
            mAlphaTweenEnd = true;
            SetAlpha(endAlpha);
            return;
        }

        // 记录数据
        mAlphaTweenTime = time;
        mStartAlpha = startAlpha;
        mEndAlpha = endAlpha;
        mAlphaElapseTime = 0f;

        // Alpha渐变结束
        mAlphaTweenEnd = false;
    }

    /// <summary>
    /// Updates the alpha.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public void UpdateTweenAlpha(float deltaTime)
    {
        // 如果模型Alpha渐变已经结束
        if (mAlphaTweenEnd)
            return;

        // 累计时间
        mAlphaElapseTime += deltaTime;

        // 如果已经到了最大变化时间
        if (Game.FloatGreat(mAlphaElapseTime, mAlphaTweenTime))
        {
            // 设置最终的Alpha
            SetAlpha(mEndAlpha);

            // 标识Alpha渐变已经结束
            mAlphaTweenEnd = true;

            return;
        }

        // 设置模型Alpha
        float rate = mAlphaElapseTime / mAlphaTweenTime;
        SetAlpha(mStartAlpha * (1 - rate) + mEndAlpha * rate);
    }

    /// <summary>
    /// 角色缩放
    /// </summary>
    public void SetScale(float x, float y, float z)
    {
        mGameObject.transform.localScale = new Vector3(x, y, z);
    }

    /// <summary>
    /// 设置角色皮肤
    /// </summary>
    public void SetSkin(string skin)
    {
        SkeletonRenderer skeletonRender = mGameObject.GetComponent<SkeletonRenderer>();

        // 目标对象没有 SkeletonRenderer组件
        if (skeletonRender == null)
            return;

        // 设置皮肤
        skeletonRender.initialSkinName = skin;
        skeletonRender.Initialize(true);
    }

    /// <summary>
    /// 显示血条
    /// </summary>
    public void ShowHp(bool showhp = false)
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        Hp hpWnd = mGameObject.GetComponent<Hp>();

        if (!showhp)
        {
            // 隐藏
            if (hpWnd == null)
                return;

            // 隐藏血条
            hpWnd.HideHp();
        }
        else
        {
            // 显示
            if (hpWnd == null)
                hpWnd = mGameObject.AddComponent<Hp>();

            // 显示血条
            hpWnd.ShowHp(mGameObject.name);
        }
    }

    /// <summary>
    /// 显示血条
    /// </summary>
    public void ShowValueWnd(bool showWnd = false)
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);

        string wndName = "PetValueDebugWnd_" + mGameObject.name;
        GameObject mValueWnd = WindowMgr.GetWindow(wndName);

        if (!showWnd)
        {
            if (mValueWnd == null)
                return;

            if (!mValueWnd.activeInHierarchy)
                return;

            mValueWnd.SendMessage("HideWnd");
        }
        else
        {
            // 查找角色对象
            Property target = Rid.FindObjectByRid(mGameObject.name);
            if( target == null)
                return;

            // 角色处于死亡状态
            if (target.CheckStatus("DIED"))
                return;

            // 创建血条
            if (mValueWnd != null)
            {
                if(!mValueWnd.activeInHierarchy)
                    mValueWnd.SetActive(true);

                mValueWnd.SendMessage("ShowWnd", target.Query<int>("is_boss") == 1);
                return;
            }

            // 创建血条
            mValueWnd = WindowMgr.CreateWindow(wndName, PetValueDebugWnd.PrefebResource, null, 1.0f, true);
            if (mValueWnd == null)
            {
                LogMgr.Trace("血条创建失败。");
                return;
            }

            // 绑定对象显示血条
            mValueWnd.SendMessage("SetBind", mGameObject.name);
            mValueWnd.SendMessage("ShowWnd", target.Query<int>("is_boss") == 1);
        }
    }

    /// <summary>
    /// 设置hp的sortOrder
    /// </summary>
    /// <param name="order">Order.</param>
    public void SetHpSortOrder(int order)
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        Hp hpWnd = mGameObject.GetComponent<Hp>();

        if (hpWnd == null)
            return;

        hpWnd.SetSortOrder(order);
    }

    /// <summary>
    /// 显示经验条
    /// </summary>
    public void ShowExp(bool exphp = false, LPC.LPCMapping map = null)
    {
        System.Diagnostics.Debug.Assert(mGameObject != null);
        Exp expWnd = mGameObject.GetComponent<Exp>();

        if (!exphp)
        {
            // 隐藏
            if (expWnd != null)
                mGameObject.SendMessage("HideExp");
        }
        else
        {
            // 显示
            if (expWnd == null)
                expWnd = mGameObject.AddComponent<Exp>();

            // 绑定数据;
            expWnd.ShowExp(map, mGameObject.name);
        }
    }

    #endregion

}
