#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8888

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ZPanel/ZPanel.csproj", "ZPanel/"]
COPY ["ZPanel.CodeGenerator/ZPanel.CodeGenerator.csproj", "ZPanel.CodeGenerator/"]
COPY ["Infrastructure/ZPanel.Infrastructure.csproj", "Infrastructure/"]
COPY ["ZPanel.Common/ZPanel.Common.csproj", "ZPanel.Common/"]
COPY ["ZPanel.ServiceCore/ZPanel.ServiceCore.csproj", "ZPanel.ServiceCore/"]
COPY ["ZPanel.Repository/ZPanel.Repository.csproj", "ZPanel.Repository/"]
COPY ["ZPanel.Model/ZPanel.Model.csproj", "ZPanel.Model/"]
COPY ["ZPanel.Service/ZPanel.Service.csproj", "ZPanel.Service/"]
COPY ["ZPanel.Tasks/ZPanel.Tasks.csproj", "ZPanel.Tasks/"]
RUN dotnet restore "./ZPanel/ZPanel.csproj"
COPY . .
WORKDIR "/src/ZPanel"
RUN dotnet build "./ZPanel.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ZPanel.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZPanel.dll"]