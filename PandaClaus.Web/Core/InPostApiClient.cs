using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PandaClaus.Web.Core.DTOs.InPost;
using PandaClaus.Web.Core.Utilities;

namespace PandaClaus.Web.Core;

public class InPostApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InPostApiClient> _logger;
    private readonly string _apiToken;
    private readonly string _organizationId;
    private const string BaseUrl = "https://api-shipx-pl.easypack24.net";

    public InPostApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<InPostApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiToken = configuration["InPostApiToken"]!;
        _organizationId = configuration["InPostOrganizationId"]!;
        
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<CreateShipmentResponse?> CreateShipmentAsync(CreateShipmentRequest request)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(request, options);
        var requestUrl = $"/v1/organizations/{_organizationId}/shipments";
        
        // Create HttpRequestMessage for better control
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Add headers if needed (Authorization is already set on HttpClient)
        httpRequest.Headers.Add("X-Request-ID", Guid.NewGuid().ToString());
        
        // Log the request details
        _logger.LogInformation("Sending InPost API request to: {RequestUrl}", $"{BaseUrl}{requestUrl}");
        _logger.LogDebug("InPost API Request Body: {RequestBody}", json);
        _logger.LogDebug("Request ID: {RequestId}", httpRequest.Headers.GetValues("X-Request-ID").First());
        
        var response = await _httpClient.SendAsync(httpRequest);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("InPost API request successful: {StatusCode}", response.StatusCode);
            _logger.LogDebug("InPost API Response: {ResponseContent}", responseContent);
            return JsonSerializer.Deserialize<CreateShipmentResponse>(responseContent, options);
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError("InPost API request failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
        throw new Exception($"InPost API error: {response.StatusCode} - {errorContent}");
    }

    public async Task<List<CreateShipmentResponse>> CreateMultipleShipmentsAsync(List<CreateShipmentRequest> requests)
    {
        var results = new List<CreateShipmentResponse>();
        
        _logger.LogInformation("Creating {RequestCount} InPost shipments", requests.Count);
        
        foreach (var request in requests)
        {
            try
            {
                var result = await CreateShipmentAsync(request);
                if (result != null)
                {
                    results.Add(result);
                    _logger.LogDebug("Successfully created shipment with ID: {ShipmentId}", result.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create shipment for {FirstName} {LastName}: {ErrorMessage}", 
                    request.Receiver?.FirstName, request.Receiver?.LastName, ex.Message);
            }
        }

        _logger.LogInformation("Created {SuccessCount} out of {TotalCount} InPost shipments", results.Count, requests.Count);
        
        return results;
    }
}