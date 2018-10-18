/// <summary>
/// TowerSceneItem.cs
/// Created by fengsc 2017/08/31
/// 通天之层数格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;
using TMPro;

public class TowerSceneItem : MonoBehaviour
{
    // 窗口点击回调
    public delegate void OnClickWndDelegate(GameObject ob);

    public OnClickWndDelegate OnClick { get; set; }

    private SpriteRenderer mBg;

    private TextMeshPro mLayerText;

    private TweenPosition mArrowTween;

    private TweenAlpha mTweenAlpha;

    // 通天塔副本配置数据
    private CsvRow mRow;

    private int mCurDifficulty;

    void Awake()
    {
        mBg = GetComponent<SpriteRenderer>();

        Transform layerTrans = transform.Find("layer");
        if (layerTrans != null)
            mLayerText = layerTrans.GetComponent<TextMeshPro>();

        Transform arrowTrans = transform.Find("arrow");
        if (arrowTrans != null)
            mArrowTween = arrowTrans.GetComponent<TweenPosition>();

        mTweenAlpha = GetComponent<TweenAlpha>();
        if (mTweenAlpha != null)
            mTweenAlpha.AddOnFinished(OnTweenFinish);
    }

    void OnEnable()
    {
        // 重绘窗口
        Redraw();
    }

    void OnDisable()
    {
        SetAlpha(0);
    }

    void OnTweenFinish()
    {
        SetAlpha(1);
    }

    /// <summary>
    /// 设置子物体的透明度
    /// </summary>
    void SetAlpha(float alpha)
    {
        mLayerText.alpha = alpha;

        if (mArrowTween != null)
        {
            SpriteRenderer sp = mArrowTween.GetComponent<SpriteRenderer>();
            if (sp != null)
            {
                Color color = sp.color;

                color.a = alpha;

                sp.color = color;
            }
        }

        Transform bonusWnd = transform.Find("TowerBonusWnd");
        if (bonusWnd == null)
            return;

        SpriteRenderer bg = bonusWnd.Find("bg").GetComponent<SpriteRenderer>();
        if (bg != null)
        {
            Color bgColor = bg.color;

            bgColor.a = alpha;

            bg.color = bgColor;
        }

        SpriteRenderer icon = bonusWnd.Find("icon").GetComponent<SpriteRenderer>();
        if (icon != null)
        {
            Color iconColor = icon.color;

            iconColor.a = alpha;

            icon.color = iconColor;
        }

        TextMeshPro text = bonusWnd.Find("text").GetComponent<TextMeshPro>();
        if (text != null)
            text.alpha = alpha;
    }

    // 重绘界面
    void Redraw()
    {
        mLayerText.gameObject.SetActive(false);

        if (mArrowTween != null)
            mArrowTween.gameObject.SetActive(false);

        if (mRow == null)
            return;

        // 当前层数
        int intLayer = mRow.Query<int>("layer") + 1;

        // 10的倍数为boss层
        if (intLayer % 10 == 0)
        {
            // 显示奖励物品
            ShowBonus();
        }

        string layer = intLayer.ToString();

        mLayerText.text = layer;

        string spriteName = string.Empty;

        if (TowerMgr.IsClearanced(ME.user, mCurDifficulty, intLayer - 1))
        {
            // 当前副本通关
            spriteName = "tower_ready_";

            mLayerText.gameObject.SetActive(true);
        }
        else if (TowerMgr.IsUnlocked(ME.user, mCurDifficulty, intLayer - 1))
        {
            // 当前副本解锁
            spriteName = "tower_clear_";

            // 播放tween动画
            if (mArrowTween != null)
            {
                mArrowTween.gameObject.SetActive(true);

                mArrowTween.PlayForward();
            }
        }
        else
        {
            // 没有解锁
            spriteName = "tower_lock_";
        }

        int mantissa = 0;

        if (! int.TryParse(layer.Substring(layer.Length - 1, 1), out mantissa))
            return;

        if (mantissa == 1 || mantissa == 5)
        {
            spriteName += "m";

            mLayerText.rectTransform.localPosition = new Vector3(0.01f, -0.03f, 0);

            mLayerText.fontSize = 4;
        }
        else if (mantissa == 0)
        {
            spriteName += "l";

            mLayerText.rectTransform.localPosition = new Vector3(0.01f, -0.02f, 0);

            mLayerText.fontSize = 5;
        }
        else
        {
            spriteName += "s";

            mLayerText.rectTransform.localPosition = new Vector3(0.015f, 0.02f, 0);

            mLayerText.fontSize = 3;
        }

        Sprite sprite = ResourceMgr.LoadSprite(string.Format("Assets/Art/Scene/tower/{0}.png", spriteName));
        if (sprite == null)
            return;

        mBg.sprite = sprite;
    }

    /// <summary>
    /// 显示奖励物品
    /// </summary>
    void ShowBonus()
    {
        GameObject clone = null;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform trans = transform.GetChild(i);
            if (trans == null)
                continue;

            if (!trans.name.Equals(string.Format("TowerBonusWnd")))
                continue;

            clone = trans.gameObject;

            break;
        }

        GameObject wnd;

        if (clone == null)
        {
            GameObject prefab = ResourceMgr.Load("Assets/Prefabs/Scene/TowerBonusWnd.prefab") as GameObject;
            if (prefab == null)
                return;

            wnd = Instantiate(prefab);
        }
        else
            wnd = clone;

        // 设置当前游戏物体为父级
        wnd.transform.SetParent(transform);

        wnd.transform.localPosition = new Vector3(0.6f, 0, 0);

        wnd.transform.localRotation = Quaternion.Euler(Vector3.zero);

        wnd.transform.localScale = Vector3.one;

        wnd.name = "TowerBonusWnd";

        TowerBonusWnd script = wnd.GetComponent<TowerBonusWnd>();
        if (script == null)
            return;

        // 奖励数据
        LPCMapping bonus = TowerMgr.GetBonusByLayer(mRow.Query<int>("difficulty"), mRow.Query<int>("layer"));

        // 绑定数据
        script.Bind(bonus);

        // 激活gameobject
        wnd.SetActive(true);
    }

    /// <summary>
    /// Raises the click window event.
    /// </summary>
    public void OnClickWnd()
    {
        // 没有解锁
        if (!TowerMgr.IsUnlocked(ME.user, mCurDifficulty, mRow.Query<int>("layer")))
            return;

        // 打开选择战斗界面
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        SelectFighterWnd script = wnd.GetComponent<SelectFighterWnd>();
        if (script == null)
            return;

        script.TowerBind(TowerWnd.WndType, mRow.Query<string>("instance_id"), mRow.Query<int>("layer"), mCurDifficulty);

        // 关闭通天塔窗口
        WindowMgr.DestroyWindow(TowerWnd.WndType);

        if (OnClick == null)
            return;

        // 执行点击回调
        OnClick(gameObject);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(CsvRow data, int diffculty)
    {
        // 通天塔难度
        mCurDifficulty = diffculty;

        if (data == null)
            return;

        mRow = data;

        Redraw();
    }
}
