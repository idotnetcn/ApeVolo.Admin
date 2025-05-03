using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.Permission;

/// <summary>
/// 菜单查询参数
/// </summary>
public class MenuQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 菜单标题
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Title { get; set; }

    /// <summary>
    /// 父级ID
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal, IsGreaterThanNumberDefault = false)]
    public long ParentId { get; set; }
}
