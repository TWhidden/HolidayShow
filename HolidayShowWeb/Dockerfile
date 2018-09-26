FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
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

ENV ASPNETCORE_URLS http://+:80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

# set up node
ENV NODE_VERSION 8.9.4
ENV NODE_DOWNLOAD_SHA 21fb4690e349f82d708ae766def01d7fec1b085ce1f5ab30d9bda8ee126ca8fc
RUN curl -SL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.gz" --output nodejs.tar.gz \
    && echo "$NODE_DOWNLOAD_SHA nodejs.tar.gz" | sha256sum -c - \
    && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
    && rm nodejs.tar.gz \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs

# Copy the source files
COPY . .

WORKDIR /src/HolidayShowWeb

# start the build dotnetcore build publish
FROM build AS publish
RUN dotnet publish HolidayShowWeb.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

EXPOSE 80
ENTRYPOINT ["dotnet", "HolidayShowWeb.dll"]


