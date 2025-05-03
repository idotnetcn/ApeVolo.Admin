using Ape.Volo.Common.Attributes;

namespace Ape.Volo.ViewModel.Core.Permission.Department;

/// <summary>
/// 部门Vo
/// </summary>
[AutoMapping(typeof(Entity.Core.Permission.Department), typeof(DepartmentSmallVo))]
public class DepartmentSmallVo
{
    /// <summary>
    /// ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }
}
