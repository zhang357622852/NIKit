/// <summary>
/// LandaTaskGotoWnd.cs
/// Created by fengsc 2018/04/02
/// 兰达平原任务前往窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class LandaTaskGotoWnd : WindowBase<LandaTaskGotoWnd>
{
    // 窗口标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    public UILabel mConditionTips;

    // 任务描述
    public UILabel mTaskDesc;

    public UILabel mInstructionTips;

    // 帮助查看
    public UILabel mHelpBtn;

    public UILabel mBonusTips;

    public PetItemWnd mPetItemWnd;

    // 宠物名称
    public UILabel mPetName;

    // 使魔元素
    public UISprite mElement;

    // 前往按钮
    public GameObject mGotoBtn;
    public UILabel mGotoBtnLb;

    public UILabel mTips;

    public UILabel mTips1;

    private int mMapId = 0;

    private int mTaskId = 0;

    Property mOb = null;

    void Start()
    {
        TweenScale mTweenScale = GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);

        // 初始化本地化文本
        InitText();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();
    }

    void OnDestroy()
    {
        // 销毁宠物对象
        if (mOb != null)
            mOb.Destroy();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("LandaTaskGotoWnd_1");
        mConditionTips.text = LocalizationMgr.Get("LandaTaskGotoWnd_2");
        mInstructionTips.text = LocalizationMgr.Get("LandaTaskGotoWnd_3");
        mTips1.text = LocalizationMgr.Get("LandaTaskGotoWnd_5");
        mHelpBtn.text = LocalizationMgr.Get("LandaTaskGotoWnd_6");
        mBonusTips.text = LocalizationMgr.Get("LandaTaskGotoWnd_7");
        mGotoBtnLb.text = LocalizationMgr.Get("LandaTaskGotoWnd_8");
        mTips.text = LocalizationMgr.Get("LandaTaskGotoWnd_9");
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 帮助按钮点击回调
    /// </summary>
    void OnClickHelpBtn(GameObject go)
    {
        // 打开帮助窗口
        GameObject mHelpWnd = WindowMgr.OpenWnd("HelpWnd");

        mHelpWnd.GetComponent<HelpWnd>().Bind(HelpConst.MAP_ID);
    }

    /// <summary>
    /// 前往按钮点击回调
    /// </summary>
    void OnClickGotoBtn(GameObject go)
    {
        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnWorldMaskCallBack));

        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OnEnterWorldMapScene));
    }

    void OnWorldMaskCallBack(object para, params object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OnEnterWorldMapScene));
    }

    /// <summary>
    /// 地图场景加载完成会掉
    /// </summary>
    private void OnEnterWorldMapScene(object para, object[] param)
    {
        WindowMgr.HideMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        //获取副本选择界面;
        GameObject selectInstanceWnd = WindowMgr.OpenWnd(SelectInstanceWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (selectInstanceWnd == null)
            return;

        SelectInstanceWnd selectInstanceScript = selectInstanceWnd.GetComponent<SelectInstanceWnd>();

        if (selectInstanceScript == null)
            return;

        // 地图配置数据获取失败
        CsvRow row = MapMgr.GetMapConfig(mMapId);
        if (row == null)
            return;

        LPCArray pos = row.Query<LPCArray>("pos");

        // 绑定数据
        selectInstanceScript.Bind(mMapId, new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat));

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 使魔格子点击回调
    /// </summary>
    void OnClickPetItem(GameObject go)
    {
        // 打开使魔弹框
        GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType);
        if (wnd == null)
            return;

        // 获取脚本对象
        PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

        // 绑定数据
        script.Bind(mOb);
        script.ShowBtn(true);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mHelpBtn.gameObject).onClick = OnClickHelpBtn;
        UIEventListener.Get(mGotoBtn).onClick = OnClickGotoBtn;
        UIEventListener.Get(mPetItemWnd.gameObject).onClick = OnClickPetItem;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 副本列表
        List<string> instanceList = InstanceMgr.GetDifficultyInstanceByMapId(mMapId, InstanceConst.INSTANCE_DIFFICULTY_EASY);

        // 通关数量
        int clearanceAmount = 0;

        for (int i = 0; i < instanceList.Count; i++)
        {
            // 累计通关副本数量
            if (InstanceMgr.IsClearanced(ME.user, instanceList[i]))
                clearanceAmount++;
        }

        // 任务描述
        mTaskDesc.text = string.Format(LocalizationMgr.Get("LandaTaskGotoWnd_4"), clearanceAmount, instanceList.Count);

        // 任务奖励列表
        LPCArray array = TaskMgr.GetBonus(ME.user, mTaskId);
        if (array.Count == 0)
            return;

        LPCMapping data = array[0].AsMapping;

        data.Add("rid", Rid.New());

        if (mOb != null)
            mOb.Destroy();

        // 创建宠物对象
        mOb = PropertyMgr.CreateProperty(data);
        if (mOb == null)
            return;

        // 绑定数据
        mPetItemWnd.SetBind(mOb);

        int classId = mOb.GetClassID();

        // 显示使魔元素图标
        mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(classId));

        mPetName.text = MonsterMgr.GetName(classId, mOb.GetRank());
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int mapId, int taskId)
    {
        mMapId = mapId;

        mTaskId = taskId;
    }
}
