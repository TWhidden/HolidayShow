FROM arm32v7/ubuntu:bionic AS base
WORKDIR /app

#Default Container Port 5555 will be used.
ARG DEFAULTPORT=5555
ARG DEFAULTSTORAGE='/Data'

EXPOSE $DEFAULTPORT

ENV DEVICEID=
ENV SERVER=
ENV PORT=$DEFAULTPORT
ENV STORAGE=$DEFAULTSTORAGE
ENV PORT=$DEFAULTPORT
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENV TZ=America/Los_Angeles
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

#Copy base projects because we use project links
COPY . .

WORKDIR /src/HolidayShowClient.Core

FROM build AS publish
RUN dotnet publish -r linux-arm -c Release -o /app HolidayShowClient.Core.csproj

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY /Docker/Client/entrypoint.sh .

#Copy some required assemblies to the base directory
#two different C++ libs use these. They have a different lib signature
#so bringing them both over.
WORKDIR /usr/lib
COPY /References/libwiringPi.so .
COPY /References/libwiringPi.so.2.46 .

WORKDIR /app

#QT is required for the relay plate - this is a big dependancy, and the library should later be staticly linked. 
RUN apt-get update && \
    apt-get install -y liblttng-ust0 libcurl3 libssl1.0.0 libkrb5-3 zlib1g libicu60 qt5-default

ENTRYPOINT ["/app/entrypoint.sh"]