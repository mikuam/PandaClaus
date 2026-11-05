using Azure;
using Azure.Communication.Email;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PandaClaus.Web;

public class EmailSender
{
    private const string PageUrl = "https://pandaclaus.pl/";
    private const string EmailFrom = "DoNotReply@pandaclaus.pl";

    private readonly EmailClient _emailClient;
    private readonly GoogleSheetsClient _googleSheetsClient;
    private readonly Core.VoucherStore _vouchers;

    public EmailSender(IConfiguration configuration, GoogleSheetsClient googleSheetsClient, Core.VoucherStore vouchers)
    {
        _googleSheetsClient = googleSheetsClient;
        _vouchers = vouchers;
        _emailClient = new EmailClient(configuration["EmailConnectionString"]!);
        
        // Set QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task SendConfirmationEmail(string hash)
    {
        var letter = (await _googleSheetsClient.FetchLetterByHashAsync(hash))!;
        var voucherContent = Core.VoucherEmailContent.Render(await _vouchers.GetOrAllocateAsync(letter.Hash));

        var subject = "Potwierdzenie rezerwacji listu oraz ważne informacje";
        var plainTextContent = $@"Drodzy PANDAstyczni Darczyńcy!
Bardzo dziękujemy za Wasze zaangażowanie w akcję Panda Claus 2026.
Gratulujemy {letter.AssignedTo}! Potwierdzamy realizowanie wybranego przez Ciebie listu nr {letter.Number}. List znajdziesz pod tym adresem: {GetLetterUrl(letter)}
W tym roku trafiło do nas około 200 listów od dzieci po stracie rodziców, rodzeństwa czy opiekunów prawnych. To dwieście dziecięcych marzeń, dwieście historii, dwieście prezentów czekających na spełnienie.
{voucherContent.Plain}
Akcja z roku na rok rośnie i jest coraz większym wyzwaniem logistycznym. W związku z tym przypominamy najważniejsze zasady i dziękujemy Wam za wyrozumiałość:
- Na paczki czekamy do 27.11.2026 roku!
- Paczki muszą być oznaczone numerami listów — to BARDZO ważne i pozwoli nam uniknąć pomyłek. 
- Prosimy Was także o wydrukowanie (z załącznika) kodu QR i przyklejenie go na paczce - ułatwi to naszym Magazynowym Elfom pracę i dzięki temu dostaniesz od nas POTWIERDZENIE ODBIORU PACZKI!
- Prosimy o wysyłanie skompletowanych paczek. Tylko w taki sposób jesteśmy w stanie uniknąć pomyłek. Prosimy nie zamawiać zabawek bezpośrednio ze sklepu do nas i dosyłanie osobno innych upominków. To prowadzi do błędów i innych trudności organizacyjnych. Takie paczki nie będą niestety przyjmowane.
- Prosimy nie pakować paczek w ozdobny papier - podczas finału akcji nasi wolontariusze sprawdzają każdą z nich i szykują prezenty do wysyłki :).
- Zapraszamy do skorzystania z zasobów naszego Partnera - Biblioart - zachęcamy do sprawdzenia strony www - może to właśnie tam znajdziecie wymarzony prezent naszych podopiecznych: https://bibliopunkt.pl/
- Jeżeli realizujesz kilka listów, paczkę dla każdego dziecka spakuj osobno i umieść wszystkie możliwie w jednym kartonie - opisując paczkę każdym z numerów listów, które realizujesz!
- Do listu możesz dołączyć upominek dla rodziców dziecka, list lub kartkę oraz słodycze itd.

Jak co roku naszym Partnerem jest InPost, dlatego paczki muszą mieć rozmiary zgodne z wymaganiami Paczkomatów InPost - Elfowie Kurierzy będą pracować w pocie czoła - pomóżmy im i trzymajmy się wymiarów paczkomatowych - mała ściąga tutaj - 
Gabaryt A – maksymalne wymiary 8 × 38 × 64 cm, maksymalna waga: 25 kg 
Gabaryt B – maksymalne wymiary 19 × 38 × 64 cm, maksymalna waga: 25 kg
Gabaryt C – maksymalne wymiary 41 × 38 × 64 cm, maksymalna waga: 25 kg

Paczkę wyślij do nas pod adres:
Siedziba Spectre Cars
ul. Leśna 12A
62-023 Borówiec
Nadając przesyłkę, podaj e-mail: pandaclaus@pandateam.pl oraz numer telefonu: +48 537 925 335.

Z góry BARDZO dziękujemy za dostosowanie się do naszych szczegółowych wytycznych. Pomoże nam to sprawnie przeprowadzić akcję i dostarczyć prezenty do naszych podopiecznych :)
Już teraz zapraszamy Was do zgłaszania się do wolontariatu podczas finału, który odbędzie się 3 i 4 grudnia 2026 roku (czwartek-piątek) w budynku Fundacji Malta, Ratajczaka 44/I piętro, 61-728 Poznań. Chęć pomocy można zgłosić poprzez wysłanie wiadomości e-mail na adres: pandaclaus@pandateam.pl (wolontariaty pracownicze także prosimy zgłaszać e-mailowo). Będzie nam miło, jak pomożecie nam w finale! :)
Z całym #pandateam
Fundacja Panda Team
";

        var receiveConfirmationUrl = CreateReceiveConfirmationUrl(letter);


        var htmlContent = $@"<h1>Drodzy PANDAstyczni Darczyńcy!</h1>
