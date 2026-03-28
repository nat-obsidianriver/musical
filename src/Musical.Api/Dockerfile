FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Musical.Core/Musical.Core.csproj Musical.Core/
COPY src/Musical.Api/Musical.Api.csproj Musical.Api/
RUN dotnet restore Musical.Api/Musical.Api.csproj
COPY src/Musical.Core/ Musical.Core/
COPY src/Musical.Api/ Musical.Api/
RUN dotnet publish Musical.Api/Musical.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Musical.Api.dll"]
