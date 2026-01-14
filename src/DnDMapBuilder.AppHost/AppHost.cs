var builder = DistributedApplication.CreateBuilder(args);

// Disable IDE run session integration (use direct process launching)
builder.Configuration["RunSession:UseIdeRunSession"] = "false";

var sql = builder.AddSqlServer("Database")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("dndmapbuilder");

builder.AddProject<Projects.DnDMapBuilder_Api>("dndmapapi")
    .WithReference(sql)
    .WithEnvironment("ConnectionStrings__DefaultConnection", sql)
    .WaitFor(sql)
    .WithEnvironment("MIGRATIONS_EXECUTE", "true");

builder.Build().Run();
