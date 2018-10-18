/// <summary>
/// CoursePlanWnd.cs
/// Created by fengsc 2018/08/18
/// 我的计划窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoursePlanWnd : WindowBase<CoursePlanWnd>
{
    #region 成员变量

    // 分页标题
    public UILabel mTitle;

    // 小标题
    public UILabel[] mSubTitle;

    // 历程描述
    public UILabel[] mCourseDesc;

    // 分页
    private int mPage;

    private Property mWho;

    #endregion

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

            i++;
        }
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
