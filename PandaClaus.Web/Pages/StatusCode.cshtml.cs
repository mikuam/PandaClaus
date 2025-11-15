using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class StatusCodeModel : BasePageModel
{
    public int StatusCode { get; set; }
    public string StatusCodeMessage { get; set; } = "B??d";
    public string StatusCodeDescription { get; set; } = "Wyst?pi? b??d podczas przetwarzania ??dania.";
    public string? OriginalPath { get; set; }

    private readonly ILogger<StatusCodeModel> _logger;

    public StatusCodeModel(ILogger<StatusCodeModel> logger)
    {
        _logger = logger;
    }

    public void OnGet(int? statusCode)
    {
        StatusCode = statusCode ?? 500;
        OriginalPath = HttpContext.Request.Query["originalPath"].ToString();

        // Set user-friendly messages based on status code
        (StatusCodeMessage, StatusCodeDescription) = StatusCode switch
        {
            400 => ("Nieprawid?owe ??danie", "??danie zawiera nieprawid?owe dane lub parametry."),
            401 => ("Brak autoryzacji", "Musisz by? zalogowany, aby uzyska? dost?p do tej strony."),
            403 => ("Brak dost?pu", "Nie masz uprawnie? do wy?wietlenia tej strony."),
            404 => ("Strona nie znaleziona", "Strona, której szukasz, nie istnieje lub zosta?a przeniesiona."),
            405 => ("Metoda niedozwolona", "Ta operacja nie jest dozwolona dla tego zasobu."),
            408 => ("Przekroczono limit czasu", "??danie trwa?o zbyt d?ugo."),
            429 => ("Zbyt wiele ??da?", "Wys?ano zbyt wiele ??da? w krótkim czasie. Spróbuj ponownie pó?niej."),
            500 => ("B??d serwera", "Wyst?pi? wewn?trzny b??d serwera. Przepraszamy za niedogodno?ci."),
            502 => ("B??dna brama", "Serwer otrzyma? nieprawid?ow? odpowied?."),
            503 => ("Us?uga niedost?pna", "Serwis jest tymczasowo niedost?pny. Spróbuj ponownie pó?niej."),
            504 => ("Przekroczono limit czasu bramy", "Serwer nie otrzyma? odpowiedzi w wymaganym czasie."),
            _ => ($"B??d {StatusCode}", "Wyst?pi? nieoczekiwany b??d.")
        };

        _logger.LogWarning("Status code page accessed. StatusCode: {StatusCode}, Path: {Path}", 
            StatusCode, OriginalPath ?? "Unknown");
    }
}