<p>Bardzo dziękujemy za Wasze zaangażowanie w akcję <b>Panda Claus 2026.</b></p>
<p>Gratulujemy {letter.AssignedTo}! Potwierdzamy realizowanie wybranego przez Ciebie listu nr {letter.Number}. List znajdziesz pod tym adresem: <a href=""{GetLetterUrl(letter)}"">{GetLetterUrl(letter)}</a></p>
<p>W tym roku trafiło do nas około 200 listów od dzieci po stracie rodziców, rodzeństwa czy opiekunów prawnych. To dwieście dziecięcych marzeń, dwieście historii, dwieście prezentów czekających na spełnienie.</p>
{voucherContent.Html}
<p>Akcja z roku na rok rośnie i jest coraz większym wyzwaniem logistycznym. W związku z tym przypominamy najważniejsze zasady i dziękujemy Wam za wyrozumiałość:</p>
<ul>
    <li><b>Na paczki czekamy do 27.11.2026 roku!</b></li>
    <li><b style=""color: red;"">Paczki muszą być oznaczone numerami listów</b> — to <b>BARDZO</b> ważne i pozwoli nam uniknąć pomyłek.</li>
    <li>Prosimy Was także o <b style=""color: red;"">wydrukowanie (z załącznika) kodu QR</b> i przyklejenie go na paczce - ułatwi to naszym Magazynowym Elfom pracę i dzięki temu dostaniesz od nas <b>POTWIERDZENIE ODBIORU PACZKI!</b></li>
    <li><b>Prosimy o wysyłanie skompletowanych paczek. Tylko w taki sposób jesteśmy w stanie uniknąć pomyłek.</b> Prosimy nie zamawiać zabawek bezpośrednio ze sklepu do nas i dosyłanie osobno innych upominków. To prowadzi do błędów i innych trudności organizacyjnych. Takie paczki nie będą niestety przyjmowane.</li>
    <li>Prosimy nie pakować paczek w ozdobny papier - podczas finału akcji nasi wolontariusze sprawdzają każdą z nich i szykują prezenty do wysyłki :).</li>
