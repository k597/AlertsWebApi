using System.Text.Json.Serialization;

namespace ObWebApi3.Services
{
    public class AlertResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; }
        public List<string> Ips { get; set; }
    }

    public class AlertDtoA
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; } 
        public List<string> Ips { get; set; } 
    }

    public class AlertDtoB
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }  // String severity (very low, low, high)
        public List<string> Ips { get; set; }  
    }

    public class AlertDtoUpdate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Ips { get; set; } 
    }

    public class PaginationMetadata
    {
        [JsonPropertyName("total_record_count")]
        public int TotalRecordCount { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("page_no")]
        public int PageNo { get; set; }

        [JsonPropertyName("page_count")]
        public int PageCount { get; set; }
    }

    public class AppSettings
    {
        public int BlacklistThreshold { get; set; }
    }
}

