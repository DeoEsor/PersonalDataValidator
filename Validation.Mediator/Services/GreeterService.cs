using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Validation.Mediator
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Greeter.GreeterBase overrides

        public override async Task<RecordsValidationResult> Validate(RecordsValidationRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation($"Called method \"{nameof(Validate)}\".");

                return new RecordsValidationResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occured during \"{nameof(Validate)}\" method running.");
                return new RecordsValidationResult();
            }
        }

        #endregion
    }
}
