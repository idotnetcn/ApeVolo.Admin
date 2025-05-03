using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ape.Volo.Common.Global;

/// <summary>
/// 程序集类型
/// </summary>
public static class GlobalType
{
    public const string ApiAssembly = "Ape.Volo.Api";
    public const string CoreAssembly = "Ape.Volo.Core";
    public const string CommonAssembly = "Ape.Volo.Common";
    public const string IBusinessAssembly = "Ape.Volo.IBusiness";
    public const string BusinessAssembly = "Ape.Volo.Business";
    public const string RepositoryAssembly = "Ape.Volo.Repository";
    public const string TaskServiceAssembly = "Ape.Volo.TaskService";
    public const string EntityAssembly = "Ape.Volo.Entity";
    public const string SharedModelAssembly = "Ape.Volo.SharedModel";
    public const string ViewModelAssembly = "Ape.Volo.ViewModel";
    public const string EventBusAssembly = "Ape.Volo.EventBus";

    public static readonly List<Type> ApiTypes;
    public static readonly List<Type> CoreTypes;
    public static readonly List<Type> CommonTypes;
    public static readonly List<Type> IBusinessTypes;
    public static readonly List<Type> BusinessTypes;
    public static readonly List<Type> RepositoryTypes;
    public static readonly List<Type> TaskServiceTypes;
    public static readonly List<Type> EntityTypes;
    public static readonly List<Type> SharedModelTypes;
    public static readonly List<Type> ViewModelTypes;
    public static readonly List<Type> EventBusTypes;

    static GlobalType()
    {
        ApiTypes = LoadAssemblyTypes(ApiAssembly);
        CoreTypes = LoadAssemblyTypes(CoreAssembly);
        CommonTypes = LoadAssemblyTypes(CommonAssembly);
        IBusinessTypes = LoadAssemblyTypes(IBusinessAssembly);
        BusinessTypes = LoadAssemblyTypes(BusinessAssembly);
        RepositoryTypes = LoadAssemblyTypes(RepositoryAssembly);
        TaskServiceTypes = LoadAssemblyTypes(TaskServiceAssembly);
        EntityTypes = LoadAssemblyTypes(EntityAssembly);
        SharedModelTypes = LoadAssemblyTypes(SharedModelAssembly);
        ViewModelTypes = LoadAssemblyTypes(ViewModelAssembly);
        EventBusTypes = LoadAssemblyTypes(EventBusAssembly);
    }

    private static List<Type> LoadAssemblyTypes(string dllName)
    {
        var assembly = LoadAssembly(dllName + ".dll");
        return assembly.GetTypes().Where(u => u.IsPublic).ToList();
    }

    private static Assembly LoadAssembly(string dllName)
    {
        var basePath = AppContext.BaseDirectory;
        var dllFile = Path.Combine(basePath, dllName);
        if (!File.Exists(dllFile))
        {
            throw new System.Exception($"{dllName} 文件未生成, 编译项目成功后重试！");
        }

        return Assembly.LoadFrom(dllFile);
    }
}
