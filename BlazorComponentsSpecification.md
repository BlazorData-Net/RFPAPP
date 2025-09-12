# Blazor UI Components Specification

This document outlines the Blazor components needed to implement the job scheduling web application interface.

## 🏗️ Component Architecture

### Layout Components
```
MainLayout.razor
├── NavMenu.razor
├── HeaderBar.razor
└── FooterBar.razor
```

### Page Components
```
Pages/
├── Dashboard.razor
├── Jobs/
│   ├── JobList.razor
│   ├── JobEditor.razor
│   ├── JobDetails.razor
│   └── JobScheduleEditor.razor
├── Configuration/
│   ├── Queues.razor
│   ├── Containers.razor
│   ├── AI.razor
│   └── Storage.razor
├── Logs/
│   ├── JobLogs.razor
│   └── SystemLogs.razor
└── CodeManagement/
    ├── ModuleUpload.razor
    ├── CodeEditor.razor
    └── ModuleManager.razor
```

### Shared Components
```
Components/
├── Charts/
│   ├── JobStatusChart.razor
│   ├── PerformanceChart.razor
│   └── AgentUtilizationChart.razor
├── Forms/
│   ├── JobForm.razor
│   ├── ScheduleForm.razor
│   └── ConfigurationForm.razor
├── Grids/
│   ├── JobGrid.razor
│   ├── LogGrid.razor
│   └── AgentGrid.razor
└── Common/
    ├── StatusBadge.razor
    ├── ProgressIndicator.razor
    ├── ActionButton.razor
    └── ConfirmDialog.razor
```

## 📊 Dashboard Components

### Dashboard.razor
**Purpose**: Main dashboard showing system overview and job status
**Features**:
- Real-time job status widgets
- System health indicators  
- Quick action buttons
- Performance metrics charts

```html
@page "/dashboard"
@using Microsoft.AspNetCore.SignalR.Client
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime
@implements IAsyncDisposable

<PageTitle>Dashboard - Job Scheduler</PageTitle>

<RadzenStack Orientation="Orientation.Vertical" Gap="20px">
    <!-- Status Overview Cards -->
    <RadzenRow Gap="20px">
        <RadzenColumn Size="3">
            <StatusCard Title="Running Jobs" 
                       Value="@runningJobs" 
                       Icon="play_circle" 
                       Color="success" />
        </RadzenColumn>
        <RadzenColumn Size="3">
            <StatusCard Title="Queued Jobs" 
                       Value="@queuedJobs" 
                       Icon="queue" 
                       Color="warning" />
        </RadzenColumn>
        <RadzenColumn Size="3">
            <StatusCard Title="Failed Jobs" 
                       Value="@failedJobs" 
                       Icon="error" 
                       Color="danger" />
        </RadzenColumn>
        <RadzenColumn Size="3">
            <StatusCard Title="Active Agents" 
                       Value="@activeAgents" 
                       Icon="computer" 
                       Color="info" />
        </RadzenColumn>
    </RadzenRow>

    <!-- Real-time Charts -->
    <RadzenRow Gap="20px">
        <RadzenColumn Size="8">
            <RadzenCard>
                <RadzenText TextStyle="TextStyle.H6" TagName="TagName.H3">
                    Job Execution Timeline
                </RadzenText>
                <JobExecutionChart Data="@executionData" />
            </RadzenCard>
        </RadzenColumn>
        <RadzenColumn Size="4">
            <RadzenCard>
                <RadzenText TextStyle="TextStyle.H6" TagName="TagName.H3">
                    System Health
                </RadzenText>
                <SystemHealthIndicator Health="@systemHealth" />
            </RadzenCard>
        </RadzenColumn>
    </RadzenRow>

    <!-- Recent Activity -->
    <RadzenCard>
        <RadzenText TextStyle="TextStyle.H6" TagName="TagName.H3">
            Recent Job Activity
        </RadzenText>
        <RecentJobGrid Jobs="@recentJobs" />
    </RadzenCard>
</RadzenStack>

@code {
    private HubConnection? hubConnection;
    private int runningJobs = 0;
    private int queuedJobs = 0;
    private int failedJobs = 0;
    private int activeAgents = 0;
    private List<JobExecutionData> executionData = new();
    private SystemHealthData systemHealth = new();
    private List<JobInstanceSummary> recentJobs = new();

    protected override async Task OnInitializedAsync()
    {
        // Initialize SignalR connection
        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/jobStatusHub"))
            .Build();

        hubConnection.On<JobStatusUpdate>("JobStatusUpdated", OnJobStatusUpdated);
        hubConnection.On<SystemHealthUpdate>("SystemHealthUpdated", OnSystemHealthUpdated);

        await hubConnection.StartAsync();
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        // Load dashboard data from API
        // Implementation details...
    }

    private async Task OnJobStatusUpdated(JobStatusUpdate update)
    {
        // Update UI with real-time job status changes
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnSystemHealthUpdated(SystemHealthUpdate update)
    {
        // Update system health indicators
        systemHealth = update.HealthData;
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }
}
```

