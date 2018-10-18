using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class FlagItemWnd : WindowBase<FlagItemWnd>
{
    public UITexture mBaseIcon;

    public UITexture mIcon;

    public UITexture mStyleIcon;

    LPCArray mFlag = LPCArray.Empty;

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mIcon.gameObject.SetActive(false);
        mStyleIcon.gameObject.SetActive(false);

        if (mFlag == null || mFlag.Count == 0)
        {
            mBaseIcon.color = Color.HSVToRGB(0, 0, 1);

            return;
        }

        mIcon.gameObject.SetActive(true);
        mStyleIcon.gameObject.SetActive(true);

        // 图标路径
        string path = "Assets/Art/UI/Icon/gang/{0}.png";

        // 底图
        LPCArray baseData = mFlag[0].AsArray;

        float baseH = baseData[1].AsFloat / 255f;

        float baseS = baseData[2].AsFloat / 255f;

        mBaseIcon.mainTexture = ResourceMgr.LoadTexture(string.Format(path, baseData[0].AsString));

        // HSV
        mBaseIcon.color = Color.HSVToRGB(baseH, baseS, 1);

        // 图标
        LPCArray iconData = mFlag[1].AsArray;

        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format(path, iconData[0].AsString));

        // 样式
        LPCArray styleData = mFlag[2].AsArray;

        float styleV = styleData[1].AsFloat / 255f;

        mStyleIcon.mainTexture = ResourceMgr.LoadTexture(string.Format(path, styleData[0].AsString));

        // 设置图片的明暗
        mStyleIcon.color = Color.HSVToRGB(0, 0, styleV);
    }

    public void Bind(LPCArray flag)
    {
        mFlag = flag;

        // 绘制窗口
        Redraw();
    }
}
