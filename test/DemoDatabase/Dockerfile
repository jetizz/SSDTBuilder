ARG SA_PASSWORD=D0cker!a8s

FROM jetiz/ssdt-builder:latest AS build
WORKDIR /src
COPY . .
WORKDIR /output
RUN SSDTBuilder \
    --verbose \
    --dacpac "dac.dacpac" \
    --project "/src/DemoDatabase.sqlproj" \
    --profile "/src/PublishProfile.xml" \
    --output "/output" \
    --script "Scripts/00_schema.sql" \
    --copy-loose
    #--remove-sqlcmd-variables \
    --target-db-name "MyDatabase"


FROM mcr.microsoft.com/mssql/server:2019-latest AS final
ARG SA_PASSWORD
ENV ACCEPT_EULA="Y" \
    MSSQL_PID="Express" \
    SA_PASSWORD="${SA_PASSWORD}"
WORKDIR /init
COPY --from=build /output .
RUN ( /opt/mssql/bin/sqlservr --accept-eula & ) | grep -q "Service Broker manager has started" \
    && du -sh Scripts/* | sort -rh | grep -P "sql$" | awk '{print $2}' | while read f; do fp=`pwd`/$f; \
        echo 'Executing: ' $fp; \
        /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -i $fp; \
    done \
    && pkill sqlservr 

EXPOSE 1433
CMD ["/opt/mssql/bin/sqlservr"]