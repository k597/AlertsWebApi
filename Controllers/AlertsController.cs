using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObWebApi3;
using ObWebApi3.Services;
namespace ObWebApi.Controllers;


[Route("api/[controller]")]
[ApiController]
public class AlertsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AlertService _alertService;
    private readonly IAlertRepository _alertRepository;

    public AlertsController(AlertService alertService, AppDbContext context, IAlertRepository alertRepository)
    {
        _alertService = alertService;
        _context = context;
        _alertRepository = alertRepository;
    }

    [HttpGet("external-alerts-a")]
    public async Task<IActionResult> GetExternalAlertsA(int pageSize = 10, int pageNo = 1)
    {
        try
        {
            var alerts = await _alertService.GetAlertsAAsync(pageSize, pageNo);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching alerts: {ex.Message}");
        }
    }

    [HttpGet("external-alerts-b")]
    public async Task<IActionResult> GetExternalAlertsB(int pageSize = 10, int pageNo = 1)
    {
        try
        {
            var alerts = await _alertService.GetAlertsBAsync(pageSize, pageNo);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching alerts: {ex.Message}");
        }
    }

    [HttpPost("process-external-alerts-a")]
    public async Task<IActionResult> PostProcessExternalAlertsA(int pageSize = 10, int pageNo = 1)
    {
        try
        {
            var processedAlertsStatus = await _alertService.ProcessAlertsAAsync(pageSize, pageNo);
            return Ok(new
            {
                Message = "Alerts fetched and stored successfully." + " " + processedAlertsStatus
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching and storing alerts: {ex.Message}");
        }
    }

    [HttpPost("process-external-alerts-b")]
    public async Task<IActionResult> PostProcessExternalAlertsB(int pageSize = 10, int pageNo = 1)
    {
        try
        {
            var processedAlertsStatus = await _alertService.ProcessAlertsBAsync(pageSize, pageNo);
            return Ok(new
            {
                Message = "Alerts fetched and stored successfully." + " " + processedAlertsStatus
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching and storing alerts: {ex.Message}");
        }
    }

    // GET: api/alerts
    [HttpGet("all")]
    public async Task<IActionResult> GetAlerts()
    {
        try
        {
            var alerts = await _context.Alerts
                .Include(a => a.AlertIpAddresses)
                .ThenInclude(aia => aia.IpAddress)
                .Select(a => new AlertGetDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Severity = a.Severity,
                    Count = a.Count,
                    IpAddresses = a.AlertIpAddresses
                        .Select(aia => aia.IpAddress.Address)
                        .ToList()
                })
                .ToListAsync();

            return Ok(alerts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching alerts: {ex.Message}");
        }
    }

    // GET: api/alerts/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AlertGetDto>> GetAlert(int id)
    {
        try
        {
            var alert = await _context.Alerts
                .Include(a => a.AlertIpAddresses) 
                    .ThenInclude(aia => aia.IpAddress)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alert == null)
            {
                return NotFound($"Alert with ID {id} not found.");
            }

            var alertDto = new AlertGetDto
            {
                Id = alert.Id,
                Title = alert.Title,
                Description = alert.Description,
                Severity = alert.Severity,
                Count = alert.Count,
                IpAddresses = alert.AlertIpAddresses
                    .Select(aia => aia.IpAddress.Address)
                    .ToList()
            };

            return Ok(alertDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching alert: {ex.Message}");
        }
    }

    // POST: api/alerts
    [HttpPost("new")]
    public async Task<ActionResult<AlertGetDto>> PostAlert(AlertDtoA alertPostDto)
    {
        try
        {
            var alertGetDto = await _alertService.SaveAlertAndGetDtoAsync(alertPostDto);

            return CreatedAtAction(nameof(GetAlert), new { id = alertGetDto.Id }, alertGetDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while saving the alert: {ex.Message}");
        }
    }

    // PUT: api/alerts/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAlert(int id, AlertDtoA alertDto)
    {
        if (id != alertDto.Id)
        {
            return BadRequest("The ID in the URL does not match the ID in the request body.");
        }

        try
        {
            var updatedAlert = await _alertService.UpdateAlertAsync(alertDto);

            if (updatedAlert == null)
            {
                return NotFound("The alert with the specified ID does not exist.");
            }

            return NoContent(); // Return 204 status when the update is successful
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating the alert: {ex.Message}");
        }
    }


    // DELETE: api/alerts/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAlert(int id)
    {
        try
        {
            var isDeleted = await _alertService.DeleteAlertAsync(id);
            if (!isDeleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while deleting the alert: {ex.Message}");
        }
    }


    [HttpGet("statistics")]
    public async Task<IActionResult> GetAlertStatistics()
    {
        var totalAlerts = await _context.Alerts.CountAsync();
        var totalIPs = await _context.IpAddresses.CountAsync();

        if (totalAlerts == 0 || totalIPs == 0)
        {
            return Ok(new
            {
                TotalAlerts = totalAlerts,
                TotalIPs = totalIPs,
                BlacklistedIPCount = 0,
                BlacklistedIPPercent = 0.0,
                InternalIPCount = 0,
                InternalIPPercent = 0.0,
                ExternalIPCount = 0,
                ExternalIPPercent = 0.0
            });
        }

        var blacklistedIPCount = await _context.IpAddresses.CountAsync(ip => ip.Blacklisted);
        var internalIPCount = await _context.IpAddresses.CountAsync(ip => ip.SourceType == SourceType.Internal);
        var externalIPCount = await _context.IpAddresses.CountAsync(ip => ip.SourceType == SourceType.External);

        return Ok(new
        {
            TotalAlerts = totalAlerts,
            TotalIPs = totalIPs,
            BlacklistedIPCount = blacklistedIPCount,
            BlacklistedIPPercent = (double)blacklistedIPCount / totalIPs * 100,
            InternalIPCount = internalIPCount,
            InternalIPPercent = (double)internalIPCount / totalIPs * 100,
            ExternalIPCount = externalIPCount,
            ExternalIPPercent = (double)externalIPCount / totalIPs * 100
        });
    }

}
