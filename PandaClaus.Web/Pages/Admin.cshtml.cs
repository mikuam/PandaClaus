using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaClaus.Web.Core;
using PandaClaus.Web.Services;

namespace PandaClaus.Web.Pages;

public class AdminModel : BasePageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly ICsvExporter _csvExporter;
    private readonly BlobClient _blobClient;
    private readonly InPostApiClient _inPostApiClient;
    private readonly InPostShipmentRequestBuilder _shipmentRequestBuilder;

    [BindProperty]
    public string LetterNumbers { get; set; }

    [BindProperty]
    public int LetterCount { get; set; }

    public AdminModel(GoogleSheetsClient client, ICsvExporter csvExporter, BlobClient blobClient, 
        InPostApiClient inPostApiClient, InPostShipmentRequestBuilder shipmentRequestBuilder)
    {
        _client = client;
        _csvExporter = csvExporter;
        _blobClient = blobClient;
        _inPostApiClient = inPostApiClient;
        _shipmentRequestBuilder = shipmentRequestBuilder;
    }

    public async Task<IActionResult> OnGetAsync(int rowNumber)
    {
        if (!IsAdmin)
        {
            return RedirectToPage("./Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostExportNextAsync(int letterCount)
    {
        if (IsAdmin)
        {
            var letters = (await _client.FetchLetters())
                .Where(l => l.Status == LetterStatus.SPAKOWANE)
                .Take(letterCount);
            return await GenerateFileAndUpdateStatus(letters);
        }

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostExportFromNumbersAsync(string letterNumbers)
    {
        if (IsAdmin)
        {
            var letterNumbersSeparate = letterNumbers.Split(',').Select(l => l.Trim().ToLowerInvariant());

            var letters = (await _client.FetchLetters()).Where(l => letterNumbersSeparate.Contains(l.Number.ToLowerInvariant()));
            return await GenerateFileAndUpdateStatus(letters);
        }

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostSendToInPostAsync(string inpostLetterNumbers)
    {
        if (IsAdmin)
        {
            var letterNumbersSeparate = inpostLetterNumbers.Split(',').Select(l => l.Trim().ToLowerInvariant());
            var letters = (await _client.FetchLetters()).Where(l => letterNumbersSeparate.Contains(l.Number.ToLowerInvariant()));
            
            return await SendPackagesToInPost(letters);
        }

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostUpdateContentTypeAsync()
    {
        if (IsAdmin)
        {
            var letters = await _client.FetchLetters();
            var images = letters.SelectMany(l => l.ImageIds).Distinct().ToList();

            foreach (var image in images)
            {
                var contentType = ImageHelper.GetContentType(image);

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    await _blobClient.UpdateContentType(image, contentType);
                }
                else
                {
                    
                }
            }
        }

        return RedirectToPage("./Index");
    }

    private async Task<IActionResult> GenerateFileAndUpdateStatus(IEnumerable<Letter> letters)
    {
        var letterNumbers = letters.Select(l => l.Number).ToList();
        var packages = (await _client.FetchPackages()).Where(p => letterNumbers.Contains(p.LetterNumber));

        var lettersWithPackages = letters.ToDictionary(l => l, l => packages.Where(p => p.LetterNumber == l.Number));
        var csvExport = _csvExporter.Export(lettersWithPackages);

        foreach (var letter in letters)
        {
            _client.UpdateStatus(letter.RowNumber, LetterStatus.ZAADRESOWANE, letter.Uwagi);
        }

        var byteArray = System.Text.Encoding.UTF8.GetBytes(csvExport);
        var stream = new MemoryStream(byteArray);
        var fileName = $"letters_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        return File(stream, "text/csv", fileName);
    }

    public async Task<IActionResult> SendPackagesToInPost(IEnumerable<Letter> letters)
    {
        if (!IsAdmin)
        {
            return RedirectToPage("./Index");
        }

        var letterNumbers = letters.Select(l => l.Number).ToList();
        var packages = (await _client.FetchPackages()).Where(p => letterNumbers.Contains(p.LetterNumber));

        var lettersWithPackages = letters.ToDictionary(l => l, l => packages.Where(p => p.LetterNumber == l.Number));

        try
        {
            // Use the new service to create shipment requests
            var shipmentRequests = _shipmentRequestBuilder.CreateShipmentRequests(lettersWithPackages);

            // Send requests to InPost API
            var shipmentResponses = await _inPostApiClient.CreateMultipleShipmentsAsync(shipmentRequests);

            // Update letter status to indicate shipments were created
            foreach (var letter in letters)
            {
                await _client.UpdateStatus(letter.RowNumber, LetterStatus.ZAADRESOWANE, 
                    $"Utworzono przesyłki InPost: {shipmentResponses.Count} z {shipmentRequests.Count}");
            }

            // Return success message with created shipments count
            TempData["SuccessMessage"] = $"Pomyślnie utworzono {shipmentResponses.Count} z {shipmentRequests.Count} przesyłek InPost.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Błąd podczas tworzenia przesyłek InPost: {ex.Message}";
        }

        return Page();
    }
}

