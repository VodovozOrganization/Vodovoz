using System;
using CustomerApp.Contracts.Common;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Sale.Templates;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors.Clients;
using Vodovoz.Errors.ExternalCounterparties;
using Vodovoz.Errors.Orders;
using Vodovoz.Settings.Sale;
using VodovozBusiness.EntityRepositories.Sale;
using VodovozBusiness.Errors.Sale;
using VodovozBusiness.Extensions;

namespace CustomerOrdersApi.Library.V8.Services.Validators
{
	/// <inheritdoc/>
	public class TemplateControllerRequestValidator : ITemplateControllerRequestValidator
	{
		private readonly ILogger<TemplateControllerRequestValidator> _logger;
		private readonly IOnlineOrderTemplateSettings _templateSettings;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;
		private readonly IOnlineOrderTemplateRepository _orderTemplateRepository;

		public TemplateControllerRequestValidator(
			ILogger<TemplateControllerRequestValidator> logger,
			IOnlineOrderTemplateSettings templateSettings,
			IOnlineOrderRepository onlineOrderRepository,
			ICounterpartyRepository counterpartyRepository,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IDeliveryScheduleRepository deliveryScheduleRepository,
			IOnlineOrderTemplateRepository orderTemplateRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_templateSettings = templateSettings ?? throw new ArgumentNullException(nameof(templateSettings));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_orderTemplateRepository = orderTemplateRepository ?? throw new ArgumentNullException(nameof(orderTemplateRepository));
		}
		
		/// <inheritdoc/>
		public Result Validate(
			IUnitOfWork uow,
			GetOrderTemplateScheduleOptionsRequest request
			)
		{
			var counterpartyId = request.ErpCounterpartyId;
			var externalCounterpartyId = request.ExternalCounterpartyId;

			var checkResult = CheckCommonProperties(uow, request.Source, counterpartyId, externalCounterpartyId, request.OnlineOrderId);
			
			if(checkResult.IsFailure)
			{
				return checkResult;
			}

			//TODO добавить проверку на автозаказ, может он уже создан или к этому онлайну нельзя создавать его

			return Result.Success();
		}

		public Result Validate(IUnitOfWork uow, CheckOrderTemplateRequest request)
		{
			var counterpartyId = request.ErpCounterpartyId;
			var externalCounterpartyId = request.ExternalCounterpartyId;

			var checkResult = CheckCommonProperties(uow, request.Source, counterpartyId, externalCounterpartyId, request.OnlineOrderId);
			
			if(checkResult.IsFailure)
			{
				return checkResult;
			}

			var checkOrder = ValidateOnlineOrder(uow, request.OnlineOrderId, counterpartyId);

			if(checkOrder.IsFailure)
			{
				return checkOrder;
			}
			
			var checkTemplateDeliverySchedule = ValidateTemplateDeliveryScheduleProperties(uow, request);

			if(checkTemplateDeliverySchedule.IsFailure)
			{
				return checkTemplateDeliverySchedule;
			}
			
			return Result.Success();
		}

		private Result ValidateOnlineOrder(IUnitOfWork uow, int onlineOrderId, int? counterpartyId)
		{
			var onlineOrderFromCounterparty = _onlineOrderRepository.OnlineOrderFromCounterparty(uow, onlineOrderId, counterpartyId);

			if(!onlineOrderFromCounterparty)
			{
				return Result.Failure(OnlineOrderErrors.IsOrderNotBelongCounterparty);
			}

			if(!_onlineOrderRepository.IsAutoOrderEnabled(uow, onlineOrderId))
			{
				return Result.Failure(OnlineOrderErrors.IsOrderTemplateUnavailable);
			}

			if(_orderTemplateRepository.GetActiveOnlineOrderTemplatesCount(uow, counterpartyId.Value) > ActiveTemplatesMaxCount)
			{
				return Result.Failure(OnlineOrderErrors.IsOrderTemplatesLimitExceeded);
			}
			
			return Result.Success();
		}

