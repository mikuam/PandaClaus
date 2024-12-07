using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaClaus.Web.Core;

namespace PandaClaus.Web.Pages
{
    public class StatsModel : PageModel
    {
        private readonly GoogleSheetsClient _client;

        public Statistics Statistics { get; set; }

        public StatsModel(GoogleSheetsClient client)
        {
            _client = client;
        }

        public async Task OnGet()
        {
            var letters = await _client.FetchLetters();
            var packages = await _client.FetchPackages();

            Statistics = new Statistics
            {
                Letters = letters.Count,
                NieWiadomo = letters.Count(l => l.Status == LetterStatus.NIE_WIADOMO),
                Wiadomo = letters.Count(l => l.Status > LetterStatus.NIE_WIADOMO),
                WiadomoPercentage = letters.Count(l => l.Status > LetterStatus.NIE_WIADOMO) * 100 / letters.Count,
                Dostarczone = letters.Count(l => l.Status >= LetterStatus.DOSTARCZONE),
                DostarczonePercentage = letters.Count(l => l.Status >= LetterStatus.DOSTARCZONE) * 100 / letters.Count,
                WTrakcieSprawdzania = letters.Count(l => l.Status >= LetterStatus.W_TRAKCIE_SPRAWDZANIA),
                WTrakcieSprawdzaniaPercentage = letters.Count(l => l.Status >= LetterStatus.W_TRAKCIE_SPRAWDZANIA) * 100 / letters.Count,
                Odlozone = letters.Count(l => l.Status == LetterStatus.ODLOZONE),
                Sprawdzone = letters.Count(l => l.Status >= LetterStatus.SPRAWDZONE),
                SprawdzonePercentage = letters.Count(l => l.Status >= LetterStatus.SPRAWDZONE) * 100 / letters.Count,
                DoSprawdzenia = letters.Count(l => l.Status < LetterStatus.SPRAWDZONE),
                Spakowane = letters.Count(l => l.Status >= LetterStatus.SPAKOWANE),
                SpakowanePercentage = letters.Count(l => l.Status >= LetterStatus.SPAKOWANE) * 100 / letters.Count,
                Zaadresowane = letters.Count(l => l.Status >= LetterStatus.ZAADRESOWANE),
                ZaadresowanePercentage = letters.Count(l => l.Status >= LetterStatus.ZAADRESOWANE) * 100 / letters.Count,
                ZaadresowanePaczki = packages.Count
            };
        }
    }
}
