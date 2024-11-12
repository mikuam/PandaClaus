using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class StatusUpdateModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;
    private readonly BlobClient _blobClient;

    [BindProperty]
    public string LetterNumber { get; set; }

    [BindProperty]
    public Letter Letter { get; set; }

    [BindProperty]
    public bool IsAdmin { get; set; }

    public StatusUpdateModel(GoogleSheetsClient client, EmailSender emailSender, BlobClient blobClient)
    {
        _emailSender = emailSender;
        _client = client;
        _blobClient = blobClient;
    }

    public async Task OnGetAsync(int rowNumber)
    {
        IsAdmin = CheckIsAdmin();

        Letter = await _client.FetchLetterAsync(rowNumber);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var rowNumber = int.Parse(Request.Form["RowNumber"]!);

        var letter = await _client.FetchLetterAsync(rowNumber);
        if (!letter.IsAssigned)
        {
            await _client.AssignLetterAsync(new LetterAssignment
            {
                RowNumber = rowNumber,
                Name = Letter.AssignedTo,
                CompanyName = Letter.AssignedToCompanyName,
                Email = Letter.AssignedToEmail,
                PhoneNumber = Letter.AssignedToPhone,
                Info = Letter.AssignedToInfo
            });
        }

        await _emailSender.SendConfirmationEmail(rowNumber);
        
        return RedirectToPage("./Confirmation", new { rowNumber});
    }

    public async Task<IActionResult> OnPostDeleteImageAsync(int rowNumber, string imageUrl)
    {
        if (CheckIsAdmin())
        {
            Letter = await _client.FetchLetterAsync(rowNumber);

            var imageUrlToRemove = Letter.ImageUrls.FirstOrDefault(i => i.Contains(imageUrl));
            if (imageUrlToRemove is not null)
            {
                var imageIdToRemove = Letter.ImageIds.FirstOrDefault(imageUrlToRemove.Contains);
                if (imageIdToRemove is not null && Letter.ImageIds.Contains(imageIdToRemove))
                {
                    Letter.ImageIds.Remove(imageIdToRemove);
                    await _client.UpdateImageIds(Letter);
                }
            }
        }

        return RedirectToPage("./StatusUpdate", new { rowNumber });
    }
}

