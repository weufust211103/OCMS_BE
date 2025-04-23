# Base image để chạy ứng dụng (ASP.NET runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y \
    wget gnupg ca-certificates \
    fonts-liberation libappindicator3-1 libasound2 libatk-bridge2.0-0 \
    libatk1.0-0 libcups2 libdbus-1-3 libgdk-pixbuf2.0-0 libnspr4 \
    libnss3 libx11-xcb1 libxcomposite1 libxdamage1 libxrandr2 \
    xdg-utils libu2f-udev libvulkan1 libxss1 libgbm1 chromium-browser \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium-browser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["OCMS_WebAPI/OCMS_WebAPI.csproj", "OCMS_WebAPI/"]
COPY ["OCMS_BOs/OCMS_BOs.csproj", "OCMS_BOs/"]
COPY ["OCMS_Repositories/OCMS_Repositories.csproj", "OCMS_Repositories/"]
COPY ["OCMS_Services/OCMS_Services.csproj", "OCMS_Services/"]

RUN dotnet restore "OCMS_WebAPI/OCMS_WebAPI.csproj"

COPY . .

WORKDIR "/src/OCMS_WebAPI"
RUN dotnet build "OCMS_WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OCMS_WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "OCMS_WebAPI.dll"]
