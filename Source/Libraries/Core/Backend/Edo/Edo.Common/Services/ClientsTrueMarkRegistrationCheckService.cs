using Edo.Common.Errors;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Settings.Edo;

namespace Edo.Common.Services
{
	public class ClientsTrueMarkRegistrationCheckService : IClientsTrueMarkRegistrationCheckService
	{
		private readonly ILogger<ClientsTrueMarkRegistrationCheckService> _logger;
		private readonly ITrueMarkApiClient _trueMarkApiClient;
		private readonly IEdoSettings _edoSettings;

		public ClientsTrueMarkRegistrationCheckService(
			ILogger<ClientsTrueMarkRegistrationCheckService> logger,
			ITrueMarkApiClient trueMarkApiClient,
			IEdoSettings edoSettings
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
		}

		public async Task<Result<RegistrationInChestnyZnakStatus>> GetTrueMarkRegistrationStatus(
			string inn,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(inn))
			{
				throw new ArgumentException($"'{nameof(inn)}' cannot be null or whitespace.", nameof(inn));
			}

			try
			{
				var trueMarkResponse = await _trueMarkApiClient.GetParticipantRegistrationForWaterStatusAsync(
					_edoSettings.TrueMarkApiParticipantRegistrationForWaterUri, inn, cancellationToken);

				if(!string.IsNullOrWhiteSpace(trueMarkResponse.ErrorMessage))
				{
					_logger.LogError(
						"Ошибка при запросе статуса регистрации клиента в Честном Знаке для ИНН {Inn}:\n{ErrorMessage}",
						inn,
						trueMarkResponse.ErrorMessage);

					return Result.Failure<RegistrationInChestnyZnakStatus>(
						TrueMarkRegistrationCheckErrors.CreateClientTrueMarkRegistrationCheckRequestError(inn, trueMarkResponse.ErrorMessage));
				}

				var status = trueMarkResponse.RegistrationStatusString.ToRegistrationInChestnyZnakStatus();

				if(status is null)
				{
					_logger.LogError(
						"Запрос статуса регистрации клиента в Честном Знаке вернул неизвестное значение. ИНН {Inn}:\nСтатус: {Status}",
						inn,
						trueMarkResponse.RegistrationStatusString);

					return Result.Failure<RegistrationInChestnyZnakStatus>(
						TrueMarkRegistrationCheckErrors.CreateUnknownRegistrationStatusInTrueMarkError(inn, trueMarkResponse.RegistrationStatusString));
				}

				return status.Value;
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при запросе статуса регистрации в Честном Знаке для ИНН {Inn}",
					inn);

				return Result.Failure<RegistrationInChestnyZnakStatus>(
					TrueMarkRegistrationCheckErrors.CreateClientTrueMarkRegistrationCheckUnhandledError(inn, ex.Message));
			}
		}
	}
}
