FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

#Default Container Port 5555 will be used.
ARG DEFAULTPORT=5555

EXPOSE $DEFAULTPORT

ENV DBSERVER=""
ENV DBUSER=""
ENV DBPASS=""
ENV DBNAME=""
ENV PORT=$DEFAULTPORT

ENV TZ=America/Los_Angeles
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

#Copy base projects because we use project links
COPY HolidayShowServer/HolidayShowServer.csproj HolidayShowServer/
COPY HolidayShowServer.Core/HolidayShowServer.Core.csproj HolidayShowServer.Core/

#Copy base projects because we use project links
COPY HolidayShowLib/HolidayShowLib.csproj HolidayShowLib/
COPY HolidayShowLib.Core/HolidayShowLib.Core.csproj HolidayShowLib.Core/

#Copy base projects because we use project links
COPY HolidayShow.Data/HolidayShow.Data.csproj HolidayShow.Data/
COPY HolidayShow.Data.Core/HolidayShow.Data.Core.csproj HolidayShow.Data.Core/

RUN dotnet restore HolidayShowServer.Core/HolidayShowServer.Core.csproj
COPY . .
WORKDIR /src/HolidayShowServer.Core
RUN dotnet build HolidayShowServer.Core.csproj -c ReleaseCore -o /app

FROM build AS publish
RUN dotnet publish HolidayShowServer.Core.csproj -c ReleaseCore -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY server-entrypoint.sh .

ENTRYPOINT ["/app/server-entrypoint.sh"]


