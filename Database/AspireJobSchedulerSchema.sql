-- =====================================
-- Microsoft Aspire Job Scheduler Database Schema
-- =====================================

-- Create the main database (if needed)
-- CREATE DATABASE AspireJobScheduler;
-- GO
-- USE AspireJobScheduler;
-- GO

-- =====================================
-- TABLE: Jobs
-- Master list of all job definitions
-- =====================================
CREATE TABLE Jobs (
    JobId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    ModulePath NVARCHAR(500), -- Path to NuGet package in blob storage
    ModuleVersion NVARCHAR(50),
    EntryPoint NVARCHAR(255), -- Main class/method to execute
    Enabled BIT NOT NULL DEFAULT 1,
    Queued BIT NOT NULL DEFAULT 0,
    InProcess BIT NOT NULL DEFAULT 0,
    InError BIT NOT NULL DEFAULT 0,
    LastRunTime DATETIME2,
    NextRunTime DATETIME2,
    CreatedBy NVARCHAR(255),
    Created DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Modified DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_Jobs_Enabled (Enabled),
    INDEX IX_Jobs_Name (Name),
    INDEX IX_Jobs_NextRunTime (NextRunTime)
);

-- =====================================
-- TABLE: JobGroups  
-- Organizational groups for jobs
-- =====================================
CREATE TABLE JobGroups (
    JobGroupId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL UNIQUE,
    Description NVARCHAR(MAX),
    Color NVARCHAR(7), -- Hex color for UI display
    CreatedBy NVARCHAR(255),
    Created DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Modified DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_JobGroups_Name (Name)
);

-- =====================================
-- TABLE: Job_JobGroup
-- Many-to-many mapping between Jobs and JobGroups  
-- =====================================
CREATE TABLE Job_JobGroup (
    JobId UNIQUEIDENTIFIER NOT NULL,
    JobGroupId UNIQUEIDENTIFIER NOT NULL,
    Created DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    PRIMARY KEY (JobId, JobGroupId),
    FOREIGN KEY (JobId) REFERENCES Jobs(JobId) ON DELETE CASCADE,
    FOREIGN KEY (JobGroupId) REFERENCES JobGroups(JobGroupId) ON DELETE CASCADE
);

-- =====================================
-- TABLE: JobSchedule
-- Defines when jobs should run
-- =====================================
CREATE TABLE JobSchedule (
    JobScheduleId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    JobId UNIQUEIDENTIFIER NOT NULL,
    
    -- Recurrence settings
    RunEveryHour INT, -- Run every N hours (NULL for not hourly)
    RunEveryMinute INT, -- Run every N minutes (NULL for not minutely)
    
    -- Time constraints  
    StartTime TIME, -- Daily start time (NULL for no constraint)
    EndTime TIME,   -- Daily end time (NULL for no constraint)
    
    -- Day of week flags (bit flags: 1=Sunday, 2=Monday, 4=Tuesday, etc.)
    DaysOfWeek INT NOT NULL DEFAULT 127, -- Default: all days (1+2+4+8+16+32+64)
    
    -- Date range constraints
    ValidFrom DATETIME2,
    ValidTo DATETIME2,
    
    -- Cron expression (alternative to above settings)
    CronExpression NVARCHAR(100),
    
    Enabled BIT NOT NULL DEFAULT 1,
    CreatedBy NVARCHAR(255),
    Created DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Modified DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (JobId) REFERENCES Jobs(JobId) ON DELETE CASCADE,
    INDEX IX_JobSchedule_JobId (JobId),
    INDEX IX_JobSchedule_Enabled (Enabled)
);

