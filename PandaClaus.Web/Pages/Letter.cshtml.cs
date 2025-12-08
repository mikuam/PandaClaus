using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PandaClaus.Web.Core;

namespace PandaClaus.Web.Pages;
public class LetterModel : BasePageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;
    private readonly BlobClient _blobClient;
    private readonly IConfiguration _configuration;

    [BindProperty]
    public Letter? Letter { get; set; }

    [BindProperty]
    public List<IFormFile> UploadPhotos { get; set; }

    [BindProperty]
    public int NumberOfBoxes { get; set; } = 1;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    
    public string BlobUrl => _configuration["BlobUrl"]!;

    public LetterModel(GoogleSheetsClient client, EmailSender emailSender, BlobClient blobClient, IConfiguration configuration)
    {
        _emailSender = emailSender;
        _client = client;
        _blobClient = blobClient;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync(int rowNumber)
    {
        Letter = await _client.FetchLetterAsync(rowNumber);
        
        if (Letter == null)
        {
            ErrorMessage = "Nie znaleziono listu o podanym numerze. Sprawdź poprawność linku lub wróć do listy listów.";
            return Page();
        }

        // Get messages from TempData
        if (TempData.ContainsKey("SuccessMessage"))
        {
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
        }
        if (TempData.ContainsKey("ErrorMessage"))
        {
            ErrorMessage = TempData["ErrorMessage"]?.ToString();
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
                var imageIdToRemove = Letter.ImageIds.FirstOrDefault(i => imageUrlToRemove.Contains(i)
                                                                          || (imageUrlToRemove + "_delete").Contains(i));
                if (imageIdToRemove is not null)
                {
                    var imageIndex = Letter.ImageIds.IndexOf(imageIdToRemove);
                    if (imageIndex >= 0)
                    {
                        // Mark image as deleted by adding "_delete" suffix
                        if (!imageIdToRemove.EndsWith("_delete"))
                        {
                            Letter.ImageIds[imageIndex] = imageIdToRemove + "_delete";
                        }
                        else
                        {
                            // If already marked as deleted, restore it by removing the suffix
                            Letter.ImageIds[imageIndex] = imageIdToRemove.Replace("_delete", "");
                        }
                        
                        await _client.UpdateImageIds(Letter);
                    }
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
            var uwagi = Request.Form["Uwagi"].ToString();

            await _client.UpdateLetterDetailsWithChild(rowNumber, childName, childAge, description, presents, uwagi);
        }

        return RedirectToPage("./Letter", new { rowNumber });
    }

    public async Task<IActionResult> OnPostDeleteLetterAsync(int rowNumber)
    {
        if (IsAdmin)
        {
            var letter = await _client.FetchLetterAsync(rowNumber);
            
            if (letter == null)
            {
                return RedirectToPage("./Index");
            }

            // Toggle the IsDeleted status
            await _client.UpdateIsDeleted(rowNumber, !letter.IsDeleted);
        }

        return RedirectToPage("./Letter", new { rowNumber });
    }

    public async Task<IActionResult> OnPostSendConfirmationEmailAsync(int rowNumber)
    {
        if (IsAdmin)
        {
            var letter = await _client.FetchLetterAsync(rowNumber);
            
            if (letter == null)
            {
                return RedirectToPage("./Index");
            }

            if (letter.IsAssigned && !string.IsNullOrEmpty(letter.AssignedToEmail))
            {
                await _emailSender.SendConfirmationEmail(rowNumber);
                TempData["SuccessMessage"] = "E-mail potwierdzający został wysłany!";
            }
            else
            {
                TempData["ErrorMessage"] = "List nie jest zarezerwowany lub brak adresu e-mail.";
            }
        }

        return RedirectToPage("./Letter", new { rowNumber });
    }

    public async Task<IActionResult> OnPostConfirmDeliveryAsync(int rowNumber, int numberOfBoxes)
    {
        if (IsAdmin)
        {
            var letter = await _client.FetchLetterAsync(rowNumber);
            
            if (letter == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono listu.";
                return RedirectToPage("./Index");
            }

            if (!letter.IsAssigned)
            {
                TempData["ErrorMessage"] = "List nie jest zarezerwowany.";
                return RedirectToPage("./Letter", new { rowNumber });
            }

            // Update status to DOSTARCZONE and add note about number of boxes
            var uwagi = letter.Uwagi ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(uwagi))
            {
                uwagi += $"\n";
            }
            uwagi += $"Potwierdzone dostarczenie {numberOfBoxes} paczek - {DateTime.Now:yyyy-MM-dd HH:mm}";

            await _client.UpdateStatus(rowNumber, LetterStatus.DOSTARCZONE, uwagi);

            // Send confirmation email
            await _emailSender.SendPackageReceived(rowNumber);

            TempData["SuccessMessage"] = $"Potwierdzono dostarczenie {numberOfBoxes} paczek dla listu {letter.Number}. E-mail potwierdzający został wysłany do darczyńcy.";
        }

        return RedirectToPage("./Letter", new { rowNumber });
    }
}

