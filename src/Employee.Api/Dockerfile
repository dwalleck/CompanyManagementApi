# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime stage - using AWS Lambda .NET 8 base image
FROM public.ecr.aws/lambda/dotnet:8

# Copy the published app
COPY --from=build-env /app/out ${LAMBDA_TASK_ROOT}

# Set the CMD to your handler (format: Assembly::Namespace.Class::Method)
CMD ["Employee.Api"]