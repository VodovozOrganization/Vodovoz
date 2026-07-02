using Mango.Vpbx.Client.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.V6.Services
{
	/// <inheritdoc/>
	public class CallsService : ICallsService
	{
		private readonly ILogger<CallsService> _logger;
		private readonly IMangoCallsService _mangoCallsService;

		/// <inheritdoc/>
		public CallsService(
			ILogger<CallsService> logger,
			IMangoCallsService mangoCallsService)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_mangoCallsService = mangoCallsService ?? throw new System.ArgumentNullException(nameof(mangoCallsService));
		}

		/// <inheritdoc/>
		public Task<Result<Guid>> MakeWebhookCall(string extension, string toNumber, string lineNumber, CancellationToken cancellationToken)
		{
			return _mangoCallsService.MakeWebhookCall(extension, toNumber, lineNumber, cancellationToken);
		}
	}
}
