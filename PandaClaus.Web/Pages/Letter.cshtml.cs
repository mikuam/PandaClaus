using Microsoft.AspNetCore.Mvc;
using PandaClaus.Web.Core;
using PandaClaus.Web.Services;

namespace PandaClaus.Web.Pages;
public class LetterModel : BasePageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;
    private readonly BlobClient _blobClient;
    private readonly IConfiguration _configuration;
    private readonly GetResponseClient _getResponseClient;

    [BindProperty]
    public Letter? Letter { get; set; }

    [BindProperty]
    public List<IFormFile> UploadPhotos { get; set; }

    [BindProperty]
    public int NumberOfBoxes { get; set; } = 1;

    [BindProperty]
    public bool ConsentToNewsletter { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    
    public string BlobUrl => _configuration["BlobUrl"]!;

    public LetterModel(GoogleSheetsClient client, EmailSender emailSender,
        BlobClient blobClient, IConfiguration configuration, GetResponseClient getResponseClient)
    {
        _getResponseClient = getResponseClient;
        _emailSender = emailSender;
        _client = client;
        _blobClient = blobClient;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync(string? hash)
    {
        if (IsAdmin) Response.Headers.CacheControl = "private, no-store";
        Letter = !string.IsNullOrEmpty(hash)
            ? await _client.FetchLetterByHashAsync(hash)
            : null;

        if (Letter == null || (!IsAdmin && !LetterAccessPolicy.CanReadPublicly(Letter.IsVisible, Letter.IsDeleted)))
        {
            Letter = null;
            return NotFound();
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

    public async Task<IActionResult> OnPostAsync(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return BadRequest();

        var letter = await _client.FetchLetterByHashAsync(hash);
        if (letter == null ||
            !LetterAccessPolicy.CanReserve(letter.IsVisible, letter.IsDeleted, letter.IsAssigned))
            return NotFound();

        if (!letter.IsAssigned)
        {
            await _client.AssignLetterAsync(new LetterAssignment
            {
                Hash = letter.Hash,
                Name = Letter.AssignedTo,
                CompanyName = Letter.AssignedToCompanyName,
                Email = Letter.AssignedToEmail,
                PhoneNumber = Letter.AssignedToPhone,
                Info = Letter.AssignedToInfo
            });

            if (ConsentToNewsletter && !string.IsNullOrWhiteSpace(Letter.AssignedToEmail))
            {
                await _getResponseClient.SubscribeAsync(Letter.AssignedToEmail.Trim(), Letter.AssignedTo?.Trim() ?? string.Empty);
            }
        }

        await _emailSender.SendConfirmationEmail(letter.Hash);

        HttpContext.Session.SetString("ReservedLetterHash", letter.Hash);
        return RedirectToPage("./Confirmation", new { hash = letter.Hash });
    }

    public async Task<IActionResult> OnPostDeleteImageAsync(string hash, string imageUrl)
    {
        if (IsAdmin)
        {
            Letter = await _client.FetchLetterByHashAsync(hash);

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
                        
                        await _client.UpdateImageIds(Letter.Hash, Letter.ImageIds);
                    }
                }
            }
        }

        return RedirectToPage("./Letter", new { hash });
    }

    public async Task<IActionResult> OnPostUploadImageAsync(string hash)
    {
        if (IsAdmin)
        {
            Letter = await _client.FetchLetterByHashAsync(hash);
            
            if (Letter == null)
            {
                return RedirectToPage("./Index");
            }

            var photoIds = await _blobClient.UploadPhotos(UploadPhotos);
            if (photoIds.Count != 0)
            {
                Letter.ImageIds.AddRange(photoIds);
                await _client.UpdateImageIds(Letter.Hash, Letter.ImageIds);
            }
        }

        return RedirectToPage("./Letter", new { hash });
    }

    public async Task<IActionResult> OnPostUpdateDetailsAsync(string hash)
    {
        if (IsAdmin)
        {
            var letter = await _client.FetchLetterByHashAsync(hash);
            
            if (letter == null)
            {
                return RedirectToPage("./Index");
            }

            var childName = Request.Form["ChildName"].ToString();
            var childAge = int.Parse(Request.Form["ChildAge"].ToString());
            var description = Request.Form["Description"].ToString();
            var presents = Request.Form["Presents"].ToString();
            var uwagi = Request.Form["Uwagi"].ToString();

            await _client.UpdateLetterDetailsWithChild(letter.Hash, childName, childAge, description, presents, uwagi);
        }

        return RedirectToPage("./Letter", new { hash });
    }

    public async Task<IActionResult> OnPostDeleteLetterAsync(string hash)
    {
        if (IsAdmin)
        {
            var letter = await _client.FetchLetterByHashAsync(hash);
            
            if (letter == null)
            {
                return RedirectToPage("./Index");
            }

            // Toggle the IsDeleted status
            await _client.UpdateIsDeleted(letter.Hash, !letter.IsDeleted);
        }

        return RedirectToPage("./Letter", new { hash });
    }

    public async Task<IActionResult> OnPostSendConfirmationEmailAsync(string hash)
    {
        if (IsAdmin)
        {
            var letter = await _client.FetchLetterByHashAsync(hash);
            
            if (letter == null)
            {
                return RedirectToPage("./Index");
            }

            if (letter.IsAssigned && !string.IsNullOrEmpty(letter.AssignedToEmail))
            {
                await _emailSender.SendConfirmationEmail(letter.Hash);
                TempData["SuccessMessage"] = "E-mail potwierdzający został wysłany!";
            }
            else
            {
                TempData["ErrorMessage"] = "List nie jest zarezerwowany lub brak adresu e-mail.";
            }
        }

        return RedirectToPage("./Letter", new { hash });
    }

    public async Task<IActionResult> OnPostConfirmDeliveryAsync(string hash, int numberOfBoxes)
    {
        if (IsAdmin)
        {
            var letter = await _client.FetchLetterByHashAsync(hash);
            
            if (letter == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono listu.";
                return RedirectToPage("./Index");
            }

            if (!letter.IsAssigned)
            {
                TempData["ErrorMessage"] = "List nie jest zarezerwowany.";
                return RedirectToPage("./Letter", new { hash });
            }

            // Update status to DOSTARCZONE and add note about number of boxes
            var uwagi = letter.Uwagi ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(uwagi))
            {
                uwagi += $"\n";
            }
            uwagi += $"Potwierdzone dostarczenie {numberOfBoxes} paczek - {DateTime.Now:yyyy-MM-dd HH:mm}";

            await _client.UpdateStatus(letter.Hash, LetterStatus.DOSTARCZONE, uwagi);

            // Send confirmation email
            await _emailSender.SendPackageReceived(letter.Hash);

            TempData["SuccessMessage"] = $"Potwierdzono dostarczenie {numberOfBoxes} paczek dla listu {letter.Number}. E-mail potwierdzający został wysłany do darczyńcy.";
        }

        return RedirectToPage("./Letter", new { hash });
    }
}

