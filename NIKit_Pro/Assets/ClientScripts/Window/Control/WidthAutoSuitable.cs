/// <summary>
/// WidthAutoSuitable.cs
/// Created by xuhd Jan/17/2015
/// 宽度自适应
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WidthAutoSuitable : MonoBehaviour
{
    // 拉伸因子
    public static float ScaleFactor = 1.0f;

    void Awake()
    {
        UIRoot root = WindowMgr.UIRoot.GetComponent<UIRoot>();
        if (root == null)
        {
            LogMgr.Trace("UIRoot是null");
            return;
        }

        float s = (float)root.activeHeight / Screen.height;
        int screenWidth = Mathf.CeilToInt(Screen.width * s);

        // 拉伸比例
        float scaleFactor = (float)screenWidth / 720;

        transform.localScale *= scaleFactor;

        // 位置调整
        UIPanel panel = GetComponent<UIPanel>();
        if (panel == null)
        {
            LogMgr.Trace("窗口没有panel");
            return;
        }

        // 由于scale是使GameObject由中心往四个方向等长度拉伸
        // 所以x方向的位置不需要移动
        // y方向移动高度变化的1/2，使得拉伸效果为：底边不动，向上拉伸
        float yDistance = panel.height * (scaleFactor - 1.0f) / 2.0f;

        transform.localPosition += new Vector3(0.0f, yDistance, 0.0f);

        // 接下来要调整panel，使其裁剪区域也相应变小

        // 记录panel的原始中心点位置，用于后面计算设置新的中心点
        float oldCenterX = panel.baseClipRegion.x;
        float oldCenterY = panel.baseClipRegion.y;

        // 计算panel的y方向长度变化
        float yPanelDistance = panel.height * (1.0f - 1.0f / scaleFactor);

        // 设置panel的偏移，原理与设置Transform偏移类似
        panel.SetRect(oldCenterX, oldCenterY - yPanelDistance / 2.0f, panel.width, panel.height / scaleFactor);

        // 记录拉伸因子
        ScaleFactor = scaleFactor;
    }
}
