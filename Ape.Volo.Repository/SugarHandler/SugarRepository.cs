using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Ape.Volo.Common;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.System;
using Ape.Volo.Repository.UnitOfWork;
using SqlSugar;

namespace Ape.Volo.Repository.SugarHandler;

/// <summary>
/// SqlSugar仓储
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class SugarRepository<TEntity> : ISugarRepository<TEntity> where TEntity : class, new()
{
    public SugarRepository(IUnitOfWork unitOfWork)
    {
        var sqlSugarScope = unitOfWork.GetDbClient();
        var tenantAttribute = typeof(TEntity).GetCustomAttribute<TenantAttribute>();
        if (tenantAttribute != null)
        {
            SugarClient = sqlSugarScope.GetConnectionScope(tenantAttribute.configId.ToString());
            return;
        }

        var multiDbTenantAttribute = typeof(TEntity).GetCustomAttribute<MultiDbTenantAttribute>();
        if (multiDbTenantAttribute != null)
            if (App.HttpUser.IsNotNull() && App.HttpUser.TenantId > 0)
            {
                var tenants = sqlSugarScope.Queryable<Tenant>().WithCache(86400).ToList();
                var tenant = tenants.FirstOrDefault(x => x.TenantId == App.HttpUser.TenantId);
                if (tenant != null)
                {
                    var iTenant = sqlSugarScope.AsTenant();
                    if (!iTenant.IsAnyConnection(tenant.ConfigId))
                        iTenant.AddConnection(TenantHelper.GetConnectionConfig(tenant.ConfigId, tenant.DbType,
                            tenant.ConnectionString));

                    SugarClient = iTenant.GetConnectionScope(tenant.ConfigId);
                    return;
                }
            }


        SugarClient = sqlSugarScope;
    }

    public ISqlSugarClient SugarClient { get; }
}
