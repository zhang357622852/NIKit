/// <summary>
/// ModelWnd.cs
/// Created by tanzy 05/30/2016
/// 宠物模型管理器
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;
using Spine;
using Spine.Unity;

/// <summary>
/// 模型窗口
/// </summary>
public class ModelWnd : MonoBehaviour
{
    [HideInInspector]
    public Property mPetOb;

    [HideInInspector]
    private GameObject mModelOb = null;
    private GameObject mShadowOb = null;
    private string mModelId;

    // 窗口唯一标识
    private string instanceID = string.Empty;

    // 模型点击回调
    private CallBack mTask;

    #region 内部接口

    /// 挂接
    /// </summary>
    /// <param name="ob">Ob.</param>
    /// <param name="parent">Parent.</param>
    private void Attach(GameObject ob, Transform parent)
    {
        Vector3 pos = ob.transform.localPosition;
        Quaternion rotate = ob.transform.localRotation;
        Vector3 scale = ob.transform.localScale;

        // 设置模型的父节点
        ob.transform.parent = parent;

        // 重置对象的位置信息
        ob.transform.localPosition = pos;
        ob.transform.localRotation = rotate;
        ob.transform.localScale = new Vector3(scale.x * Game.UnitToPixelScale,
            scale.y * Game.UnitToPixelScale,
            scale.z * Game.UnitToPixelScale);
    }

    /// <summary>
    /// 异步载入模型
    /// </summary>
    /// <returns>The load model.</returns>
    /// <param name="classId">Class identifier.</param>
    /// <param name="rank">Rank.</param>
    /// <param name="layer">Layer.</param>
    private IEnumerator SynLoadModel(int classId, int rank, int layer)
    {
        // 没有模型id
        string modelId = MonsterMgr.GetModel(classId);
        if (string.IsNullOrEmpty(modelId))
            yield break;

        // 创建角色对象
        // 载入资源
        string prefabRes = MonsterMgr.GetModelResPath(modelId);

        // 异步载入资源
        yield return Coroutine.DispatchService(ResourceMgr.LoadAsync(prefabRes));

        // 如果模型需要显示模型和当前显示模型不同
        if (! string.Equals(mModelId, modelId) || mModelOb == null)
        {
            // 先UnLoadModel掉旧的模型
            UnLoadModel();

            // 载入资源
            GameObject petModel = ResourceMgr.Load(prefabRes) as GameObject;

            // 再克隆一份
            mModelOb = GameObject.Instantiate(petModel, petModel.transform.localPosition, petModel.transform.localRotation) as GameObject;

            // 记录当前显示模型id
            mModelId = modelId;

            // 设置模型的原始位置
            mModelOb.transform.localPosition = Vector3.zero;
            mModelOb.transform.localScale = Vector3.one;
            mModelOb.name = "model_" + modelId;

            // 设置层
            mModelOb.layer = layer;

            // 获取皮肤信息，如果没有皮肤信息则使用的默认配置
            SetSkin(CALC_ACTOR_SKIN.Call(rank));

            // 获取动画组件，播放默认的ilde动作
            Animator mAnimator = mModelOb.GetComponent<Animator>();
            if (mAnimator != null)
            {
                // 直接播放，无需融合
                mAnimator.Play(CombatConfig.ANIMATION_BASE_LAYER + CombatConfig.DEFAULT_PLAY,
                    CombatConfig.ANIMATION_BASE_LAYER_INEDX,
                    0.0f);
            }

            // 挂接模型
            Attach(mModelOb, gameObject.transform);
        }
        else
        {
            // 设置层
            mModelOb.layer = layer;

            // 获取皮肤信息，如果没有皮肤信息则使用的默认配置
            SetSkin(CALC_ACTOR_SKIN.Call(rank));

            // 获取动画组件，播放默认的ilde动作
            Animator mAnimator = mModelOb.GetComponent<Animator>();
            if (mAnimator != null)
            {
                // 直接播放，无需融合
                mAnimator.Play(CombatConfig.ANIMATION_BASE_LAYER + CombatConfig.DEFAULT_PLAY,
                    CombatConfig.ANIMATION_BASE_LAYER_INEDX,
                    0.0f);
            }
        }

        // 设置模型的Layer
        mModelOb.GetComponent<MeshRenderer>().sortingLayerName = "Default";

        // 设置阴影组件的layer
        Transform shadow = mModelOb.transform.Find("EntityShadow");
        if (shadow != null)
        {
            // 获取gameObject
            mShadowOb = shadow.gameObject;

            // 设置层级
            mShadowOb.layer = layer;

            // 设置阴影Layer
            SpriteRenderer sRender = mShadowOb.GetComponent<SpriteRenderer>();
            sRender.sortingLayerName = "Default";

            // 这个地方必须将sortingOrder设置为0，将模型和阴影设置到相同sortingOrder
            // 最后通过renderQueue设置渲染顺序
            sRender.sortingOrder = 0;
        }

        // 添加uiwidget控件
        mModelOb.AddComponent<UIWidget>();

        // 修正model的renderQueue
        OnFixRenderQueue();

        // 绑定模型点击事件
        UIEventListener.Get(mModelOb).onClick = OnClickModel;
    }

