namespace PandaClaus.Web.Core.DTOs.InPost;

public class SenderData
{
    public string? CompanyName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public AddressData? Address { get; set; }
}