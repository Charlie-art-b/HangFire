using System.Net.Http.Json;
using SERVERHANGFIRE.Flows.DTOs;
using Microsoft.Extensions.Logging;

namespace SERVERHANGFIRE.Flows.Services
{
    public interface IHttpClientService
    {
        Task<bool> SendReportRequestAsync(PdfRequestDto request);
        Task<bool> SendLogAsync(LogRequestDto log);
    }

    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly ILogger<HttpClientService> _logger;

        public HttpClientService(
            HttpClient httpClient, 
            IKafkaProducerService kafkaProducer, // Inyectamos el servicio de Kafka
            ILogger<HttpClientService> logger)
        {
            _httpClient = httpClient;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        public async Task<bool> SendReportRequestAsync(PdfRequestDto request)
        {
            try
            {
                Console.WriteLine($"Payload a enviar al PDF server: {System.Text.Json.JsonSerializer.Serialize(request)}");

                // Enviar solicitud al PDF server vía Kafka
                var success = await _kafkaProducer.SendPdfRequestAsync(request);

                if (success)
                {
                    _logger.LogInformation("✅ PDF request enviado correctamente a Kafka. CorrelationId={CorrelationId}", request.CorrelationId);
                }
                else
                {
                    _logger.LogError("❌ Error al enviar PDF request a Kafka. CorrelationId={CorrelationId}", request.CorrelationId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Excepción al enviar PDF request a Kafka. CorrelationId={CorrelationId}", request.CorrelationId);
                return false;
            }
        }

        public async Task<bool> SendLogAsync(LogRequestDto log)
        {
            // Ahora usamos el servicio de Kafka en lugar de HttpClient
            return await _kafkaProducer.SendLogAsync(log);
        }
    }
}