</ul>
<p>Zapraszamy do skorzystania z zasobów naszego Partnera - <b>Biblioart</b> - zachęcamy do sprawdzenia strony www - może to właśnie tam znajdziecie wymarzony prezent naszych podopiecznych: <a href=""https://bibliopunkt.pl/"" target=""_blank"">https://bibliopunkt.pl/</a></p>
<p>Jeżeli realizujesz kilka listów, paczkę dla każdego dziecka spakuj osobno i umieść wszystkie możliwie w jednym kartonie - opisując paczkę każdym z numerów listów, które realizujesz!</p>
<p>Do listu możesz dołączyć upominek dla rodziców dziecka, list lub kartkę oraz słodycze itd.</p>
<p>Jak co roku naszym Partnerem jest <b>InPost</b>, dlatego paczki muszą mieć rozmiary zgodne z wymaganiami <b>Paczkomatów InPost</b> - Elfowie Kurierzy będą pracować w pocie czoła - pomóżmy im i trzymajmy się wymiarów paczkomatowych - mała ściąga tutaj:</p>
<ul>
    <li>Gabaryt A – maksymalne wymiary 8 × 38 × 64 cm, maksymalna waga: 25 kg</li>
    <li>Gabaryt B – maksymalne wymiary 19 × 38 × 64 cm, maksymalna waga: 25 kg</li>
    <li>Gabaryt C – maksymalne wymiary 41 × 38 × 64 cm, maksymalna waga: 25 kg</li>
