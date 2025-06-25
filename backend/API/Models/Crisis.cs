namespace backend.Models
{
    public class Crisis
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        public string Status { get; set; } = string.Empty; // "Open", "In Progress", "Resolved"
        public DateTime DateReported { get; set; } = DateTime.Now;
        public DateTime? DateResolved { get; set; }
        public string? Resolution { get; set; }
        public string? ReportedBy { get; set; }
        public string? AssignedTo { get; set; }
        public List<string>? Tags { get; set; } = new List<string>();
        public List<string>? AffectedSystems { get; set; } = new List<string>();
    }
}
