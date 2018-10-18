/// <summary>
/// 通讯相关的一些配置 
/// </summary>
public class CommConfig
{
    public const int CLIENT_USER   = 1;
    public const int INTERNAL_COMM = 2;
    public const int FROM_SERVER   = 4;
    
    /// <summary>
    /// 包体的最大大小 
    /// </summary>
    public const int MAX_PACKET_SIZE = (1024 * 1024);
}
