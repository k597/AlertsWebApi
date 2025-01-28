using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ObWebApi3
{
    public class Alert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; }
        
        // Many-to-Many Relationship
        public ICollection<AlertIpAddress> AlertIpAddresses { get; set; }
        public int Count { get; set; } = 1; 
    }

    public class IpAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; } 
        public string Address { get; set; } 
        public bool Blacklisted { get; set; }
        public SourceType SourceType { get; set; }
        public int Count { get; set; } = 1;

        // Many-to-Many Relationship
        public ICollection<AlertIpAddress> AlertIpAddresses { get; set; }
    }

    public class AlertIpAddress
    {
        public int AlertId { get; set; }
        public Alert Alert { get; set; }

        public int IpAddressId { get; set; }
        public IpAddress IpAddress { get; set; }
    }

    public enum SourceType
    {
        Internal,
        External
    }

    public class AlertGetDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; }
        public int Count { get; set; }
        public List<string> IpAddresses { get; set; } = new();
    }


    public class AlertPostDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; }
        public List<IpAddressPostDto> IpAddresses { get; set; } = new();
        public int Count { get; set; } = 0;
    }

    public class IpAddressPostDto
    {
        public string Address { get; set; }
        public bool Blacklisted { get; set; }
        public int SourceType { get; set; }
        public int Count { get; set; } = 0; // Optional
    }

}

