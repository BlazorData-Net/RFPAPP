# Azure Services Configuration Guide

This document provides step-by-step instructions for setting up the Azure services required for the Microsoft Aspire Job Scheduler solution.

## 🗂️ Azure Resource Group Setup

### Create Resource Group
```bash
# Create a resource group for all job scheduler resources
az group create \
  --name "rg-aspire-job-scheduler" \
  --location "East US" \
  --tags "Project=AspireJobScheduler" "Environment=Production"
```

## 📊 SQL Database Setup

### Create SQL Server and Database
```bash
# Create Azure SQL Server
az sql server create \
  --name "sql-aspire-job-scheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --location "East US" \
  --admin-user "sqladmin" \
  --admin-password "YourSecurePassword123!"

# Configure firewall to allow Azure services
az sql server firewall-rule create \
  --server "sql-aspire-job-scheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --name "AllowAzureServices" \
  --start-ip-address "0.0.0.0" \
  --end-ip-address "0.0.0.0"

# Create the job scheduler database
az sql db create \
  --server "sql-aspire-job-scheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --name "AspireJobScheduler" \
  --service-objective "S0" \
  --collation "SQL_Latin1_General_CP1_CI_AS"
```

### Connection String Configuration
```json
{
  "ConnectionStrings": {
    "JobSchedulerDatabase": "Server=tcp:sql-aspire-job-scheduler.database.windows.net,1433;Initial Catalog=AspireJobScheduler;Persist Security Info=False;User ID=sqladmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

## 🚌 Azure Service Bus Setup

### Create Service Bus Namespace
```bash
# Create Service Bus namespace
az servicebus namespace create \
  --name "sb-aspire-job-scheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --location "East US" \
  --sku "Standard"

# Create the jobs-pending queue
az servicebus queue create \
  --namespace-name "sb-aspire-job-scheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --name "jobs-pending" \
  --max-size 1024 \
  --message-ttl "PT1H" \
  --lock-duration "PT5M" \
  --max-delivery-count 5

# Create additional queues for different job types
az servicebus queue create \
  --namespace-name "sb-aspire-job-scheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --name "jobs-priority" \
  --max-size 1024 \
  --message-ttl "PT30M" \
  --lock-duration "PT5M" \
  --max-delivery-count 3

# Get connection string
az servicebus namespace authorization-rule keys list \
  --namespace-name "sb-aspire-job-scheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --name "RootManageSharedAccessKey" \
  --query "primaryConnectionString" \
  --output tsv
```

### Service Bus Configuration
```json
{
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://sb-aspire-job-scheduler.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YourKey",
    "Queues": {
      "JobsPending": "jobs-pending",
      "JobsPriority": "jobs-priority"
    }
  }
}
```

## 📦 Azure Blob Storage Setup

### Create Storage Account
```bash
# Create storage account
az storage account create \
  --name "staspirejobscheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --location "East US" \
  --sku "Standard_LRS" \
  --kind "StorageV2" \
  --access-tier "Hot"

# Get storage account key
STORAGE_KEY=$(az storage account keys list \
  --account-name "staspirejobscheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --query "[0].value" \
  --output tsv)

# Create blob containers
az storage container create \
  --name "job-modules" \
  --account-name "staspirejobscheduler" \
  --account-key "$STORAGE_KEY" \
  --public-access "off"

az storage container create \
  --name "job-logs" \
  --account-name "staspirejobscheduler" \
  --account-key "$STORAGE_KEY" \
  --public-access "off"

az storage container create \
  --name "job-outputs" \
  --account-name "staspirejobscheduler" \
  --account-key "$STORAGE_KEY" \
  --public-access "off"

az storage container create \
  --name "job-artifacts" \
  --account-name "staspirejobscheduler" \
  --account-key "$STORAGE_KEY" \
  --public-access "off"
```

### Storage Configuration
```json
{
  "BlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=staspirejobscheduler;AccountKey=YourStorageKey;EndpointSuffix=core.windows.net",
    "Containers": {
      "JobModules": "job-modules",
      "JobLogs": "job-logs", 
      "JobOutputs": "job-outputs",
      "JobArtifacts": "job-artifacts"
    }
  }
}
```

## 🐳 Azure Container Instances Setup

### Create Container Registry
```bash
# Create Azure Container Registry
az acr create \
  --name "acraspirejobscheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --location "East US" \
  --sku "Basic" \
  --admin-enabled true

# Get ACR credentials
az acr credential show \
  --name "acraspirejobscheduler" \
  --resource-group "rg-aspire-job-scheduler"