-- =====================================
-- TABLE: JobInstance
-- Individual execution requests (scheduled or manual)
-- =====================================
CREATE TABLE JobInstance (
    JobInstanceId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    JobId UNIQUEIDENTIFIER NOT NULL,
    JobAgentId UNIQUEIDENTIFIER, -- Which agent is processing this
    
    -- Execution state
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Running, Completed, Failed, Cancelled
    InProcess BIT NOT NULL DEFAULT 0,
    HasError BIT NOT NULL DEFAULT 0,
    
    -- Timing information
    QueuedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    StartTime DATETIME2,
    EndTime DATETIME2,
    
    -- Execution details
    ExecutionContext NVARCHAR(MAX), -- JSON with execution parameters
    ErrorMessage NVARCHAR(MAX),
    Result NVARCHAR(MAX), -- JSON with execution results
    
    -- Tracking
    RequestedBy NVARCHAR(255), -- User who requested (for manual runs)
    RequestType NVARCHAR(50) NOT NULL DEFAULT 'Scheduled', -- Scheduled, Manual, Retry
    
    Created DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (JobId) REFERENCES Jobs(JobId) ON DELETE CASCADE,
    INDEX IX_JobInstance_JobId (JobId),
    INDEX IX_JobInstance_Status (Status),
    INDEX IX_JobInstance_QueuedTime (QueuedTime),
    INDEX IX_JobInstance_StartTime (StartTime)
);

-- =====================================
-- TABLE: JobData
-- Key-value parameters for jobs
-- =====================================
CREATE TABLE JobData (
    JobDataId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    JobId UNIQUEIDENTIFIER, -- NULL for instance-specific data
    JobInstanceId UNIQUEIDENTIFIER, -- NULL for job-level data
    
    -- Parameter definition
    [Key] NVARCHAR(255) NOT NULL,
    DataType NVARCHAR(50) NOT NULL, -- String, Integer, DateTime, Boolean, JSON
    
    -- Value storage (use appropriate field based on DataType)
    StringValue NVARCHAR(MAX),
    IntValue BIGINT,
    DateTimeValue DATETIME2,
    BooleanValue BIT,
    JsonValue NVARCHAR(MAX),
    
    -- Metadata
    Description NVARCHAR(500),
    IsRequired BIT NOT NULL DEFAULT 0,
    IsSecret BIT NOT NULL DEFAULT 0, -- For sensitive data (will be encrypted)
    
    Created DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (JobId) REFERENCES Jobs(JobId) ON DELETE CASCADE,
    FOREIGN KEY (JobInstanceId) REFERENCES JobInstance(JobInstanceId) ON DELETE CASCADE,
    INDEX IX_JobData_JobId (JobId),
    INDEX IX_JobData_JobInstanceId (JobInstanceId),
    INDEX IX_JobData_Key ([Key])
);

-- =====================================
-- TABLE: JobAgents
-- Registry of agent containers
-- =====================================
CREATE TABLE JobAgents (
    JobAgentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL UNIQUE,
    HostName NVARCHAR(255),
    ContainerInstanceId NVARCHAR(255), -- Azure Container Instance ID
    
    -- Agent state
    Enabled BIT NOT NULL DEFAULT 1,
    InProcess BIT NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Unknown', -- Online, Offline, Busy, Error
    
    -- Capabilities
    SupportedJobTypes NVARCHAR(500), -- Comma-separated list
    MaxConcurrentJobs INT NOT NULL DEFAULT 1,
    CurrentJobCount INT NOT NULL DEFAULT 0,
    
    -- Health monitoring
    LastHeartbeat DATETIME2,
    LastJobTime DATETIME2,
    TotalJobsProcessed INT NOT NULL DEFAULT 0,
    TotalJobsSucceeded INT NOT NULL DEFAULT 0,
    TotalJobsFailed INT NOT NULL DEFAULT 0,
    
    -- Agent metadata
    Version NVARCHAR(50),
    Environment NVARCHAR(100), -- Development, Staging, Production
    Region NVARCHAR(100),
    
    Created DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Modified DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_JobAgents_Name (Name),
    INDEX IX_JobAgents_Status (Status),
    INDEX IX_JobAgents_LastHeartbeat (LastHeartbeat)
);

