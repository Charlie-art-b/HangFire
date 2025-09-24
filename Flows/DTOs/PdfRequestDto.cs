namespace SERVERHANGFIRE.Flows.DTOs

{
    public class LogRequestDto
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Payload { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
   
    public class PdfRequestDto
    {
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public List <int> Products { get; set; } = new List <int> ();
    }
}