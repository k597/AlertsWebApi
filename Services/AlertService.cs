namespace ObWebApi3.Services;

using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Text.Json;

public class AlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AlertService> _logger;

    public AlertService(IAlertRepository alertRepository, AppDbContext context, HttpClient httpClient, ILogger<AlertService> logger)
    {
        _alertRepository = alertRepository;
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<AlertDtoA>> GetAlertsAAsync(int pageSize, int pageNo)
    {
        var url = $"api/alerts/a?page_size={pageSize}&page_no={pageNo}";

        _logger.LogInformation("Request URL: {Url}", $"{_httpClient.BaseAddress}{url}");


        var request = new HttpRequestMessage(HttpMethod.Get, url);

        HttpResponseMessage? response = null;

        try
        {
            response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Request to {Url} failed", url);
            throw; 
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Received non-success status code: {StatusCode} for {Url}", response.StatusCode, url);
            throw new Exception($"Error fetching alerts: {response.ReasonPhrase}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var alerts = JsonSerializer.Deserialize<List<AlertDtoA>>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        _logger.LogInformation("Successfully fetched {Count} alerts from {Url}", alerts?.Count ?? 0, url);

        return alerts ?? new List<AlertDtoA>();
    }

    public async Task<List<AlertDtoB>> GetAlertsBAsync(int pageSize, int pageNo)
    {
        var url = $"api/alerts/b?page_size={pageSize}&page_no={pageNo}";

        _logger.LogInformation("Request URL: {Url}", $"{_httpClient.BaseAddress}{url}");

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        HttpResponseMessage? response = null;

        try
        {
            response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request failed: {ex.Message}");
            throw; 
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error fetching alerts: {response.ReasonPhrase}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var alerts = JsonSerializer.Deserialize<List<AlertDtoB>>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return alerts ?? new List<AlertDtoB>();
    }

    public async Task<string> ProcessAlertsAAsync(int pageSize, int pageNo)
    {
        return await ProcessAlertsAsync<AlertDtoA>(
            "api/alerts/a",
            TransformDtoAToEntity,
            pageSize,
            pageNo
        );
    }

    public async Task<string> ProcessAlertsBAsync(int pageSize, int pageNo)
    {
        return await ProcessAlertsAsync<AlertDtoB>(
            "api/alerts/b",
            TransformDtoBToEntity,
            pageSize,
            pageNo
        );
    }

    public async Task<string> ProcessAlertsAsync<TDto>(
    string endpoint,
    Func<TDto, Alert> transformDtoToEntity,
    int pageSize,
    int pageNo)
    {
        int totalPages = 1;
        int totalProcessed = 0;

        if (pageNo < 1)
        {
            pageNo = 1;
        }

        do
        {
            var url = $"{endpoint}?page_size={pageSize}&page_no={pageNo}";
            _logger.LogInformation("Request URL: {Url}", $"{_httpClient.BaseAddress}{url}");
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            HttpResponseMessage? response = null;

            try
            {
                response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Request to {Url} failed", url);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Received non-success status code: {StatusCode} for {Url}", response.StatusCode, url);
                throw new Exception($"Error fetching alerts: {response.ReasonPhrase}");
            }

            if (response.Headers.TryGetValues("X-Pagination", out var paginationHeaders))
            {
                var paginationHeader = paginationHeaders.FirstOrDefault();
                if (paginationHeader != null)
                {
                    var pagination = JsonSerializer.Deserialize<PaginationMetadata>(paginationHeader, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    totalPages = pagination?.PageCount ?? 1;
                    pageSize = pagination?.PageSize ?? 10;
                }
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var alerts = JsonSerializer.Deserialize<List<TDto>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            _logger.LogInformation("Successfully fetched {Count} alerts from {Url}", alerts?.Count ?? 0, url);

            if (alerts != null && alerts.Any())
            {
                var alertsEntities = alerts.Select(transformDtoToEntity).ToList();

                foreach (var alert in alertsEntities)
                {
                    await _alertRepository.SaveAlertAsync(alert);
                    totalProcessed++;
                }

                _logger.LogInformation("Processed page {pageNo} of {totalPages}. Total alerts processed so far: {totalProcessed}.", pageNo, totalPages, totalProcessed);
            }
            else
            {
                _logger.LogInformation("No alerts found on page {pageNo}. Ending processing.", pageNo);
                break;
            }

            pageNo++;
        } while (pageNo <= totalPages);
        _logger.LogInformation("Total alerts processed: {totalProcessed}.", totalProcessed);
        return $"Total alerts processed: {totalProcessed}.";
    }


    public async Task<AlertGetDto> SaveAlertAndGetDtoAsync(AlertDtoA alertPostDto)
    {
        var alert = TransformDtoAToEntity(alertPostDto);
        var savedAlert = await _alertRepository.SaveAlertAsync(alert);
        _logger.LogInformation("New alert with id: {savedAlert.Id} created successfully.", savedAlert.Id);
        return new AlertGetDto
        {
            Id = savedAlert.Id,
            Title = savedAlert.Title,
            Description = savedAlert.Description,
            Severity = savedAlert.Severity,
            Count = savedAlert.Count,
            IpAddresses = savedAlert.AlertIpAddresses.Select(aia => aia.IpAddress.Address).ToList()
        };
    }

    public async Task<AlertGetDto?> UpdateAlertAsync(AlertDtoA alertDto)
    {
        var alertEntity = TransformDtoAToEntity(alertDto);

        var updatedAlert = await _alertRepository.UpdateAlertAsync(alertEntity);

        if (updatedAlert == null)
        {
            return null;
        }
        _logger.LogInformation("Update alert with id: {updatedAlert.Id} saved successfully.", updatedAlert.Id);
        return new AlertGetDto
        {
            Id = updatedAlert.Id,
            Title = updatedAlert.Title,
            Description = updatedAlert.Description,
            Severity = updatedAlert.Severity,
            Count = updatedAlert.Count,
            IpAddresses = updatedAlert.AlertIpAddresses.Select(aia => aia.IpAddress.Address).ToList()
        };
    }

    public async Task<bool> DeleteAlertAsync(int id)
    {
        var alert = await _alertRepository.GetAlertByIdWithIpAddressesAsync(id);
        if (alert == null)
            return false;

        _context.AlertIpAddresses.RemoveRange(alert.AlertIpAddresses);

        foreach (var ipAddress in alert.AlertIpAddresses.Select(aia => aia.IpAddress))
        {
            if (ipAddress.Count > 1)
            {
                await _alertRepository.DecrementIpAddressCountAsync(ipAddress);
            }
            else
            {
                await _alertRepository.DeleteIpAddressAsync(ipAddress);
            }
        }

        await _alertRepository.DeleteAlertAsync(alert);
        _logger.LogInformation("Delete alert with id: {alert.Id} and related ip addresses completed successfully.", alert.Id);
        return true;
    }


    public Alert TransformDtoAToEntity(AlertDtoA alertDto)
    {
        return new Alert
        {
            Id = alertDto.Id,
            Title = alertDto.Title,
            Description = alertDto.Description,
            Severity = alertDto.Severity,  
            AlertIpAddresses = alertDto.Ips.Select(ip => new AlertIpAddress
            {
                IpAddress = new IpAddress
                {
                    Address = ip,
                    Blacklisted = false,  
                    SourceType = ClassifyIp(ip),  
                    Count = 1 
                }
            }).ToList()
        };
    }

    public Alert TransformDtoBToEntity(AlertDtoB alertDto)
    {
        return new Alert
        {
            Id = alertDto.Id,
            Title = alertDto.Title,
            Description = alertDto.Description,
            Severity = ConvertSeverityToInt(alertDto.Severity),  
            AlertIpAddresses = alertDto.Ips.Select(ip => new AlertIpAddress
            {
                IpAddress = new IpAddress
                {
                    Address = ip,
                    Blacklisted = false,  
                    SourceType = ClassifyIp(ip),  
                    Count = 1 
                }
            }).ToList()
        };
    }

    private SourceType ClassifyIp(string ipAddress)
    {
        return ipAddress.StartsWith("192.") || ipAddress.StartsWith("10.") ? SourceType.Internal : SourceType.External;
    }

    private int ConvertSeverityToInt(string severity)
    {
        switch (severity.ToLower())
        {
            case "very low":
                return 0;
            case "low":
                return 1;
            case "medium":
                return 2;
            case "high":
                return 3;
            default:
                return -1;
        }
    }
}