-- =====================================
-- TABLE: JobLogs
-- Execution logs tied to specific job instances
-- =====================================
CREATE TABLE JobLogs (
    JobLogId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    JobInstanceId UNIQUEIDENTIFIER NOT NULL,
    JobAgentId UNIQUEIDENTIFIER,
    
    -- Log entry details
    LogLevel NVARCHAR(20) NOT NULL, -- Trace, Debug, Info, Warning, Error, Critical
    Message NVARCHAR(MAX) NOT NULL,
    Exception NVARCHAR(MAX),
    
    -- Log metadata
    Source NVARCHAR(255), -- Component that generated the log
    Category NVARCHAR(255), -- Log category/namespace
    EventId INT,
    
    -- Timing
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Flags for quick filtering
    IsError BIT NOT NULL DEFAULT 0,
    IsWarning BIT NOT NULL DEFAULT 0,
    
    FOREIGN KEY (JobInstanceId) REFERENCES JobInstance(JobInstanceId) ON DELETE CASCADE,
    FOREIGN KEY (JobAgentId) REFERENCES JobAgents(JobAgentId),
    INDEX IX_JobLogs_JobInstanceId (JobInstanceId),
    INDEX IX_JobLogs_Timestamp (Timestamp),
    INDEX IX_JobLogs_LogLevel (LogLevel),
    INDEX IX_JobLogs_IsError (IsError)
);

-- =====================================
-- TABLE: JobArtifacts
-- File artifacts produced by job executions
-- =====================================
CREATE TABLE JobArtifacts (
    JobArtifactId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    JobInstanceId UNIQUEIDENTIFIER NOT NULL,
    
    -- File information
    FileName NVARCHAR(255) NOT NULL,
    ContentType NVARCHAR(100),
    SizeBytes BIGINT,
    BlobPath NVARCHAR(500), -- Path in Azure Blob Storage
    
    -- Metadata
    ArtifactType NVARCHAR(100), -- Output, Log, Report, Data, etc.
    Description NVARCHAR(500),
    
    Created DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2, -- When artifact should be cleaned up
    
    FOREIGN KEY (JobInstanceId) REFERENCES JobInstance(JobInstanceId) ON DELETE CASCADE,
    INDEX IX_JobArtifacts_JobInstanceId (JobInstanceId),
    INDEX IX_JobArtifacts_ArtifactType (ArtifactType),
    INDEX IX_JobArtifacts_ExpiresAt (ExpiresAt)
);

-- =====================================
-- TABLE: SystemConfiguration
-- System-wide configuration settings
-- =====================================
CREATE TABLE SystemConfiguration (
    ConfigKey NVARCHAR(255) PRIMARY KEY,
    ConfigValue NVARCHAR(MAX),
    DataType NVARCHAR(50) NOT NULL DEFAULT 'String',
    Description NVARCHAR(500),
    Category NVARCHAR(100),
    IsSecret BIT NOT NULL DEFAULT 0,
    Modified DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(255)
);

-- =====================================
-- VIEWS FOR COMMON QUERIES
-- =====================================

-- View: Active Jobs with Next Run Times
CREATE VIEW vw_ActiveJobsWithSchedule AS
SELECT 
    j.JobId,
    j.Name,
    j.Description,
    j.Enabled,
    j.InProcess,
    j.InError,
    j.LastRunTime,
    j.NextRunTime,
    js.RunEveryHour,
    js.RunEveryMinute,
    js.DaysOfWeek,
    jg.Name AS GroupName
FROM Jobs j
LEFT JOIN JobSchedule js ON j.JobId = js.JobId AND js.Enabled = 1
LEFT JOIN Job_JobGroup jjg ON j.JobId = jjg.JobId
LEFT JOIN JobGroups jg ON jjg.JobGroupId = jg.JobGroupId
WHERE j.Enabled = 1;

