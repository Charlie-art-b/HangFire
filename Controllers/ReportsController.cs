// Controllers/ReportsController.cs
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SERVERHANGFIRE.Flows.DTOs;
using SERVERHANGFIRE.Flows.Services;
using SERVERHANGFIRE.Flows.Validation;

namespace SERVERHANGFIRE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IBackgroundJobClient _hangfire;
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IBackgroundJobClient hangfire,
            IKafkaProducerService kafkaProducer,
            ILogger<ReportsController> logger)
        {
            _hangfire = hangfire;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] ReportRequestDto request)
        {
            // Validación
            if (!ReportRequestValidator.IsValid(request, out string errorMessage))
            {
                return BadRequest(new { Error = errorMessage });
            }

            // Generar CorrelationId si no viene
            string correlationId = string.IsNullOrWhiteSpace(request.CorrelationId)
                ? Guid.NewGuid().ToString()
                : request.CorrelationId;

            try
            {
                // Programar tarea retrasada (5 minutos como pide el requerimiento)
                _hangfire.Schedule<IReportJobService>(
                    job => job.ProcessReportRequest(
                        request.CustomerId,
                        request.StartDate,
                        request.EndDate,
                        correlationId
                    ),
                    TimeSpan.FromMinutes(5) // 5 minutos de delay
                );

                // Log inicial
                var initialLog = new LogRequestDto
                {
                    CorrelationId = correlationId,
                    Service = "HangfireServer",
                    Endpoint = "CreateReport",
                    Timestamp = DateTime.UtcNow,
                    Payload = $"Solicitud recibida - CustomerId: {request.CustomerId}, " +
                             $"Rango: {request.StartDate:yyyy-MM-dd} a {request.EndDate:yyyy-MM-dd}",
                    Success = true
                };

                await _kafkaProducer.SendLogAsync(initialLog);

                _logger.LogInformation("✅ Solicitud encolada. CorrelationId={CorrelationId}", correlationId);

                return Ok(new
                {
                    CorrelationId = correlationId,
                    Status = "Scheduled",
                    ScheduledTime = DateTime.UtcNow.AddMinutes(5),
                    Message = "La generación del reporte comenzará en 5 minutos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al encolar tarea. CorrelationId={CorrelationId}", correlationId);
                return StatusCode(500, new { Error = "Error interno del servidor" });
            }
        }
    }
}