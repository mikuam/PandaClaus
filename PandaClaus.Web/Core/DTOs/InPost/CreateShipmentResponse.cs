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
    public List<ParcelResponse>? Parcels { get; set; }
    public string? Href { get; set; }
    public string? ReturnTrackingNumber { get; set; }
    public string? Service { get; set; }
    public bool IsReturn { get; set; }
    public CustomAttributesData? CustomAttributes { get; set; }
    public CodData? Cod { get; set; }
    public InsuranceData? Insurance { get; set; }
}

public class ParcelResponse
{
    public long Id { get; set; }
    public string? IdentifyNumber { get; set; }
    public string? TrackingNumber { get; set; }
    public bool IsNonStandard { get; set; }
    public string? Template { get; set; }
    public DimensionsData? Dimensions { get; set; }
    public WeightData? Weight { get; set; }
}

public class DimensionsData
{
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string? Unit { get; set; }
}

public class WeightData
{
    public decimal Amount { get; set; }
    public string? Unit { get; set; }
}