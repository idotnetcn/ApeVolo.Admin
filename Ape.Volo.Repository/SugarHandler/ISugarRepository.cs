using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.Repository.SugarHandler;

/// <summary>
/// sqlSugar接口
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ISugarRepository<TEntity> where TEntity : class
{
    ISqlSugarClient SugarClient { get; }
}
