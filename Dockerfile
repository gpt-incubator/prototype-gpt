#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Application/PrototypeGPT.Application/PrototypeGPT.Application.csproj", "src/Application/PrototypeGPT.Application/"]
COPY ["src/Application/PrototypeGPT.Application.Services/PrototypeGPT.Application.Services.csproj", "src/Application/PrototypeGPT.Application.Services/"]
COPY ["src/Domain/PrototypeGPT.Domain/PrototypeGPT.Domain.csproj", "src/Domain/PrototypeGPT.Domain/"]
COPY ["src/Infrastructure/PrototypeGPT.Infrastructure/PrototypeGPT.Infrastructure.csproj", "src/Infrastructure/PrototypeGPT.Infrastructure/"]
COPY ["src/Infrastructure/PrototypeGPT.Infrastructure.DataAccess/PrototypeGPT.Infrastructure.DataAccess.csproj", "src/Infrastructure/PrototypeGPT.Infrastructure.DataAccess/"]
COPY ["src/Infrastructure/PrototypeGPT.Infrastructure.Identity/PrototypeGPT.Infrastructure.Identity.csproj", "src/Infrastructure/PrototypeGPT.Infrastructure.Identity/"]
COPY ["src/Presentation/PrototypeGPT.Api/PrototypeGPT.Api.csproj", "src/Presentation/PrototypeGPT.Api/"]

RUN dotnet restore "src/Presentation/PrototypeGPT.Api/PrototypeGPT.Api.csproj"
COPY . .
WORKDIR "/src/src/Presentation/PrototypeGPT.Api"
RUN dotnet build "PrototypeGPT.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PrototypeGPT.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

ENV ASPNETCORE_URLS http://+:5000
EXPOSE 5000

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PrototypeGPT.Api.dll"]