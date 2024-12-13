using System.Reflection.Emit;
using System.Text;
using PandaClaus.Web.Core.DTOs;

namespace PandaClaus.Web.Core;

public interface ICsvExporter
{
    string Export(Dictionary<Letter, IEnumerable<Package>> lettersWithPackages);
}

public class CsvExporter : ICsvExporter
{
    public string Export(Dictionary<Letter, IEnumerable<Package>> lettersWithPackages)
    {
        var csv = new StringBuilder();
        csv.AppendLine("e-mail;telefon;rozmiar;paczkomat;numer_referencyjny;dodatkowa_ochrona;za_pobraniem;imie_i_nazwisko;nazwa_firmy;ulica;kod_pocztowy;miejscowosc;typ_przesylki;paczka_w_weekend");

        foreach (var (letter, packages) in lettersWithPackages)
        {
            foreach (var package in packages)
            {
                var packageId = $"PANDA_{letter.Number}-{package.PackageNumber}/{package.TotalPackages}";
                csv.AppendLine($"{letter.Email};{letter.PhoneNumber};{package.Size};{letter.PaczkomatCode};{packageId};0;0;{letter.ParentName} {letter.ParentSurname};;{GetStreetWithNumber(letter)};{letter.PostalCode};{letter.City};{GetDeliveryType(package.Size)};NIE");
            }
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

    private string GetDeliveryType(Gabaryt gabaryt)
    {
        return gabaryt == Gabaryt.N || gabaryt == Gabaryt.D ? "kurier" : "paczkomat";
    }
}
