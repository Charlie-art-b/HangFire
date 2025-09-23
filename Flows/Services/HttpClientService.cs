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
                var response = await _httpClient.PostAsJsonAsync("https://webhook.site/adf6bb7a-d640-4edb-a445-83d1b1905ae1", request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ PDF server respondió correctamente. CorrelationId={CorrelationId}", request.CorrelationId);
                    return true;
                }
                else
                {
                    _logger.LogError("❌ PDF server devolvió error {StatusCode}. CorrelationId={CorrelationId}", 
                        response.StatusCode, request.CorrelationId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al conectar con el PDF server. CorrelationId={CorrelationId}", request.CorrelationId);
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