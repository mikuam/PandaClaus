namespace PandaClaus.Web.Core;

public static class StatusHelper
{
    public static string GetStatusDescription(LetterStatus status)
    {
        return status switch
        {
            LetterStatus.NIE_WIADOMO => "Nie wiadomo czy są paczki",
            LetterStatus.DOSTARCZONE => "Dostarczono paczki",
            LetterStatus.W_TRAKCIE_SPRAWDZANIA => "Paczki w trakcie sprawdzania",
            LetterStatus.ODLOZONE => "Paczki odłożone do ponownego sprawdzenia",
            LetterStatus.SPAKOWANE => "Paczki sprawdzone i spakowane",
            LetterStatus.ZAADRESOWANE => "Paczki zaadresowane",
            _ => string.Empty
        };
    }
}

