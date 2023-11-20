FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /src

# Default language
ENV LC_ALL en_US.UTF-8
ENV LANG en_US.UTF-8

# Build context must be {root-of-repo}/backend/API
COPY . .

RUN ls -l

RUN dotnet build API.csproj -c Release -o /app

FROM build-env AS publish-env
RUN dotnet publish API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime-env

WORKDIR /app
COPY --from=publish-env /app .

RUN ls -l

ENTRYPOINT [dotnet, API.dll]