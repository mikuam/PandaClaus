using PandaClaus.Web.Core;

namespace PandaClaus.Web;

public class Letter
{
    public required int RowNumber { get; set; }
    public required string Number { get; set; }
    public required string ParentName { get; set; }
    public required string ParentSurname { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string HouseNumber { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public required string PostalCode { get; set; }
    public string PaczkomatCode { get; set; } = string.Empty;
    public required string ChildName { get; set; }
    public required int ChildAge { get; set; }
    public required string Description { get; set; }
    public required string Presents { get; set; }
    public required List<string> ImageIds { get; set; }
    public required List<string> ImageUrls { get; set; }
    public string ImageThumbnailId { get; set; } = string.Empty;
    public string ImageThumbnailUrl { get; set; } = string.Empty;

    public required DateTime Added { get; set; }
    public required bool IsVisible { get; set; }
    public required bool IsAssigned { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string AssignedToCompanyName { get; set; } = string.Empty;
    public string AssignedToEmail { get; set; } = string.Empty;
    public string AssignedToPhone { get; set; } = string.Empty;
    public string AssignedToInfo { get; set; } = string.Empty;
    public string Uwagi { get; set; } = string.Empty;
    public LetterStatus Status { get; set; } = LetterStatus.NIE_WIADOMO;
}