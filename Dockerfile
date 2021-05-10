#
# Stage 0
# Build the project
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app/src

# copy csproj and restore dependencies
COPY ./Eventuras.sln .
COPY ./src/Eventuras.Web/*.csproj ./src/Eventuras.Web/
COPY ./src/Eventuras.WebApi/*.csproj ./src/Eventuras.WebApi/
COPY ./src/Eventuras.Services/*.csproj ./src/Eventuras.Services/
COPY ./src/Eventuras.Services.Converto/*.csproj ./src/Eventuras.Services.Converto/
COPY ./src/Eventuras.Services.Auth0/*.csproj ./src/Eventuras.Services.Auth0/
COPY ./src/Eventuras.Services.TalentLms/*.csproj ./src/Eventuras.Services.TalentLms/
COPY ./src/Eventuras.Services.Zoom/*.csproj ./src/Eventuras.Services.Zoom/
COPY ./src/Eventuras.Infrastructure/*.csproj ./src/Eventuras.Infrastructure/
COPY ./src/Eventuras.Domain/*.csproj ./src/Eventuras.Domain/
COPY ./tests/Eventuras.UnitTests/*.csproj ./tests/Eventuras.UnitTests/
COPY ./tests/Eventuras.Web.Tests/*.csproj ./tests/Eventuras.Web.Tests/
COPY ./tests/Eventuras.WebApi.Tests/*.csproj ./tests/Eventuras.WebApi.Tests/
COPY ./tests/Eventuras.Services.Converto.Tests/*.csproj ./tests/Eventuras.Services.Converto.Tests/
COPY ./tests/Eventuras.Services.TalentLms.Tests/*.csproj ./tests/Eventuras.Services.TalentLms.Tests/
COPY ./tests/Eventuras.TestAbstractions/*.csproj ./tests/Eventuras.TestAbstractions/
RUN dotnet restore

# copy everything else
COPY . ./

# Publish
WORKDIR /app/src/src/Eventuras.WebApi
RUN dotnet publish -c Release -o /app/out

#
# Stage 1
# Copy the built files over
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS publish

# Copy files over from the build stage
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "Eventuras.WebApi.dll"]
