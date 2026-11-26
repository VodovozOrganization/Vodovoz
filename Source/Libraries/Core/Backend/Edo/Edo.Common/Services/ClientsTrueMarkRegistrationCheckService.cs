using Edo.Common.Errors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
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

		public async Task<Result<TrueMarkRegistrationInfo>> GetTrueMarkRegistrationStatus(
			string inn,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(inn))
			{
				throw new ArgumentException($"'{nameof(inn)}' cannot be null or whitespace.", nameof(inn));
			}

			var statuses = await GetTrueMarkRegistrationsStatuses(new[] { inn }, cancellationToken);

			return statuses.TryGetValue(inn, out var registrationStatus)
				? registrationStatus
				: TrueMarkRegistrationCheckErrors.ClientTrueMarkRegistrationCheckRequestError;
		}

		public async Task<IDictionary<string, Result<TrueMarkRegistrationInfo>>> GetTrueMarkRegistrationsStatuses(
			IEnumerable<string> inns,
			CancellationToken cancellationToken = default)
		{
			if(inns is null)
			{
				throw new ArgumentNullException(nameof(inns));
			}

			var registrationsData = new List<ParticipantRegistrationDto>();
			var maxInnsPerRequest = _trueMarkApiClient.ParticipantsCheckMaxCount;
			var innsCount = inns.Count();

			for(var i = 0; i * maxInnsPerRequest < innsCount; i++)
			{
				var innsPortion = inns.Skip(i * maxInnsPerRequest).Take(maxInnsPerRequest).ToArray();

				var trueMarkResponse = await GetParticipantsRegistrationsStatuses(innsPortion, cancellationToken);
				registrationsData.AddRange(trueMarkResponse);
			}

			return registrationsData.ToDictionary(
				x => x.Inn,
				x => ConvertToRegistrationInfo(x));
		}

		private async Task<IEnumerable<ParticipantRegistrationDto>> GetParticipantsRegistrationsStatuses(
			IEnumerable<string> inns,
			CancellationToken cancellationToken)
		{
			try
			{
				return await _trueMarkApiClient.GetParticipantsRegistrations(inns, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при запросе статусов регистрации клиентов в Честном Знаке для ИНН: {Inns}",
					string.Join(", ", inns));

				var registrationsData = new List<ParticipantRegistrationDto>();
				foreach(var inn in inns)
				{
					registrationsData.Add(new ParticipantRegistrationDto
					{
						Inn = inn,
						ErrorMessage = $"Ошибка при проверке статуса регистрации клиента в ЧЗ: {ex.Message}"
					});
				}
				return registrationsData;
			}
		}

		private Result<TrueMarkRegistrationInfo> ConvertToRegistrationInfo(ParticipantRegistrationDto registrationData)
		{
			if(registrationData is null)
			{
				return Result.Failure<TrueMarkRegistrationInfo>(
					TrueMarkRegistrationCheckErrors.ClientTrueMarkRegistrationCheckRequestError);
			}

			if(!string.IsNullOrWhiteSpace(registrationData.ErrorMessage))
			{
				return Result.Failure<TrueMarkRegistrationInfo>(
					TrueMarkRegistrationCheckErrors.CreateClientTrueMarkRegistrationCheckRequestError(
						registrationData.Inn,
						registrationData.ErrorMessage));
			}

			if(string.IsNullOrWhiteSpace(registrationData.Status))
			{
				return Result.Failure<TrueMarkRegistrationInfo>(
					TrueMarkRegistrationCheckErrors.CreateClientTrueMarkRegistrationCheckRequestError(
						registrationData.Inn,
						"Ответ от АПИ ЧЗ не содержит значение актуального статуса"));
			}

			RegistrationInChestnyZnakStatus status;
			switch(registrationData.Status)
			{
				case "Зарегистрирован":
				case "Восстановлен":
					status = RegistrationInChestnyZnakStatus.Registered;
					break;
				case "Предварительная регистрация началась":
				case "Предварительная регистрация производителя":
				case "Предварительная регистрация продавца":
					status = RegistrationInChestnyZnakStatus.InProcess;
					break;
				case "Заблокирован":
					status = RegistrationInChestnyZnakStatus.Blocked;
					break;
				case "Не зарегистрирован":
				case "Удален":
					status = RegistrationInChestnyZnakStatus.Unknown;
					break;
				default:
					return TrueMarkRegistrationCheckErrors.CreateClientTrueMarkRegistrationCheckRequestError(
						registrationData.Inn,
						$"Неизвестное значение статуса в ответе от ЧЗ: \"{registrationData.Status}\"");
			}

			var productGroups = status == RegistrationInChestnyZnakStatus.Registered
				? registrationData.ProductGroups?.Distinct().ToList() ?? new List<TrueMarkProductGroup>()
				: new List<TrueMarkProductGroup>();

			return new TrueMarkRegistrationInfo
			{
				Status = status,
				ProductGroups = productGroups
			};
		}
	}
}