</ul>
<p><b>Paczkę wyślij do nas pod adres:</b><br/>
<b>Siedziba Spectre Cars</b><br/>
<b>ul. Leśna 12A</b><br/>
<b>62-023 Borówiec</b></p>
<p>Nadając przesyłkę, podaj e-mail: <a href=""mailto:pandaclaus@pandateam.pl"">pandaclaus@pandateam.pl</a> oraz numer telefonu: +48 537 925 335.</p>
<p>Z góry BARDZO dziękujemy za dostosowanie się do naszych szczegółowych wytycznych. Pomoże nam to sprawnie przeprowadzić akcję i dostarczyć prezenty do naszych podopiecznych :)</p>
<p>Już teraz zapraszamy Was do zgłaszania się do wolontariatu podczas finału, który odbędzie się 3 i 4 grudnia 2026 roku (czwartek-piątek) w budynku Fundacji Malta, Ratajczaka 44/I piętro, 61-728 Poznań. Chęć pomocy można zgłosić poprzez wysłanie wiadomości e-mail na adres: <a href=""mailto:pandaclaus@pandateam.pl"">pandaclaus@pandateam.pl</a> (wolontariaty pracownicze także prosimy zgłaszać e-mailowo). Będzie nam miło, jak pomożecie nam w finale! :)</p>
<p>Z całym #pandateam życzymy wszystkiego PANDAstycznego!</p>
<p><b>Fundacja Panda Team</b></p>
<hr/>
<h2>Potwierdzenie odbioru paczki</h2>
<p><b>Wydrukuj załączony plik PDF i przyklej na wszystkie wysłane do nas paczki!</b></p>
<p>To bardzo pomoże nam w zarządzaniu paczkami w magazynie.</p>";


        var email = new EmailMessage(
            EmailFrom,
            letter.AssignedToEmail,
            new EmailContent(subject) { PlainText = plainTextContent, Html = htmlContent });
        email.Headers.Add("Message-ID", "<do-not-reply@example.com>");

        // Generate PDF with QR code and attach it
        var pdfBytes = GenerateQRCodePdf(receiveConfirmationUrl, letter.Number);
        var pdfAttachment = new EmailAttachment($"PandaClaus_QRCode_List_{letter.Number}.pdf", "application/pdf", new BinaryData(pdfBytes));
        email.Attachments.Add(pdfAttachment);

        await SendCheckedAsync(email);
    }

    private byte[] GenerateQRCodePdf(string url, string letterNumber)
    {
        var qrCodeBytes = GenerateQRCodeBytes(url);
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(14));

                page.Content()
                    .Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().AlignCenter().Text("Panda Claus 2026")
                            .FontSize(28).Bold().FontColor(Colors.Red.Medium);

                        column.Item().AlignCenter().Text($"{letterNumber}")
                            .FontSize(100).Bold();

                        column.Item().PaddingTop(50).AlignCenter().Text("Zeskanuj kod QR, aby potwierdzić dostarczenie paczki")
                            .FontSize(16);

                        column.Item().PaddingTop(20).AlignCenter()
                            .Width(250)
                            .Image(qrCodeBytes);

                        column.Item().PaddingTop(10).AlignCenter().Text(url)
                            .FontSize(10).Italic();
                    });
            });
        });

        return document.GeneratePdf();
    }

    private byte[] GenerateQRCodeBytes(string url)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    private string GetLetterUrl(Letter letter)
    {
        return PageUrl + $"Letters/{letter.Hash}";
    }

    public async Task SendLetterAdded(string hash)
    {
        var letter = await _googleSheetsClient.FetchLetterByHashAsync(hash);

        var subject = "Panda Claus - potwierdzenie dodania listu";

        var plainTextContent = $"Cześć {letter.ParentName}!\n\n" +
                               $"Potwierdzamy dodanie przez Ciebie listu dla: {letter.ChildName}\n\n" +
                               $"List wymaga jeszcze weryfikacji przez naszych wolontariuszy. Jeżeli wszystko będzie w porządku, list zostanie opublikowany na naszej stronie internetowej." +
                               $"Wszelkie pytania prosimy kierować na adres e - mail pandaclaus@pandateam.pl.\n\n" +
                               $"Pozdrawiamy,\n" +
                               $"Zespół Panda Team";

        var email = new EmailMessage(
            EmailFrom,
            letter.Email,
            new EmailContent(subject) { PlainText = plainTextContent });
        email.Headers.Add("Message-ID", "<do-not-reply@example.com>");

        await SendCheckedAsync(email);
    }

    public async Task SendPackageReceived(string hash)
    {
        var letter = await _googleSheetsClient.FetchLetterByHashAsync(hash);
        if (letter == null || !letter.IsAssigned) return;

        var subject = "Panda Claus - potwierdzenie dostarczenia paczek dla listu";

        var plainTextContent = $"Cześć {letter.AssignedTo}!\n\n" +
                               $"Potwierdzamy dostarczenie przez Ciebie paczek do listu numer {letter.Number} dla: {letter.ChildName}\n\n" +
                               $"Paczka znajduje się teraz w naszym magazynie i podczas finału akcji zostanie przygotowana do wysłania do potrzebującego dziecka.\n\n" +
                               $"Wszelkie pytania prosimy kierować na adres e-mail: pandaclaus@pandateam.pl.\n\n" +
                               $"Pozdrawiamy,\n" +
                               $"Zespół Panda Team";

        var htmlContent = $@"<h1>Cześć {letter.AssignedTo}!</h1>
<p>Potwierdzamy dostarczenie przez Ciebie paczek do listu numer <strong>{letter.Number}</strong> dla: <strong>{letter.ChildName}</strong></p>
<p>Paczka znajduje się teraz w naszym magazynie i podczas finału akcji zostanie przygotowana do wysłania do potrzebującego dziecka.</p>
<p>Wszelkie pytania prosimy kierować na adres e-mail: <a href=""mailto:pandaclaus@pandateam.pl"">pandaclaus@pandateam.pl</a>.</p>
<p>Pozdrawiamy,<br/>Zespół Panda Team</p>";

        var email = new EmailMessage(
            EmailFrom,
            letter.AssignedToEmail,
            new EmailContent(subject) { PlainText = plainTextContent, Html = htmlContent });
        email.Headers.Add("Message-ID", "<do-not-reply@example.com>");

        await SendCheckedAsync(email);
    }

    private async Task SendCheckedAsync(EmailMessage email)
    {
        var operation = await _emailClient.SendAsync(WaitUntil.Completed, email);
        if (operation.Value.Status != EmailSendStatus.Succeeded)
            throw new InvalidOperationException("Email provider did not accept delivery.");
    }

    private string CreateReceiveConfirmationUrl(Letter letter)
    {
        return PageUrl + $"ReceiveConfirmation?hash={letter.Hash}&code={letter.Number}";
    }
}
