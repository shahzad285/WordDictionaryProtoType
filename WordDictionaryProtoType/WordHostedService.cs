using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace WordDictionaryProtoType
{
    public class WordHostedService : IHostedService
    {
        private readonly WordService _wordService;
        private readonly ILogger<WordHostedService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WordHostedService(WordService wordService, ILogger<WordHostedService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _wordService = wordService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
           
                _wordService.PopulateWordOffsets();
                _logger.LogInformation("Word offsets populated successfully.");
           
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Clean up if necessary
            return Task.CompletedTask;
        }
    }
}
