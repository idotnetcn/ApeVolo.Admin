using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.System.Dict;

namespace Ape.Volo.ViewModel.Core.System.Dict;

/// <summary>
/// 字典详情Vo
/// </summary>
[AutoMapping(typeof(DictDetail), typeof(DictDetailVo))]
public class DictDetailVo : BaseEntityDto<long>
{
    /// <summary>
    /// 字典ID
    /// </summary>
    //[JsonIgnore]
    //[JsonProperty]
    public long DictId { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int DictSort { get; set; }
}
