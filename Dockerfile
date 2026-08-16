FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY MyECommerceWebApp/MyECommerceWebApp.csproj MyECommerceWebApp/
RUN dotnet restore MyECommerceWebApp/MyECommerceWebApp.csproj
COPY MyECommerceWebApp/ MyECommerceWebApp/
RUN dotnet publish MyECommerceWebApp/MyECommerceWebApp.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyECommerceWebApp.dll"]