### StatusCard.razor
**Purpose**: Reusable card component for displaying key metrics

```html
<RadzenCard Style="@($"border-left: 4px solid var(--rz-{Color})")">
    <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="10px">
        <RadzenIcon Icon="@Icon" Style="@($"font-size: 2rem; color: var(--rz-{Color})")" />
        <RadzenStack Orientation="Orientation.Vertical" Gap="0">
            <RadzenText TextStyle="TextStyle.H4" TagName="TagName.H4" Class="rz-mb-0">
                @Value.ToString("N0")
            </RadzenText>
            <RadzenText TextStyle="TextStyle.Body2" Class="rz-text-secondary-color">
                @Title
            </RadzenText>
        </RadzenStack>
    </RadzenStack>
</RadzenCard>

@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public int Value { get; set; }
    [Parameter] public string Icon { get; set; } = "";
    [Parameter] public string Color { get; set; } = "primary";
}
```

## 💼 Job Management Components

### JobList.razor
**Purpose**: Display and manage all jobs with filtering and search

```html
@page "/jobs"
@inject JobService JobService
@inject DialogService DialogService
@inject NotificationService NotificationService

<PageTitle>Jobs - Job Scheduler</PageTitle>

<RadzenStack Orientation="Orientation.Vertical" Gap="20px">
    <!-- Toolbar -->
    <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" JustifyContent="JustifyContent.SpaceBetween">
        <RadzenText TextStyle="TextStyle.H4" TagName="TagName.H1">Jobs</RadzenText>
        <RadzenStack Orientation="Orientation.Horizontal" Gap="10px">
            <RadzenButton Text="New Job" 
                         Icon="add" 
                         ButtonStyle="ButtonStyle.Primary" 
                         Click="@CreateNewJob" />
            <RadzenButton Text="Import Jobs" 
                         Icon="upload" 
                         ButtonStyle="ButtonStyle.Secondary" 
                         Click="@ImportJobs" />
        </RadzenStack>
    </RadzenStack>

    <!-- Filters -->
    <RadzenCard>
        <RadzenStack Orientation="Orientation.Horizontal" Gap="20px" AlignItems="AlignItems.End">
            <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
                <RadzenLabel Text="Search" />
                <RadzenTextBox @bind-Value="@searchText" 
                              Placeholder="Search jobs..." 
                              Style="width: 300px;" 
                              oninput="@OnSearchChanged" />
            </RadzenStack>
            <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
                <RadzenLabel Text="Group" />
                <RadzenDropDown @bind-Value="@selectedGroupId" 
                               Data="@jobGroups" 
                               TextProperty="Name" 
                               ValueProperty="JobGroupId" 
                               Placeholder="All Groups" 
                               AllowClear="true"
                               Style="width: 200px;" 
                               Change="@OnGroupFilterChanged" />
            </RadzenStack>
            <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
                <RadzenLabel Text="Status" />
                <RadzenDropDown @bind-Value="@selectedStatus" 
                               Data="@statusOptions" 
                               Placeholder="All Status" 
                               AllowClear="true"
                               Style="width: 150px;" 
                               Change="@OnStatusFilterChanged" />
            </RadzenStack>
            <RadzenButton Text="Clear Filters" 
                         Icon="clear" 
                         ButtonStyle="ButtonStyle.Light" 
                         Click="@ClearFilters" />
        </RadzenStack>
    </RadzenCard>

    <!-- Jobs Grid -->
    <RadzenDataGrid @ref="jobsGrid" 
                    Data="@jobs" 
                    TItem="JobSummary" 
                    AllowFiltering="true" 
                    AllowSorting="true" 
                    AllowPaging="true" 
                    PageSize="20"
                    PagerHorizontalAlign="HorizontalAlign.Left"
                    ShowPagingSummary="true">
        
        <Columns>
            <RadzenDataGridColumn TItem="JobSummary" Property="Name" Title="Job Name" Width="250px">
                <Template Context="job">
                    <RadzenLink Path="@($"/jobs/{job.JobId}")" Text="@job.Name" />
                </Template>
            </RadzenDataGridColumn>
            
            <RadzenDataGridColumn TItem="JobSummary" Property="GroupName" Title="Group" Width="150px" />
            
            <RadzenDataGridColumn TItem="JobSummary" Property="Status" Title="Status" Width="120px">
                <Template Context="job">
                    <StatusBadge Status="@job.Status" />
                </Template>
            </RadzenDataGridColumn>
            
            <RadzenDataGridColumn TItem="JobSummary" Property="LastRunTime" Title="Last Run" Width="180px">
                <Template Context="job">
                    @if (job.LastRunTime.HasValue)
                    {
                        <span title="@job.LastRunTime.Value.ToString("F")">
                            @job.LastRunTime.Value.ToString("yyyy-MM-dd HH:mm")
                        </span>
                    }
                    else
                    {
                        <span class="text-muted">Never</span>
                    }
                </Template>
            </RadzenDataGridColumn>
            
            <RadzenDataGridColumn TItem="JobSummary" Property="NextRunTime" Title="Next Run" Width="180px">
                <Template Context="job">
                    @if (job.NextRunTime.HasValue)
                    {
                        <span title="@job.NextRunTime.Value.ToString("F")">
                            @job.NextRunTime.Value.ToString("yyyy-MM-dd HH:mm")
                        </span>
                    }
                    else
                    {
                        <span class="text-muted">Not scheduled</span>
                    }
                </Template>
            </RadzenDataGridColumn>
            
            <RadzenDataGridColumn TItem="JobSummary" Property="Enabled" Title="Enabled" Width="100px">
                <Template Context="job">
                    <RadzenSwitch @bind-Value="job.Enabled" 
                                 Change="@(args => OnJobEnabledChanged(job, args))" />
                </Template>
            </RadzenDataGridColumn>
            
            <RadzenDataGridColumn TItem="JobSummary" Sortable="false" Filterable="false" Width="150px" Title="Actions">
                <Template Context="job">
                    <RadzenStack Orientation="Orientation.Horizontal" Gap="5px">
                        <RadzenButton Icon="play_arrow" 
                                     ButtonStyle="ButtonStyle.Success" 
                                     Size="ButtonSize.Small" 
                                     Click="@(() => RunJobNow(job))" 
                                     Disabled="@(!job.Enabled)"
                                     title="Run Now" />
                        <RadzenButton Icon="edit" 
                                     ButtonStyle="ButtonStyle.Primary" 
                                     Size="ButtonSize.Small" 
                                     Click="@(() => EditJob(job))" 
                                     title="Edit" />
                        <RadzenButton Icon="delete" 
                                     ButtonStyle="ButtonStyle.Danger" 
                                     Size="ButtonSize.Small" 
                                     Click="@(() => DeleteJob(job))" 
                                     title="Delete" />
                    </RadzenStack>
                </Template>
            </RadzenDataGridColumn>
        </Columns>
    </RadzenDataGrid>
</RadzenStack>

@code {
    private RadzenDataGrid<JobSummary> jobsGrid = new();
    private List<JobSummary> jobs = new();
    private List<JobGroup> jobGroups = new();
    private List<string> statusOptions = new() { "Enabled", "Disabled", "Running", "Error" };
    
    private string searchText = "";
    private Guid? selectedGroupId;
    private string? selectedStatus;

    protected override async Task OnInitializedAsync()
    {
        await LoadJobs();
        await LoadJobGroups();
    }

    private async Task LoadJobs()
    {
        jobs = await JobService.GetJobsAsync(searchText, selectedGroupId, selectedStatus);
    }

    private async Task LoadJobGroups()
    {
        jobGroups = await JobService.GetJobGroupsAsync();
    }

    private async Task OnSearchChanged(ChangeEventArgs e)
    {
        searchText = e.Value?.ToString() ?? "";
        await LoadJobs();
    }

    private async Task OnGroupFilterChanged(object value)
    {
        selectedGroupId = value as Guid?;
        await LoadJobs();
    }

    private async Task OnStatusFilterChanged(object value)
    {
        selectedStatus = value as string;
        await LoadJobs();
    }

    private async Task ClearFilters()
    {
        searchText = "";
        selectedGroupId = null;
        selectedStatus = null;
        await LoadJobs();
    }

    private async Task CreateNewJob()
    {
        var result = await DialogService.OpenAsync<JobEditor>("Create New Job",
            new Dictionary<string, object>(),
            new DialogOptions { Width = "800px", Height = "600px" });

        if (result != null)
        {
            await LoadJobs();
            NotificationService.Notify(NotificationSeverity.Success, "Success", "Job created successfully");
        }
    }

    private async Task EditJob(JobSummary job)
    {
        var result = await DialogService.OpenAsync<JobEditor>("Edit Job",
            new Dictionary<string, object> { { "JobId", job.JobId } },
            new DialogOptions { Width = "800px", Height = "600px" });

        if (result != null)
        {
            await LoadJobs();
            NotificationService.Notify(NotificationSeverity.Success, "Success", "Job updated successfully");
        }
    }

    private async Task RunJobNow(JobSummary job)
    {
        var confirmed = await DialogService.Confirm(
            $"Are you sure you want to run '{job.Name}' now?",
            "Run Job", new ConfirmOptions { OkButtonText = "Run", CancelButtonText = "Cancel" });

        if (confirmed == true)
        {
            await JobService.RunJobNowAsync(job.JobId);
            NotificationService.Notify(NotificationSeverity.Info, "Job Queued", $"'{job.Name}' has been queued for execution");
        }
    }

    private async Task DeleteJob(JobSummary job)
    {
        var confirmed = await DialogService.Confirm(
            $"Are you sure you want to delete '{job.Name}'? This action cannot be undone.",
            "Delete Job", new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed == true)
        {
            await JobService.DeleteJobAsync(job.JobId);
            await LoadJobs();
            NotificationService.Notify(NotificationSeverity.Success, "Success", "Job deleted successfully");
        }
    }

    private async Task OnJobEnabledChanged(JobSummary job, bool enabled)
    {
        await JobService.SetJobEnabledAsync(job.JobId, enabled);
        NotificationService.Notify(NotificationSeverity.Info, 
            enabled ? "Job Enabled" : "Job Disabled", 
            $"'{job.Name}' has been {(enabled ? "enabled" : "disabled")}");
    }
}
```

