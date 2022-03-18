using System;
using System.Linq;
using Vodovoz.EntityRepositories.Counterparties;

namespace RoboAtsService.Requests
{
	/// <summary>
	/// Обработчик запроса проверки наличия клиента
	/// </summary>
	public class ClientCheckHandler : GetRequestHandlerBase
	{
		const string _requestType = "client";

		private readonly RoboatsRepository _roboatsRepository;

		public override string Request => RoboatsRequestType.ClientCheck;

		public ClientCheckHandler(RoboatsRepository roboatsRepository, RequestDto requestDto) : base(requestDto)
		{
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));

			if(requestDto.RequestType != _requestType)
			{
				throw new InvalidOperationException($"Обработчик {nameof(ClientCheckHandler)} может обрабатывать только запросы с типом {_requestType}");
			}
		}

		public override string Execute()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			var counterpartyCount = counterpartyIds.Count();
			if(counterpartyCount > 1)
			{
				return ErrorMessage;
			}

			var counterpartyId = counterpartyIds.First();

			switch(RequestDto.RequestSubType)
			{
				case "firstname":
					return GetCounterpartyNameId(counterpartyId);
				case "patronymic":
					return GetCounterpartyPatronymicId(counterpartyId);
				default:
					return $"{counterpartyCount}";
			}
		}

		private string GetCounterpartyNameId(int counterpartyId)
		{
			var nameId = _roboatsRepository.GetRoboatsCounterpartyNameId(counterpartyId);
			if(nameId == 0)
			{
				return "NO DATA";
			}
			return $"{nameId}";
		}

		private string GetCounterpartyPatronymicId(int counterpartyId)
		{
			var patronymicId = _roboatsRepository.GetRoboatsCounterpartyPatronymicId(counterpartyId);
			if(patronymicId == 0)
			{
				return "NO DATA";
			}
			return $"{patronymicId}";
		}
	}
}
