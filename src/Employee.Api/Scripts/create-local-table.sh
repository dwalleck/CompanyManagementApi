#!/bin/bash

# Create DynamoDB table in local instance
# Usage: ./create-local-table.sh [endpoint-url]

ENDPOINT_URL=${1:-http://localhost:8000}
TABLE_NAME="Employees"

echo "Creating DynamoDB table '$TABLE_NAME' at $ENDPOINT_URL..."

aws dynamodb create-table \
    --endpoint-url $ENDPOINT_URL \
    --table-name $TABLE_NAME \
    --attribute-definitions \
        AttributeName=EmployeeId,AttributeType=S \
    --key-schema \
        AttributeName=EmployeeId,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST \
    --region us-east-1 \
    2>/dev/null

if [ $? -eq 0 ]; then
    echo "Table created successfully!"
else
    echo "Table might already exist or there was an error."
fi

# List tables to verify
echo "Listing tables..."
aws dynamodb list-tables --endpoint-url $ENDPOINT_URL --region us-east-1