```

### Job Agent Container Template
```yaml
# job-agent-template.yaml
apiVersion: 2019-12-01
location: East US
name: job-agent-{instance-id}
properties:
  imageRegistryCredentials:
  - server: acraspirejobscheduler.azurecr.io
    username: acraspirejobscheduler
    password: {ACR_PASSWORD}
  containers:
  - name: job-agent
    properties:
      image: acraspirejobscheduler.azurecr.io/job-agent:latest
      resources:
        requests:
          cpu: 1
          memoryInGb: 2
      environmentVariables:
      - name: SERVICEBUS_CONNECTION_STRING
        secureValue: {SERVICE_BUS_CONNECTION_STRING}
      - name: DATABASE_CONNECTION_STRING
        secureValue: {DATABASE_CONNECTION_STRING}
      - name: BLOB_STORAGE_CONNECTION_STRING
        secureValue: {BLOB_STORAGE_CONNECTION_STRING}
      - name: AGENT_ID
        value: {AGENT_ID}
      - name: ENVIRONMENT
        value: Production
  osType: Linux
  restartPolicy: Always
tags:
  Project: AspireJobScheduler
  Component: JobAgent
type: Microsoft.ContainerInstance/containerGroups
```

### Container Instance Scaling Script
```bash
#!/bin/bash
# scale-job-agents.sh

RESOURCE_GROUP="rg-aspire-job-scheduler"
CONTAINER_NAME_PREFIX="job-agent"
DESIRED_COUNT=$1

if [ -z "$DESIRED_COUNT" ]; then
    echo "Usage: $0 <desired_count>"
    exit 1
fi

# Get current container instances
CURRENT_INSTANCES=$(az container list \
    --resource-group "$RESOURCE_GROUP" \
    --query "[?starts_with(name, '$CONTAINER_NAME_PREFIX')] | length(@)")

echo "Current instances: $CURRENT_INSTANCES"
echo "Desired instances: $DESIRED_COUNT"

# Scale up if needed
if [ "$DESIRED_COUNT" -gt "$CURRENT_INSTANCES" ]; then
    INSTANCES_TO_CREATE=$((DESIRED_COUNT - CURRENT_INSTANCES))
    echo "Creating $INSTANCES_TO_CREATE new instances..."
    
    for ((i=1; i<=INSTANCES_TO_CREATE; i++)); do
        INSTANCE_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
        AGENT_NAME="$CONTAINER_NAME_PREFIX-$INSTANCE_ID"
        
        echo "Creating container instance: $AGENT_NAME"
        az container create \
            --resource-group "$RESOURCE_GROUP" \
            --file "job-agent-template.yaml" \
            --name "$AGENT_NAME" \
            --environment-variables \
                AGENT_ID="$INSTANCE_ID" \
                AGENT_NAME="$AGENT_NAME"
    done
fi

# Scale down if needed  
if [ "$DESIRED_COUNT" -lt "$CURRENT_INSTANCES" ]; then
    INSTANCES_TO_DELETE=$((CURRENT_INSTANCES - DESIRED_COUNT))
    echo "Deleting $INSTANCES_TO_DELETE instances..."
    
    # Get list of container instances to delete
    INSTANCES_TO_DELETE_LIST=$(az container list \
        --resource-group "$RESOURCE_GROUP" \
        --query "[?starts_with(name, '$CONTAINER_NAME_PREFIX')] | [0:$INSTANCES_TO_DELETE].name" \
        --output tsv)
    
    for INSTANCE_NAME in $INSTANCES_TO_DELETE_LIST; do
        echo "Deleting container instance: $INSTANCE_NAME"
        az container delete \
            --resource-group "$RESOURCE_GROUP" \
            --name "$INSTANCE_NAME" \
            --yes
    done
fi

echo "Scaling complete."
```

## 🔐 Azure Key Vault Setup (Optional but Recommended)

### Create Key Vault for Secrets
```bash
# Create Key Vault
az keyvault create \
  --name "kv-aspire-job-scheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --location "East US" \
  --sku "standard"

# Store connection strings as secrets
az keyvault secret set \
  --vault-name "kv-aspire-job-scheduler" \
  --name "DatabaseConnectionString" \
  --value "Server=tcp:sql-aspire-job-scheduler.database.windows.net,1433;..."

az keyvault secret set \
  --vault-name "kv-aspire-job-scheduler" \
  --name "ServiceBusConnectionString" \
  --value "Endpoint=sb://sb-aspire-job-scheduler.servicebus.windows.net/;..."

az keyvault secret set \
  --vault-name "kv-aspire-job-scheduler" \
  --name "BlobStorageConnectionString" \
  --value "DefaultEndpointsProtocol=https;AccountName=staspirejobscheduler;..."
```

## 📈 Azure Monitor and Application Insights

### Create Application Insights
```bash
# Create Application Insights
az monitor app-insights component create \
  --app "ai-aspire-job-scheduler" \
  --location "East US" \
  --resource-group "rg-aspire-job-scheduler" \
  --application-type "web" \
  --kind "web"

# Get instrumentation key
az monitor app-insights component show \
  --app "ai-aspire-job-scheduler" \
  --resource-group "rg-aspire-job-scheduler" \
  --query "instrumentationKey" \
  --output tsv
```

### Monitoring Configuration
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key",
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/"
  },
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
      }
    }
  }
}
```

## 🔧 Aspire AppHost Configuration

### Updated Program.cs for AppHost
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server
var sqlServer = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("jobscheduler");

// Add Redis for caching
var redis = builder.AddRedis("redis")
    .WithDataVolume();

