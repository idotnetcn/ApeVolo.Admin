# 使用官方 .NET 运行时镜像作为基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8002

# 使用官方 .NET SDK 镜像作为构建镜像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Ape.Volo.Api/Ape.Volo.Api.csproj", "Ape.Volo.Api/"]
COPY ["Ape.Volo.Business/Ape.Volo.Business.csproj", "Ape.Volo.Business/"]
COPY ["Ape.Volo.Common/Ape.Volo.Common.csproj", "Ape.Volo.Common/"]
COPY ["Ape.Volo.Core/Ape.Volo.Core.csproj", "Ape.Volo.Core/"]
COPY ["Ape.Volo.Entity/Ape.Volo.Entity.csproj", "Ape.Volo.Entity/"]
COPY ["Ape.Volo.EventBus/Ape.Volo.EventBus.csproj", "Ape.Volo.EventBus/"]
COPY ["Ape.Volo.IBusiness/Ape.Volo.IBusiness.csproj", "Ape.Volo.IBusiness/"]
COPY ["Ape.Volo.Infrastructure/.Ape.Volo.Infrastructure.csproj", "Ape.Volo.Infrastructure/"]
COPY ["Ape.Volo.Repository/Ape.Volo.Repository.csproj", "Ape.Volo.Repository/"]
COPY ["Ape.Volo.SharedModel/Ape.Volo.SharedModel.csproj", "Ape.Volo.SharedModel/"]
COPY ["Ape.Volo.TaskService/Ape.Volo.TaskService.csproj", "Ape.Volo.TaskService/"]
COPY ["Ape.Volo.ViewModel/Ape.Volo.ViewModel.csproj", "Ape.Volo.ViewModel/"]
RUN dotnet restore "Ape.Volo.Api/Ape.Volo.Api.csproj"

COPY . .
WORKDIR "/src/Ape.Volo.Api"
RUN dotnet build "Ape.Volo.Api.csproj" -c Release -o /app/build

# 发布应用
FROM build AS publish
RUN dotnet publish "Ape.Volo.Api.csproj" -c Release -o /app/publish

# 运行阶段
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ape.Volo.Api.dll"]
