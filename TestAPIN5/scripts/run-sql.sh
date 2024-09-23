#!/bin/bash

# Start SQL Server in the background
(/opt/mssql/bin/sqlservr &)

echo "Waiting for SQL Server to start..."
sleep 30s  # Adjust the sleep time if necessary

# Use the SA_PASSWORD environment variable
if [ -z "$SA_PASSWORD" ]; then
  echo "SA_PASSWORD is not set. Exiting..."
  exit 1
fi

# Run the SQL script to create the database
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -i /docker-entrypoint-initdb.d/create-database.sql -C

if [ $? -eq 0 ]; then
  echo "Database created successfully!"
else
  echo "Failed to create the database."
fi

# Keep the container running by tailing the SQL Server log
tail -f /var/opt/mssql/log/errorlog