    private void OnClickModel(GameObject ob)
    {
        if (mTask != null)
            mTask.Go(mModelOb);
    }

    /// <summary>
    /// 获取渲染层级
    /// </summary>
    private int GetRenderQueue(GameObject wnd)
    {
        // 获取UIPanel父窗口的渲染层级, 模型渲染
        UIPanel panel = wnd.GetComponent<UIPanel>();
        if (panel != null)
            return panel.startingRenderQueue;

        // 获取UIWidget父窗口的渲染层级
        UIWidget widget = wnd.GetComponent<UIWidget>();
        if (widget != null)
            return widget.drawCall.renderQueue;

        // 获取父窗口,尝试通过父窗口获取渲染层级
        Transform parent = wnd.transform.parent;
        if (parent != null)
            return GetRenderQueue(parent.gameObject);

        // 否则不需要设置渲染层级, 默认返回3000
        // NGUI默认渲染层级从3000开始
        return 3000;
    }

    /// <summary>
    /// Awake this instance.
    /// </summary>
    void Awake()
    {
        instanceID = gameObject.GetInstanceID().ToString();

        UIPanel.onLateUpdateFinish += OnFixRenderQueue;
    }

    /// <summary>
    /// 修正模型的渲染顺序
    /// </summary>
    public void OnFixRenderQueue()
    {
        // 窗口没有绑定模型不处理
        if (mModelOb == null)
            return;

        // 获取模型的MeshRenderer
        MeshRenderer render = mModelOb.GetComponent<MeshRenderer>();
        if (render == null)
            return;

        // 渲染层级没有发生变化不处理
        int renderQueue = GetRenderQueue(gameObject);
        if (renderQueue == render.material.renderQueue)
            return;

        // 设置阴影的renderQueue
        if (mShadowOb != null)
        {
            SpriteRenderer sRender = mShadowOb.GetComponent<SpriteRenderer>();

            // 必须保证模型的阴影在模型后面
            sRender.material.renderQueue = renderQueue - 1;
        }

        // 设置模型的渲染层级
        render.material.renderQueue = renderQueue;
    }

    /// <summary>
    /// 析构窗口
    /// </summary>
    void OnDestroy()
    {
        // 关闭已不再载入资源协程
        Coroutine.StopCoroutine(instanceID);

        // 卸载模型对象
        UnityEngine.Object.Destroy(mModelOb);
        mModelOb = null;

        UIPanel.onLateUpdateFinish -= OnFixRenderQueue;
    }

    #endregion

    /// <summary>
    /// 设置皮肤
    /// </summary>
    /// <param name="skin">Skin.</param>
    public void SetSkin(string skin)
    {
        // 没有指定皮肤
        if (string.IsNullOrEmpty(skin))
            return;

        // 获取骨骼动组件
        SkeletonRenderer skeletonRender = mModelOb.GetComponent<SkeletonRenderer>();
        if (skeletonRender == null)
            return;

        // 设置皮肤
        skeletonRender.initialSkinName = skin;
        skeletonRender.Initialize(true);
    }

