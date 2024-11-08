namespace PandaClaus.Web;

public class Letter
{
    public int RowNumber { get; set; }
    public string Number { get; set; }
    public string ParentName { get; set; }
    public string ParentSurname { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string HouseNumber { get; set; }
    public string ApartmentNumber { get; set; }
    public string PostalCode { get; set; }
    public string PaczkomatCode { get; set; }
    public string ChildName { get; set; }
    public string ChildAge { get; set; }
    public string Description { get; set; }
    public List<string> ImageIds { get; set; }
    public List<string> ImageUrls { get; set; }
    public string ImageThumbnailId { get; set; }
    public string ImageThumbnailUrl { get; set; }

    public DateTime Added { get; set; }
    public bool IsVisible { get; set; }
    public bool IsAssigned { get; set; }
    public string AssignedTo { get; set; }
    public string AssignedToCompanyName { get; set; }
    public string AssignedToEmail { get; set; }
    public string AssignedToPhone { get; set; }
    public string AssignedToInfo { get; set; }
}