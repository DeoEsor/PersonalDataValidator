FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NSPValidator/NSPValidator.csproj", "NSPValidator/"]
RUN dotnet restore "NSPValidator/NSPValidator.csproj"
COPY . .
WORKDIR "/src/NSPValidator"
RUN dotnet build "NSPValidator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NSPValidator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NSPValidator.dll"]
