namespace WineFilesApi.Domain.Entities;

public sealed class PmIndex
{
    public string CdxName { get; set; } = string.Empty;   // fccdxname
    public string TagName { get; set; } = string.Empty;   // fctagname
    public string TagKey { get; set; } = string.Empty;   // fmtagkey  (memo)
    public bool Unique { get; set; }                   // flunique  (logical)
    public bool Descending { get; set; }                   // fldescend (logical)
}