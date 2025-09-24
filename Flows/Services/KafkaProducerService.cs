
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using SERVERHANGFIRE.Flows.DTOs;

namespace SERVERHANGFIRE.Flows.Services
{
    public class KafkaOptions
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string LogsTopic { get; set; } = "hangfire-logs";
        public string PdfRequestsTopic { get; set; } = "pdf-requests";
    }

    public interface IKafkaProducerService
    {
        Task<bool> SendLogAsync(LogRequestDto log);
        Task<bool> SendPdfRequestAsync(PdfRequestDto request);
    }

    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<Null, string> _pdfProducer;
        private readonly IProducer<Null, string> _logProducer;
        private readonly string _pdfTopic;
        private readonly string _logTopic;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IOptions<KafkaOptions> options, ILogger<KafkaProducerService> logger)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                MessageTimeoutMs = 5000
            };

            _pdfProducer = new ProducerBuilder<Null, string>(config).Build();
            _logProducer = new ProducerBuilder<Null, string>(config).Build();

            _pdfTopic = options.Value.PdfRequestsTopic;
            _logTopic = options.Value.LogsTopic;

            _logger = logger;
        }

        public async Task<bool> SendPdfRequestAsync(PdfRequestDto request)
        {
            try
            {
                var message = JsonSerializer.Serialize(request);
                await _pdfProducer.ProduceAsync(_pdfTopic, new Message<Null, string> { Value = message });
                _logger.LogInformation("✅ PDF Request enviado a Kafka. CorrelationId={CorrelationId}", request.CorrelationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar PDF request a Kafka. CorrelationId={CorrelationId}", request.CorrelationId);
                return false;
            }
        }

        public async Task<bool> SendLogAsync(LogRequestDto log)
        {
            try
            {
                var message = JsonSerializer.Serialize(log);
                await _logProducer.ProduceAsync(_logTopic, new Message<Null, string> { Value = message });
                _logger.LogInformation("✅ Log enviado a Kafka. CorrelationId={CorrelationId}", log.CorrelationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar log a Kafka. CorrelationId={CorrelationId}", log.CorrelationId);
                return false;
            }
        }

        public void Dispose()
        {
            _pdfProducer?.Flush(TimeSpan.FromSeconds(5));
            _pdfProducer?.Dispose();
            _logProducer?.Flush(TimeSpan.FromSeconds(5));
            _logProducer?.Dispose();
        }
    }
}