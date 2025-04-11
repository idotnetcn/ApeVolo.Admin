using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.System;
using Ape.Volo.IBusiness.Dto.System;
using Ape.Volo.IBusiness.ExportModel.System;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.QueryModel;
using Microsoft.AspNetCore.Http;

namespace Ape.Volo.Business.System;

public class FileRecordService : BaseServices<FileRecord>, IFileRecordService
{
    #region 构造函数

    public FileRecordService()
    {
    }

    #endregion

    #region 基础方法

    public async Task<OperateResult> CreateAsync(string description, IFormFile file)
    {
        if (await TableWhere(x => x.Description == description).AnyAsync())
        {
            return OperateResult.Error($"文件描述=>{description}=>已存在!");
        }

        var fileExtensionName = FileHelper.GetExtensionName(file.FileName);
        var fileTypeName = FileHelper.GetFileTypeName(fileExtensionName);
        var fileTypeNameEn = FileHelper.GetFileTypeNameEn(fileTypeName);

        string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + IdHelper.NextId() +
                          file.FileName.Substring(Math.Max(file.FileName.LastIndexOf('.'), 0));

        var prefix = App.WebHostEnvironment.WebRootPath;
        string filePath = Path.Combine(prefix, "uploads", "file", fileTypeNameEn);
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        filePath = Path.Combine(filePath, fileName);
        await using (var fs = new FileStream(filePath, FileMode.CreateNew))
        {
            await file.CopyToAsync(fs);
            fs.Flush();
        }

        string relativePath = Path.GetRelativePath(prefix, filePath);
        relativePath = "/" + relativePath.Replace("\\", "/");
        var fileRecord = new FileRecord
        {
            Description = description,
            OriginalName = file.FileName,
            NewName = fileName,
            FilePath = relativePath,
            Size = FileHelper.GetFileSize(file.Length),
            ContentType = file.ContentType,
            ContentTypeName = fileTypeName,
            ContentTypeNameEn = fileTypeNameEn
        };
        var result = await AddAsync(fileRecord);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> UpdateAsync(CreateUpdateFileRecordDto createUpdateFileRecordDto)
    {
        //取出待更新数据
        var oldFileRecord = await TableWhere(x => x.Id == createUpdateFileRecordDto.Id).FirstAsync();
        if (oldFileRecord.IsNull())
        {
            return OperateResult.Error("数据不存在！");
        }

        if (oldFileRecord.Description != createUpdateFileRecordDto.Description &&
            await TableWhere(x => x.Description == createUpdateFileRecordDto.Description).AnyAsync())
        {
            return OperateResult.Error($"文件描述=>{createUpdateFileRecordDto.Description}=>已存在!");
        }

        var fileRecord = App.Mapper.MapTo<FileRecord>(createUpdateFileRecordDto);
        var result = await UpdateAsync(fileRecord);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var appSecretList = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        await LogicDelete<FileRecord>(x => ids.Contains(x.Id));
        foreach (var appSecret in appSecretList)
        {
            FileHelper.Delete(appSecret.FilePath);
        }

        return OperateResult.Success();
    }

    public async Task<List<FileRecordDto>> QueryAsync(FileRecordQueryCriteria fileRecordQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<FileRecord>
        {
            Pagination = pagination,
            ConditionalModels = fileRecordQueryCriteria.ApplyQueryConditionalModel()
        };
        return App.Mapper.MapTo<List<FileRecordDto>>(
            await TablePageAsync(queryOptions));
    }

    public async Task<List<ExportBase>> DownloadAsync(FileRecordQueryCriteria fileRecordQueryCriteria)
    {
        var conditionalModels = fileRecordQueryCriteria.ApplyQueryConditionalModel();
        var fileRecords = await TableWhere(conditionalModels).ToListAsync();
        List<ExportBase> fileRecordExports = new List<ExportBase>();
        fileRecordExports.AddRange(fileRecords.Select(x => new FileRecordExport()
        {
            Description = x.Description,
            ContentType = x.ContentType,
            ContentTypeName = x.ContentTypeName,
            ContentTypeNameEn = x.ContentTypeNameEn,
            OriginalName = x.OriginalName,
            NewName = x.NewName,
            FilePath = x.FilePath,
            Size = x.Size,
            CreateTime = x.CreateTime
        }));
        return fileRecordExports;
    }

    #endregion
}
