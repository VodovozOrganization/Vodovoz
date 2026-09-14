using Mango.Core.Dto.Vpbx.Requests;
using Mango.Core.Dto.Vpbx.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mango.Vpbx.Client.Services
{
	/// <inheritdoc/>
	public class MangoVpbxCallsService : IMangoVpbxCallsService
	{
		private const string _callbackEndpoint = "vpbx/commands/callback";

		private readonly ILogger<MangoVpbxCallsService> _logger;
		private readonly IMangoVpbxApiClient _apiClient;

		public MangoVpbxCallsService(
			ILogger<MangoVpbxCallsService> logger,
			IMangoVpbxApiClient apiClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
		}

		/// <inheritdoc/>
		public async Task SendCallbackCommand(string extension, string toNumber, CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(extension))
			{
				throw new ArgumentException($"{nameof(extension)} не может быть пустым", nameof(extension));
			}

			if(string.IsNullOrWhiteSpace(toNumber))
			{
				throw new ArgumentException($"{nameof(toNumber)} не может быть пустым", nameof(toNumber));
			}

			var commandId = Guid.NewGuid().ToString("N");

			var request = new MakeVpbxCallbackRequest
			{
				CommandId = commandId,
				From = new VpbxCallbackFrom
				{
					Extension = extension
				},
				ToNumber = toNumber
			};

			_logger.LogInformation(
				"Отправляем команду обратного звонка {CommandId} ВАТС Манго с добавочного номера {Extension} на номер {ToNumber}",
				commandId,
				extension,
				toNumber);

			await _apiClient.PostAsync<MakeVpbxCallbackRequest, VpbxCallbackResponse>(
				_callbackEndpoint,
				request,
				true,
				cancellationToken);

			_logger.LogInformation("Команда обратного звонка {CommandId} принята ВАТС Манго", commandId);
		}
	}
}
