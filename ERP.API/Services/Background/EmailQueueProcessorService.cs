namespace ERP.API.Services.Background
{
    public class EmailQueueProcessorService : BackgroundService
    {
        private readonly ILogger<EmailQueueProcessorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _processingInterval;

        public EmailQueueProcessorService(
            ILogger<EmailQueueProcessorService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _processingInterval = configuration.GetValue<int>("Email:QueueProcessingInterval", 30);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Queue Processor Service started (Gmail integration disabled)");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Gmail integration removed. Background processor left in place but idle.
                    _logger.LogDebug("Email processing disabled - no action taken");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in email queue processor");
                }

                await Task.Delay(TimeSpan.FromSeconds(_processingInterval), stoppingToken);
            }

            _logger.LogInformation("Email Queue Processor Service stopped");
        }
    }
}
