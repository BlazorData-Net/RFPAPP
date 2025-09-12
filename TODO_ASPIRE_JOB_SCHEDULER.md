# TODO: Microsoft Aspire Job Scheduling Solution Implementation

This document outlines the tasks needed to configure and implement a Microsoft Aspire solution for job scheduling with the specified features.

## 🏗️ Infrastructure & Database Setup

### Database Schema Implementation
- [ ] Create SQL Server database schema with the following tables:
  - [ ] **Jobs** - Master list of all job definitions
    - [ ] JobId (PK), Name, Description, Enabled, Queued, InProcess, InError, ModulePath, Created, Modified
  - [ ] **JobGroups** - Organizational groups for jobs  
    - [ ] JobGroupId (PK), Name, Description, Created, Modified
  - [ ] **Job_JobGroup** - Many-to-many mapping between Jobs and JobGroups
    - [ ] JobId (FK), JobGroupId (FK)
  - [ ] **JobSchedule** - Defines when jobs should run
    - [ ] JobScheduleId (PK), JobId (FK), RunEveryHour, StartTime, EndTime, DaysOfWeek, Enabled, Created, Modified
  - [ ] **JobInstance** - Individual execution requests
    - [ ] JobInstanceId (PK), JobId (FK), InProcess, HasError, StartTime, EndTime, Created, RequestedBy
  - [ ] **JobData** - Key-value parameters for jobs
    - [ ] JobDataId (PK), JobId (FK), JobInstanceId (FK), Key, StringValue, IntValue, DateTimeValue, Created
  - [ ] **JobAgents** - Registry of agent containers
    - [ ] JobAgentId (PK), Name, Enabled, InProcess, LastHeartbeat, Created, Modified
  - [ ] **JobLogs** - Execution logs
    - [ ] JobLogId (PK), JobInstanceId (FK), JobAgentId (FK), Message, IsError, Created

### Azure Services Configuration  
- [ ] **Azure Service Bus Setup**
  - [ ] Create Service Bus namespace in Azure
  - [ ] Create `jobs-pending` queue for job execution requests
  - [ ] Configure connection strings and authentication
  - [ ] Add Service Bus client configuration to Aspire AppHost

- [ ] **Azure Container Instances Setup**
  - [ ] Create container registry for job agent images
  - [ ] Design job agent container base image with .NET runtime
  - [ ] Configure container instance deployment templates
  - [ ] Set up auto-scaling policies for job agents

- [ ] **Azure Blob Storage Setup**
  - [ ] Create storage account for job artifacts
  - [ ] Create containers: `job-modules`, `job-logs`, `job-outputs`
  - [ ] Configure access policies and SAS tokens
  - [ ] Set up blob lifecycle management policies

## 🔧 Aspire AppHost Configuration

### Service Registration & Dependencies
- [ ] **Update RFPPOC.AppHost/Program.cs**
  - [ ] Add SQL Server dependency with connection string
  - [ ] Add Azure Service Bus dependency
  - [ ] Add Azure Blob Storage dependency  
  - [ ] Add Redis cache for job state management
  - [ ] Configure service discovery between components

### New Aspire Services
- [ ] **Job Scheduler Service**
  - [ ] Create new project: `RFPAPP.JobScheduler`
  - [ ] Implement background service for scanning JobSchedule table
  - [ ] Add logic to create JobInstance records for due jobs
  - [ ] Add Service Bus message publishing for job queue
  - [ ] Configure as hosted service in Aspire

- [ ] **Job Agent Service** 
  - [ ] Create new project: `RFPAPP.JobAgent`
  - [ ] Implement Service Bus message consumer
  - [ ] Add NuGet package download and execution logic
  - [ ] Add job logging and error handling
  - [ ] Configure for container deployment

- [ ] **Job Management API**
  - [ ] Create new project: `RFPAPP.JobManagement.API`
  - [ ] Implement REST endpoints for job CRUD operations
  - [ ] Add job scheduling and execution endpoints
  - [ ] Add file upload endpoints for NuGet packages
  - [ ] Configure authentication and authorization

## 🎨 Blazor Web Application Features

### Dashboard Implementation
- [ ] **Real-time Job Status Dashboard**
  - [ ] Create `Pages/Dashboard.razor` component
  - [ ] Implement SignalR hub for real-time updates
  - [ ] Add job status widgets (Running, Queued, Failed, Completed)
  - [ ] Add job execution timeline visualization
  - [ ] Add system health indicators (agents, queues, storage)

### Configuration Management
- [ ] **Queues Configuration**
  - [ ] Create `Pages/Configuration/Queues.razor`
  - [ ] Add forms for Service Bus queue configuration
  - [ ] Add queue monitoring and statistics
  - [ ] Add queue purge and management operations

- [ ] **Azure Container Service Configuration**
  - [ ] Create `Pages/Configuration/Containers.razor`
  - [ ] Add container instance configuration forms
  - [ ] Add container template management
  - [ ] Add scaling policy configuration

- [ ] **AI Configuration**
  - [ ] Create `Pages/Configuration/AI.razor`
  - [ ] Extend existing OpenAI settings for job processing
  - [ ] Add AI model selection for different job types
  - [ ] Add AI usage monitoring and limits

