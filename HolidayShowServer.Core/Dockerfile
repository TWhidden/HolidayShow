FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

EXPOSE 5555

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY HolidayShowServer.Core/HolidayShowServer.Core.csproj HolidayShowServer.Core/
COPY HolidayShowLib.Core/HolidayShowLib.Core.csproj HolidayShowLib.Core/
COPY HolidayShow.Data.Core/HolidayShow.Data.Core.csproj HolidayShow.Data.Core/
RUN dotnet restore HolidayShowServer.Core/HolidayShowServer.Core.csproj
COPY . .
WORKDIR /src/HolidayShowServer.Core
RUN dotnet build HolidayShowServer.Core.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish HolidayShowServer.Core.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "holidayshowserver.core.dll"]
