#!/bin/bash
until /opt/mssql-tools18/bin/sqlcmd -S db -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" &> /dev/null
do
  echo "Waiting for SQL Server..."
  sleep 2
done

/opt/mssql-tools18/bin/sqlcmd -S db -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/SkillSync_Database_Schema.sql
echo "Schema applied."
