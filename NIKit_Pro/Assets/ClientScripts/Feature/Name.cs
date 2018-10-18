/// <summary>
/// Name.cs
/// Copy from zhangyg 2014-10-22
/// 名字特性
/// </summary>

public class Name
{
    private string name;

    /// <summary>
    /// 设置物件的名字 
    /// </summary>
    /// <param name="name">物件名字</param>
    public void SetName (string name)
    {
        this.name = name;
    }
    
    /// <summary>
    /// 取得物件的名字 
    /// </summary>
    /// <returns>物件名字</returns>
    public string GetName ()
    {
        return name;
    }
}
