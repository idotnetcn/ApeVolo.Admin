using Ape.Volo.Common.Global;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Logs
{
    /// <summary>
    /// 
    /// </summary>
    [Tenant(SqlSugarConfig.LogId)]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_information"}_{{year}}{{month}}{{day}}")]
    public class InformationLog : SerilogBase
    {
    }
}