    /// <summary>
    /// 载入模型
    /// </summary>
    public GameObject LoadModel(Property pet, int layer)
    {
        if (pet == null)
            return null;

        mPetOb = pet;

        return LoadModel(pet.GetClassID(), pet.GetRank(), layer);
    }

    /// <summary>
    /// 载入模型
    /// </summary>
    public GameObject LoadModel(int classId, int rank, int layer)
    {
        // 先UnLoadModel掉旧的模型
        UnLoadModel();

        // 没有模型id
        string modelId = MonsterMgr.GetModel(classId);
        if (string.IsNullOrEmpty(modelId))
            return null;

        // 创建角色对象
        // 载入资源
        string prefabRes = string.Format("Assets/Prefabs/Model/{0}.prefab", modelId);
        GameObject petModel = ResourceMgr.Load(prefabRes) as GameObject;

        // 再克隆一份
        mModelOb = GameObject.Instantiate(petModel, petModel.transform.localPosition, petModel.transform.localRotation) as GameObject;

        // 设置模型的原始位置
        mModelOb.transform.localPosition = Vector3.zero;
        mModelOb.transform.localScale = Vector3.one;
        mModelOb.name = "model_" + modelId;

        // 设置层
        mModelOb.layer = layer;

        // 获取皮肤信息，如果没有皮肤信息则使用的默认配置
        SetSkin(CALC_ACTOR_SKIN.Call(rank));

        // 挂接模型
        Attach(mModelOb, gameObject.transform);

        // 设置模型的Layer
        mModelOb.GetComponent<MeshRenderer>().sortingLayerName = "Default";

        // 设置阴影组件的layer
        Transform shadow = mModelOb.transform.Find("EntityShadow");
        if (shadow != null)
        {
            // 获取gameObject
            mShadowOb = shadow.gameObject;

            // 设置层级
            mShadowOb.layer = layer;

            // 设置阴影Layer
            SpriteRenderer sRender = mShadowOb.GetComponent<SpriteRenderer>();
            sRender.sortingLayerName = "Default";

            // 这个地方必须将sortingOrder设置为0，将模型和阴影设置到相同sortingOrder
            // 最后通过renderQueue设置渲染顺序
            sRender.sortingOrder = 0;
        }

        // 修正model的renderQueue
        OnFixRenderQueue();

        return mModelOb;
    }

    /// <summary>
    /// 异步载入模型
    /// </summary>
    /// <returns>The load model.</returns>
    /// <param name="pet">Pet.</param>
    /// <param name="layer">Layer.</param>
    public void LoadModelSync(Property pet, int layer, CallBack task = null)
    {
        // 宠物对象不存在
        if (pet == null)
            return;

        // 记录宠物对象
        mPetOb = pet;

        // 记录
        mTask = task;

        // 关闭已不再载入资源协程
        Coroutine.StopCoroutine(instanceID);

        // 协程中异步载入模型
        Coroutine.DispatchService(SynLoadModel(pet.GetClassID(), pet.GetRank(), layer), instanceID);
    }

    /// <summary>
    /// 异步载入模型
    /// </summary>
    /// <returns>The load model.</returns>
    /// <param name="classId">Class identifier.</param>
    /// <param name="rank">Rank.</param>
    /// <param name="layer">Layer.</param>
    public void LoadModelSync(int classId, int rank, int layer, CallBack task = null)
    {
        mTask = task;

        // 关闭已不再载入资源协程
        Coroutine.StopCoroutine(instanceID);

        // 协程中异步载入模型
        Coroutine.DispatchService(SynLoadModel(classId, rank, layer), instanceID);
    }

    /// <summary>
    /// 卸载旧模型
    /// </summary>
    public void UnLoadModel()
    {
        // 关闭已不再载入资源协程
        Coroutine.StopCoroutine(instanceID);

        // 模型对象不存在
        if (mModelOb == null)
            return;

        // 需要先将mModelOb设置为非激活状态, 否则火爆如下错误
        // Cannot change GameObject hierarchy while activating or deactivating the parent.
        mModelOb.SetActive(false);

        // 设置模型的父节点
        mModelOb.transform.parent = null;

        // 重置mModelId
        mModelId = string.Empty;

        // 回收资源
        UnityEngine.Object.Destroy(mModelOb);
    }
}