-- View: Recent Job Executions
CREATE VIEW vw_RecentJobExecutions AS
SELECT 
    ji.JobInstanceId,
    ji.JobId,
    j.Name AS JobName,
    ji.Status,
    ji.QueuedTime,
    ji.StartTime,
    ji.EndTime,
    DATEDIFF(SECOND, ji.StartTime, COALESCE(ji.EndTime, GETUTCDATE())) AS DurationSeconds,
    ja.Name AS AgentName,
    ji.RequestType,
    ji.RequestedBy
FROM JobInstance ji
INNER JOIN Jobs j ON ji.JobId = j.JobId
LEFT JOIN JobAgents ja ON ji.JobAgentId = ja.JobAgentId
WHERE ji.Created >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY ji.Created DESC;

-- View: Agent Status Summary
CREATE VIEW vw_AgentStatus AS
SELECT 
    ja.JobAgentId,
    ja.Name,
    ja.Status,
    ja.CurrentJobCount,
    ja.MaxConcurrentJobs,
    ja.LastHeartbeat,
    CASE 
        WHEN ja.LastHeartbeat < DATEADD(MINUTE, -5, GETUTCDATE()) THEN 'Stale'
        WHEN ja.Status = 'Online' AND ja.CurrentJobCount < ja.MaxConcurrentJobs THEN 'Available'
        WHEN ja.Status = 'Online' AND ja.CurrentJobCount >= ja.MaxConcurrentJobs THEN 'Busy'
        ELSE ja.Status
    END AS EffectiveStatus,
    ja.TotalJobsProcessed,
    ja.TotalJobsSucceeded,
    ja.TotalJobsFailed,
    CASE 
        WHEN ja.TotalJobsProcessed > 0 
        THEN ROUND((ja.TotalJobsSucceeded * 100.0) / ja.TotalJobsProcessed, 2)
        ELSE 0
    END AS SuccessRate
FROM JobAgents ja;

-- =====================================
-- STORED PROCEDURES
-- =====================================

-- Procedure: Get next jobs to execute
CREATE PROCEDURE sp_GetPendingJobs
    @MaxJobs INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxJobs)
        ji.JobInstanceId,
        ji.JobId,
        j.Name,
        j.ModulePath,
        j.EntryPoint,
        ji.ExecutionContext
    FROM JobInstance ji
    INNER JOIN Jobs j ON ji.JobId = j.JobId
    WHERE ji.Status = 'Pending'
        AND j.Enabled = 1
    ORDER BY ji.QueuedTime;
END;

-- Procedure: Update job agent heartbeat
CREATE PROCEDURE sp_UpdateAgentHeartbeat
    @AgentId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = 'Online',
    @CurrentJobCount INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE JobAgents 
    SET LastHeartbeat = GETUTCDATE(),
        Status = @Status,
        CurrentJobCount = @CurrentJobCount,
        Modified = GETUTCDATE()
    WHERE JobAgentId = @AgentId;
END;

-- Procedure: Complete job instance
CREATE PROCEDURE sp_CompleteJobInstance
    @JobInstanceId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Result NVARCHAR(MAX) = NULL,
    @ErrorMessage NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @JobId UNIQUEIDENTIFIER;
    DECLARE @AgentId UNIQUEIDENTIFIER;
    
    -- Update job instance
    UPDATE JobInstance 
    SET Status = @Status,
        EndTime = GETUTCDATE(),
        InProcess = 0,
        HasError = CASE WHEN @Status IN ('Failed', 'Error') THEN 1 ELSE 0 END,
        Result = @Result,
        ErrorMessage = @ErrorMessage
    WHERE JobInstanceId = @JobInstanceId;
    
    -- Get job and agent info for further updates
    SELECT @JobId = JobId, @AgentId = JobAgentId 
    FROM JobInstance 
    WHERE JobInstanceId = @JobInstanceId;
    
    -- Update job status
    UPDATE Jobs 
    SET InProcess = CASE WHEN EXISTS(
        SELECT 1 FROM JobInstance 
        WHERE JobId = @JobId AND Status = 'Running'
    ) THEN 1 ELSE 0 END,
    LastRunTime = GETUTCDATE(),
    InError = CASE WHEN @Status IN ('Failed', 'Error') THEN 1 ELSE 0 END
    WHERE JobId = @JobId;
    
    -- Update agent statistics
    UPDATE JobAgents 
    SET CurrentJobCount = CurrentJobCount - 1,
        TotalJobsProcessed = TotalJobsProcessed + 1,
        TotalJobsSucceeded = CASE WHEN @Status = 'Completed' THEN TotalJobsSucceeded + 1 ELSE TotalJobsSucceeded END,
        TotalJobsFailed = CASE WHEN @Status IN ('Failed', 'Error') THEN TotalJobsFailed + 1 ELSE TotalJobsFailed END,
        LastJobTime = GETUTCDATE()
    WHERE JobAgentId = @AgentId;
