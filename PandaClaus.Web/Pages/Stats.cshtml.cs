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
            var letters = (await _client.FetchLetters()).Where(l => !l.IsDeleted).ToList();
            var packages = await _client.FetchPackages();

            var lettersCount = letters.Count;
            var nieWiadomo = letters.Count(l => l.Status == LetterStatus.NIE_WIADOMO);
            var wiadomosc = letters.Count(l => l.Status > LetterStatus.NIE_WIADOMO);
            var dostarczone = letters.Count(l => l.Status >= LetterStatus.DOSTARCZONE);
            var wTrakcieSprawdzania = letters.Count(l => l.Status >= LetterStatus.W_TRAKCIE_SPRAWDZANIA);
            var odlozone = letters.Count(l => l.Status == LetterStatus.ODLOZONE);
            var sprawdzone = letters.Count(l => l.Status >= LetterStatus.SPRAWDZONE);
            var doSprawdzenia = letters.Count(l => l.Status < LetterStatus.SPRAWDZONE);
            var spakowane = letters.Count(l => l.Status >= LetterStatus.SPAKOWANE);
            var zaadresowane = letters.Count(l => l.Status >= LetterStatus.ZAADRESOWANE);

            Statistics = new Statistics
            {
                Letters = lettersCount,
                NieWiadomo = nieWiadomo,
                Wiadomo = wiadomosc,
                WiadomoPercentage = lettersCount == 0 ? 0 : wiadomosc * 100 / lettersCount,
                Dostarczone = dostarczone,
                DostarczonePercentage = lettersCount == 0 ? 0 : dostarczone * 100 / lettersCount,
                WTrakcieSprawdzania = wTrakcieSprawdzania,
                WTrakcieSprawdzaniaPercentage = lettersCount == 0 ? 0 : wTrakcieSprawdzania * 100 / lettersCount,
                Odlozone = odlozone,
                Sprawdzone = sprawdzone,
                SprawdzonePercentage = lettersCount == 0 ? 0 : sprawdzone * 100 / lettersCount,
                DoSprawdzenia = doSprawdzenia,
                Spakowane = spakowane,
                SpakowanePercentage = lettersCount == 0 ? 0 : spakowane * 100 / lettersCount,
                Zaadresowane = zaadresowane,
                ZaadresowanePercentage = lettersCount == 0 ? 0 : zaadresowane * 100 / lettersCount,
                ZaadresowanePaczki = packages.Count
            };
        }
    }
}
