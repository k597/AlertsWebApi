namespace ObWebApi3
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using ObWebApi3;
    using ObWebApi3.Services;
    using System.Threading.Tasks;

    public interface IAlertRepository
    {
        Task<Alert> SaveAlertAsync(Alert alert);
        Task<Alert?> UpdateAlertAsync(Alert alert);
        Task<Alert?> GetAlertByIdWithIpAddressesAsync(int id);
        Task DeleteAlertAsync(Alert alert);
        Task DeleteIpAddressAsync(IpAddress ipAddress);
        Task DecrementIpAddressCountAsync(IpAddress ipAddress);
    }

    public class AlertRepository : IAlertRepository
    {
        private readonly AppDbContext _context;
        private readonly BlacklistService _blacklistService;

        public AlertRepository(AppDbContext context, BlacklistService blacklistService)
        {
            _context = context;
            _blacklistService = blacklistService ?? throw new ArgumentNullException(nameof(blacklistService));
        }

        public async Task<Alert> SaveAlertAsync(Alert alert)
        {
            var existingAlert = await _context.Alerts
                .Include(a => a.AlertIpAddresses) 
                .ThenInclude(aia => aia.IpAddress) 
                .FirstOrDefaultAsync(a => a.Title == alert.Title && a.Description == alert.Description && a.Severity == alert.Severity);

            if (existingAlert != null)
            {
                existingAlert.Count++;

                foreach (var alertIpAddress in alert.AlertIpAddresses)
                {
                    var ip = alertIpAddress.IpAddress;

                    var existingIp = await _context.IpAddresses.FirstOrDefaultAsync(i => i.Address == ip.Address);

                    if (existingIp != null)
                    {
                        var existingLink = existingAlert.AlertIpAddresses
                            .FirstOrDefault(aia => aia.IpAddressId == existingIp.Id);

                        if (existingLink == null)
                        {
                            existingAlert.AlertIpAddresses.Add(new AlertIpAddress
                            {
                                AlertId = existingAlert.Id,
                                IpAddressId = existingIp.Id
                            });                            
                            existingIp.Count++;
                        }
                        existingIp.Blacklisted = await _blacklistService.IsBlacklistedAsync(existingIp);
                    }
                    else
                    {
                        var newIp = new IpAddress
                        {
                            Address = ip.Address,
                            SourceType = ip.SourceType,
                            Count = 1
                        };
                        newIp.Blacklisted = await _blacklistService.IsBlacklistedAsync(newIp);
                        _context.IpAddresses.Add(newIp);

                        existingAlert.AlertIpAddresses.Add(new AlertIpAddress
                        {
                            IpAddress = newIp
                        });                        
                    }
                }
            }
            else
            {
                var newAlert = new Alert
                {
                    Title = alert.Title,
                    Description = alert.Description,
                    Severity = alert.Severity,
                    Count = 1,
                    AlertIpAddresses = new List<AlertIpAddress>()
                };

                foreach (var alertIpAddress in alert.AlertIpAddresses)
                {
                    var ip = alertIpAddress.IpAddress;

                    var existingIp = await _context.IpAddresses.FirstOrDefaultAsync(i => i.Address == ip.Address);

                    if (existingIp != null)
                    {
                        newAlert.AlertIpAddresses.Add(new AlertIpAddress
                        {
                            IpAddressId = existingIp.Id
                        });

                        existingIp.Count++;
                        existingIp.Blacklisted = await _blacklistService.IsBlacklistedAsync(existingIp);
                    }
                    else
                    {
                        var newIp = new IpAddress
                        {
                            Address = ip.Address,
                            SourceType = ip.SourceType,
                            Count = 1
                        };
                        newIp.Blacklisted = await _blacklistService.IsBlacklistedAsync(newIp);
                        _context.IpAddresses.Add(newIp);

                        newAlert.AlertIpAddresses.Add(new AlertIpAddress
                        {
                            IpAddress = newIp
                        });
                    }
                }

                _context.Alerts.Add(newAlert);
                existingAlert = newAlert;
            }

            await _context.SaveChangesAsync();
            return existingAlert;
        }


        public async Task<Alert?> UpdateAlertAsync(Alert alert)
        {
            var existingAlert = await _context.Alerts
                .Include(a => a.AlertIpAddresses)
                .ThenInclude(aia => aia.IpAddress)
                .FirstOrDefaultAsync(a => a.Id == alert.Id);

            if (existingAlert == null)
            {
                return null;
            }

            existingAlert.Title = alert.Title;
            existingAlert.Description = alert.Description;
            //existingAlert.Severity = alert.Severity;
            //existingAlert.Count++;

            foreach (var alertIpAddress in alert.AlertIpAddresses)
            {
                var ip = alertIpAddress.IpAddress;

                var existingIp = await _context.IpAddresses
                    .FirstOrDefaultAsync(i => i.Address == ip.Address);

                if (existingIp != null)
                {
                    var existingLink = existingAlert.AlertIpAddresses
                        .FirstOrDefault(aia => aia.IpAddressId == existingIp.Id);

                    if (existingLink == null)
                    {
                        existingAlert.AlertIpAddresses.Add(new AlertIpAddress
                        {
                            AlertId = existingAlert.Id,
                            IpAddressId = existingIp.Id
                        });
                    }

                    existingIp.Count++;
                    existingIp.Blacklisted = await _blacklistService.IsBlacklistedAsync(existingIp);
                }
                else
                {
                    var newIp = new IpAddress
                    {
                        Address = ip.Address,
                        SourceType = ip.SourceType,
                        Count = 1
                    };

                    newIp.Blacklisted = await _blacklistService.IsBlacklistedAsync(newIp);

                    _context.IpAddresses.Add(newIp);

                    existingAlert.AlertIpAddresses.Add(new AlertIpAddress
                    {
                        IpAddress = newIp
                    });
                }
            }

            await _context.SaveChangesAsync();

            return existingAlert;
        }

        public async Task DeleteAlertAsync(Alert alert)
        {
            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
        }

        public async Task<Alert?> GetAlertByIdWithIpAddressesAsync(int id)
        {
            return await _context.Alerts
                .Include(a => a.AlertIpAddresses)
                .ThenInclude(aia => aia.IpAddress)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        public async Task DecrementIpAddressCountAsync(IpAddress ipAddress)
        {
            if (ipAddress.Count > 0)
            {
                ipAddress.Count--;
                _context.IpAddresses.Update(ipAddress);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteIpAddressAsync(IpAddress ipAddress)
        {
            _context.IpAddresses.Remove(ipAddress);
            await _context.SaveChangesAsync();
        }

    }

}

