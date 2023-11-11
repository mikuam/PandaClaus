using Azure;
using Azure.Communication.Email;

namespace PandaClaus.Web;

public class EmailSender
{
    private const string PageUrl = "https://pandaclausweb20231003123201.azurewebsites.net/";
    private readonly EmailClient _emailClient;
    private readonly GoogleSheetsClient _googleSheetsClient;

    public EmailSender(IConfiguration configuration, GoogleSheetsClient googleSheetsClient)
    {
        _googleSheetsClient = googleSheetsClient;
        _emailClient = new EmailClient(configuration["EmailConnectionString"]!);
    }

    public async Task SendConfirmationEmail(int rowNumber)
    {
        var letter = await _googleSheetsClient.FetchLetterAsync(rowNumber);

        var subject = "Panda Claus - potwierdzenie przypisania listu";
        var plainTextContent = $"Cześć {letter.AssignedTo}!\n\n" +
                               $"Potwierdzamy przypisanie listu do Ciebie. " +
                               $"Oto link do listu: {GetLetterUrl(letter)} " +
                               $"W razie pytań prosimy o kontakt na adres e-mail pandaclaus@pandateam.pl.\n\n";

        await _emailClient.SendAsync(
            WaitUntil.Completed,
            "pandaclaus@pandateam.pl",
            letter.AssignedToEmail,
            subject,
            string.Empty,
            plainTextContent);
    }

    private string GetLetterUrl(Letter letter)
    {
        return PageUrl + $"Letter/?rowNumber={letter.RowNumber}";
    }
}
