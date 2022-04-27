FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PhoneNumberValidator/PhoneNumberValidator.csproj", "PhoneNumberValidator/"]
RUN dotnet restore "PhoneNumberValidator/PhoneNumberValidator.csproj"
COPY . .
WORKDIR "/src/PhoneNumberValidator"
RUN dotnet build "PhoneNumberValidator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhoneNumberValidator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhoneNumberValidator.dll"]
