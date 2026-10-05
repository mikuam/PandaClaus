namespace PandaClaus.Web.Services;

public class GetResponseClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GetResponseClient> _logger;

    public GetResponseClient(HttpClient httpClient, IConfiguration configuration, ILogger<GetResponseClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SubscribeAsync(string email, string name)
    {
        var apiKey = _configuration["GetResponseApiKey"];
        var campaignId = _configuration["GetResponseCampaignId"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(campaignId))
        {
            _logger.LogWarning("GetResponse is not configured, skipping newsletter subscription.");
            return false;
        }

        try
        {
            var baseUrl = _configuration["GetResponseApiUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "https://api.getresponse.com/v3";
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/contacts")
            {
                Content = JsonContent.Create(new
                {
                    name,
                    email,
                    campaign = new { campaignId }
                })
            };
            request.Headers.Add("X-Auth-Token", $"api-key {apiKey}");

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("GetResponse subscription failed: {Status} {Body}", response.StatusCode, body);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetResponse subscription failed.");
            return false;
        }
    }
}
