namespace PandaClaus.Web;

public class Letter
{
    public int RowNumber { get; set; }
    public string Number { get; set; }
    public string ParentName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string PaczkomatCode { get; set; }
    public string ChildAge { get; set; }
    public string Description { get; set; }
    public List<string> ImageIds { get; set; }

    public DateTime Added { get; set; }
    public bool IsVisible { get; set; }
    public bool IsAssigned { get; set; }
    public string AssignedTo { get; set; }
    public string AssignedToCompanyName { get; set; }
    public string AssignedToEmail { get; set; }
    public string AssignedToPhone { get; set; }
    public string AssignedToInfo { get; set; }
}