### JobEditor.razor
**Purpose**: Create and edit job definitions with schedules

```html
@inject JobService JobService
@inject NotificationService NotificationService

<RadzenTemplateForm TItem="JobEditModel" Data="@model" Submit="@OnSubmit">
    <RadzenStack Orientation="Orientation.Vertical" Gap="20px">
        <!-- Basic Information -->
        <RadzenFieldset Text="Basic Information">
            <RadzenStack Orientation="Orientation.Vertical" Gap="15px">
                <RadzenRow Gap="20px">
                    <RadzenColumn Size="6">
                        <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
                            <RadzenLabel Text="Job Name" />
                            <RadzenTextBox @bind-Value="@model.Name" 
                                          Style="width: 100%;" 
                                          MaxLength="255" />
                            <RadzenRequiredValidator Component="Name" Text="Job name is required" />
                        </RadzenStack>
                    </RadzenColumn>
                    <RadzenColumn Size="6">
                        <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
                            <RadzenLabel Text="Job Group" />
                            <RadzenDropDown @bind-Value="@model.JobGroupId" 
                                           Data="@jobGroups" 
                                           TextProperty="Name" 
                                           ValueProperty="JobGroupId" 
                                           Placeholder="Select a group..." 
                                           Style="width: 100%;" />
                        </RadzenStack>
                    </RadzenColumn>
                </RadzenRow>
                
                <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
                    <RadzenLabel Text="Description" />
                    <RadzenTextArea @bind-Value="@model.Description" 
                                   Rows="3" 
                                   Style="width: 100%;" />
                </RadzenStack>
            </RadzenStack>
        </RadzenFieldset>

        <!-- Module Configuration -->
        <RadzenFieldset Text="Module Configuration">
            <RadzenStack Orientation="Orientation.Vertical" Gap="15px">
                <RadzenRow Gap="20px">
                    <RadzenColumn Size="8">
                        <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
                            <RadzenLabel Text="Module Path" />
                            <RadzenTextBox @bind-Value="@model.ModulePath" 
                                          Style="width: 100%;" 
                                          Placeholder="Path to NuGet package in blob storage" />
                        </RadzenStack>
                    </RadzenColumn>
                    <RadzenColumn Size="4">
                        <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
                            <RadzenLabel Text="Version" />
                            <RadzenTextBox @bind-Value="@model.ModuleVersion" 
                                          Style="width: 100%;" 
                                          Placeholder="1.0.0" />
                        </RadzenStack>
                    </RadzenColumn>
                </RadzenRow>
                
                <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
                    <RadzenLabel Text="Entry Point" />
                    <RadzenTextBox @bind-Value="@model.EntryPoint" 
                                  Style="width: 100%;" 
                                  Placeholder="MyNamespace.MyClass.Execute" />
                </RadzenStack>
            </RadzenStack>
        </RadzenFieldset>

        <!-- Schedule Configuration -->
        <RadzenFieldset Text="Schedule Configuration">
            <ScheduleEditor @bind-Schedule="@model.Schedule" />
        </RadzenFieldset>

        <!-- Job Parameters -->
        <RadzenFieldset Text="Job Parameters">
            <JobParametersEditor @bind-Parameters="@model.Parameters" />
        </RadzenFieldset>

        <!-- Actions -->
        <RadzenStack Orientation="Orientation.Horizontal" Gap="10px" JustifyContent="JustifyContent.End">
            <RadzenButton Text="Cancel" 
                         ButtonStyle="ButtonStyle.Light" 
                         Click="@Cancel" />
            <RadzenButton Text="Save" 
                         ButtonType="ButtonType.Submit" 
                         ButtonStyle="ButtonStyle.Primary" 
                         IsBusy="@isSaving" />
        </RadzenStack>
    </RadzenStack>
</RadzenTemplateForm>

@code {
    [Parameter] public Guid? JobId { get; set; }
    [CascadingParameter] public DialogService DialogService { get; set; } = default!;

    private JobEditModel model = new();
    private List<JobGroup> jobGroups = new();
    private bool isSaving = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadJobGroups();
        
        if (JobId.HasValue)
        {
            await LoadJob(JobId.Value);
        }
    }

    private async Task LoadJobGroups()
    {
        jobGroups = await JobService.GetJobGroupsAsync();
    }

    private async Task LoadJob(Guid jobId)
    {
        var job = await JobService.GetJobAsync(jobId);
        if (job != null)
        {
            model = JobEditModel.FromJob(job);
        }
    }

    private async Task OnSubmit()
    {
        try
        {
            isSaving = true;
            
            if (JobId.HasValue)
            {
                await JobService.UpdateJobAsync(JobId.Value, model);
            }
            else
            {
                await JobService.CreateJobAsync(model);
            }
            
            DialogService.Close(true);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            isSaving = false;
        }
    }

    private void Cancel()
    {
        DialogService.Close(false);
    }
}
```

