using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaClaus.Web.Core;

namespace PandaClaus.Web.Pages;
public class StatusUpdateModel : PageModel
{
    private readonly GoogleSheetsClient _client;

    [BindProperty]
    public Letter Letter { get; set; }

    [BindProperty]
    public Gabaryt Package1Size { get; set; }

    [BindProperty]
    public Gabaryt Package2Size { get; set; }

    [BindProperty]
    public Gabaryt Package3Size { get; set; }

    public StatusUpdateModel(GoogleSheetsClient client)
    {
        _client = client;
    }

    public async Task OnGetAsync()
    {
        if (int.TryParse(Request.Query["number"].ToString(), out var number))
        {
            Letter = await _client.FetchLetterAsync(number);
        }
    }

    public async Task<IActionResult> OnPostSetLetterNumberAsync()
    {
        var letterNumber = Request.Form["letterNumber"].ToString();
        var allLetters = await _client.FetchLetters();
        var letter = allLetters.FirstOrDefault(l => l.Number.Equals(letterNumber, StringComparison.CurrentCultureIgnoreCase));

        if (letter == null)
        {
            return RedirectToPage("./StatusUpdate");
        }

        Letter = letter;

        return RedirectToPage("./StatusUpdate", new { number = letter.RowNumber });
    }

    public async Task<IActionResult> OnPostSetStatusAsync()
    {
        if (int.TryParse(Request.Form["number"].ToString(), out var rowNumber))
        {
            var status = Enum.Parse<LetterStatus>(Request.Query["status"].ToString());
            var uwagi = Request.Form["uwagi"].ToString();

            await _client.UpdateStatus(rowNumber, status, uwagi);

            if (status == LetterStatus.SPAKOWANY)
            {
                Letter = await _client.FetchLetterAsync(rowNumber);

                var package1Size = GetPackageSize("package1Size");
                var package2Size = GetPackageSize("package2Size");
                var package3Size = GetPackageSize("package3Size");

                if (package1Size == Gabaryt.None)
                {
                    package1Size = Gabaryt.A;
                }
                var packages = new List<Gabaryt> { package1Size, package2Size, package3Size }.Where(p => p != Gabaryt.None);
                await _client.CreatePackages(Letter.Number, packages);
            }
        }

        return RedirectToPage("./StatusUpdate", new { message = "Status zaktualizowany pomyślnie" });
    }

    private Gabaryt GetPackageSize(string parameterName)
    {
        return string.IsNullOrWhiteSpace(Request.Form[parameterName].ToString())
            ? Package1Size
            : Enum.Parse<Gabaryt>(Request.Form[parameterName].ToString());
    }
}