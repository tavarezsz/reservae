FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Reservae.csproj ./
RUN dotnet restore Reservae.csproj
COPY . ./
RUN dotnet publish Reservae.csproj --configuration Release --no-restore --output /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /out ./
RUN mkdir -p /app/wwwroot/uploads/images /app/data-protection-keys \
    && chown -R "$APP_UID:$APP_UID" /app/wwwroot/uploads /app/data-protection-keys
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Reservae.dll"]
