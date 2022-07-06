using Microsoft.Extensions.Logging;
using RoboAtsService.Monitoring;
using System;
using System.Linq;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Parameters;

namespace RoboAtsService.Requests
{
	/// <summary>
	/// Обработчик запроса проверки наличия клиента
	/// </summary>
	public class ClientCheckHandler : GetRequestHandlerBase
	{
		const string _requestType = "client";
		private readonly ILogger<ClientCheckHandler> _logger;
		private readonly RoboatsSettings _roboatsSettings;
		private readonly RoboatsRepository _roboatsRepository;
		private readonly RoboatsCallRegistrator _callRegistrator;

		public override string Request => RoboatsRequestType.ClientCheck;

		public ClientCheckHandler(ILogger<ClientCheckHandler> logger, RoboatsSettings roboatsSettings, RoboatsRepository roboatsRepository, RequestDto requestDto, RoboatsCallRegistrator callRegistrator) : base(requestDto)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_callRegistrator = callRegistrator ?? throw new ArgumentNullException(nameof(callRegistrator));
			if(requestDto.RequestType != _requestType)
			{
				throw new InvalidOperationException($"Обработчик {nameof(ClientCheckHandler)} может обрабатывать только запросы с типом {_requestType}. Обратитесь в отдел разработки.");
			}
		}
		public override string Execute()
		{
			try
			{
				return ExecuteRequest();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обработке запроса информации о клиенте возникло исключение");
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.Exception, RoboatsCallOperation.OnClientHandle,
						$"При обработке запроса информации о клиенте возникло исключение: {ex.Message}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}
		}

		public string ExecuteRequest()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			var counterpartyCount = counterpartyIds.Count();
			if(counterpartyCount > 1)
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.ClientDuplicate, RoboatsCallOperation.ClientCheck, 
					$"Для телефона {ClientPhone} найдены несколько контрагентов: {string.Join(", ", counterpartyIds)}.");
				return ErrorMessage;
			}

			int? counterpartyId = null;
			if(counterpartyCount == 1)
			{
				counterpartyId = counterpartyIds.First();
			}	

			switch(RequestDto.RequestSubType)
			{
				case "firstname":
					return GetCounterpartyNameId(counterpartyId);
				case "patronymic":
					return GetCounterpartyPatronymicId(counterpartyId);
				default:
					return GetCounterpartyAvailability(counterpartyId);
			}
		}

		private string GetCounterpartyAvailability(int? counterpartyId)
		{
			if(!_roboatsSettings.RoboatsEnabled)
			{
				return "Service disabled";
			}
			var result = counterpartyId != null ? "1" : "0";
			return result;
		}

		private string GetCounterpartyNameId(int? counterpartyId)
		{
			if(!counterpartyId.HasValue)
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.ClientNotFound, RoboatsCallOperation.GetClientName, 
					$"Для телефона {ClientPhone} не найден контрагент.");
				return "NO DATA";
			}
			var nameId = _roboatsRepository.GetRoboatsCounterpartyNameId(counterpartyId.Value, ClientPhone);
			if(nameId == 0)
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.ClientNameNotFound, RoboatsCallOperation.GetClientName, 
					$"У контрагента {counterpartyId.Value} не найдено имя.");
				return "NO DATA";
			}
			return $"{nameId}";
		}

		private string GetCounterpartyPatronymicId(int? counterpartyId)
		{
			if(!counterpartyId.HasValue)
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.ClientNotFound, RoboatsCallOperation.GetClientPatronymic,
					$"Для телефона {ClientPhone} не найден контрагент.");
				return "NO DATA";
			}
			var patronymicId = _roboatsRepository.GetRoboatsCounterpartyPatronymicId(counterpartyId.Value, ClientPhone);
			if(patronymicId == 0)
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.ClientPatronymicNotFound, RoboatsCallOperation.GetClientPatronymic,
					$"У контрагента {counterpartyId.Value} не найдено отчество.");
				return "NO DATA";
			}
			return $"{patronymicId}";
		}
	}
}
