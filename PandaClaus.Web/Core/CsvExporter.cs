using System.Text;
using PandaClaus.Web.Pages;

namespace PandaClaus.Web.Core;

public class CsvExporter : ICsvExporter
{
    public string Export(IEnumerable<Letter> enumerable)
    {
        var csv = new StringBuilder();
        csv.AppendLine("e-mail;telefon;rozmiar;paczkomat;numer_referencyjny;dodatkowa_ochrona;za_pobraniem;imie_i_nazwisko;nazwa_firmy;ulica;kod_pocztowy;miejscowosc;typ_przesylki;paczka_w_weekend");

        foreach (var letter in enumerable)
        {
            csv.AppendLine($"{letter.Email};{letter.PhoneNumber};{letter.Gabaryt};{letter.PaczkomatCode};PANDA_{letter.Number};10;0;{letter.ParentName} {letter.ParentSurname};;{GetStreetWithNumber(letter)};{letter.PostalCode};{letter.City};paczkomat;NIE");
        }

        return csv.ToString();
    }

    private string GetStreetWithNumber(Letter letter)
    {
        var line = $"{letter.Street} {letter.HouseNumber}";

        return string.IsNullOrWhiteSpace(letter.ApartmentNumber)
            ? line
            : line + " / " + letter.ApartmentNumber;
    }
}
