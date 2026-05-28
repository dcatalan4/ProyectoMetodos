FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ProFinalM.Web/ProFinalM.Web.csproj ProFinalM.Web/
RUN dotnet restore ProFinalM.Web/ProFinalM.Web.csproj

COPY . .
RUN dotnet publish ProFinalM.Web/ProFinalM.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet ProFinalM.Web.dll --urls http://0.0.0.0:${PORT:-8080}"]
