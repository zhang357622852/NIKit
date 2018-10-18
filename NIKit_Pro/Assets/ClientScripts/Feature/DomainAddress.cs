/// <summary>
/// DomainAddress.cs
/// Copy from zhangyg 2014-10-22
/// 域属性
/// </summary>

using System.Diagnostics;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 域属性 
/// </summary>
public class DomainAddress
{
    private Property owner;
    
    // domain - property映射表
    private static Dictionary<string, Property> domainObs = new Dictionary<string, Property>();

    public DomainAddress(Property property, LPCMapping data)
    {
        this.owner = property;

        // 设置域名
        if (data["domain_address"] != null && data["domain_address"].IsString)
            SetDomainAddress(data["domain_address"].AsString);
    }

    public void Destroy()
    {
        string domain = GetDomainAddress();
        if (domain != null)
            // 取消域名与对象的映射
            RemoveDomainObject(domain);
    }

    /// <summary>
    /// 设置域名 
    /// </summary>
    public void SetDomainAddress(string domain)
    {
        Debug.Assert(owner.dbase.Query("domain_address") == null ||
            owner.dbase.Query("domain_address").Equals(domain));

        if (GetDomainAddress() != null)
            // 先取消旧的域对象映射
            RemoveDomainObject(GetDomainAddress());

        owner.dbase.Set("domain_address", domain);
        SetDomainObject(domain, owner);
    }

    /// <summary>
    /// 获取对象的域名 
    /// </summary>
    public string GetDomainAddress()
    {
        LPCValue v = owner.dbase.Query("domain_address");
        if (v == null || !v.IsString)
            return null;
        return v.AsString;
    }

    #region 域名与对象的映射关系管理

    /// <summary>
    /// 查询一个域对象 
    /// </summary>
    public static Property FindObjectByDomainAddress(string domain)
    {
        if (domainObs.ContainsKey(domain))
            return domainObs[domain];

        return null;
    }

    /// <summary>
    /// 生成域名 
    /// </summary>
    public static string GenerateDomainAddress(string d, string catalog, int thread)
    {
        if (thread > 0)
            return string.Format("{0}#{1}.{2}", d, thread, catalog);
        
        return string.Format("{0}.{1}", d, catalog);
    }

    /// <summary>
    /// 根据域名(形式为：rid@XXX#1)得到rid 
    /// </summary>
    public static string GetRidByDomain(string domain)
    {
        int end = domain.IndexOf("@");
        string str = domain.Substring(0, end);
        
        // 干掉分线号
        return Game.Explode(str, "#")[0];
    }

    /// <summary>
    /// 根据完整域地址取得rid
    /// XX@rid.XX
    /// </summary>
    public static string GetRidByDomainAddress(string domainAddress)
    {
        int start = domainAddress.IndexOf("@");
        int end = domainAddress.IndexOf(".");
        
        string str = domainAddress.Substring(start + 1, end - start - 1);
        return Game.Explode(str, "#")[0];
    }

    /// <summary>
    /// 返回所有本地域的信息 
    /// </summary>
    public static List<string> GetAllDomains()
    {
        return new List<string>(domainObs.Keys);
    }

    /// <summary>
    /// 移除一个域对象 
    /// </summary>
    public static void RemoveDomainObject(string domain)
    {
        if (domainObs.ContainsKey(domain))
            domainObs.Remove(domain);
    }

    /// <summary>
    /// 设置一个域对象 
    /// </summary>
    public static void SetDomainObject(string domain, Property o)
    {
        domainObs[domain] = o;
    }

    #endregion
}
