namespace WineFilesApi.Infrastructure.Repositories;

using System.Data.OleDb;
using System.Text.Json;
using System.Text.RegularExpressions;
using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Infrastructure.Data;

public sealed class DataRepository(
    FoxProConnectionFactory connectionFactory) : IDataRepository
{
    private static readonly Regex IdentifierRegex =
        new(
            @"^[A-Za-z_][A-Za-z0-9_]*$",
            RegexOptions.Compiled);

    private static readonly HashSet<string> AllowedOperators =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "=", "<>", "!=", ">", ">=", "<", "<=", "LIKE", "NOT LIKE"
        };

    public async Task<IReadOnlyList<Dictionary<string, object?>>> ExecuteQueryAsync(
    string sql, int maxRows, CancellationToken ct)
    {
        // 1. Validate the SQL — must be a single SELECT statement.
        SqlValidator.EnsureReadOnlySelect(sql);

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(ct);

        using var command = new OleDbCommand(sql, connection)
        {
            CommandTimeout = 30   // seconds
        };

        using var reader = await command.ExecuteReaderAsync(ct);

        var results = new List<Dictionary<string, object?>>();
        while (await reader.ReadAsync(ct) && results.Count < maxRows)
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.GetValue(i);
                row[name] = value == DBNull.Value ? null : value;
            }
            results.Add(row);
        }

        return results;
    }

    public async Task<IReadOnlyList<Dictionary<string, object?>>> GetDataAsync(
        string table,
        IReadOnlyList<string> columns,
        IReadOnlyList<FoxProFilter>? filters,
        CancellationToken ct)
    {
        ValidateIdentifier(table, "Table");

        if (columns == null || columns.Count == 0)
            throw new ArgumentException("At least one column must be specified.", nameof(columns));

        foreach (var column in columns)
            ValidateIdentifier(column, "Column");

        var selectList = string.Join(", ", columns);
        var sql = $"SELECT {selectList} FROM {table}";

        var parameters = new List<OleDbParameter>();

        if (filters is { Count: > 0 })
        {
            var whereConditions = new List<string>();

            foreach (var filter in filters)
            {
                if (filter == null)
                    throw new ArgumentException("Filter cannot be null.", nameof(filters));

                ValidateIdentifier(filter.Column, "Filter column");

                var operatorValue = NormalizeOperator(filter.Operator);
                whereConditions.Add($"{filter.Column} {operatorValue} ?");
                parameters.Add(CreateParameter(filter.Value));
            }

            sql += " WHERE " + string.Join(" AND ", whereConditions);
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(ct);

        using var command = new OleDbCommand(sql, connection);

        foreach (var parameter in parameters)
            command.Parameters.Add(parameter);

        using var reader = await command.ExecuteReaderAsync(ct);

        var results = new List<Dictionary<string, object?>>();

        while (await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.GetValue(i);
                row[columnName] = value == DBNull.Value ? null : value;
            }

            results.Add(row);
        }

        return results;
    }

    private static OleDbParameter CreateParameter(object? value)
    {
        // Unwrap JsonElement from System.Text.Json.
        if (value is JsonElement json)
            value = ConvertJsonElement(json);

        if (value is null or DBNull)
        {
            return new OleDbParameter
            {
                OleDbType = OleDbType.VarChar,
                Value = DBNull.Value
            };
        }

        return value switch
        {
            string s => new OleDbParameter { OleDbType = OleDbType.VarChar, Value = s },
            bool b => new OleDbParameter { OleDbType = OleDbType.Boolean, Value = b },
            int i => new OleDbParameter { OleDbType = OleDbType.Integer, Value = i },
            long l => new OleDbParameter { OleDbType = OleDbType.BigInt, Value = l },
            short sh => new OleDbParameter { OleDbType = OleDbType.SmallInt, Value = sh },
            decimal d => new OleDbParameter { OleDbType = OleDbType.Decimal, Value = d },
            double db => new OleDbParameter { OleDbType = OleDbType.Double, Value = db },
            float f => new OleDbParameter { OleDbType = OleDbType.Single, Value = f },
            DateTime dt => new OleDbParameter { OleDbType = OleDbType.Date, Value = dt },
            _ => new OleDbParameter { Value = value }
        };
    }

    private static object? ConvertJsonElement(JsonElement json)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.String:
                return json.GetString();

            case JsonValueKind.Number:
                if (json.TryGetInt32(out var i)) return i;
                if (json.TryGetInt64(out var l)) return l;
                if (json.TryGetDecimal(out var d)) return d;
                return json.GetDouble();

            case JsonValueKind.True: return true;
            case JsonValueKind.False: return false;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;

            case JsonValueKind.Object:
            case JsonValueKind.Array:
                throw new ArgumentException(
                    $"Unsupported JSON value kind '{json.ValueKind}' for filter value.");

            default:
                return json.ToString();
        }
    }

    private static string NormalizeOperator(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "=";

        var operatorValue = value.Trim().ToUpperInvariant();

        if (!AllowedOperators.Contains(operatorValue))
            throw new ArgumentException(
                $"Operator '{value}' is not supported. " +
                $"Allowed operators: {string.Join(", ", AllowedOperators)}");

        return operatorValue;
    }

    private static void ValidateIdentifier(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} is required.", parameterName);

        if (!IdentifierRegex.IsMatch(value))
            throw new ArgumentException(
                $"Invalid {parameterName.ToLowerInvariant()} '{value}'. " +
                "Only letters, numbers and underscore are allowed, " +
                "and the first character must be a letter or underscore.",
                parameterName);
    }
}