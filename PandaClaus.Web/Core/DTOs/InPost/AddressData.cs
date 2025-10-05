namespace PandaClaus.Web.Core.DTOs.InPost;

public class AddressData
{
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? ApartmentNumber { get; set; }
    public string? PostCode { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; } = "PL";
}