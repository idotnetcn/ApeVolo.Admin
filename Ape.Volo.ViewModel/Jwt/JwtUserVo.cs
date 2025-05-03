using Ape.Volo.ViewModel.Core.Permission.User;

namespace Ape.Volo.ViewModel.Jwt;

/// <summary>
/// JWT令牌用户
/// </summary>
public class JwtUserVo
{
    /// <summary>
    /// 用户
    /// </summary>
    public UserVo User { get; set; }

    /// <summary>
    /// 角色权限
    /// </summary>
    public List<string> Roles { get; set; }

    /// <summary>
    /// 数据权限
    /// </summary>
    public List<string> DataScopes { get; set; }
}
