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

COPY . .

WORKDIR /src/HolidayShowServer.Core

FROM build AS publish
RUN dotnet publish HolidayShowServer.Core.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY /Docker/Server/entrypoint.sh .

ENTRYPOINT ["/app/entrypoint.sh"]


