using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Mango;

namespace Mango.Vpbx.Client.Services
{
	/// <inheritdoc/>
	public class MangoCallsService : IMangoCallsService
	{
		private readonly ILogger<MangoCallsService> _logger;
		private readonly IMangoSettings _mangoSettings;

		public MangoCallsService(
			ILogger<MangoCallsService> logger,
			IMangoSettings mangoSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		/// <inheritdoc/>
		public Task<Guid> MakeWebhookCall(string extension, string toNumber, string lineNumber, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
