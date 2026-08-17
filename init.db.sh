#!/bin/bash
set -euo pipefail

find_sqlcmd() {
  if [ -x /opt/mssql-tools18/bin/sqlcmd ]; then
    echo /opt/mssql-tools18/bin/sqlcmd
  elif [ -x /opt/mssql-tools/bin/sqlcmd ]; then
    echo /opt/mssql-tools/bin/sqlcmd
  else
    echo ""
  fi
}

SQLCMD="$(find_sqlcmd)"
if [ -z "$SQLCMD" ]; then
  echo "sqlcmd no esta instalado en la imagen de SQL Server."
  exit 1
fi

SQLCMD_ARGS=(-S localhost -U sa -P "$MSSQL_SA_PASSWORD" -l 30)
if [ -x /opt/mssql-tools18/bin/sqlcmd ]; then
  SQLCMD_ARGS+=(-C)
fi

echo "Iniciando SQL Server..."
/opt/mssql/bin/sqlservr &
SQLSERVER_PID=$!

echo "Esperando a que SQL Server acepte conexiones..."
READY=0
for i in $(seq 1 60); do
  if "$SQLCMD" "${SQLCMD_ARGS[@]}" -Q "SELECT 1" >/dev/null 2>&1; then
    echo "SQL Server listo."
    READY=1
    break
  fi
  if ! kill -0 "$SQLSERVER_PID" 2>/dev/null; then
    echo "SQL Server termino antes de quedar listo."
    exit 1
  fi
  sleep 2
done

if [ "$READY" -ne 1 ]; then
  echo "SQL Server no acepto conexiones a tiempo."
  exit 1
fi

echo "Creando base de datos si no existe..."
"$SQLCMD" "${SQLCMD_ARGS[@]}" -i /init.sql

touch /tmp/db-ready
echo "Base de datos ecommercedev lista."

wait "$SQLSERVER_PID"
