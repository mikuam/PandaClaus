using Azure;
using Azure.Communication.Email;

namespace PandaClaus.Web;

public class EmailSender
{
    private const string PageUrl = "https://pandaclaus.pandateam.pl/";
    private const string EmailFrom = "DoNotReply@\r\n3e425720-d311-4859-9dbd-725f2a71aad6.azurecomm.net";

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

        var subject = "Potwierdzenie rezerwacji listu oraz ważne informacje";
        var plainTextContent = $"Cześć {letter.AssignedTo}.\n\n" +
                               $"Dziękujemy za to, że chcesz zostać PANDAstycznym Mikołajem :)! Potwierdzamy realizowanie wybranego przez Ciebie listu nr {letter.Number}" +
                               $"Poniżej garść WAŻNYCH informacji:\r\n" +
                               $"Czas na realizację i dostarczenie do nas paczki jest do 8 grudnia 2023.\r\n" +
                               $"Masz dwie możliwości dostarczenia paczki:\r\n" +
                               $"Paczkomatem Inpost do Paczkomatu POZ174M podając e-mail pandaclaus@pandateam.pl oraz numer telefonu +48 534 897 105 (jeżeli paczka nie zmieści się do Paczkomatu Inpost - daj nam znać na wyżej podany adres mailowy)\r\n" +
                               $"Osobiście do siedziby naszej fundacji Hope Spot na ul. Bolka 8A w Poznaniu w dniach: 5-7 grudnia od 10:30 do 14:30 oraz 8 grudnia od 16:30 do 19:00 (inne dni i godziny po wcześniejszym umówieniu telefonicznym)\r\n" +
                               $"WAŻNE! Prosimy Cię o wyraźne oznaczenie paczki numerami listów, najlepiej w kilku miejscach.\r\n" +
                               $"Jeżeli realizujesz kilka listów, paczkę dla każdego dziecka spakuj osobno i umieść wszystkie możliwie w jednym kartonie.\r\n" +
                               $"Jeżeli to możliwe, prosimy, aby największa paczka nie przekraczała wymiarów 41x38x64 cm (choć zdajemy sobie sprawę, że nie zawsze da się to zrealizować)\r\n\r\n" +
                               $"W dniu finału akcji będziemy otwierać każdą z paczek i przygotowywać do dalszej wysyłki. Nie musisz więc pakować jej w ozdobny papier, po prostu wyraźnie oddzielcie prezenty dla każdego z dzieci i podpisz numerem listu na zewnętrznej stronie.\r\n\r\n" +
                               $"Wartość prezentu zapisanego w liście dla jednego dziecka nie powinna przekraczać 300 zł. Jeżeli jest inaczej i nie możesz zrealizować przez to listu, koniecznie daj nam znać pisząc na adres pandaclaus@pandateam.pl.\r\n\r\n" +
                               $"W liście podane jest główne marzenie dziecka. Jeżeli jednak chcesz dorzucić coś od siebie, to śmiało możesz to zrobić. Jeśli dziecko pisze o zabawce z np. z Psiego Patrolu, to można mu dodatkowo kupić jakieś małe gadżety związane z prezentem głównym np. naklejki, gazetki itp. \r\n\r\n" +
                               $"Do paczek koniecznie dołączcie kartki z życzeniami albo krótki spersonalizowany list. Prosimy, aby podpisać je np. „Święty Mikołaj z pomocnikami” i wymienić Wasze imiona/nazwę firmy, czy klasy szkolnej/grupy przedszkolnej. Wiele dzieci wierzy mocno, że to idzie prosto od świętego Mikołaja, ale zarazem chcielibyśmy, aby rodzice wiedzieli, kto był Darczyńcą. Jeśli bardzo chcecie pozostać anonimowi to możecie napisać ogólnikowo np. Święty Mikołaj z pomocnikami z firmy produkującej XYZ, z branży XYZ lub szkoły w XYZ. \r\n\r\n" +
                               $"Będzie wspaniale, jeśli dorzucisz jakiś drobny upominek dla rodziców (kawa, herbata, słodycze, damskie i męskie skarpetki, kubki świąteczne, świece itp.). Rodzice naszych podopiecznych często stawiają siebie i swoje potrzeby na samym końcu… na pewno będzie im miło. \r\n\r\n" +
                               $"W razie pytań, pisz na pandaclaus@pandateam.pl lub dzwoń na numer +48 534 897 105.\r\n\r\n" +
                               $"I ostatnia prośba. Wysyłajcie nam swoje zdjęcia z realizacji prezentów, a wrzucając je w social media oznaczajcie nas @fundacjapandateam.\r\n\r\n" +
                               $"Jeszcze raz BARDZO dziękujemy za Wasze dobre serca!\r\n\r\n" +
                               $"Wszystkiego PANDAstycznego!\r\n\r\n" +
                               $"Ekipa z Panda Team\r\n";

