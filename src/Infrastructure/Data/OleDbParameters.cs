using System.Data.OleDb;

namespace WineFilesApi.Infrastructure.Data;

public static class OleDbParameters
{
    public static OleDbParameter VarChar(string? value) =>
        new() { OleDbType = OleDbType.VarChar, Value = value ?? string.Empty };

    public static OleDbParameter VarWChar(string? value) =>
        new() { OleDbType = OleDbType.VarWChar, Value = value ?? string.Empty };

    public static OleDbParameter Memo(string? value) =>
        new() { OleDbType = OleDbType.LongVarWChar, Value = value ?? string.Empty };

    public static OleDbParameter Integer(int value) =>
        new() { OleDbType = OleDbType.Integer, Value = value };

    public static OleDbParameter Numeric(decimal value) =>
        new() { OleDbType = OleDbType.Numeric, Value = value };

    public static OleDbParameter Date(DateTime? value) =>
        new() { OleDbType = OleDbType.Date, Value = (object?)value ?? DBNull.Value };

    public static OleDbParameter Logical(bool value) =>
        new() { OleDbType = OleDbType.Boolean, Value = value };
}