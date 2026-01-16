using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Edo;
using VodovozBusiness.Errors.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public class TrueMarkRegistrationCheckService
	{
		private readonly ILogger<TrueMarkRegistrationCheckService> _logger;
		private readonly IEdoSettings _edoSettings;
		private readonly ITrueMarkApiClient _client;

		public TrueMarkRegistrationCheckService(
			ILogger<TrueMarkRegistrationCheckService> logger,
			IEdoSettings edoSettings,
			ITrueMarkApiClient client)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_client = client ?? throw new ArgumentNullException(nameof(client));
		}

		public async Task<Result<(string RegistrationStatusMessage, RegistrationInChestnyZnakStatus? RegistrationStatus)>>
			CheckRegistrationFromTrueMarkAsync(string inn, CancellationToken cancellationToken)
		{
			TrueMarkRegistrationResultDto trueMarkResponse;

			try
			{
				trueMarkResponse = await _client.GetParticipantRegistrationForWaterStatusAsync(
					_edoSettings.TrueMarkApiParticipantRegistrationForWaterUri,
					inn,
					cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при проверке регистрации клиента с ИНН {INN} в ЧЗ", inn);
				return Result.Failure<(string RegistrationStatusMessage, RegistrationInChestnyZnakStatus? RegistrationStatus)>(
					TrueMarkServiceErrors.UnexpectedError($"Ошибка при проверке в Честном Знаке.\n{ex.Message}"));
			}

			if(!string.IsNullOrWhiteSpace(trueMarkResponse.ErrorMessage))
			{
				return Result.Failure<(string RegistrationStatusMessage, RegistrationInChestnyZnakStatus? RegistrationStatus)>(
					TrueMarkServiceErrors.UnknownRegistrationStatusError(
						$"Результат проверки в Честном Знаке:\n{trueMarkResponse.ErrorMessage}"));
			}

			var statusConverter = new TrueMarkApiRegistrationStatusConverter();
			var status = statusConverter.ConvertToChestnyZnakStatus(trueMarkResponse.RegistrationStatusString);

			if(status == null)
			{
				return Result.Failure<(string RegistrationStatusMessage, RegistrationInChestnyZnakStatus? RegistrationStatus)>(
					TrueMarkServiceErrors.UnknownRegistrationStatusError(
						$"Такой статус участника в Честном Знаке у нас не используется:\n{trueMarkResponse.RegistrationStatusString}"));
			}
			
			return Result.Success((trueMarkResponse.RegistrationStatusString, status));
		}
		
		public Result<(string RegistrationStatusMessage, RegistrationInChestnyZnakStatus? RegistrationStatus)> CheckRegistrationFromTrueMark(
			string inn, CancellationToken cancellationToken)
		{
			TrueMarkRegistrationResultDto trueMarkResponse;

			try
			{
				trueMarkResponse = _client.GetParticipantRegistrationForWaterStatusAsync(
						_edoSettings.TrueMarkApiParticipantRegistrationForWaterUri,
						inn,
						cancellationToken)
					.Result;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при проверке регистрации клиента с ИНН {INN} в ЧЗ", inn);
				return Result.Failure<(string RegistrationStatusMessage, RegistrationInChestnyZnakStatus? RegistrationStatus)>(
					TrueMarkServiceErrors.UnexpectedError($"Ошибка при проверке в Честном Знаке.\n{ex.Message}"));
			}

			if(!string.IsNullOrWhiteSpace(trueMarkResponse.ErrorMessage))
			{
				return Result.Failure<(string RegistrationStatusMessage, RegistrationInChestnyZnakStatus? RegistrationStatus)>(
					TrueMarkServiceErrors.UnknownRegistrationStatusError(
						$"Результат проверки в Честном Знаке:\n{trueMarkResponse.ErrorMessage}"));
			}

			var statusConverter = new TrueMarkApiRegistrationStatusConverter();
			var status = statusConverter.ConvertToChestnyZnakStatus(trueMarkResponse.RegistrationStatusString);

			if(status == null)
			{
				return Result.Failure<(string RegistrationStatusMessage, RegistrationInChestnyZnakStatus? RegistrationStatus)>(
					TrueMarkServiceErrors.UnknownRegistrationStatusError(
						$"Такой статус участника в Честном Знаке у нас не используется:\n{trueMarkResponse.RegistrationStatusString}"));
			}
			
			return Result.Success((trueMarkResponse.RegistrationStatusString, status));
		}
	}
}
