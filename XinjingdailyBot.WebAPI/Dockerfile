#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["XinjingdailyBot.WebAPI/XinjingdailyBot.WebAPI.csproj", "XinjingdailyBot.WebAPI/"]
COPY ["XinjingdailyBot.Command/XinjingdailyBot.Command.csproj", "XinjingdailyBot.Command/"]
COPY ["XinjingdailyBot.Infrastructure/XinjingdailyBot.Infrastructure.csproj", "XinjingdailyBot.Infrastructure/"]
COPY ["XinjingdailyBot.Interface/XinjingdailyBot.Interface.csproj", "XinjingdailyBot.Interface/"]
COPY ["XinjingdailyBot.Model/XinjingdailyBot.Model.csproj", "XinjingdailyBot.Model/"]
COPY ["XinjingdailyBot.Repository/XinjingdailyBot.Repository.csproj", "XinjingdailyBot.Repository/"]
COPY ["XinjingdailyBot.Service/XinjingdailyBot.Service.csproj", "XinjingdailyBot.Service/"]
COPY ["XinjingdailyBot.Tasks/XinjingdailyBot.Tasks.csproj", "XinjingdailyBot.Tasks/"]
RUN dotnet restore "XinjingdailyBot.WebAPI/XinjingdailyBot.WebAPI.csproj"
COPY . .
WORKDIR "/src/XinjingdailyBot.WebAPI"
RUN dotnet build "XinjingdailyBot.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "XinjingdailyBot.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "XinjingdailyBot.WebAPI.dll"]