## ⚙️ Configuration Components

### Queues.razor
**Purpose**: Manage Service Bus queue configuration and monitoring

```html
@page "/configuration/queues"
@inject QueueService QueueService

<PageTitle>Queue Configuration - Job Scheduler</PageTitle>

<RadzenStack Orientation="Orientation.Vertical" Gap="20px">
    <RadzenText TextStyle="TextStyle.H4" TagName="TagName.H1">Queue Configuration</RadzenText>
    
    <!-- Queue Status Overview -->
    <RadzenCard>
        <RadzenText TextStyle="TextStyle.H6" TagName="TagName.H3">Queue Status</RadzenText>
        <RadzenDataGrid Data="@queueStats" TItem="QueueStatistics">
            <Columns>
                <RadzenDataGridColumn TItem="QueueStatistics" Property="Name" Title="Queue Name" />
                <RadzenDataGridColumn TItem="QueueStatistics" Property="ActiveMessages" Title="Active Messages" />
                <RadzenDataGridColumn TItem="QueueStatistics" Property="DeadLetterMessages" Title="Dead Letter" />
                <RadzenDataGridColumn TItem="QueueStatistics" Property="MaxSizeInMegabytes" Title="Max Size (MB)" />
                <RadzenDataGridColumn TItem="QueueStatistics" Property="Status" Title="Status">
                    <Template Context="queue">
                        <StatusBadge Status="@queue.Status" />
                    </Template>
                </RadzenDataGridColumn>
                <RadzenDataGridColumn TItem="QueueStatistics" Sortable="false" Title="Actions">
                    <Template Context="queue">
                        <RadzenStack Orientation="Orientation.Horizontal" Gap="5px">
                            <RadzenButton Icon="refresh" 
                                         ButtonStyle="ButtonStyle.Primary" 
                                         Size="ButtonSize.Small" 
                                         Click="@(() => RefreshQueue(queue.Name))" 
                                         title="Refresh" />
                            <RadzenButton Icon="clear_all" 
                                         ButtonStyle="ButtonStyle.Warning" 
                                         Size="ButtonSize.Small" 
                                         Click="@(() => PurgeQueue(queue.Name))" 
                                         title="Purge Messages" />
                        </RadzenStack>
                    </Template>
                </RadzenDataGridColumn>
            </Columns>
        </RadzenDataGrid>
    </RadzenCard>

    <!-- Queue Configuration -->
    <RadzenCard>
        <RadzenText TextStyle="TextStyle.H6" TagName="TagName.H3">Queue Settings</RadzenText>
        <RadzenTemplateForm TItem="QueueConfiguration" Data="@queueConfig" Submit="@SaveQueueConfig">
            <!-- Configuration form fields -->
        </RadzenTemplateForm>
    </RadzenCard>
</RadzenStack>

@code {
    private List<QueueStatistics> queueStats = new();
    private QueueConfiguration queueConfig = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadQueueStatistics();
        await LoadQueueConfiguration();
    }

    // Implementation methods...
}
```

