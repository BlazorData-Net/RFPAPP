var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.RFPResponsePOC>("rfpresponsepoc", launchProfileName: "https")
    .WithEndpoint("https", endpoint =>
    {
        endpoint.Port = 7150;          // ✅ Set custom port here so localStorage will work
        endpoint.IsProxied = false;    // ✅ Don't use Aspire's reverse proxy
    });

builder.Build().Run();