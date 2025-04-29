using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Common.Enums;

public enum MenuType
{
    /// <summary>
    /// 目录
    /// </summary>
    [Display(Name = "Enum.Menu.Catalog")]
    Catalog = 1,

    /// <summary>
    /// 菜单
    /// </summary>
    [Display(Name = "Enum.Menu.Menu")]
    Menu = 2,

    /// <summary>
    /// 按钮
    /// </summary>
    [Display(Name = "Enum.Menu.Button")]
    Button = 3
}
