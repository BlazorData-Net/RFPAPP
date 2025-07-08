var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.RFPResponsePOC>("rfpresponsepoc");

builder.Build().Run();
