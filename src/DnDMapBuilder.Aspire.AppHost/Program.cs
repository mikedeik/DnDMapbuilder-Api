

var builder = DistributedApplication.CreateBuilder(args);

var apiProject = builder.AddProject<Projects.DnDMapBuilder_Api>("api")
    .WithExternalHttpEndpoints();

builder.Build().Run();
