using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Logs
{
    /// <summary>
    /// 信息日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_information"}_{{year}}{{month}}{{day}}")]
    public class InformationLog : SerilogBase
    {
    }
}
