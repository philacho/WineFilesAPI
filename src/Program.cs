using WineFilesApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddWineFilesApi(builder.Configuration);   // ← was AddFoxProApi

var app = builder.Build();
app.UseWineFilesApiPipeline();                              // ← was UseFoxProApiPipeline
app.Run();