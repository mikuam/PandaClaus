using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaClaus.Web.Core;

namespace PandaClaus.Web.Pages;
public class StatusUpdateModel : BasePageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;

    [BindProperty]
    public Letter Letter { get; set; }

    [BindProperty]
    public Gabaryt Package1Size { get; set; }

    [BindProperty]
    public Gabaryt Package2Size { get; set; }

    [BindProperty]
    public Gabaryt Package3Size { get; set; }

    public StatusUpdateModel(GoogleSheetsClient client, EmailSender emailSender)
    {
        _emailSender = emailSender;
        _client = client;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Response.Headers.CacheControl = "private, no-store";
        if (!IsAdmin) return RedirectToPage("./Index");
        var hash = Request.Query["hash"].ToString();
        if (!string.IsNullOrWhiteSpace(hash))
        {
            Letter = await _client.FetchLetterByHashAsync(hash);
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSetLetterNumberAsync()
    {
        if (!IsAdmin) return RedirectToPage("./Index");
        var letterNumber = Request.Form["letterNumber"].ToString();
        var allLetters = await _client.FetchLetters();
        var letter = allLetters.FirstOrDefault(l => l.Number.Equals(letterNumber, StringComparison.CurrentCultureIgnoreCase));

        if (letter == null)
        {
            return RedirectToPage("./StatusUpdate");
        }

        Letter = letter;

        return RedirectToPage("./StatusUpdate", new { hash = letter.Hash });
    }

    public async Task<IActionResult> OnPostSetStatusAsync()
    {
        if (!IsAdmin) return RedirectToPage("./Index");
        var hash = Request.Form["hash"].ToString();
        if (!string.IsNullOrWhiteSpace(hash) &&
            Enum.TryParse<LetterStatus>(Request.Query["status"].ToString(), out var status) &&
            Enum.IsDefined(status))
        {
            Letter = await _client.FetchLetterByHashAsync(hash);
            if (Letter == null) return NotFound();
            var uwagi = Request.Form["uwagi"].ToString();
            await _client.UpdateStatus(hash, status, uwagi);
            if (status == LetterStatus.DOSTARCZONE)
            {
                await _emailSender.SendPackageReceived(hash);
            }

            if (status == LetterStatus.SPAKOWANE)
            {
                Letter = await _client.FetchLetterByHashAsync(hash);

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