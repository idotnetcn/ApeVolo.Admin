using Newtonsoft.Json;

namespace Ape.Volo.ViewModel.Jwt;

/// <summary>
/// 登录结果
/// </summary>
public class LoginResultVo
{
    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("user")]
    public JwtUserVo JwtUserVo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("token")]
    public TokenVo TokenVo { get; set; }
}
