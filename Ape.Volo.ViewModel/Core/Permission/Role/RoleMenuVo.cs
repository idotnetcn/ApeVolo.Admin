using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;

namespace Ape.Volo.ViewModel.Core.Permission.Role;

/// <summary>
/// 角色菜单Vo
/// </summary>
[AutoMapping(typeof(Entity.Core.Permission.Menu), typeof(RoleMenuVo))]
public class RoleMenuVo
{
    /// <summary>
    /// ID
    /// </summary>
    [Display(Name = "Sys.Id")]
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }
}
