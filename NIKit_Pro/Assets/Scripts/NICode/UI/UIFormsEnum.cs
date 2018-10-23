using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UIFormsType
{
    Normal = 1, //基础功能界面窗口
    Popup = 2,  //弹窗类型
}

public enum UIFormsLifeType
{ 
    //频繁打开的窗口(背包，英雄界面)此种窗口需要缓存,关闭此种窗口，就是隐藏，没有销毁
    GoldLife = 1,
    //无需缓存的窗口,关闭此种窗口，就是销毁
    HumanLife,
}

public enum UIFormsLayer
{
    MainUILayer = 1, //主界面层
    CommonUILayer = 1000, //通用层(1.功能层 2.弹窗层)
    RewardLayer = 2000, //物品奖励层
    TipLayer = 3000, //Tip层
    NotifyLayer = 4000, //跑马灯层(全服公告内容等等)
    GuideLayer = 5000, //引导层
    LoadingLayer = 7000, //Loading加载层
    ServerLayer = 10000, //网络转菊花层     
}