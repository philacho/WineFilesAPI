using Serilog;
using WineFilesApi.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    //makes the app run as a Windows Service
    builder.Host.UseWindowsService(options =>
    {
        options.ServiceName = "WineFilesApi";
    });

    var logFolder = builder.Configuration["Logging:LogFolder"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "Logs");
    var retainedFileCount = builder.Configuration.GetValue<int?>(
        "Logging:RetainedFileCountLimit") ?? 30;
    var fileSizeLimit = builder.Configuration.GetValue<long?>(
        "Logging:FileSizeLimitBytes") ?? 10_485_760;

    Directory.CreateDirectory(logFolder);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "WineFilesApi")
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine(logFolder, "winefiles-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: retainedFileCount,
                fileSizeLimitBytes: fileSizeLimit,
                rollOnFileSizeLimit: true,
                shared: true,
                outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} " +
                    "[{Level:u3}] " +
                    "{Message:lj}{NewLine}{Exception}");
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHealthChecks();
    builder.Services.AddWineFilesApi(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseWineFilesApiPipeline();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "WineFilesApi terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}