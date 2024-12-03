using Microsoft.Extensions.Logging;
using RoboatsService.Monitoring;
using RoboAtsService.Contracts.Requests;
using System;
using System.Linq;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;

namespace RoboatsService.Handlers
{
	/// <summary>
	/// Обработчик запросов получения данных о воде
	/// </summary>
	public class WaterTypeHandler : GetRequestHandlerBase
	{
		private readonly ILogger<WaterTypeHandler> _logger;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly RoboatsCallRegistrator _callRegistrator;

		public override string Request => RoboatsRequestType.WaterType;

		public WaterTypeHandler(ILogger<WaterTypeHandler> logger, IRoboatsRepository roboatsRepository, RequestDto requestDto, RoboatsCallRegistrator callRegistrator) : base(requestDto)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_callRegistrator = callRegistrator ?? throw new ArgumentNullException(nameof(callRegistrator));
		}

		public override string ErrorMessage => $"ERROR. request={Request}&show={RequestDto.RequestSubType}";

		public override string Execute()
		{
			try
			{
				return ExecuteRequest();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обработке запроса информации о доступных типах воды возникло исключение");
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.Exception, RoboatsCallOperation.OnWaterTypeHandle,
						$"При обработке запроса информации о доступных типах воды возникло исключение: {ex.Message}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}
		}

		public string ExecuteRequest()
		{
			if(RequestDto.RequestSubType != "quantity")
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.UnknownRequestType, RoboatsCallOperation.OnWaterTypeHandle,
					$"Неизвестный тип запроса {RequestDto.RequestSubType}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}

			var waterTypes = _roboatsRepository.GetWaterTypes();
			if(!waterTypes.Any())
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.AvailableWatersNotFound, RoboatsCallOperation.OnWaterTypeHandle,
					$"Не найдены доступные для заказа типы воды. Проверьте справочник типов воды для Roboats.");
				return ErrorMessage;
			}

			return string.Join('|', waterTypes.Select(x => x.RoboatsId));
		}
	}

}