END;

-- =====================================
-- INITIAL DATA
-- =====================================

-- Insert default job groups
INSERT INTO JobGroups (Name, Description, Color) VALUES
('System', 'System maintenance and monitoring jobs', '#007acc'),
('Data Processing', 'Data import, export, and transformation jobs', '#28a745'),
('Reports', 'Scheduled report generation jobs', '#ffc107'),
('AI/ML', 'Artificial intelligence and machine learning jobs', '#e83e8c'),
('Integration', 'Third-party system integration jobs', '#6f42c1');

-- Insert default system configuration
INSERT INTO SystemConfiguration (ConfigKey, ConfigValue, Description, Category) VALUES
('MaxConcurrentJobs', '10', 'Maximum number of jobs that can run simultaneously', 'Execution'),
('JobRetentionDays', '30', 'Number of days to keep completed job instances', 'Cleanup'),
('LogRetentionDays', '7', 'Number of days to keep job logs', 'Cleanup'),
('ArtifactRetentionDays', '14', 'Number of days to keep job artifacts', 'Cleanup'),
('DefaultJobTimeout', '3600', 'Default job timeout in seconds', 'Execution'),
('HeartbeatInterval', '30', 'Agent heartbeat interval in seconds', 'Monitoring'),
('QueuePollingInterval', '5', 'Job queue polling interval in seconds', 'Scheduling');

-- =====================================
-- TRIGGERS FOR AUDIT LOGGING
-- =====================================

-- Trigger to update Modified timestamp on Jobs
CREATE TRIGGER tr_Jobs_UpdateModified
ON Jobs
AFTER UPDATE
AS
BEGIN
    UPDATE Jobs 
    SET Modified = GETUTCDATE()
    FROM Jobs j
    INNER JOIN inserted i ON j.JobId = i.JobId;
END;

-- Trigger to update Modified timestamp on JobGroups  
CREATE TRIGGER tr_JobGroups_UpdateModified
ON JobGroups
AFTER UPDATE
AS
BEGIN
    UPDATE JobGroups 
    SET Modified = GETUTCDATE()
    FROM JobGroups jg
    INNER JOIN inserted i ON jg.JobGroupId = i.JobGroupId;
END;

-- =====================================
-- INDEXES FOR PERFORMANCE
-- =====================================

-- Additional indexes for common query patterns
CREATE INDEX IX_JobInstance_Status_QueuedTime ON JobInstance (Status, QueuedTime);
CREATE INDEX IX_JobLogs_JobInstanceId_Timestamp ON JobLogs (JobInstanceId, Timestamp);
CREATE INDEX IX_JobData_JobId_Key ON JobData (JobId, [Key]) WHERE JobInstanceId IS NULL;
CREATE INDEX IX_JobData_JobInstanceId_Key ON JobData (JobInstanceId, [Key]) WHERE JobId IS NULL;

PRINT 'Database schema created successfully!';
PRINT 'Next steps:';
PRINT '1. Configure connection strings in your Aspire AppHost';
PRINT '2. Create Entity Framework models based on this schema';
PRINT '3. Implement the job scheduler and agent services';