#!/bin/bash

# Build and push Lambda container image to ECR
# Usage: ./build-and-push.sh <aws-account-id> <region> [image-tag]

set -e

# Check if required parameters are provided
if [ "$#" -lt 2 ]; then
    echo "Usage: $0 <aws-account-id> <region> [image-tag]"
    echo "Example: $0 123456789012 us-east-1 latest"
    exit 1
fi

AWS_ACCOUNT_ID=$1
AWS_REGION=$2
IMAGE_TAG=${3:-latest}

# ECR repository name
REPOSITORY_NAME="employee-api"

# Full image URI
IMAGE_URI="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:$IMAGE_TAG"

echo "Building and pushing Employee API Lambda container image..."
echo "Repository: $REPOSITORY_NAME"
echo "Image URI: $IMAGE_URI"

# Get ECR login token
echo "Logging into ECR..."
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Create repository if it doesn't exist
echo "Creating ECR repository if it doesn't exist..."
aws ecr describe-repositories --repository-names $REPOSITORY_NAME --region $AWS_REGION 2>/dev/null || \
    aws ecr create-repository --repository-name $REPOSITORY_NAME --region $AWS_REGION

# Build the Docker image
echo "Building Docker image..."
docker build -t $REPOSITORY_NAME:$IMAGE_TAG .

# Tag the image for ECR
echo "Tagging image for ECR..."
docker tag $REPOSITORY_NAME:$IMAGE_TAG $IMAGE_URI

# Push the image to ECR
echo "Pushing image to ECR..."
docker push $IMAGE_URI

echo "Successfully pushed image: $IMAGE_URI"
echo ""
echo "To deploy using SAM:"
echo "sam deploy --template-file serverless-container.template --stack-name employee-api --parameter-overrides ImageUri=$IMAGE_URI Environment=dev --capabilities CAPABILITY_IAM"