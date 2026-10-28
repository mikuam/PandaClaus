namespace PandaClaus.Web.Core;

public static class StatusHelper
{
    // Keep SPRAWDZONE readable for existing Sheets rows, but do not offer it for new updates.
    public static bool IsSelectableStatus(LetterStatus status) =>
        Enum.IsDefined(status) && status != LetterStatus.SPRAWDZONE;

    public static string GetStatusDescription(LetterStatus status)
    {
        return status switch
        {
            LetterStatus.NIE_WIADOMO => "Nie wiadomo czy są paczki",
            LetterStatus.DOSTARCZONE => "Dostarczono paczki",
            LetterStatus.W_TRAKCIE_SPRAWDZANIA => "Paczki w trakcie sprawdzania",
            LetterStatus.ODLOZONE => "Paczki odłożone do ponownego sprawdzenia",
            LetterStatus.SPRAWDZONE => "Paczki sprawdzone (do spakowania)",
            LetterStatus.SPAKOWANE => "Paczki sprawdzone i spakowane",
            LetterStatus.ZAADRESOWANE => "Paczki zaadresowane",
            _ => string.Empty
        };
    }
}

