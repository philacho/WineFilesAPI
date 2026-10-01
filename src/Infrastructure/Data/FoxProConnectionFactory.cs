using System.Data.OleDb;

namespace WineFilesApi.Infrastructure.Data;

public sealed class FoxProConnectionFactory(IConfiguration configuration)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("FoxPro")
        ?? throw new InvalidOperationException("Connection string 'FoxPro' is missing.");

    public OleDbConnection CreateConnection() => new(_connectionString);
}