- [ ] **Storage Configuration**  
  - [ ] Create `Pages/Configuration/Storage.razor`
  - [ ] Add Azure Blob Storage configuration
  - [ ] Add storage quota and lifecycle management
  - [ ] Add storage monitoring and cleanup tools

### Job Scheduler & Management
- [ ] **Job Creation & Editing**
  - [ ] Create `Pages/Jobs/JobEditor.razor`
  - [ ] Add job definition forms (name, description, schedule)
  - [ ] Add job parameter configuration
  - [ ] Add job group assignment interface
  - [ ] Add job testing and validation tools

- [ ] **Code File Management**
  - [ ] Create `Pages/Jobs/CodeManagement.razor`
  - [ ] Add NuGet package upload functionality
  - [ ] Create online C#/Python code editor component
  - [ ] Integrate Monaco Editor for syntax highlighting
  - [ ] Add AI-assisted code generation features
  - [ ] Add code compilation and validation

### Logging & Monitoring
- [ ] **Job Logs Viewer**
  - [ ] Create `Pages/Logs/JobLogs.razor`
  - [ ] Add real-time log streaming with SignalR
  - [ ] Add log filtering and search capabilities
  - [ ] Add log export and download features
  - [ ] Add log retention and archival policies

## 🔄 Job Execution Flow Implementation

### Module Creation & Upload
- [ ] **NuGet Package Management**
  - [ ] Create upload API endpoint for .nupkg files
  - [ ] Add package validation and security scanning
  - [ ] Store packages in Azure Blob Storage
  - [ ] Register package metadata in Jobs table

### Job Scheduling Logic
- [ ] **Scheduler Service Implementation**
  - [ ] Background service to query JobSchedule table
  - [ ] Implement recurrence logic (hourly, daily, weekly patterns)
  - [ ] Handle start/stop time constraints
  - [ ] Create JobInstance records for due executions
  - [ ] Queue job messages to Service Bus

### Agent Container Orchestration
- [ ] **Job Agent Implementation**
  - [ ] Service Bus message consumer for job requests
  - [ ] Job context retrieval from SQL database
  - [ ] NuGet package download from Blob Storage
  - [ ] Secure job execution environment
  - [ ] Real-time logging to JobLogs table
  - [ ] Job completion status updates

### Monitoring & Management
- [ ] **Real-time Monitoring**
  - [ ] SignalR hub for job status broadcasting
  - [ ] Agent heartbeat monitoring
  - [ ] Queue depth monitoring  
  - [ ] Performance metrics collection
  - [ ] Error alerting and notifications

## 🔒 Security & Authentication

### Access Control
- [ ] **Authentication Integration**
  - [ ] Implement Azure AD authentication
  - [ ] Add role-based access control (Admin, User, Viewer)
  - [ ] Secure API endpoints with JWT tokens
  - [ ] Add audit logging for sensitive operations

### Job Security
- [ ] **Execution Sandboxing** 
  - [ ] Container-based job isolation
  - [ ] Resource limits and quotas
  - [ ] Network security policies
  - [ ] Code signing and validation

## 📊 Observability & Telemetry

### Application Insights Integration
- [ ] **Telemetry Configuration**
  - [ ] Add Application Insights to all services
  - [ ] Configure custom metrics for job performance
  - [ ] Add dependency tracking for Azure services
  - [ ] Set up alerting rules for critical failures

### Performance Monitoring
- [ ] **Metrics & Dashboards**
  - [ ] Job execution time metrics
  - [ ] Queue processing rates
  - [ ] Agent utilization statistics  
  - [ ] Storage usage and costs
  - [ ] AI API usage and costs

## 🧪 Testing & Validation

### Unit Testing
- [ ] **Service Testing**
  - [ ] Unit tests for job scheduling logic
  - [ ] Unit tests for job execution workflows
  - [ ] Unit tests for configuration management
  - [ ] Integration tests with Azure services

### End-to-End Testing
- [ ] **System Testing**
  - [ ] Test complete job lifecycle
  - [ ] Test failover and recovery scenarios
  - [ ] Test scaling under load
  - [ ] Test security boundaries

## 📚 Documentation

### Technical Documentation
- [ ] **Architecture Documentation**
  - [ ] System architecture diagrams
  - [ ] Data flow documentation
  - [ ] API documentation with OpenAPI
  - [ ] Deployment and configuration guides

### User Documentation  
- [ ] **User Guides**
  - [ ] Job creation and management guide
  - [ ] Configuration management guide
  - [ ] Troubleshooting and FAQ
  - [ ] Best practices for job development

---

## 🚀 Implementation Priority

**Phase 1: Foundation** (Weeks 1-2)
- Database schema implementation
- Basic Aspire service configuration
- Core job management API

**Phase 2: Scheduling** (Weeks 3-4)  
- Job scheduler service
- Basic job agent implementation
- Job execution workflow

**Phase 3: UI & Management** (Weeks 5-6)
- Blazor dashboard and configuration pages
- Job editor and code management
- Real-time monitoring

**Phase 4: Advanced Features** (Weeks 7-8)
- AI integration and assistance
- Advanced monitoring and alerting
- Security hardening and testing

**Phase 5: Polish & Deploy** (Weeks 9-10)
- Performance optimization
- Documentation completion
- Production deployment