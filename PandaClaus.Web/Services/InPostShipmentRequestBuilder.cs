using PandaClaus.Web.Core;
using PandaClaus.Web.Core.DTOs;
using PandaClaus.Web.Core.DTOs.InPost;

namespace PandaClaus.Web.Services;

public class InPostShipmentRequestBuilder
{
    public List<CreateShipmentRequest> CreateShipmentRequests(
        Dictionary<Letter, IEnumerable<Package>> lettersWithPackages)
    {
        var shipmentRequests = new List<CreateShipmentRequest>();

        foreach (var letterWithPackages in lettersWithPackages)
        {
            var letter = letterWithPackages.Key;
            var letterPackages = letterWithPackages.Value.ToList();

            // Create shipment request for each package
            foreach (var package in letterPackages)
            {
                var shipmentRequest = CreateSingleShipmentRequest(letter, package);
                shipmentRequests.Add(shipmentRequest);
            }
        }

        return shipmentRequests;
    }

    private CreateShipmentRequest CreateSingleShipmentRequest(Letter letter, Package package)
    {
        // Split parent name into first and last name
        var nameParts = $"{letter.ParentName} {letter.ParentSurname}".Trim().Split(' ', 2);
        var firstName = nameParts.Length > 0 ? nameParts[0] : letter.ParentName;
        var lastName = nameParts.Length > 1 ? nameParts[1] : letter.ParentSurname;

        return new CreateShipmentRequest
        {
            Sender = CreateSenderData(),
            Receiver = CreateReceiverData(firstName, lastName, letter, package.Size),
            Parcels = new ParcelsData
            {
                Template = GetInPostTemplate(package.Size)
            },
            Insurance = new InsuranceData
            {
                Amount = GetInsuranceAmount(package.Size),
                Currency = "PLN"
            },
            CustomAttributes = CreateCustomAttributes(letter, package.Size),
            Service = GetInPostService(package.Size),
            Reference = $"PandaClaus-{letter.Number}-{package.PackageNumber}"
        };
    }

    private static SenderData CreateSenderData()
    {
        return new SenderData
        {
            CompanyName = "Panda Team",
            FirstName = "Panda",
            LastName = "Claus",
            Email = "pandaclaus@pandateam.pl",
            Phone = "+48534897105",
            Address = new AddressData
            {
                Street = "G?ogowska",
                BuildingNumber = "16",
                PostCode = "60-734",
                City = "Pozna?",
                CountryCode = "PL"
            }
        };
    }

    private static ReceiverData CreateReceiverData(string firstName, string lastName, Letter letter, Gabaryt packageSize)
    {
        var receiverData = new ReceiverData
        {
            FirstName = firstName,
            LastName = lastName,
            Email = letter.Email,
            Phone = letter.PhoneNumber
        };

        // For Gabaryt.D (courier service), add delivery address
        if (packageSize == Gabaryt.D)
        {
            receiverData.Address = new AddressData
            {
                Street = letter.Street,
                BuildingNumber = letter.HouseNumber,
                ApartmentNumber = string.IsNullOrWhiteSpace(letter.ApartmentNumber) ? null : letter.ApartmentNumber,
                PostCode = letter.PostalCode,
                City = letter.City,
                CountryCode = "PL"
            };
        }

        return receiverData;
    }

    private static CustomAttributesData CreateCustomAttributes(Letter letter, Gabaryt packageSize)
    {
        var customAttributes = new CustomAttributesData
        {
            SendingMethod = "dispatch_order"
        };

        // For Gabaryt.D (courier service), don't set target point as it's door-to-door delivery
        // For other sizes (locker service), use Paczkomat code if provided
        if (packageSize != Gabaryt.D && !string.IsNullOrWhiteSpace(letter.PaczkomatCode))
        {
            customAttributes.TargetPoint = letter.PaczkomatCode;
        }

        return customAttributes;
    }

    private static string GetInPostService(Gabaryt size)
    {
        return size switch
        {
            Gabaryt.D => "inpost_courier_standard",
            _ => "inpost_locker_standard"
        };
    }

    private static string GetInPostTemplate(Gabaryt size)
    {
        return size switch
        {
            Gabaryt.A => "small",
            Gabaryt.B => "medium",
            Gabaryt.C => "large",
            Gabaryt.D => "xlarge",
            _ => "small"
        };
    }

    private static decimal GetInsuranceAmount(Gabaryt size)
    {
        return size switch
        {
            Gabaryt.A => 25m,
            Gabaryt.B => 50m,
            Gabaryt.C => 100m,
            Gabaryt.D => 200m,
            _ => 25m
        };
    }
}