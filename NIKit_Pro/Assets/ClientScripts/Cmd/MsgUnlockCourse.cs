using LPC;
using System.Diagnostics;

/// <summary>
/// 服务器通知历程解锁数据
/// </summary>
public class MsgUnlockCourse : MsgHandler
{
    public string GetName()
    {
        return "msg_unlock_course";
    }

    public void Go(LPCValue para)
    {
        // 记录解锁数据
        GameCourseMgr.AddUnlockNewCourse(ME.user, para.AsMapping.GetValue<int>("course_id"));
    }
}