		private Result ValidateTemplateDeliveryScheduleProperties(IUnitOfWork uow, CheckOrderTemplateRequest request)
		{
			if(request.Schedule is null)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsEmptyExternalCounterparty);
			}
			
			var schedule = request.Schedule;

			if(schedule.Type == TemplateDeliveryScheduleType.Weekly)
			{
				if(!schedule.WeekDay.HasValue)
				{
					return Result.Failure(OnlineOrderTemplateApiErrors.IsEmptyScheduleWeekDay);
				}

				if(!schedule.RepeatEveryWeeks.HasValue)
				{
					return Result.Failure(OnlineOrderTemplateApiErrors.IsEmptyScheduleRepeatEveryWeeks);
				}

				if(schedule.RepeatEveryWeeks < _templateSettings.RepeatEveryWeeksMin
					|| schedule.RepeatEveryWeeks > _templateSettings.RepeatEveryWeeksMax)
				{
					return Result.Failure(OnlineOrderTemplateApiErrors.IsScheduleRepeatEveryWeeksInvalid);
				}

				if(!schedule.FirstDeliveryDate.HasValue)
				{
					return Result.Failure(OnlineOrderTemplateApiErrors.IsEmptyScheduleFirstDeliveryDate);
				}
				
				//проверить допустимость значения
			}
			else
			{
				if(!schedule.DayOfMonth.HasValue)
				{
					return Result.Failure(OnlineOrderTemplateApiErrors.IsEmptyScheduleMonthDay);
				}

				if(schedule.DayOfMonth < _templateSettings.MonthDayMin
					|| schedule.DayOfMonth > _templateSettings.MonthDayMax)
				{
					return Result.Failure(OnlineOrderTemplateApiErrors.IsScheduleMonthDayInvalid);
				}
			}
			
			var deliverySchedule = _deliveryScheduleRepository.Get(uow, schedule.DeliveryScheduleId);

			if(deliverySchedule is null)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsDeliveryScheduleUnknown);
			}
			
			//допустимость значения интервала
			var deliveryScheduleInvalid = ;

			if(deliveryScheduleInvalid)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsDeliveryScheduleInvalid);
			}
			
			return Result.Success();
		}

		private Result CheckCommonProperties(
			IUnitOfWork uow,
			ExternalSource source,
			int? counterpartyId,
			Guid? externalCounterpartyId,
			int onlineOrderId
		)
		{
			if(!counterpartyId.HasValue)
			{
				_logger.LogWarning("Пришел запрос с пустым значением идентификатора клиента");
				return Result.Failure(OnlineOrderTemplateApiErrors.IsEmptyCounterparty);
			}
			
			if(!_counterpartyRepository.CounterpartyExists(uow, counterpartyId))
			{
				_logger.LogWarning("Пришел запрос от неизвестного клиента {CounterpartyId}", counterpartyId);
				return Result.Failure(CounterpartyApiErrors.NotFound);
			}

			if(!externalCounterpartyId.HasValue)
			{
				_logger.LogWarning("Пришел запрос с пустым значением идентификатора пользователя");
				return Result.Failure(OnlineOrderTemplateApiErrors.IsEmptyExternalCounterparty);
			}
			
			if(!_externalCounterpartyRepository.ExternalCounterpartyExists(uow, externalCounterpartyId.Value, source.ToCounterpartyFrom()))
			{
				_logger.LogWarning(
					"Пришел запрос от неизвестного пользователя {ExternalCounterpartyId} {Source}",
					externalCounterpartyId,
					source.ToString());
				return Result.Failure(ExternalCounterpartyApiErrors.NotFound);
			}
			
			var onlineOrderNotExists = !_onlineOrderRepository.OnlineOrderExists(uow, onlineOrderId);

			if(onlineOrderNotExists)
			{
				_logger.LogWarning("Заказ {OnlineOrderId} не найден", onlineOrderId);
				return Result.Failure(OnlineOrderErrors.OnlineOrderNotFound("404"));
			}

			return Result.Success();
		}
	}
}
