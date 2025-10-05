namespace PandaClaus.Web.Core.DTOs.InPost;

public class CreateShipmentResponse
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Reference { get; set; }
    public DateTime? CreatedAt { get; set; }
    public ReceiverData? Receiver { get; set; }
    public SenderData? Sender { get; set; }
    public ParcelsData? Parcels { get; set; }
}