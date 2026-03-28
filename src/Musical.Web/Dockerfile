FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Musical.Core/Musical.Core.csproj Musical.Core/
COPY src/Musical.Web/Musical.Web.csproj Musical.Web/
RUN dotnet restore Musical.Web/Musical.Web.csproj
COPY src/Musical.Core/ Musical.Core/
COPY src/Musical.Web/ Musical.Web/
RUN dotnet publish Musical.Web/Musical.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Musical.Web.dll"]