## 📝 Code Management Components

### CodeEditor.razor
**Purpose**: Online code editor with syntax highlighting and AI assistance

```html
@inject IJSRuntime JSRuntime
@inject AIService AIService

<RadzenStack Orientation="Orientation.Vertical" Gap="10px" Style="height: 600px;">
    <!-- Editor Toolbar -->
    <RadzenStack Orientation="Orientation.Horizontal" Gap="10px" AlignItems="AlignItems.Center">
        <RadzenDropDown @bind-Value="@selectedLanguage" 
                       Data="@languages" 
                       Style="width: 150px;" 
                       Change="@OnLanguageChanged" />
        <RadzenButton Text="AI Assist" 
                     Icon="auto_awesome" 
                     ButtonStyle="ButtonStyle.Secondary" 
                     Click="@ShowAIAssist" />
        <RadzenButton Text="Format Code" 
                     Icon="code" 
                     ButtonStyle="ButtonStyle.Light" 
                     Click="@FormatCode" />
        <RadzenSeparator Orientation="Orientation.Vertical" />
        <RadzenButton Text="Save" 
                     Icon="save" 
                     ButtonStyle="ButtonStyle.Primary" 
                     Click="@SaveCode" />
    </RadzenStack>

    <!-- Monaco Editor Container -->
    <div id="monaco-editor" style="flex: 1; border: 1px solid #ddd;"></div>

    <!-- AI Assistance Panel -->
    @if (showAIPanel)
    {
        <RadzenCard Style="height: 200px;">
            <RadzenStack Orientation="Orientation.Vertical" Gap="10px">
                <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" JustifyContent="JustifyContent.SpaceBetween">
                    <RadzenText TextStyle="TextStyle.H6">AI Code Assistant</RadzenText>
                    <RadzenButton Icon="close" 
                                 ButtonStyle="ButtonStyle.Light" 
                                 Size="ButtonSize.Small" 
                                 Click="@(() => showAIPanel = false)" />
                </RadzenStack>
                <RadzenTextArea @bind-Value="@aiPrompt" 
                               Placeholder="Describe what you want the code to do..." 
                               Rows="3" 
                               Style="width: 100%;" />
                <RadzenStack Orientation="Orientation.Horizontal" Gap="10px">
                    <RadzenButton Text="Generate Code" 
                                 Icon="auto_awesome" 
                                 ButtonStyle="ButtonStyle.Primary" 
                                 Click="@GenerateCode" 
                                 IsBusy="@isGenerating" />
                    <RadzenButton Text="Explain Code" 
                                 Icon="help" 
                                 ButtonStyle="ButtonStyle.Secondary" 
                                 Click="@ExplainCode" />
                    <RadzenButton Text="Optimize" 
                                 Icon="tune" 
                                 ButtonStyle="ButtonStyle.Success" 
                                 Click="@OptimizeCode" />
                </RadzenStack>
            </RadzenStack>
        </RadzenCard>
    }
</RadzenStack>

@code {
    [Parameter] public string InitialCode { get; set; } = "";
    [Parameter] public string Language { get; set; } = "csharp";
    [Parameter] public EventCallback<string> OnCodeChanged { get; set; }

    private string[] languages = { "csharp", "python", "javascript", "sql" };
    private string selectedLanguage = "csharp";
    private bool showAIPanel = false;
    private string aiPrompt = "";
    private bool isGenerating = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeMonacoEditor();
        }
    }

    private async Task InitializeMonacoEditor()
    {
        await JSRuntime.InvokeVoidAsync("initializeMonaco", "monaco-editor", Language, InitialCode);
    }

    private async Task OnLanguageChanged()
    {
        await JSRuntime.InvokeVoidAsync("changeMonacoLanguage", selectedLanguage);
    }

    private void ShowAIAssist()
    {
        showAIPanel = true;
    }

    private async Task GenerateCode()
    {
        try
        {
            isGenerating = true;
            var currentCode = await JSRuntime.InvokeAsync<string>("getMonacoValue");
            var generatedCode = await AIService.GenerateCodeAsync(aiPrompt, selectedLanguage, currentCode);
            await JSRuntime.InvokeVoidAsync("setMonacoValue", generatedCode);
        }
        finally
        {
            isGenerating = false;
        }
    }

    private async Task ExplainCode()
    {
        var currentCode = await JSRuntime.InvokeAsync<string>("getMonacoValue");
        var explanation = await AIService.ExplainCodeAsync(currentCode, selectedLanguage);
        // Show explanation in a dialog or panel
    }

    private async Task OptimizeCode()
    {
        var currentCode = await JSRuntime.InvokeAsync<string>("getMonacoValue");
        var optimizedCode = await AIService.OptimizeCodeAsync(currentCode, selectedLanguage);
        await JSRuntime.InvokeVoidAsync("setMonacoValue", optimizedCode);
    }

    private async Task FormatCode()
    {
        await JSRuntime.InvokeVoidAsync("formatMonacoCode");
    }

    private async Task SaveCode()
    {
        var code = await JSRuntime.InvokeAsync<string>("getMonacoValue");
        await OnCodeChanged.InvokeAsync(code);
    }
}
```

