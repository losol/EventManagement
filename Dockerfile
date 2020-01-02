#
# Stage 0
# Build the project
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

# Install node
ENV NODE_VERSION 8.11.4
ENV NODE_DOWNLOAD_SHA c69abe770f002a7415bd00f7ea13b086650c1dd925ef0c3bf8de90eabecc8790
RUN curl -SL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.gz" --output nodejs.tar.gz \
    && echo "$NODE_DOWNLOAD_SHA nodejs.tar.gz" | sha256sum -c - \
    && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
    && rm nodejs.tar.gz \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs

# This is needed for PhantomJS to successfully install
RUN apt-get update
RUN apt-get install bzip2

# copy csproj and restore dependencies
WORKDIR /app/src
COPY ./EventManagement.sln .
COPY ./src/EventManagement.Web/*.csproj ./src/EventManagement.Web/
COPY ./src/EventManagement.Services/*.csproj ./src/EventManagement.Services/
COPY ./src/EventManagement.Services.Converto/*.csproj ./src/EventManagement.Services.Converto/
COPY ./src/EventManagement.Infrastructure/*.csproj ./src/EventManagement.Infrastructure/
COPY ./src/EventManagement.Domain/*.csproj ./src/EventManagement.Domain/
COPY ./tests/EventManagement.UnitTests/*.csproj ./tests/EventManagement.UnitTests/
COPY ./tests/EventManagement.IntegrationTests/*.csproj ./tests/EventManagement.IntegrationTests/
COPY ./tests/EventManagement.Services.Converto.Tests/*.csproj ./tests/EventManagement.Services.Converto.Tests/
RUN dotnet restore

# Copy the package.json file
COPY ./src/EventManagement.Web/ClientApp/package*.json ./src/EventManagement.Web/ClientApp
RUN npm --prefix ./src/EventManagement.Web/ClientApp install ./src/EventManagement.Web/ClientApp

# copy everything else
COPY . ./

# Publish
WORKDIR ./src/EventManagement.Web/ClientApp
RUN npm run build
RUN dotnet publish -c Release -o /app/out

#
# Stage 1
# Copy the built files over
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

# Install libfontconfig (required by PhantomJS)
RUN apt-get -qq update && apt-get --assume-yes install libfontconfig

# Copy files over from the build-env stage
WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "losol.EventManagement.Web.dll"]