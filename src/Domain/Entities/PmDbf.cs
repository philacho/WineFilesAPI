namespace WineFilesApi.Domain.Entities;

public sealed class PmDbf
{
    public string TableName { get; set; } = string.Empty;   // fctblname
    public string CdxName { get; set; } = string.Empty;   // fccdxname
    public string TableType { get; set; } = string.Empty;   // fctype  (R/W/S/I)
    public string ParentTable { get; set; } = string.Empty;   // fcpartbl
    public string LinkField { get; set; } = string.Empty;   // fclinkfld
    public string Description { get; set; } = string.Empty;   // fcdescript
}