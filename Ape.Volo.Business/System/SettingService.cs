using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.System;
using Ape.Volo.IBusiness.Dto.System;
using Ape.Volo.IBusiness.ExportModel.System;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.QueryModel;
using Microsoft.Extensions.Logging;
using static Ape.Volo.Common.Helper.ExceptionHelper;

namespace Ape.Volo.Business.System;

public class SettingService : BaseServices<Setting>, ISettingService
{
    #region 构造函数

    private readonly ILogger<SettingService> _logger;

    public SettingService(ILogger<SettingService> logger)
    {
        _logger = logger;
    }

    #endregion

    #region 基础方法

    public async Task<OperateResult> CreateAsync(CreateUpdateSettingDto createUpdateSettingDto)
    {
        if (await TableWhere(r => r.Name == createUpdateSettingDto.Name).AnyAsync())
        {
            return OperateResult.Error($"设置键=>{createUpdateSettingDto.Name}=>已存在!");
        }

        var setting = App.Mapper.MapTo<Setting>(createUpdateSettingDto);
        var result = await AddAsync(setting);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> UpdateAsync(CreateUpdateSettingDto createUpdateSettingDto)
    {
        //取出待更新数据
        var oldSetting = await TableWhere(x => x.Id == createUpdateSettingDto.Id).FirstAsync();
        if (oldSetting.IsNull())
        {
            return OperateResult.Error("数据不存在！");
        }

        if (oldSetting.Name != createUpdateSettingDto.Name &&
            await TableWhere(x => x.Name == createUpdateSettingDto.Name).AnyAsync())
        {
            return OperateResult.Error($"设置键=>{createUpdateSettingDto.Name}=>已存在!");
        }

        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadSettingByName +
                                    oldSetting.Name.ToMd5String16());
        var setting = App.Mapper.MapTo<Setting>(createUpdateSettingDto);
        var result = await UpdateAsync(setting);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var settings = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        foreach (var setting in settings)
        {
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadSettingByName +
                                        setting.Name.ToMd5String16());
        }

        var result = await LogicDelete<Setting>(x => ids.Contains(x.Id));

        return OperateResult.Result(result);
        ;
    }

    public async Task<List<SettingDto>> QueryAsync(SettingQueryCriteria settingQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<Setting>
        {
            Pagination = pagination,
            ConditionalModels = settingQueryCriteria.ApplyQueryConditionalModel()
        };
        return App.Mapper.MapTo<List<SettingDto>>(await TablePageAsync(queryOptions));
    }

    public async Task<List<ExportBase>> DownloadAsync(SettingQueryCriteria settingQueryCriteria)
    {
        var settings = await TableWhere(settingQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> settingExports = new List<ExportBase>();
        settingExports.AddRange(settings.Select(x => new SettingExport()
        {
            Name = x.Name,
            Value = x.Value,
            EnabledState = x.Enabled ? EnabledState.Enabled : EnabledState.Disabled,
            Description = x.Description,
            CreateTime = x.CreateTime
        }));
        return settingExports;
    }

    //[UseCache(Expiration = 30, KeyPrefix = GlobalConstants.CachePrefix.LoadSettingByName)]
    public async Task<T> GetSettingValue<T>(string settingName)
    {
        var settingList = await Table.WithCache(86400).ToListAsync();

        var setting = settingList.FirstOrDefault(x => x.Name == settingName.Trim());
        if (setting == null) return default;

        try
        {
            return (T)ConvertValue(typeof(T), setting.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(GetExceptionAllMsg(e));
            return default;
        }
    }

    private static object ConvertValue(Type type, string value)
    {
        if (type == typeof(object))
        {
            return value;
        }

        if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return string.IsNullOrEmpty(value) ? value : ConvertValue(Nullable.GetUnderlyingType(type), value);
        }

        var converter = TypeDescriptor.GetConverter(type);
        return converter.CanConvertFrom(typeof(string)) ? converter.ConvertFromInvariantString(value) : null;
    }

    #endregion
}
