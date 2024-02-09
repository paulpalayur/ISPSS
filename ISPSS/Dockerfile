FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
#EXPOSE 8081 for dev
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
RUN apt-get update && \
    apt-get install -y wget
WORKDIR /src
RUN wget https://ip-ranges.amazonaws.com/ip-ranges.json
COPY ["ISPSS/ISPSS.csproj", "ISPSS/"]
RUN dotnet restore "./ISPSS/./ISPSS.csproj"
COPY . .
WORKDIR "/src/ISPSS"
RUN dotnet build "./ISPSS.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ISPSS.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /src/ip-ranges.json /app/Misc/
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ISPSS.dll"]