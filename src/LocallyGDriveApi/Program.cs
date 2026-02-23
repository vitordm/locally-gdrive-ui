using LocallyGDriveApi.Logging;
using LocallyGDriveApi.Middlewares;
using LocallyGDriveApi.Routes;
using LocallyGDriveApi.Shared;
using Newtonsoft.Json;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new SimpleJsonFormatter())
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "InventoryServiceApi")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(new SimpleJsonFormatter()));


builder.Configuration.AddEnvironmentVariables();
builder.Services.AddServices(builder.Configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseMiddlewares();
app.MapControllers();
app.RegisterApplicatonRoutes();

app.Run();
