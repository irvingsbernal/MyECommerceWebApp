FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY MyECommerceWebApp/MyECommerceWebApp.csproj MyECommerceWebApp/
RUN dotnet restore MyECommerceWebApp/MyECommerceWebApp.csproj
COPY MyECommerceWebApp/ MyECommerceWebApp/
WORKDIR /src/MyECommerceWebApp
RUN dotnet build MyECommerceWebApp.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish MyECommerceWebApp.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyECommerceWebApp.dll"]
