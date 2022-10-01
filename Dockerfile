#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SliderCaptcha/SliderCaptcha.csproj", "SliderCaptcha/"]
RUN dotnet restore "SliderCaptcha/SliderCaptcha.csproj"
COPY . .
WORKDIR "/src/SliderCaptcha"
RUN rm -rf "wwwroot/"
RUN dotnet build "SliderCaptcha.csproj" -c Release -o /app/build

RUN apt-get update && \
    apt-get install -y \
    nodejs npm

# https://github.com/nodesource/distributions
RUN curl -fsSL https://deb.nodesource.com/setup_current.x | bash - && \
    apt-get install -y nodejs

FROM build AS publish
RUN dotnet publish "SliderCaptcha.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SliderCaptcha.dll"]
