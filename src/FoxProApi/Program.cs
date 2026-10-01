using WineFilesApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddFoxProApi(builder.Configuration);

var app = builder.Build();
app.UseFoxProApiPipeline();
app.Run();