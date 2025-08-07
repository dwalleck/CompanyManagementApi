#!/bin/bash

# Setup PostgreSQL database for local development
# Usage: ./setup-local-database.sh [host] [port] [username]

HOST=${1:-localhost}
PORT=${2:-5432}
USERNAME=${3:-postgres}
DATABASE_NAME="employees"

echo "Setting up PostgreSQL database '$DATABASE_NAME' at $HOST:$PORT..."

# Create database if it doesn't exist
createdb -h $HOST -p $PORT -U $USERNAME $DATABASE_NAME 2>/dev/null

if [ $? -eq 0 ]; then
    echo "Database created successfully!"
else
    echo "Database might already exist."
fi

# Run EF Core migrations
echo "Running EF Core migrations..."
cd "$(dirname "$0")/.."
dotnet ef database update

if [ $? -eq 0 ]; then
    echo "Migrations applied successfully!"
    echo "Database setup complete!"
else
    echo "Error applying migrations."
    exit 1
fi