// Add Service Bus (when available in Aspire)
// var serviceBus = builder.AddAzureServiceBus("servicebus");

// Add Blob Storage (when available in Aspire)  
// var blobStorage = builder.AddAzureStorage("storage")
//     .AddBlobs("blobs");

// Add the main Blazor web application
var webApp = builder.AddProject<Projects.RFPResponseAPP>("webapp")
    .WithReference(sqlServer)
    .WithReference(redis)
    .WithEndpoint("https", endpoint =>
    {
        endpoint.Port = 7150;
        endpoint.IsProxied = false;
    });

// Add Job Scheduler Service
var jobScheduler = builder.AddProject<Projects.RFPAPP_JobScheduler>("jobscheduler")
    .WithReference(sqlServer)
    .WithReference(redis);

// Add Job Management API
var jobApi = builder.AddProject<Projects.RFPAPP_JobManagement_API>("jobapi")
    .WithReference(sqlServer)
    .WithReference(redis);

// In production, you would add actual Azure services:
// .WithReference(serviceBus)
// .WithReference(blobStorage);

builder.Build().Run();
```

## 🚀 Deployment Script

### Complete Deployment Automation
```bash
#!/bin/bash
# deploy-azure-infrastructure.sh

set -e

RESOURCE_GROUP="rg-aspire-job-scheduler"
LOCATION="East US"
SQL_ADMIN_PASSWORD="YourSecurePassword123!"

echo "Creating Azure infrastructure for Aspire Job Scheduler..."

# Create resource group
echo "Creating resource group..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION"

# Create SQL Server and Database
echo "Creating SQL Server and Database..."
az sql server create \
  --name "sql-aspire-job-scheduler" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --admin-user "sqladmin" \
  --admin-password "$SQL_ADMIN_PASSWORD"

az sql server firewall-rule create \
  --server "sql-aspire-job-scheduler" \
  --resource-group "$RESOURCE_GROUP" \
  --name "AllowAzureServices" \
  --start-ip-address "0.0.0.0" \
  --end-ip-address "0.0.0.0"

az sql db create \
  --server "sql-aspire-job-scheduler" \
  --resource-group "$RESOURCE_GROUP" \
  --name "AspireJobScheduler" \
  --service-objective "S0"

# Create Service Bus
echo "Creating Service Bus..."
az servicebus namespace create \
  --name "sb-aspire-job-scheduler" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --sku "Standard"

az servicebus queue create \
  --namespace-name "sb-aspire-job-scheduler" \
  --resource-group "$RESOURCE_GROUP" \
  --name "jobs-pending"

# Create Storage Account
echo "Creating Storage Account..."
az storage account create \
  --name "staspirejobscheduler" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --sku "Standard_LRS"

STORAGE_KEY=$(az storage account keys list \
  --account-name "staspirejobscheduler" \
  --resource-group "$RESOURCE_GROUP" \
  --query "[0].value" \
  --output tsv)

az storage container create \
  --name "job-modules" \
  --account-name "staspirejobscheduler" \
  --account-key "$STORAGE_KEY"

az storage container create \
  --name "job-logs" \
  --account-name "staspirejobscheduler" \
  --account-key "$STORAGE_KEY"

az storage container create \
  --name "job-outputs" \
  --account-name "staspirejobscheduler" \
  --account-key "$STORAGE_KEY"

# Create Container Registry
echo "Creating Container Registry..."
az acr create \
  --name "acraspirejobscheduler" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --sku "Basic" \
  --admin-enabled true

# Create Application Insights
echo "Creating Application Insights..."
az monitor app-insights component create \
  --app "ai-aspire-job-scheduler" \
  --location "$LOCATION" \
  --resource-group "$RESOURCE_GROUP" \
  --application-type "web"

echo "Azure infrastructure deployment complete!"
echo ""
echo "Next steps:"
echo "1. Update your appsettings.json with the connection strings"
echo "2. Run the database schema script"
echo "3. Build and push the job agent container image"
echo "4. Deploy your Aspire application"
```

## 📋 Configuration Checklist

### Pre-deployment Checklist
- [ ] Azure subscription with appropriate permissions
- [ ] Azure CLI installed and authenticated
- [ ] Resource group created
- [ ] Unique names chosen for all resources
- [ ] Secure passwords generated

### Post-deployment Checklist  
- [ ] SQL database schema deployed
- [ ] Connection strings updated in configuration
- [ ] Service Bus queues created and tested
- [ ] Blob storage containers created
- [ ] Container registry configured
- [ ] Application Insights configured
- [ ] Key Vault secrets stored (if using)
- [ ] Firewall rules configured
- [ ] Monitoring and alerting set up

### Security Checklist
- [ ] Strong passwords used for all services
- [ ] Firewall rules properly configured
- [ ] Key Vault used for sensitive configuration
- [ ] Container images scanned for vulnerabilities
- [ ] Network security groups configured
- [ ] Azure AD authentication enabled
- [ ] RBAC permissions properly assigned

---

This configuration provides a robust, scalable Azure infrastructure for the Aspire Job Scheduler solution with proper security, monitoring, and management capabilities.