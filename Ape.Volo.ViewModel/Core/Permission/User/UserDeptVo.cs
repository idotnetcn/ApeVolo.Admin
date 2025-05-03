using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;

namespace Ape.Volo.ViewModel.Core.Permission.User;

/// <summary>
/// 用户部门Vo
/// </summary>
[AutoMapping(typeof(Entity.Core.Permission.Department), typeof(UserDeptVo))]
public class UserDeptVo
{
    /// <summary>
    /// ID
    /// </summary>
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }
}
