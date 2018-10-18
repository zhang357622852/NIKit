/// <summary>
/// ColorChanger.cs
/// Created by wangxw 2014-11-27
/// 模型变色器
/// A：基础色变色，直接改SpriteRenderer的Color值
/// B：绝对色变色，使用BrushWhiteColor材质变色
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;

// 变色类型
public class ColorChangerType
{
    // 基础色变色，叠加效果，白色无变化
    public const int CCT_BASE = 0;

    // 绝对色变色，白色为完全变白
    public const int CCT_BRUSH = 1;

    /// <summary>
    /// 根据别名获取ColorChangerType
    /// </summary>
    public static int GetTypeByAlias(string typeAlias)
    {
        // 状态
        if (string.Equals(typeAlias, "CCT_BRUSH"))
            return CCT_BRUSH;

        // 动作
        return CCT_BASE;
    }
}

public class ColorChanger
{
    #region 成员

    // 材质
    public static readonly Material BrushMaterial = ResourceMgr.Load("Assets/Art/Material/BrushWhiteColor.mat") as UnityEngine.Material;

    public static readonly Material BaseMaterial = new Material(Shader.Find("Sprites/Default"));

    #endregion

    #region 属性

    // 变色ID号
    public string CCid { get; private set; }

    // 目标颜色
    public Color TargetColor { get; set; }

    // 变色类型
    public int CCType { get; private set; }

    /// <summary>
    /// 获取基础值
    /// 这个值会被引用，不能使用静态变量来定义，所以写成一个静态函数
    /// </summary>
    public static ColorChanger BaseColor{ get { return new ColorChanger(CombatConfig.CCID_BASE, Color.white); } }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="id">唯一编号</param>
    /// <param name="color">目标颜色</param>
    /// <param name="ccType">变色类型</param>
    public ColorChanger(string id, Color color, int t = ColorChangerType.CCT_BASE)
    {
        CCid = id;
        TargetColor = color;
        CCType = t;
    }

    /// <summary>
    /// 对目标执行变色
    /// </summary>
    /// <param name="gob">目标对象</param>
    /// <param name="alpha">额外alpha，和原有a相乘</param>
    public void DoChange(GameObject gob, float alpha = 1f)
    {
        // 变色对象不存在
        if (gob == null)
        {
            LogMgr.Trace("变色的目标对象消失。");
            return;
        }

        // 骨骼动画
        SkeletonRenderer skeletonRender = gob.GetComponent<SkeletonRenderer>();
        if (skeletonRender != null)
        {
            switch (CCType)
            {            
                case ColorChangerType.CCT_BASE:
                    // 基础变色，替换材质叠加色
                    skeletonRender.skeleton.SetColor(new Color(TargetColor.r, TargetColor.g, TargetColor.b, TargetColor.a * alpha));
                    break;
                case ColorChangerType.CCT_BRUSH:
                    // 刷版变色类型
                    skeletonRender.skeleton.SetSkin("white");
                    break;
            }

            return;
        }

        // 获取渲染对象
        SpriteRenderer render = gob.GetComponent<SpriteRenderer>();
        if (render != null)
        {

            switch (CCType)
            {
                case ColorChangerType.CCT_BASE:
                // 基础变色，替换材质叠加色
                    render.material = BaseMaterial;
                    render.color = new Color(TargetColor.r, TargetColor.g, TargetColor.b, TargetColor.a * alpha);
                    break;

                case ColorChangerType.CCT_BRUSH:
                // 刷版变色类型
                    render.material = BrushMaterial;
                    break;
            }

            return;
        }
    }
}
