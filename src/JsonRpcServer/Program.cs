using Bee.Api.AspNetCore;
using Bee.Base;
using Bee.Db.Manager;
using Bee.Definition;
using Bee.Definition.Database;
using Bee.Hosting;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var definePath = builder.Configuration["DefinePath"]
    ?? throw new InvalidOperationException("DefinePath is not configured.");
var absoluteDefinePath = Path.GetFullPath(definePath, AppContext.BaseDirectory);
if (!Directory.Exists(absoluteDefinePath))
    throw new DirectoryNotFoundException($"DefinePath directory not found: {absoluteDefinePath}");

var paths = new PathOptions { DefinePath = absoluteDefinePath };
var settings = SystemSettingsLoader.Load(paths);
SysInfo.Initialize(settings.CommonConfiguration);

DbProviderRegistry.Register(DatabaseType.SQLServer, SqlClientFactory.Instance);

builder.Services.AddBeeFramework(settings.BackendConfiguration, paths, autoCreateMasterKey: true);
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthorization();
app.UseBeeFramework();
app.MapControllers();

app.Run();