        var htmlContent = $"Cześć {letter.AssignedTo}.<br />" +
                          $"<p>Dziękujemy za to, że chcesz zostać PANDAstycznym Mikołajem :)! Potwierdzamy realizowanie wybranego przez Ciebie listu nr {letter.Number}.</p>" +
                          $"<p>List znajdziesz pod tym adresem: <a href=\"{GetLetterUrl(letter)}\">{GetLetterUrl(letter)}</a></p>" +
                          "<p>Poniżej garść WAŻNYCH informacji:</p>" +
                          "<ul>" +
                          "<li>Czas na realizację i dostarczenie do nas paczki jest do 8 grudnia 2023.</li>" +
                          "<li>Masz dwie możliwości dostarczenia paczki:</li>" +
                          "<ul style='text-indent:20px'>" +
                          "<li>Paczkomatem Inpost do Paczkomatu POZ174M podając e-mail <a href=\"mailto:pandaclaus@pandateam.pl\">pandaclaus@pandateam.pl</a> oraz numer telefonu +48 534 897 105 (jeżeli paczka nie zmieści się do Paczkomatu Inpost - daj nam znać na wyżej podany adres mailowy)</li>" +
                          "<li>Osobiście do siedziby naszej fundacji Hope Spot na ul. Bolka 8A w Poznaniu w dniach: 5-7 grudnia od 10:30 do 14:30 oraz 8 grudnia od 16:30 do 19:00 (inne dni i godziny po wcześniejszym umówieniu telefonicznym)</li>" +
                          "</ul>" +
                          "<li><strong>WAŻNE!</strong> Prosimy Cię o wyraźne oznaczenie paczki numerami listów, najlepiej w kilku miejscach.</li>" +
                          "<li>Jeżeli realizujesz kilka listów, paczkę dla każdego dziecka spakuj osobno i umieść wszystkie możliwie w jednym kartonie.</li>" +
                          "<li>Jeżeli to możliwe, prosimy, aby największa paczka nie przekraczała wymiarów 41x38x64 cm (choć zdajemy sobie sprawę, że nie zawsze da się to zrealizować)</li>" +
                          "</ul>" +
                          "<p>W dniu finału akcji będziemy otwierać każdą z paczek i przygotowywać do dalszej wysyłki. Nie musisz więc pakować jej w ozdobny papier, po prostu wyraźnie oddzielcie prezenty dla każdego z dzieci i podpisz numerem listu na zewnętrznej stronie.</p>" +
                          "<p>Wartość prezentu zapisanego w liście dla jednego dziecka nie powinna przekraczać 300 zł. Jeżeli jest inaczej i nie możesz zrealizować przez to listu, koniecznie daj nam znać pisząc na adres <a href=\"mailto:pandaclaus@pandateam.pl\">pandaclaus@pandateam.pl</a>.</p>" +
                          "<p>W liście podane jest główne marzenie dziecka. Jeżeli jednak chcesz dorzucić coś od siebie, to śmiało możesz to zrobić. Jeśli dziecko pisze o zabawce z np. z Psiego Patrolu, to można mu dodatkowo kupić jakieś małe gadżety związane z prezentem głównym np. naklejki, gazetki itp.</p>" +
                          "<p>Do paczek koniecznie dołączcie kartki z życzeniami albo krótki spersonalizowany list. Prosimy, aby podpisać je np. „Święty Mikołaj z pomocnikami” i wymienić Wasze imiona/nazwę firmy, czy klasy szkolnej/grupy przedszkolnej. Wiele dzieci wierzy mocno, że to idzie prosto od świętego Mikołaja, ale zarazem chcielibyśmy, aby rodzice wiedzieli, kto był Darczyńcą. Jeśli bardzo chcecie pozostać anonimowi to możecie napisać ogólnikowo np. Święty Mikołaj z pomocnikami z firmy produkującej XYZ, z branży XYZ lub szkoły w XYZ.</p>" +
                          "<p>Będzie wspaniale, jeśli dorzucisz jakiś drobny upominek dla rodziców (kawa, herbata, słodycze, damskie i męskie skarpetki, kubki świąteczne, świece itp.). Rodzice naszych podopiecznych często stawiają siebie i swoje potrzeby na samym końcu… na pewno będzie im miło.</p>" +
                          "<p>W razie pytań, pisz na <a href=\"mailto:pandaclaus@pandateam.pl\">pandaclaus@pandateam.pl</a> lub dzwoń na numer +48 534 897 105.</p>" +
                          "<p>I ostatnia prośba. Wysyłajcie nam swoje zdjęcia z realizacji prezentów, a wrzucając je w social media oznaczajcie nas <a href=\"https://www.instagram.com/fundacjapandateam\" target=\"_blank\">@fundacjapandateam</a>.</p>" +
                          "<p>Jeszcze raz BARDZO dziękujemy za Wasze dobre serca!</p>" +
                          "<p>Wszystkiego PANDAstycznego!</p><br />" +
                          "<p>Ekipa z Panda Team</p>";


    var email = new EmailMessage(
            EmailFrom,
            letter.AssignedToEmail,
            new EmailContent(subject) { PlainText = plainTextContent, Html = htmlContent });
        email.Headers.Add("Message-ID", "<do-not-reply@example.com>");

        // run in a background thread, don't wait for it to finish
        Task.Run(() => _emailClient.Send(WaitUntil.Completed, email));
    }

    private string GetLetterUrl(Letter letter)
    {
        return PageUrl + $"Letter?rowNumber={letter.RowNumber}";
    }

    public async Task SendLetterAdded(int rowNumber)
    {
        var letter = await _googleSheetsClient.FetchLetterAsync(rowNumber);

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
        
        Task.Run(() => _emailClient.Send(WaitUntil.Completed, email));
    }
}