## 📊 Charts and Visualization Components

### JobExecutionChart.razor
**Purpose**: Real-time chart showing job execution timeline

```html
@inject IJSRuntime JSRuntime

<div id="job-execution-chart" style="height: 300px;"></div>

@code {
    [Parameter] public List<JobExecutionData> Data { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeChart();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Data?.Any() == true)
        {
            await UpdateChart();
        }
    }

    private async Task InitializeChart()
    {
        await JSRuntime.InvokeVoidAsync("initializeJobExecutionChart", "job-execution-chart");
    }

    private async Task UpdateChart()
    {
        await JSRuntime.InvokeVoidAsync("updateJobExecutionChart", Data);
    }
}
```

### SystemHealthIndicator.razor
**Purpose**: Visual indicator of system health status

```html
<RadzenStack Orientation="Orientation.Vertical" Gap="15px">
    <!-- Overall Health Score -->
    <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="10px">
        <RadzenText TextStyle="TextStyle.H5">System Health</RadzenText>
        <RadzenBadge BadgeStyle="@GetHealthBadgeStyle()" Text="@GetHealthText()" />
    </RadzenStack>

    <!-- Health Metrics -->
    <RadzenStack Orientation="Orientation.Vertical" Gap="10px">
        <HealthMetric Title="Database" 
                     Status="@Health.DatabaseStatus" 
                     ResponseTime="@Health.DatabaseResponseTime" />
        <HealthMetric Title="Service Bus" 
                     Status="@Health.ServiceBusStatus" 
                     ResponseTime="@Health.ServiceBusResponseTime" />
        <HealthMetric Title="Blob Storage" 
                     Status="@Health.BlobStorageStatus" 
                     ResponseTime="@Health.BlobStorageResponseTime" />
        <HealthMetric Title="Job Agents" 
                     Status="@Health.AgentsStatus" 
                     ActiveCount="@Health.ActiveAgentsCount" 
                     TotalCount="@Health.TotalAgentsCount" />
    </RadzenStack>

    <!-- Resource Utilization -->
    <RadzenStack Orientation="Orientation.Vertical" Gap="5px">
        <RadzenText TextStyle="TextStyle.Subtitle2">CPU Usage</RadzenText>
        <RadzenProgressBar Value="@Health.CpuUsagePercent" 
                          Max="100" 
                          Style="@GetProgressBarStyle(Health.CpuUsagePercent)" />
        
        <RadzenText TextStyle="TextStyle.Subtitle2">Memory Usage</RadzenText>
        <RadzenProgressBar Value="@Health.MemoryUsagePercent" 
                          Max="100" 
                          Style="@GetProgressBarStyle(Health.MemoryUsagePercent)" />
    </RadzenStack>
</RadzenStack>

@code {
    [Parameter] public SystemHealthData Health { get; set; } = new();

    private BadgeStyle GetHealthBadgeStyle()
    {
        return Health.OverallStatus switch
        {
            "Healthy" => BadgeStyle.Success,
            "Warning" => BadgeStyle.Warning,
            "Critical" => BadgeStyle.Danger,
            _ => BadgeStyle.Secondary
        };
    }

    private string GetHealthText()
    {
        return $"{Health.OverallStatus} ({Health.HealthScore}%)";
    }

    private string GetProgressBarStyle(double value)
    {
        var color = value switch
        {
            < 70 => "success",
            < 85 => "warning", 
            _ => "danger"
        };
        return $"--rz-progressbar-value-background: var(--rz-{color});";
    }
}
```

