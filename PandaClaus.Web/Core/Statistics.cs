namespace PandaClaus.Web.Core;

public record Statistics
{
    public required int Letters { get; init; }
    public required int NieWiadomo { get; init; }
    public required int WiadomoPercentage { get; init; }
    public required int Wiadomo { get; init; }
    public required int Dostarczone { get; init; }
    public required int DostarczonePercentage { get; init; }
    public required int WTrakcieSprawdzania { get; init; }
    public required int WTrakcieSprawdzaniaPercentage { get; init; }
    public required int Odlozone { get; init; }
    public required int Sprawdzone { get; init; }
    public required int SprawdzonePercentage { get; init; }
    public required int DoSprawdzenia { get; init; }
    public required int Spakowane { get; init; }
    public required int SpakowanePercentage { get; init; }
    public required int Zaadresowane { get; init; }
    public required int ZaadresowanePercentage { get; init; }
    public required int ZaadresowanePaczki { get; init; }
}
