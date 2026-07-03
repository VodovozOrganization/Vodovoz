using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mango.Vpbx.Client.Services
{
	/// <inheritdoc/>
	public class MangoCallsService : IMangoCallsService
	{
		private readonly ILogger<MangoCallsService> _logger;
		private readonly HttpClient _httpClient;

		public MangoCallsService(
			ILogger<MangoCallsService> logger,
			HttpClient httpClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		/// <inheritdoc/>
		public Task<Guid> MakeWebhookCall(string extension, string toNumber, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
