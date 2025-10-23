using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaClaus.Web.Core;

namespace PandaClaus.Web.Pages;

public class ReceiveConfirmationModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;
    private readonly IConfiguration _configuration;

    [BindProperty]
    public Letter? Letter { get; set; }

    [BindProperty]
    public int NumberOfBoxes { get; set; } = 1;

    [BindProperty]
    public string SecurityCode { get; set; }

    [BindProperty]
    public bool? IsConfirmed { get; set; }

    public string? Message { get; set; }
    public bool ShowForm { get; set; } = true;

    public ReceiveConfirmationModel(GoogleSheetsClient client, EmailSender emailSender, IConfiguration configuration)
    {
        _client = client;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync(int rowNumber, string code)
    {
        Letter = await _client.FetchLetterAsync(rowNumber);
        if (Letter == null)
        {
            Message = "Nie znaleziono listu.";
            ShowForm = false;
            return Page();
        }

        // Validate the code matches the letter number
        if (!string.Equals(Letter.Number, code, StringComparison.OrdinalIgnoreCase))
        {
            Message = "Nieprawidłowy kod potwierdzenia.";
            ShowForm = false;
            return Page();
        }

        // Check if already confirmed
        if (Letter.Status >= LetterStatus.DOSTARCZONE)
        {
            Message = $"Paczka dla listu numer {Letter.Number} została już potwierdzona jako dostarczona.";
            ShowForm = false;
            return Page();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(int rowNumber, string code)
    {
        Letter = await _client.FetchLetterAsync(rowNumber);

        // Validate the code
        if (!string.Equals(Letter.Number, code, StringComparison.OrdinalIgnoreCase))
        {
            Message = "Nieprawidłowy kod potwierdzenia.";
            ShowForm = false;
            return Page();
        }

        // Validate the security code
        var expectedSecurityCode = _configuration["PackageConfirmationCode"];
        if (string.IsNullOrWhiteSpace(SecurityCode) || 
            !string.Equals(SecurityCode, expectedSecurityCode, StringComparison.OrdinalIgnoreCase))
        {
            Message = "Nieprawidłowy kod bezpieczeństwa. Skontaktuj się z organizatorem.";
            ShowForm = true;
            return Page();
        }

        // Update status to DOSTARCZONE and add note about number of boxes
        var uwagi = Letter.Uwagi ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(uwagi))
        {
            uwagi += $"\n";
        }
        uwagi += $"Potwierdzone dostarczenie {NumberOfBoxes} paczek - {DateTime.Now:yyyy-MM-dd HH:mm}";

        await _client.UpdateStatus(rowNumber, LetterStatus.DOSTARCZONE, uwagi);

        // Send confirmation email
        await _emailSender.SendPackageReceived(rowNumber);

        Message = $"Dziękujemy! Potwierdzono dostarczenie {NumberOfBoxes} paczek dla listu numer {Letter.Number}.";
        ShowForm = false;

        return Page();
    }
}
