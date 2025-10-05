namespace PandaClaus.Web.Core.DTOs.InPost;

public class CreateShipmentRequest
{
    public SenderData? Sender { get; set; }
    public ReceiverData? Receiver { get; set; }
    public ParcelsData? Parcels { get; set; }
    public InsuranceData? Insurance { get; set; }
    public CodData? Cod { get; set; }
    public CustomAttributesData? CustomAttributes { get; set; }
    public string? Service { get; set; } = "inpost_locker_standard";
    public string? Reference { get; set; }
}