## 🔧 JavaScript Integration

### monaco-editor.js
**Purpose**: Monaco Editor integration for code editing

```javascript
// Monaco Editor initialization and helper functions
window.monacoEditor = null;

window.initializeMonaco = (containerId, language, initialValue) => {
    require.config({ paths: { vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.39.0/min/vs' } });
    
    require(['vs/editor/editor.main'], function () {
        window.monacoEditor = monaco.editor.create(document.getElementById(containerId), {
            value: initialValue || '',
            language: language,
            theme: 'vs-dark',
            automaticLayout: true,
            fontSize: 14,
            minimap: { enabled: false },
            scrollBeyondLastLine: false,
            folding: true,
            lineNumbers: 'on',
            renderLineHighlight: 'all'
        });
    });
};

window.getMonacoValue = () => {
    return window.monacoEditor ? window.monacoEditor.getValue() : '';
};

window.setMonacoValue = (value) => {
    if (window.monacoEditor) {
        window.monacoEditor.setValue(value);
    }
};

window.changeMonacoLanguage = (language) => {
    if (window.monacoEditor) {
        monaco.editor.setModelLanguage(window.monacoEditor.getModel(), language);
    }
};

window.formatMonacoCode = () => {
    if (window.monacoEditor) {
        window.monacoEditor.trigger('anyString', 'editor.action.formatDocument');
    }
};
```

