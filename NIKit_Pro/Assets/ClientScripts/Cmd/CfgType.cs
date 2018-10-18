/// <summary>
/// 服务器类型定义
/// </summary>
public class CfgType
{
    public const int CFG_TYPE_AAA           = 2;
    public const int CFG_TYPE_GS            = 5;
    public const int CFG_TYPE_CONSOLE       = 6;
    public const int CFG_TYPE_AUTH_CLIENT   = 14;

    public const string CFG_AAA_ACCOUNT   = "aaa_account"; // 账号操作相关
    public const string CFG_AAA_LOGIN     = "aaa_login";   // 登录服务
    public const string CFG_SERVICE_GS    = "gs";          // GS服务
    public const string CFG_AUTH_CLIENT   = "auth_client"; // 验证客户端
}
