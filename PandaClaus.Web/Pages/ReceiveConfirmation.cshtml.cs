using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaClaus.Web.Core;

namespace PandaClaus.Web.Pages;

public class ReceiveConfirmationModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;

    [BindProperty]
    public Letter? Letter { get; set; }

    [BindProperty]
    public int NumberOfBoxes { get; set; } = 1;

    [BindProperty]
    public bool? IsConfirmed { get; set; }

    public string? Message { get; set; }
    public bool ShowForm { get; set; } = true;

    public ReceiveConfirmationModel(GoogleSheetsClient client, EmailSender emailSender)
    {
        _client = client;
        _emailSender = emailSender;
    }

    public async Task<IActionResult> OnGetAsync(int rowNumber, string code)
    {
        Letter = await _client.FetchLetterAsync(rowNumber);

        // Validate the code matches the letter number
        if (!string.Equals(Letter.Number, code, StringComparison.OrdinalIgnoreCase))
        {
            Message = "Nieprawid?owy kod potwierdzenia.";
            ShowForm = false;
            return Page();
        }

        // Check if already confirmed
        if (Letter.Status >= LetterStatus.DOSTARCZONE)
        {
            Message = $"Paczka dla listu numer {Letter.Number} zosta?a ju? potwierdzona jako dostarczona.";
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
            Message = "Nieprawid?owy kod potwierdzenia.";
            ShowForm = false;
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

        Message = $"Dzi?kujemy! Potwierdzono dostarczenie {NumberOfBoxes} paczek dla listu numer {Letter.Number}.";
        ShowForm = false;

        return Page();
    }

    public async Task<IActionResult> OnPostDeclineAsync(int rowNumber, string code)
    {
        Letter = await _client.FetchLetterAsync(rowNumber);

        // Validate the code
        if (!string.Equals(Letter.Number, code, StringComparison.OrdinalIgnoreCase))
        {
            Message = "Nieprawid?owy kod potwierdzenia.";
            ShowForm = false;
            return Page();
        }

        Message = $"Zg?oszono brak dostarczenia paczki dla listu numer {Letter.Number}. Skontaktujemy si? z Tob? w sprawie.";
        ShowForm = false;

        // Optionally: You could send a notification to admins here

        return Page();
    }
}