### charts.js
**Purpose**: Chart.js integration for data visualization

```javascript
// Chart.js helper functions
window.initializeJobExecutionChart = (containerId) => {
    const ctx = document.getElementById(containerId).getContext('2d');
    
    window.jobExecutionChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Jobs Completed',
                data: [],
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.1)',
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true
                }
            },
            plugins: {
                legend: {
                    display: true
                }
            }
        }
    });
};

window.updateJobExecutionChart = (data) => {
    if (window.jobExecutionChart && data) {
        window.jobExecutionChart.data.labels = data.map(d => d.timestamp);
        window.jobExecutionChart.data.datasets[0].data = data.map(d => d.completedJobs);
        window.jobExecutionChart.update();
    }
};
```

## 📱 Responsive Design Considerations

### Breakpoint Classes
```css
/* Mobile-first responsive design */
.job-card {
    padding: 1rem;
}

@media (min-width: 768px) {
    .job-card {
        padding: 1.5rem;
    }
}

@media (max-width: 767px) {
    .job-grid-actions {
        flex-direction: column;
        gap: 0.25rem;
    }
    
    .job-grid-actions .rz-button {
        width: 100%;
        font-size: 0.875rem;
    }
}

/* Hide certain columns on mobile */
@media (max-width: 991px) {
    .hide-on-mobile {
        display: none !important;
    }
}
```

## 🎨 Styling and Theming

### Custom CSS Variables
```css
:root {
    --job-scheduler-primary: #007acc;
    --job-scheduler-success: #28a745;
    --job-scheduler-warning: #ffc107;
    --job-scheduler-danger: #dc3545;
    --job-scheduler-info: #17a2b8;
    --job-scheduler-light: #f8f9fa;
    --job-scheduler-dark: #343a40;
}

.status-running { color: var(--job-scheduler-success); }
.status-pending { color: var(--job-scheduler-warning); }
.status-failed { color: var(--job-scheduler-danger); }
.status-completed { color: var(--job-scheduler-info); }
```

This comprehensive Blazor UI specification provides all the components needed to implement the job scheduling web application with modern, responsive design and rich interactive features.