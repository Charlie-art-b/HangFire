namespace SERVERHANGFIRE.Flows.DTOs
{
    public class ReportRequestDto
    {
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
         public string? CorrelationId { get; set; }
    }
}
