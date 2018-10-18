/// <summary>
/// SetCharIcon.cs
/// Created by fengsc 2017/11/13
/// 设置角色图片
/// </summary>
using UnityEngine;
using System.Collections;

public class SetCharIcon : MonoBehaviour
{
    private UITexture mIcon;

    private string mPrafabName = "y1";

    void Start()
    {
        Redraw();
    }

    /// <summary>
    /// 绘制界面
    /// </summary>
    void Redraw()
    {
        if (ME.user == null)
            return;

        if (mIcon == null)
        {
            Transform trans = transform.Find(mPrafabName);
            if (trans == null)
                return;

            mIcon = trans.GetComponent<UITexture>();
            if (mIcon == null)
                return;
        }

        int gender = ME.user.Query<int>("gender");

        string path = string.Empty;

        if (CharConst.FEMALE.Equals(gender))
            path = "Assets/Art/UI/Guide/zjv.png";
        else
            path = "Assets/Art/UI/Guide/zjn.png";

        Texture2D tex = ResourceMgr.LoadTexture(path);

        mIcon.mainTexture = tex;
    }
}
