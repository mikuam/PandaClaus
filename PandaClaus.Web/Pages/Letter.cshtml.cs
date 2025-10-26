using Microsoft.AspNetCore.Mvc;

namespace PandaClaus.Web.Pages;
public class LetterModel : BasePageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;
    private readonly BlobClient _blobClient;

    [BindProperty]
    public Letter? Letter { get; set; }

    [BindProperty]
    public List<IFormFile> UploadPhotos { get; set; }

    public string? ErrorMessage { get; set; }

    public LetterModel(GoogleSheetsClient client, EmailSender emailSender, BlobClient blobClient)
    {
        _emailSender = emailSender;
        _client = client;
        _blobClient = blobClient;
    }

    public async Task<IActionResult> OnGetAsync(int rowNumber)
    {
        Letter = await _client.FetchLetterAsync(rowNumber);
        
        if (Letter == null)
        {
            ErrorMessage = "Nie znaleziono listu o podanym numerze. Sprawdź poprawność linku lub wróć do listy listów.";
            return Page();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var rowNumber = int.Parse(Request.Form["RowNumber"]!);

        var letter = await _client.FetchLetterAsync(rowNumber);
        
        if (letter == null)
        {
            ErrorMessage = "Nie znaleziono listu.";
            return Page();
        }

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
        if (IsAdmin)
        {
            Letter = await _client.FetchLetterAsync(rowNumber);

            if (Letter == null)
            {
                return RedirectToPage("./Index");
            }

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

        return RedirectToPage("./Letter", new { rowNumber });
    }

    public async Task<IActionResult> OnPostUploadImageAsync(int rowNumber)
    {
        if (IsAdmin)
        {
            Letter = await _client.FetchLetterAsync(rowNumber);
            
            if (Letter == null)
            {
                return RedirectToPage("./Index");
            }

            var photoIds = await _blobClient.UploadPhotos(UploadPhotos);
            if (photoIds.Count != 0)
            {
                Letter.ImageIds.AddRange(photoIds);
                await _client.UpdateImageIds(Letter);
            }
        }

        return RedirectToPage("./Letter", new { rowNumber });
    }

    public async Task<IActionResult> OnPostUpdateDetailsAsync(int rowNumber)
    {
        if (IsAdmin)
        {
            var letter = await _client.FetchLetterAsync(rowNumber);
            
            if (letter == null)
            {
                return RedirectToPage("./Index");
            }

            var childName = Request.Form["ChildName"].ToString();
            var childAge = int.Parse(Request.Form["ChildAge"].ToString());
            var description = Request.Form["Description"].ToString();
            var presents = Request.Form["Presents"].ToString();
            
            await _client.UpdateLetterDetailsWithChild(rowNumber, childName, childAge, description, presents);
        }

        return RedirectToPage("./Letter", new { rowNumber });
    }
}

