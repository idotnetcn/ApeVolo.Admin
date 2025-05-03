using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;

namespace Ape.Volo.SharedModel.Queries.Common;

/// <summary>
/// id模型(string)
/// </summary>
public class IdCollectionString
{
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Sys.Id")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public HashSet<string> IdArray { get; set; }
}
