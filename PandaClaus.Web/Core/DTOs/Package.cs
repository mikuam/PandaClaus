namespace PandaClaus.Web.Core.DTOs;

public record Package
{
    public int RowNumber { get; init; }
    public string LetterNumber { get; init; } = string.Empty;
    public int PackageNumber { get; init; }
    public int TotalPackages { get; init; }
    public Gabaryt Size { get; init; }
    public string PackageId { get; init; }
    public DateTime? DateExported { get; init; }
}