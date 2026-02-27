# STAGE 1: base - imagen runtime minima
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# STAGE 2: build - restore y compilacion de todo
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiamos los .csproj para aprovechar la cache de capas de Docker
COPY ["dawazon2.0/dawazon2.0.csproj", "dawazon2.0/"]
COPY ["dawazonBackend/dawazonBackend.csproj", "dawazonBackend/"]
COPY ["dawazonTest/dawazonTest.csproj", "dawazonTest/"]

# Restore de dependencias
RUN dotnet restore "dawazon2.0/dawazon2.0.csproj"
RUN dotnet restore "dawazonTest/dawazonTest.csproj"

# Copiamos todo el codigo fuente
COPY dawazon2.0/ dawazon2.0/
COPY dawazonBackend/ dawazonBackend/
COPY dawazonTest/ dawazonTest/

# Build del proyecto principal
RUN dotnet build "dawazon2.0/dawazon2.0.csproj" -c $BUILD_CONFIGURATION -o /app/build

# STAGE 3: test - ejecucion de tests y reporte HTML
# Si los tests fallan el build se detiene aqui y NO
# continua al stage publish ni final
FROM build AS test
WORKDIR /src
ARG DOCKER_HOST_ARG=tcp://host.docker.internal:2375
ENV DOCKER_HOST=$DOCKER_HOST_ARG

# Instalamos ReportGenerator como tool global de .NET en el contenedor
# No necesitas instalar nada en tu proyecto, solo tener coverlet.collector
RUN dotnet tool install --global dotnet-reportgenerator-globaltool
ENV PATH="$PATH:/root/.dotnet/tools"

# Ejecutamos los tests:
#   --collect:"XPlat Code Coverage"  -> coverlet genera el coverage.cobertura.xml
#   --logger trx                     -> genera el TestResults.trx con resultados de tests
# Si algun test falla -> exit code != 0 -> Docker detiene el build aqui
RUN dotnet test "dawazonTest/dawazonTest.csproj" \
    --no-restore \
    --collect:"XPlat Code Coverage" \
    --logger "trx;LogFileName=TestResults.trx" \
    --results-directory /app/test-results \
    -c Release

# Generamos el reporte HTML combinado (cobertura + resultados)
# -reports: apunta al xml de coverlet generado por --collect
# -targetdir: carpeta de salida del HTML
# -reporttypes: Html genera index.html navegable
RUN reportgenerator \
    -reports:"/app/test-results/**/coverage.cobertura.xml" \
    -targetdir:/app/test-results/html \
    -reporttypes:Html

# STAGE 4: publish - solo llega aqui si los tests pasaron
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "dawazon2.0/dawazon2.0.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# STAGE 5: final - imagen minima solo para ejecutar la app
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dawazon2.0.dll"]

# STAGE 6: testweb - nginx sirviendo el reporte HTML
FROM nginx:latest AS testweb

# Eliminamos la web por defecto de nginx
RUN rm -rf /usr/share/nginx/html/*

# Copiamos el reporte HTML generado por ReportGenerator desde el stage test
COPY --from=test /app/test-results/html /usr/share/nginx/html