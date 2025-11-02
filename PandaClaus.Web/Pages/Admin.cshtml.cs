using Microsoft.AspNetCore.Mvc;
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
        if (string.IsNullOrWhiteSpace(inpostLetterNumbers))
        {
            return RedirectToPage("./Index");
        }

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

        var alreadySentPackages = packages.Where(p => p.DateExported != null).ToList();
        var packagesToExport = packages.Where(p => p.DateExported == null).ToList();

        if (!packagesToExport.Any())
        {
            TempData["ErrorMessage"] = "Brak paczek do wyeksportowania. Wszystkie wybrane paczki zostały już wcześniej wyeksportowane.";
            return RedirectToPage("./Admin");
        }

        var lettersWithPackages = letters.ToDictionary(l => l, l => packagesToExport.Where(p => p.LetterNumber == l.Number));
        var csvExport = _csvExporter.Export(lettersWithPackages);

        // Update letter status
        foreach (var letter in letters)
        {
            await _client.UpdateStatus(letter.RowNumber, LetterStatus.ZAADRESOWANE, letter.Uwagi);
        }

        // Mark packages as exported
        var exportDate = DateTime.Now;
        foreach (var package in packagesToExport)
        {
            await _client.UpdatePackageDateExported(package.RowNumber, exportDate);
        }

        // Save CSV to temp file
        var fileName = $"letters_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);
        await System.IO.File.WriteAllTextAsync(tempPath, csvExport);

        // Inform user about already exported packages
        if (alreadySentPackages.Any())
        {
            var excludedLetterNumbers = alreadySentPackages
                .Select(p => p.LetterNumber)
                .Distinct()
                .OrderBy(n => n);
            
            TempData["WarningMessage"] = $"Uwaga: {alreadySentPackages.Count} paczek zostało pominiętych, " +
                                         $"ponieważ były już wcześniej wyeksportowane. " +
                                         $"Numery listów: {string.Join(", ", excludedLetterNumbers)}";
        }

        var exportedCount = packagesToExport.Count;
        TempData["SuccessMessage"] = $"Pomyślnie wyeksportowano {exportedCount} paczek. Data eksportu: {exportDate:yyyy-MM-dd HH:mm:ss}";
        TempData["DownloadFileName"] = fileName;

        return RedirectToPage("./Admin");
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

    public async Task<IActionResult> OnGetDownloadCsvAsync(string fileName)
    {
        if (!IsAdmin || string.IsNullOrWhiteSpace(fileName))
        {
            return RedirectToPage("./Admin");
        }

        var tempPath = Path.Combine(Path.GetTempPath(), fileName);
        
        if (!System.IO.File.Exists(tempPath))
        {
            TempData["ErrorMessage"] = "Plik CSV nie został znaleziony. Spróbuj ponownie wygenerować eksport.";
            return RedirectToPage("./Admin");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
        
        // Clean up temp file
        try
        {
            System.IO.File.Delete(tempPath);
        }
        catch
        {
            // Ignore cleanup errors
        }

        return File(fileBytes, "text/csv", fileName);
    }
}

