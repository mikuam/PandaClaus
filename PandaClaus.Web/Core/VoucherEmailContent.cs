using System.Net;

namespace PandaClaus.Web.Core;

public static class VoucherEmailContent
{
    public static (string Plain, string Html) Render(string code) =>
        ($"Dzięki współpracy z InPost możesz nadać paczkę do nas bezpłatnie. Twój kod do użycia w aplikacji InPost, przypisany do jednego listu: {code}\nTo kod do nadania paczki, a nie kod QR do potwierdzenia odbioru w magazynie.\n",
         $"<p>Dzięki współpracy z InPost możesz nadać paczkę do nas bezpłatnie. Twój kod do użycia w aplikacji InPost, przypisany do jednego listu: <strong>{WebUtility.HtmlEncode(code)}</strong></p><p>To kod do nadania paczki, a nie kod QR do potwierdzenia odbioru w magazynie.</p>");
}
