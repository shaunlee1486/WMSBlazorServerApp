FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WMS.Presentation/WMS.Presentation.csproj", "WMS.Presentation/"]
COPY ["WMS.Infrastructure/WMS.Infrastructure.csproj", "WMS.Infrastructure/"]
COPY ["WMS.Application/WMS.Application.csproj", "WMS.Application/"]
COPY ["WMS.Domain/WMS.Domain.csproj", "WMS.Domain/"]
COPY ["WMS.SharedKernel/WMS.SharedKernel.csproj", "WMS.SharedKernel/"]
RUN dotnet restore "WMS.Presentation/WMS.Presentation.csproj"
COPY . .
WORKDIR "/src/WMS.Presentation"
RUN dotnet build "WMS.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WMS.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WMS.Presentation.dll"]
