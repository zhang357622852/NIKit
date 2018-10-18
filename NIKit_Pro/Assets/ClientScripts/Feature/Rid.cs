/// <summary>
/// Rid.cs
/// Copy from zhangyg 2014-10-22
/// RID特性
/// </summary>

using LPC;
using System.Diagnostics;
using System.Collections.Generic;

/// <summary>
/// RID特性
/// </summary>
public class Rid
{
    // 必然从属于某个property
    private Property owner;

    /// <summary>
    /// 角色对象的rid
    /// </summary>
    private string mRid;

    // rid与对象的映射表
    private static Dictionary<string, Property> ridObs = new Dictionary<string, Property>();

    // Encode 32
    static string _encodeMap = "0123456789ACDEFGHJKLMNPQRSTUWXYZ";
    static int _ridSequence = 0;
    static int _lastRidTime = 0;

    public static string New()
    {
        if(_lastRidTime == 0)
            _lastRidTime = (int)System.DateTime.Now.Ticks;

        _ridSequence++;
        _ridSequence &= 0x3FFFF;
        if(_ridSequence == 0)
            /* Round, do carry */
            _lastRidTime++;

        int ti = (int)System.DateTime.Now.Ticks;
        if(ti > _lastRidTime)
            _lastRidTime = ti;

        _lastRidTime -= 1174527508; /* Mar/22/2007 9:38 */
        int serverId = 1;
        string rid = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
           (char)_encodeMap[(_lastRidTime >> 25) & 0x1F],
           (char)_encodeMap[(_lastRidTime >> 20) & 0x1F],
           (char)_encodeMap[(_lastRidTime >> 15) & 0x1F],
           (char)_encodeMap[(_lastRidTime >> 10) & 0x1F],
           (char)_encodeMap[(_lastRidTime >> 5) & 0x1F],
           (char)_encodeMap[(_lastRidTime) & 0x1F],  /* Time */

           (char)_encodeMap[(serverId >> 7) & 0x1F],
           (char)_encodeMap[(serverId >> 2) & 0x1F],       /* ServerId[2..11] */

           (char)_encodeMap[((serverId << 3) & 0x18) | (_ridSequence >> 15) & 0x7], /* ServerId[0..2] RID_SEQ[15..17] */

           (char)_encodeMap[(_ridSequence >> 10) & 0x1F],
           (char)_encodeMap[(_ridSequence >> 5) & 0x1F],
           (char)_encodeMap[(_ridSequence) & 0x1F]); /* RID_SEQ */
        return rid;
    }

    public Rid(Property property, LPCMapping data)
    {
        this.owner = property;

        // 数据不能为空
        if(data != null && data["dbase"] != null && data["dbase"].IsMapping)
        {
            // 获取data数据
            LPCMapping dData = data["dbase"].AsMapping;
            
            // 设置rid
            if(dData.ContainsKey("rid"))
                SetRid(dData["rid"].AsString);
        }
    }

    public void Destroy()
    {
        // 取消RID与物件的映射关系
        if(GetRid() != null)
            RemoveRidObject(GetRid());
    }

    /// <summary>
    /// 设置物件的RID
    /// </summary>
    public void SetRid(string rid)
    {
        Debug.Assert(owner.dbase.Query("rid") == null || owner.dbase.Query("rid").Equals(rid));

        // 先取消旧的映射关系
        if(GetRid() != null)
            RemoveRidObject(GetRid());

        // 记录当前rid
        mRid = rid;

        // 记录dbase数据
        owner.dbase.Set("rid", rid);

        // 设隐藏关系
        SetRidObject(rid, owner);
    }

    /// <summary>
    /// 取得RID
    /// </summary>
    public string GetRid()
    {
        return mRid;
    }

    /// <summary>
    /// 设置rid与物件的映射关系
    /// </summary>
    public static void SetRidObject(string rid, Property o)
    {
        ridObs[rid] = o;
    }

    /// <summary>
    /// 取消rid与物件的映射关系
    /// </summary>
    public static void RemoveRidObject(string rid)
    {
        if(ridObs.ContainsKey(rid))
            ridObs.Remove(rid);
    }

    /// <summary>
    /// 根据RID取得物件对象
    /// </summary>
    public static Property FindObjectByRid(string rid)
    {
        // 如果是无效rid，直接返回null
        if (string.IsNullOrEmpty(rid))
            return null;

        Property ob;

        // 在列表中查找不变对象
        if (ridObs.TryGetValue(rid, out ob))
            return ob;

        // 没有该rid的目标对象返回null
        return null;
    }

    /// <summary>
    /// 取得所有的物件信息
    /// </summary>
    public static Dictionary<string, Property> objects
    {
        get { return ridObs; }
    }
}
