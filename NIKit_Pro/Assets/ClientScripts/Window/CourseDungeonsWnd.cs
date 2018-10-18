/// <summary>
/// CourseDungeonsWnd.cs
/// Created by fengsc 2018/08/18
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class CourseDungeonsWnd : WindowBase<CourseDungeonsWnd>
{
    #region 成员变量

    // 分页标题
    public UILabel mTitle;

    // 小标题
    public UILabel[] mSubTitle;

    // 历程描述
    public UILabel[] mCourseDesc;

    public UILabel[] mTips;

    public MonsterInfo[] mMonsterInfo;

    // 分页
    private int mPage;

    private Property mWho;

    private List<Property> mObList = new List<Property>();

    #endregion

    [System.Serializable]
    public class MonsterInfo
    {
        public List<PetItemWnd> mPetItemWnd;
    }

    void Awake()
    {
        for (int i = 0; i < mTips.Length; i++)
            mTips[i].text = LocalizationMgr.Get("GameCourseWnd_3");
    }

    void OnDestroy()
    {
        foreach (Property ob in mObList)
        {
            // 销毁克隆的使魔对象
            if (ob != null)
                ob.Destroy();
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 获取指定分页数据
        Dictionary<int, List<CsvRow>> pageDic = GameCourseMgr.GetCourseListByPage(mPage);

        int i = 0;
        foreach (List<CsvRow> list in pageDic.Values)
        {
            string courseDes = string.Empty;

            string color = string.Empty;

            int unlock = 0;

            for (int j = 0; j < list.Count; j++)
            {
                int courseId = list[j].Query<int>("course_id");

                if (GameCourseMgr.GetCourseData(mWho, courseId).Count == 0)
                {
                    // 未解锁
                    color = "AB9363";
                }
                else
                {
                    color = "553500";

                    unlock++;
                }

                // 历程描述
                courseDes += string.Format("[{0}]{1}[-]", color, GameCourseMgr.GetDesc(mWho, courseId) + "\n");
            }

            // 小标题
            if (i + 1 <= mSubTitle.Length)
            {
                if (unlock == 0)
                    color = "AB9363";
                else
                    color = "553500";

                mSubTitle[i].text = string.Format("[{0}]{1}[-]", color, GameCourseMgr.GetSubTitle(mWho, list[0].Query<int>("course_id")));
            }

            // 历程描述
            if(i + 1 <= mCourseDesc.Length)
                mCourseDesc[i].text = courseDes;

            if (i + 1 <= mMonsterInfo.Length)
            {
                // 使魔列表
                List<PetItemWnd> petList = mMonsterInfo[i].mPetItemWnd;

                LPCArray courseData = GameCourseMgr.GetCourseData(mWho, list[0].Query<int>("course_id"));

                LPCArray pets = LPCArray.Empty;

                if (courseData != null && courseData.Count != 0)
                {
                    if(i + 1 <= mTips.Length)
                        mTips[i].gameObject.SetActive(true);

                    pets = courseData[3].AsArray;
                }
                else
                {
                    if(i + 1 <= mTips.Length)
                        mTips[i].gameObject.SetActive(false);
                }

                for (int k = 0; k < petList.Count; k++) 
                {
                    if (k + 1 <= pets.Count && pets[k].IsInt)
                    {
                        petList[k].gameObject.SetActive(true);

                        UIEventListener.Get(petList[k].gameObject).onClick = OnClickPetItem;

                        LPCMapping para = LPCMapping.Empty;
                        para.Add("class_id", pets[k].AsInt);
                        para.Add("rid", Rid.New());

                        // 创建使魔对象
                        Property ob = PropertyMgr.CreateProperty(para);

                        // 缓存使魔对象
                        mObList.Add(ob);

                        // 绑定使魔对象
                        petList[k].SetBind(ob);
                        petList[k].ShowLevel(false);
                    }
                    else
                    {
                        petList[k].gameObject.SetActive(false);
                    }
                }
            }

            i++;
        }
    }

    /// <summary>
    /// 使魔格子点击事件
    /// </summary>
    void OnClickPetItem(GameObject go)
    {
        GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType);
        if (wnd == null)
            return;

        PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();
        if (script == null)
            return;

        // 绑定数据
        script.Bind(go.GetComponent<PetItemWnd>().item_ob, false);

        script.ShowBtn(true, false);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int page, Property who)
    {
        mPage = page;

        mWho = who;

        mTitle.text = GameCourseMgr.GetPageTitle(mWho, page);

        // 绘制窗口
        Redraw();
    }
}
