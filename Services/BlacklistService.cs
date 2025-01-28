namespace ObWebApi3.Services;

using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.Json;
public interface IBlacklistService
{
    Task<bool> IsBlacklistedAsync(IpAddress ipAddress);
}

public class BlacklistService : IBlacklistService
{
    private readonly AppSettings _appSettings;
    private readonly HttpClient _httpClient;

    public BlacklistService(IOptions<AppSettings> appSettings, HttpClient httpClient)
    {
        _appSettings = appSettings.Value;
        _httpClient = httpClient;
    }

    public async Task<bool> IsBlacklistedAsync(IpAddress ipAddress)
    {
        bool isPublic = ipAddress.SourceType == SourceType.External;

        if (ipAddress.SourceType == SourceType.External && ipAddress.Count >= _appSettings.BlacklistThreshold)
        {
            return true;
        }

        else
        {
            var apiResponse = await EnrichIpAddressAsync(ipAddress.Address);

            if (apiResponse?.Blacklisted ?? false)
            {
                return true; 
            }
            return false;
        }

        return false;
    }

    private async Task<ApiEnrichResponse> EnrichIpAddressAsync(string ipAddress)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/alerts/enrich?ip={ipAddress}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiEnrichResponse>(jsonResponse);
                return apiResponse;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enriching IP address {ipAddress}: {ex.Message}");
        }

        return null;
    }
}

public class ApiEnrichResponse
{
    public bool Blacklisted { get; set; }
    public int SourceType { get; set; }
}

