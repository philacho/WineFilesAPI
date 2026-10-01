namespace WineFilesApi.Domain.Entities;

public sealed class PmField
{
    public string TableName { get; set; } = string.Empty; // fctblname
    public string FieldName { get; set; } = string.Empty; // fcfldname
    public string DataType { get; set; } = string.Empty; // fcdatatype (C/N/D/L/M/I)
    public int FieldLen { get; set; }                 // fnfldlen
    public int Decimals { get; set; }                 // fnflddec
    public string ColPrompt { get; set; } = string.Empty; // fccolprmpt
    public string ColTitle { get; set; } = string.Empty; // fccoltitle
}