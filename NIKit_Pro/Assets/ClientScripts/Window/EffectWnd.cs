/// <summary>
/// EffectWnd.cs
/// Created by Lic 12/01/2016
/// 光效管理器
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;
using Spine;
using Spine.Unity;

public class EffectWnd : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> mEffectObList = new List<GameObject>();

#region 内部接口

    /// 挂接
    /// </summary>
    /// <param name="ob">Ob.</param>
    /// <param name="parent">Parent.</param>
    void Attach(GameObject ob, Transform parent, int effectScale)
    {
        Vector3 pos = ob.transform.localPosition;
        Quaternion rotate = ob.transform.localRotation;
        Vector3 scale = ob.transform.localScale;

        // 设置模型的父节点
        ob.transform.parent = parent;

        // 重置对象的位置信息
        ob.transform.localPosition = pos;
        ob.transform.localRotation = rotate;
        ob.transform.localScale = new Vector3(scale.x * effectScale,
            scale.y * effectScale,
            scale.z * effectScale);
    }

    /// <summary>
    /// 析构窗口
    /// </summary>
    void OnDestroy()
    {
        foreach(GameObject ob in mEffectObList)
        {
            UnityEngine.Object.Destroy(ob);
        }

        mEffectObList = new List<GameObject>();
    }

    /// <summary>
    /// 获取渲染层级
    /// </summary>
    int GetRenderQueue(GameObject wnd)
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
    /// 设置父物体及子物体的Renderer
    /// </summary>
    /// <param name="ob">Ob.</param>
    void SetRenderer(Transform tf)
    {
        if(tf.GetComponent<Renderer>() != null)
            tf.GetComponent<Renderer>().material.renderQueue = GetRenderQueue(gameObject);
            
        foreach(Transform child in tf)
        {
            if(child.GetComponent<Renderer>() != null)
                child.GetComponent<Renderer>().material.renderQueue = GetRenderQueue(gameObject);

            foreach(Transform grandson in child)
            {
                if(grandson.GetComponent<Renderer>() != null)
                    grandson.GetComponent<Renderer>().material.renderQueue = GetRenderQueue(gameObject);
            }
        }
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        foreach(GameObject item in mEffectObList)
        {
            if(item == null || !item.activeInHierarchy)
                continue;

            if(item.GetComponent<Renderer>().material.renderQueue == GetRenderQueue(gameObject))
                continue;

            SetRenderer(item.transform);
        }

    }

#endregion

#region 外部接口

    /// <summary>
    /// 载入模型
    /// </summary>
    public List<GameObject> LoadEffects(string[] effectArray, int effectScale)
    {
        // 先UnLoadModel掉旧的光效
        UnLoadEffects();

        for(int i = 0; i < effectArray.Length; i++)
        {
            // 创建角色对象
            // 载入资源
            string prefabRes = string.Format("Assets/Prefabs/3DEffect/{0}.prefab", effectArray[i]);
            GameObject effectPrefab = ResourceMgr.Load(prefabRes) as GameObject;

            if(effectPrefab == null)
                continue;

            // 再克隆一份
            GameObject effectOb = GameObject.Instantiate(effectPrefab, effectPrefab.transform.localPosition, effectPrefab.transform.localRotation) as GameObject;

            // 设置模型的原始位置
            effectOb.name = string.Format("UIEffect_{0}", effectArray[i]);

            Attach(effectOb, gameObject.transform, effectScale);

            effectOb.SetActive(false);

            mEffectObList.Add(effectOb);
        }

        return mEffectObList;
    }

    /// <summary>
    /// 卸载光效
    /// </summary>
    public void UnLoadEffects()
    {
        foreach(GameObject ob in mEffectObList)
        {
            // 设置模型的父节点
            ob.transform.parent = null;

            // 回收资源
            UnityEngine.Object.Destroy(ob);
        }
    }


#endregion

}
    

