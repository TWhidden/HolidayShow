FROM mcr.microsoft.com/dotnet/core/runtime:3.1.5-bionic AS base
WORKDIR /app

#Default Container Port 5001 will be used.
ARG DEFAULTPORT=5001

EXPOSE $DEFAULTPORT

ENV DBSERVER=""
ENV DBUSER=""
ENV DBPASS=""
ENV DBNAME=""
ENV PORT=$DEFAULTPORT

ENV TZ=America/Los_Angeles
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.301-bionic AS build
WORKDIR /src

# set up node
ENV NODE_VERSION 14.13.0
ENV NODE_DOWNLOAD_SHA 21fb4690e349f82d708ae766def01d7fec1b085ce1f5ab30d9bda8ee126ca8fc
RUN curl -SL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.gz" --output nodejs.tar.gz \
    && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
    && rm nodejs.tar.gz \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs \
	&& apt-get update \
	&& apt-get install -y apt-transport-https

# Copy the source files
COPY . .

WORKDIR /src/HolidayShowWeb/ClientApp

RUN npm install

WORKDIR /src/HolidayShowWeb

# start the build dotnetcore build publish

FROM build AS publish
RUN dotnet publish HolidayShowWeb.csproj  -r linux-x64 -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "HolidayShowWeb.dll"]


