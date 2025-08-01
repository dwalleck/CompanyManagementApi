# Container-based Lambda Deployment Guide

This guide explains how to deploy the Employee API as a container-based AWS Lambda function.

## Prerequisites

- Docker installed and running
- AWS CLI configured with appropriate credentials
- AWS SAM CLI installed (optional, for deployment)
- An AWS account with permissions to create Lambda functions and ECR repositories

## Build and Deploy Steps

### 1. Build and Push Container Image

Use the provided build script:

```bash
./build-and-push.sh <aws-account-id> <region> [image-tag]

# Example:
./build-and-push.sh 123456789012 us-east-1 v1.0.0
```

Or manually:

```bash
# Set variables
AWS_ACCOUNT_ID=123456789012
AWS_REGION=us-east-1
REPOSITORY_NAME=employee-api
IMAGE_TAG=latest

# Login to ECR
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Create ECR repository (if needed)
aws ecr create-repository --repository-name $REPOSITORY_NAME --region $AWS_REGION

# Build image
docker build -t $REPOSITORY_NAME:$IMAGE_TAG .

# Tag for ECR
docker tag $REPOSITORY_NAME:$IMAGE_TAG $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:$IMAGE_TAG

# Push to ECR
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:$IMAGE_TAG
```

### 2. Deploy with SAM

```bash
sam deploy \
  --template-file serverless-container.template \
  --stack-name employee-api \
  --parameter-overrides \
    ImageUri=$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:$IMAGE_TAG \
    Environment=dev \
  --capabilities CAPABILITY_IAM
```

### 3. Deploy with CloudFormation (Alternative)

```bash
aws cloudformation deploy \
  --template-file serverless-container.template \
  --stack-name employee-api \
  --parameter-overrides \
    ImageUri=$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:$IMAGE_TAG \
    Environment=dev \
  --capabilities CAPABILITY_IAM
```

## Local Testing

To test the container locally:

```bash
# Build the image
docker build -t employee-api:local .

# Run locally (requires AWS credentials)
docker run -p 9000:8080 \
  -e AWS_ACCESS_KEY_ID=$AWS_ACCESS_KEY_ID \
  -e AWS_SECRET_ACCESS_KEY=$AWS_SECRET_ACCESS_KEY \
  -e AWS_REGION=$AWS_REGION \
  -e DynamoDb__UseLocalDynamoDb=false \
  employee-api:local

# Test the local endpoint
curl -X POST http://localhost:9000/2015-03-31/functions/function/invocations \
  -d '{"httpMethod": "GET", "path": "/graphql"}'
```

## Environment Variables

The following environment variables can be configured:

- `ASPNETCORE_ENVIRONMENT`: Development, Staging, or Production
- `DynamoDb__TableName`: Name of the DynamoDB table (default: Employees)
- `DynamoDb__UseLocalDynamoDb`: Whether to use local DynamoDB (default: false)
- `DynamoDb__ServiceUrl`: Local DynamoDB URL (if UseLocalDynamoDb is true)

## Updating the Function

To update the Lambda function with a new image:

1. Build and push the new image with a new tag
2. Update the Lambda function:

```bash
aws lambda update-function-code \
  --function-name employee-api-EmployeeApiFunction-XXXX \
  --image-uri $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:new-tag
```

## Advantages of Container Images

- Larger deployment package size (up to 10 GB)
- Use of familiar container tooling
- Better local testing experience
- Consistent runtime environment
- Ability to include custom runtimes and dependencies