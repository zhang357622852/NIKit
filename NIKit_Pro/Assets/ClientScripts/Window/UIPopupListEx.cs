/// <summary>
/// UIPopupListEx.cs
/// Created by fengsc 2017/12/25
/// 下拉列表控件，支持图文混排
/// </summary>
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UIPopupListEx : MonoBehaviour
{
    // value 变化回调
    public delegate void OnValueChange ();
    public static OnValueChange onValueChange;

    /// <summary>
    /// Name of the sprite used to create the popup's background.
    /// </summary>
    public UISprite mBg;

    // Dropdown背景tween动画
    public TweenScale mBgTweenScale;

    /// <summary>
    /// Name of the sprite used to highlight items.
    /// </summary>
    public UISprite mHighlight;

    /// <summary>
    /// 下拉面板
    /// </summary>
    public GameObject mDropDown;

    /// <summary>
    /// 图文混排控件
    /// </summary>
    public RichTextContent mRichText;

    public UITable mTable;

    public RichTextContent mSelectValue;

    int mBgWidthOffset = 24;

    int mBgHeightOffset = 24;

    /// <summary>
    /// New line-delimited list of items.
    /// </summary>

    [HideInInspector]
    public List<string> items = new List<string>();

    // Currently selected item
    string mSelectedItem;

    List<GameObject> mRichTextList = new List<GameObject>();

    bool mIsShow = false;

    GameObject mHoverItem;

    /// <summary>
    /// Current selection.
    /// </summary>
    public virtual string value { get { return mSelectedItem; } set { Set(value); } }

    /// <summary>
    /// Whether the collider is enabled and the widget can be interacted with.
    /// </summary>
    public bool isColliderEnabled
    {
        get
        {
            Collider c = GetComponent<Collider>();

            if (c != null)
                return c.enabled;

            Collider2D b = GetComponent<Collider2D>();

            return (b != null && b.enabled);
        }
    }

    /// <summary>
    /// Set the current selection.
    /// </summary>
    public void Set (string value)
    {
        if (mSelectedItem != value)
        {
            mSelectedItem = value;

            // 清空上一次显示内容
            mSelectValue.clearContent();

            mSelectValue.ParseValue(mSelectedItem);

            // 执行回调
            if (onValueChange != null)
                onValueChange();
        }
    }

    /// <summary>
    /// Clear the popup list's contents.
    /// </summary>
    public void Clear ()
    {
        items.Clear();
    }

    /// <summary>
    /// Add a new item to the popup list.
    /// </summary>
    public void AddItem (string text)
    {
        items.Add(text);
    }

    /// <summary>
    /// Remove the specified item.
    /// </summary>
    public void RemoveItem (string text)
    {
        int index = items.IndexOf(text);

        if (index != -1)
            items.RemoveAt(index);
    }

    /// <summary>
    /// 更新悬浮图片的位置
    /// </summary>
    void UpdateHighlightPosition (Vector3 pos)
    {
        if (mHighlight == null)
            return;

        mHighlight.transform.localPosition = pos;
    }

    /// <summary>
    /// Event function triggered when the mouse hovers over an item.
    /// </summary>
    void OnItemHover (GameObject go, bool isOver)
    {
        if (isOver)
        {
            // go相对于mDropDown的位置
            Vector3 pos1 = mDropDown.transform.InverseTransformPoint(go.transform.position);

            // 更新位置
            UpdateHighlightPosition(new Vector3(mHighlight.transform.localPosition.x, pos1.y, 0));

            mHoverItem = go;
        }
    }

    /// <summary>
    /// Event function triggered when the drop-down list item gets clicked on.
    /// </summary>
    void OnItemClick (GameObject go)
    {
        // go相对于mDropDown的位置
        Vector3 pos1 = mDropDown.transform.InverseTransformPoint(go.transform.position);

        // 更新位置
        UpdateHighlightPosition(new Vector3(mHighlight.transform.localPosition.x, pos1.y, 0));

        UIEventListener listener = go.GetComponent<UIEventListener>();
        value = listener.parameter as string;

        CloseSelf();
    }

    /// Close the popup list when disabled.
    /// </summary>
    void OnDisable ()
    {
        CloseSelf();
    }

    /// <summary>
    /// 播放tween动画
    /// </summary>
    void PlayTweenAnimation()
    {
        mBgTweenScale.ResetToBeginning();

        mBgTweenScale.PlayForward();
    }

    /// <summary>
    /// Manually close the popup list.
    /// </summary>
    public void CloseSelf ()
    {
        mDropDown.SetActive(false);

        mHighlight.gameObject.SetActive(false);

        mBg.gameObject.SetActive(false);

        mHoverItem = null;

        mIsShow = false;

        for (int i = 0; i < mRichTextList.Count; i++)
        {
            Destroy(mRichTextList[i]);
        }

        mRichTextList.Clear();
    }

    /// <summary>
    /// Display the drop-down list when the game object gets clicked on.
    /// </summary>
    void OnClick ()
    {
        if (mIsShow)
        {
            OnItemClick(mHoverItem);

            CloseSelf();
        }
        else
        {
            Show();
        }
    }

    public void SetSelectValuePos(Vector3 pos)
    {
        if (mSelectValue == null)
            return;

        mSelectValue.transform.localPosition = pos;
    }

    /// <summary>
    /// Show the popup list dialog.
    /// </summary>
    public void Show ()
    {
        if (mIsShow || items.Count == 0)
            return;

        mRichTextList.Clear();

        mDropDown.SetActive(true);

        if (mDropDown.GetComponent<Collider>() != null)
        {
            Rigidbody rb = mDropDown.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        else if (mDropDown.GetComponent<Collider2D>() != null)
        {
            Rigidbody2D rb = mDropDown.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
        }

        var panel = mDropDown.GetComponent<UIPanel>();
        panel.depth = 1000000;

        // Clear the selection if it's no longer present
        if (!items.Contains(mSelectedItem))
            mSelectedItem = null;

        mRichText.gameObject.SetActive(false);

        // 总宽度
        float width = 0f;

        // 总高度
        float height = 0f;

        Vector3 hightLight = Vector3.zero;

        float xOffset = 0f;

        // Run through all items and create labels for each one
        for (int i = 0, imax = items.Count; i < imax; ++i)
        {
            string s = items[i];

            GameObject clone = Instantiate(mRichText.gameObject);

            clone.transform.SetParent(mTable.transform);

            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = Vector3.zero;

            clone.name = i.ToString();

            clone.SetActive(true);

            // Add an event listener
            UIEventListener listener = UIEventListener.Get(clone.gameObject);
            listener.onHover = OnItemHover;
            listener.onClick = OnItemClick;
            listener.parameter = s;

            RichTextContent rtc = clone.GetComponent<RichTextContent>();
            if (rtc == null)
                continue;

            // 绑定数据
            rtc.ParseValue(s);

            List<Transform> list = rtc.currentLineThing;

            UISprite icon = list[0].GetComponent<UISprite>();

            UILabel content = list[1].GetComponent<UILabel>();

            if (i == 0)
                width = icon.width + content.width + rtc.WorldSpace;

            // 设置碰撞盒的大小和位置
            BoxCollider bc = clone.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.size = new Vector3(width, icon.height, 0);

                bc.center = new Vector3(content.width / 2 + icon.width / 2 + rtc.WorldSpace / 2 + mBgWidthOffset, 0, 0);
            }

            height += icon.height;

            // Move the selection here if this is the right label
            if (mSelectedItem == s || (i == 0 && string.IsNullOrEmpty(mSelectedItem)))
            {
                // 计算悬浮图片的大小
                hightLight.x = content.width + rtc.WorldSpace;

                hightLight.y = icon.height;

                xOffset = content.transform.localPosition.x + content.width / 2;
            }

            // Add this label to the list
            mRichTextList.Add(clone);
        }

        // 设置背景的大小
        mBg.width = Mathf.RoundToInt(width) + mBgWidthOffset;

        mBg.height = Mathf.RoundToInt(height) + Mathf.RoundToInt(mTable.padding.y * (mRichTextList.Count - 1)) + mBgHeightOffset;

        mBg.gameObject.SetActive(true);

        // 播放动画
        PlayTweenAnimation();

        mTable.repositionNow = true;

        mIsShow = true;

        mTable.transform.localPosition = new Vector3(mTable.transform.localPosition.x, mBg.transform.localPosition.y - mBg.height / 2);

        Coroutine.StopCoroutine("SetHightLight");

        Coroutine.DispatchService(SetHightLight(xOffset, hightLight), "SetHightLight");
    }

    IEnumerator SetHightLight(float xOffset, Vector3 hightLight)
    {
        yield return null;

        GameObject go = mRichTextList[0];

        mHoverItem = go;

        int widthOffset = 45;

        int heightOffset = 20;

        mHighlight.width = (int) hightLight.x + widthOffset;

        mHighlight.height = (int) hightLight.y + heightOffset;

        // go相对于mDropDown的位置
        Vector3 pos1 = mDropDown.transform.InverseTransformPoint(go.transform.position);

        // 更新位置
        UpdateHighlightPosition(new Vector3(xOffset + pos1.x, pos1.y, 0));

        mHighlight.gameObject.SetActive(true);
    }
}
