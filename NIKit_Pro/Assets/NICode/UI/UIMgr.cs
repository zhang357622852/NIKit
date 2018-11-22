/*
 * 1.缓存常用UI窗体
 *      1.1: UIFormsLifeType
 *              1.1.1: HumanLife(无需缓存)
 *              1.1.2: GoldLife(缓存/关闭时只是隐藏)
 *
 * 2.UI窗体间的传值
 *      2.1: GetForms() 获得窗口实例
 *      2.2: UIEventNotify
 *
 * 3.UI窗体层级，UI导航
 *      3.1: 窗体层级: AutoCalculateDepth
 *      3.2: UI导航：自定义的栈记录打开的界面
 *
 * 4.模态窗口:遮蔽层,屏蔽下层消息
 *      4.1: 构建窗体时，自行添加遮罩层
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 窗体管理类
/// </summary>
public sealed class UIMgr : Singleton<UIMgr>, IRelease
{
    private const string PREFAB_PATH = "Assets/Prefabs/UI/Forms/{0}.prefab";

    public GameObject mFormsUIRoot = null;

    private Dictionary<string, UIBaseFormsRoot> mFormsDic = new Dictionary<string, UIBaseFormsRoot>();

    private int mCurUILayerDepth = (int)UIFormsLayer.CommonUILayer;

    #region CommonUILayer 通用层(1.功能层 2.弹窗层)

    //*********************两种窗口类型:1.基础功能界面窗口 2.可叠加弹窗类型********************//
    //1.基础功能界面窗口:1.分为可以缓存窗口和不可以缓存窗口 2.具有导航功能(自定义栈结构,特殊功能就是可以把栈中的元素Top到栈顶)
    //窗口自定义栈结构--这里用两个字典来管理 string:窗口名 int:窗口在栈中的索引 下表从0开始
    private int mCurStackFormsIndex = 0; //1作为栈的第一个下标

    private Dictionary<string, int> mFormsStackName = new Dictionary<string, int>();

    private Dictionary<int, string> mFormsStackIndex = new Dictionary<int, string>();

    //2.popup窗口类型: 1.在开启一个新 的基础功能窗口时，会清理掉可叠加窗口
    private Stack<string> mPopupFormsStack = new Stack<string>();

    #endregion

    #region 外部接口

    public void Release()
    {
        mCurUILayerDepth = (int)UIFormsLayer.CommonUILayer;
        mFormsDic.Clear();

        mCurStackFormsIndex = 0;
        mFormsStackName.Clear();
        mFormsStackIndex.Clear();

        mPopupFormsStack.Clear();
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="formsName">窗口名字</param>
    /// <param name="parentForms">父节点</param>
    /// <param name="IsAutoDepth">是否自动调整UIPanel的depth</param>
    /// <param name="isDontUnload">是否卸载资源内存</param>
    /// <returns></returns>
    public T ShowForms<T>(string formsName, Transform parentForms = null, bool IsAutoDepth = true, bool isDontUnload = false) where T: UIBaseFormsRoot
    {
        if (string.IsNullOrEmpty(formsName))
            return null;

        UIBaseFormsRoot forms = null;

        if (!mFormsDic.TryGetValue(formsName, out forms))
        {
            forms = _CreateForms<T>(formsName, parentForms, isDontUnload);

            if (forms != null)
            {
                if (!forms.gameObject.activeSelf)
                    forms.gameObject.SetActive(true);

                forms.Init();
                forms.Show();
            }
        }
        else
        {
            if (!forms.gameObject.activeSelf)
                forms.gameObject.SetActive(true);

            forms.Show();
        }

        if (forms != null)
        {
            switch (forms.mFormsLayerType)
            {
                case UIFormsLayer.CommonUILayer:
                    {
                        if (forms.mFormsType == UIFormsType.Normal)
                        {
                            //1.隐藏栈顶forms
                            if (mFormsStackIndex.ContainsKey(mCurStackFormsIndex))
                            {
                                string name = mFormsStackIndex[mCurStackFormsIndex];

                                if (mFormsDic.ContainsKey(name))
                                    mFormsDic[name].Hide();
                            }

                            //2.如果存在栈中且不是位于栈顶(是否考虑栈顶的话就不做后面的操作),需要"Top"到栈顶
                            if (mFormsStackName.ContainsKey(formsName))
                            {
                                if (mFormsStackName[formsName] != mCurStackFormsIndex)
                                {
                                    int tIndex = mFormsStackName[formsName];

                                    for (int i = tIndex; i < mCurStackFormsIndex; i++)
                                    {
                                        mFormsStackIndex[i] = mFormsStackIndex[i + 1];
                                        mFormsStackName[mFormsStackIndex[i]] = i;
                                    }

                                    mFormsStackIndex[mCurStackFormsIndex] = formsName;
                                    mFormsStackName[formsName] = mCurStackFormsIndex;
                                }
                            }
                            else
                                PushForms(formsName);

                            //3.移除Stack类型的窗口
                            if (mPopupFormsStack.Count > 0)
                            {
                                foreach (var item in mPopupFormsStack)
                                    this.DestroyForms(item);

                                mPopupFormsStack.Clear();
                            }

                            //4.重置depth,
                            //这里可以考虑做成:假栈为空的时候，界面只剩下主界面的时候重置depth
                            //目前是做成:只显示一个normal型窗口，底下normal窗口隐藏，所以这里可以重置depth
                            mCurUILayerDepth = (int)UIFormsLayer.CommonUILayer;
                        }
                        else if (forms.mFormsType == UIFormsType.Popup)
                        {
                            if (!mPopupFormsStack.Contains(formsName))
                                mPopupFormsStack.Push(formsName);
                            else
                                Debug.LogWarning("*****************栈窗口已经有此窗口*******************");
                        }

                        if (IsAutoDepth)
                            mCurUILayerDepth = AutoCalculateDepth(forms, mCurUILayerDepth);
                    }
                    break;

                    default:

                    if (IsAutoDepth)
                        AutoCalculateDepth(forms, (int)forms.mFormsLayerType);
                    break;
            }

            return forms as T;
        }

        return null;
    }

    /// <summary>
    /// 关闭单个窗口
    /// </summary>
    public void CloseForms(GameObject go)
    {
        CloseForms(go.name);
    }

    /// <summary>
    /// 关闭单个窗口
    /// </summary>
    public void CloseForms(string formsName)
    {
        if (string.IsNullOrEmpty(formsName))
            return;

        UIBaseFormsRoot forms = null;

        if (mFormsDic.TryGetValue(formsName, out forms))
        {
            switch (forms.mFormsLayerType)
            {
                case UIFormsLayer.CommonUILayer:
                    {
                        if (forms.mFormsType == UIFormsType.Normal)
                        {
                            if (forms.mFormsLifeType == UIFormsLifeType.GoldLife)
                                forms.Hide();
                            else if (forms.mFormsLifeType == UIFormsLifeType.HumanLife)
                                DestroyForms(formsName);

                            PopForms(formsName);

                            // 显示下一层窗口
                            string nextFormsName;
                            mFormsStackIndex.TryGetValue(mCurStackFormsIndex, out nextFormsName);
                            UIBaseFormsRoot nextForms = GetForms(nextFormsName);

                            if (nextForms != null)
                            {
                                if (!nextForms.gameObject.activeSelf)
                                    nextForms.gameObject.SetActive(true);
                            }
                        }
                        else if (forms.mFormsType == UIFormsType.Popup)
                            DestroyForms(formsName);
                    }
                    break;

                default:
                    if (forms.mFormsLifeType == UIFormsLifeType.GoldLife)
                        forms.Hide();
                    else if (forms.mFormsLifeType == UIFormsLifeType.HumanLife)
                        DestroyForms(formsName);
                    break;
            }
        }
    }

    /// <summary>
    /// 获得窗口
    /// </summary>
    /// <param name="formsName"></param>
    /// <returns></returns>
    public UIBaseFormsRoot GetForms(string formsName)
    {
        if (string.IsNullOrEmpty(formsName))
            return null;

        UIBaseFormsRoot forms;

        if (mFormsDic.TryGetValue(formsName, out forms))
            return forms;

        return null;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 创建窗口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="formsName"></param>
    /// <returns></returns>
    private T _CreateForms<T>(string formsName, Transform parentForms = null, bool isDontUnload = false) where T : UIBaseFormsRoot
    {
        GameObject prefabGo = ResourceMgr.Instance.Load(string.Format(PREFAB_PATH, formsName), isDontUnload) as GameObject;

        if (prefabGo == null)
        {
            NIDebug.LogError("**********************路径下没有这个窗体预制体***************************" + formsName);
            return null;
        }

        GameObject go = GameObject.Instantiate(prefabGo) as GameObject;

        if (go == null)
            return null;

        T formsScript = go.GetComponent<T>();

        if (formsScript == null)
            formsScript = go.AddComponent<T>();
        if (formsScript == null)
        {
            NIDebug.LogError("**********************不存在此脚本***************************");
            return null;
        }

        go.name = formsName;
        Transform t = go.transform;

        if (parentForms != null)
            t.parent = parentForms;
        else
        {
            if (mFormsUIRoot != null)
                t.parent = mFormsUIRoot.transform;
        }

        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        mFormsDic[formsName] = formsScript;

        return formsScript;
    }

    /// <summary>
    /// 自动给Forms层级排序
    /// </summary>
    private int AutoCalculateDepth(UIBaseFormsRoot forms, int curDepth)
    {
        UIPanel parentPanel = forms.GetComponent<UIPanel>();
        if (parentPanel == null)
        {
            Debug.LogWarning("=============自动给Forms层级排序========parentPanel是null======");
            return curDepth;
        }

        UIPanel[] childrenPanel = forms.GetComponentsInChildren<UIPanel>(true); //这里的下标0也是parentPanel

        int parentPanelDepth = parentPanel.depth;
        int maxDis = 0;
        for (int i=0; i<childrenPanel.Length; i++)
        {
            int disDepth = childrenPanel[i].depth - parentPanelDepth;
            childrenPanel[i].depth = curDepth + disDepth;
            if (disDepth > maxDis)
                maxDis = disDepth;
        }

        return curDepth + maxDis + 1;
    }

    /// <summary>
    /// 入栈窗口
    /// </summary>
    /// <param name="formsName"></param>
    private void PushForms(string formsName)
    {
        if (!mFormsStackName.ContainsKey(formsName))
        {
            mCurStackFormsIndex++;
            mFormsStackName.Add(formsName, mCurStackFormsIndex);
            mFormsStackIndex.Add(mCurStackFormsIndex, formsName);
        }
        else
            Debug.LogWarning("**************功能窗口栈已有此窗口*****************");
    }

    /// <summary>
    /// 出栈窗口
    /// </summary>
    /// <param name="formsName"></param>
    private void PopForms(string formsName)
    {
        if (!mFormsStackIndex[mCurStackFormsIndex].Equals(formsName))
        {
            Debug.LogWarning("*************此窗口不是栈顶窗口，无法Pop****************");
            return;
        }
        if (mFormsStackName.ContainsKey(formsName))
        {
            mFormsStackIndex.Remove(mFormsStackName[formsName]);
            mFormsStackName.Remove(formsName);
            mCurStackFormsIndex--;
        }
    }

    /// <summary>
    /// 销毁窗口
    /// </summary>
    /// <param name="formsName"></param>
    private void DestroyForms(string formsName)
    {
        if (string.IsNullOrEmpty(formsName))
            return;

        UIBaseFormsRoot forms = null;

        if (mFormsDic.TryGetValue(formsName, out forms))
        {
            forms.End();
            mFormsDic.Remove(formsName);
            GameObject.DestroyImmediate(forms.gameObject);
        }
    }

    #endregion

}
