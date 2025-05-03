using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.System;

/// <summary>
/// 租户查询参数
/// </summary>
public class TenantQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 名称
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Name { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public TenantType? TenantType { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public DbType? DbType { get; set; }
}
