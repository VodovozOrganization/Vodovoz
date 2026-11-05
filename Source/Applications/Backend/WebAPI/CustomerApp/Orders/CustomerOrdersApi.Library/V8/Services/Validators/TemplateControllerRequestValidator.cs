using System;
using System.Linq;
using CustomerApp.Contracts.Common;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Repositories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
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
		private readonly ICustomerTemplateRepository _customerTemplateRepository;
		private readonly CommonDeliverySchedulesService _commonDeliverySchedulesService;

		public TemplateControllerRequestValidator(
			ILogger<TemplateControllerRequestValidator> logger,
			IOnlineOrderTemplateSettings templateSettings,
			IOnlineOrderRepository onlineOrderRepository,
			ICounterpartyRepository counterpartyRepository,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IDeliveryScheduleRepository deliveryScheduleRepository,
			IOnlineOrderTemplateRepository orderTemplateRepository,
			ICustomerTemplateRepository customerTemplateRepository,
			CommonDeliverySchedulesService commonDeliverySchedulesService
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
			_customerTemplateRepository = customerTemplateRepository ?? throw new ArgumentNullException(nameof(customerTemplateRepository));
			_commonDeliverySchedulesService =
				commonDeliverySchedulesService ?? throw new ArgumentNullException(nameof(commonDeliverySchedulesService));
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

			var checkTemplateDeliverySchedule = ValidateTemplateDeliveryScheduleProperties(uow, request.OnlineOrderId, request.Schedule);

			if(checkTemplateDeliverySchedule.IsFailure)
			{
				return checkTemplateDeliverySchedule;
			}

			return Result.Success();
		}

		public Result Validate(IUnitOfWork uow, CreateOrderTemplateRequest request)
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

			//TODO Стоит ли здесь оставлять проверку по времени deliverySchedule.To < new TimeSpan(11, 0, 0)
			//т.к. теоретически этот запрос может прийти после нужного момента и срезаться на этом
			var checkTemplateDeliverySchedule = ValidateTemplateDeliveryScheduleProperties(uow, request.OnlineOrderId, request.Schedule);

			if(checkTemplateDeliverySchedule.IsFailure)
			{
				return checkTemplateDeliverySchedule;
			}

			return Result.Success();
		}

		public Result Validate(IUnitOfWork uow, GetOrderTemplateInfoDto request)
		{
			var templateId = request.OrderTemplateId;
			var counterpartyId = request.ErpCounterpartyId;
			var templateExists = _orderTemplateRepository.NotArchiveTemplateExists(uow, templateId);

			if(!templateExists)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateNotFound);
			}

			var templateFromCounterparty = _orderTemplateRepository.OnlineOrderTemplateFromCounterparty(uow, templateId, counterpartyId);

			if(!templateFromCounterparty)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsTemplateNotBelongCounterparty);
			}
			
			return Result.Success();
		}

		public Result Validate(IUnitOfWork uow, GetOrderTemplateResumeDatesRequest request)
		{
			var templateId = request.OrderTemplateId;
			var counterpartyId = request.ErpCounterpartyId;
			var templateExists = _orderTemplateRepository.NotArchiveTemplateExists(uow, templateId);

			//TODO-6121 поговорить по кодам ответа
			if(!templateExists)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateNotFound);
			}

			var templateFromCounterparty = _orderTemplateRepository.OnlineOrderTemplateFromCounterparty(uow, templateId, counterpartyId);

			if(!templateFromCounterparty)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsTemplateNotBelongCounterparty);
			}

			if(_orderTemplateRepository
					.GetActiveOnlineOrderTemplatesCount(uow, counterpartyId.Value) > _templateSettings.GetMaximumNumberActiveOnlineOrderTemplates)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplatesLimitExceeded("409"));
			}

			return Result.Success();
		}
		
		public Result Validate(IUnitOfWork uow, PauseOrderTemplateRequest request)
		{
			var templateId = request.OrderTemplateId;
			var counterpartyId = request.ErpCounterpartyId;
			var templateExists = _orderTemplateRepository.NotArchiveTemplateExists(uow, templateId);
			
			if(!templateExists)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateNotFound);
			}

			var templateFromCounterparty = _orderTemplateRepository.OnlineOrderTemplateFromCounterparty(uow, templateId, counterpartyId);

			if(!templateFromCounterparty)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsTemplateNotBelongCounterparty);
			}
			
			return Result.Success();
		}

		public Result Validate(IUnitOfWork uow, ResumeOrderTemplateRequest request)
		{
			// 1. Автозаказ существует, принадлежит клиенту, не удалён
			var templateExists = _orderTemplateRepository.NotArchiveTemplateExists(uow, request.OrderTemplateId);
			if(!templateExists)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateNotFound);
			}

			var templateFromCounterparty =
				_orderTemplateRepository.OnlineOrderTemplateFromCounterparty(uow, request.OrderTemplateId, request.ErpCounterpartyId);
			if(!templateFromCounterparty)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsTemplateNotBelongCounterparty);
			}

			// Получаем шаблон для дальнейших проверок
			var template = uow.GetById<OnlineOrderTemplate>(request.OrderTemplateId);

			// 2. Статус Paused
			if(template.Status != OnlineOrderTemplateStatus.Paused)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateActionUnavailable);
			}

			var activeTemplates = _orderTemplateRepository.GetActiveOnlineOrderTemplatesCount(uow, request.ErpCounterpartyId.Value);
			// 3. Лимит активных автозаказов
			if(activeTemplates >= _templateSettings.GetMaximumNumberActiveOnlineOrderTemplates)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplatesLimitExceeded("409"));
			}

			// 4. Токен оплаты для PaidOnline
			if(template.PaymentType == OnlineOrderPaymentType.PaidOnline)
			{
				if(template.RecurrentPayment == null || string.IsNullOrWhiteSpace(template.RecurrentPayment.Token))
				{
					return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplatePaymentMethodRequired);
				}
			}

			// 5. Адрес доставки активен
			var deliveryPoint = uow.GetById<Vodovoz.Domain.Client.DeliveryPoint>(template.DeliveryPointId);
			if(deliveryPoint == null || !deliveryPoint.IsActive)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateAddressUnavailable);
			}

			// 6. Интервал автозаказа есть в графике района
			var schedule = template.Schedule;
			var deliveryScheduleValid = true;
			ILookup<WeekDayName, DeliveryScheduleDto> scheduleLookup =
				_customerTemplateRepository.GetWeeklyDeliverySchedulesByDeliveryPointId(uow, template.DeliveryPointId);

			if(schedule.ScheduleType == TemplateDeliveryScheduleType.Weekly)
			{
				if(schedule.Weekday.HasValue)
				{
					deliveryScheduleValid = scheduleLookup[schedule.Weekday.Value]
						.Any(x => x.ErpId == schedule.DeliveryScheduleId);
				}
				else
				{
					deliveryScheduleValid = false;
				}
			}
			else if(schedule.ScheduleType == TemplateDeliveryScheduleType.Monthly)
			{
				if(_commonDeliverySchedulesService.ProcessSchedules(scheduleLookup, out var commonDeliverySchedules))
				{
					deliveryScheduleValid = commonDeliverySchedules.Any(x => x.ErpId == schedule.DeliveryScheduleId);
				}
			}

			if(!deliveryScheduleValid)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateIntervalUnavailable);
			}

			return Result.Success();
		}
		
		public Result ValidateArchiveRequest(IUnitOfWork uow, ArchiveOrderTemplateRequest request)
		{
			// 1. Автозаказ существует, принадлежит клиенту, не удалён
			var templateExists = _orderTemplateRepository.NotArchiveTemplateExists(uow, request.OrderTemplateId);
			if(!templateExists)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateNotFound);
			}

			var templateFromCounterparty =
				_orderTemplateRepository.OnlineOrderTemplateFromCounterparty(uow, request.OrderTemplateId, request.ErpCounterpartyId);
			if(!templateFromCounterparty)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsTemplateNotBelongCounterparty);
			}

			// Получаем шаблон для проверки статуса
			var template = uow.GetById<OnlineOrderTemplate>(request.OrderTemplateId);

			// 2. Статус
			if(template.Status == OnlineOrderTemplateStatus.Archived)
			{
				return Result.Success();
			}

			if(template.Status != OnlineOrderTemplateStatus.Active
				&& template.Status != OnlineOrderTemplateStatus.Paused
				&& template.Status != OnlineOrderTemplateStatus.PaymentProblem)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateArchiveUnavailable);
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
		
		private Result ValidateOnlineOrder(IUnitOfWork uow, int onlineOrderId, int? counterpartyId)
		{
			var onlineOrderFromCounterparty = _onlineOrderRepository.OnlineOrderFromCounterparty(uow, onlineOrderId, counterpartyId);

			if(!onlineOrderFromCounterparty)
			{
				return Result.Failure(OnlineOrderErrors.IsOrderNotBelongCounterparty);
			}

			if(!_onlineOrderRepository.IsAutoOrderEnabled(uow, onlineOrderId))
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsOrderTemplateUnavailable);
			}

			//TODO что считаем активным? Ведь теоретически клиент может с разных устройств настраивать несколько автозаказов?
			if(_orderTemplateRepository
					.GetActiveOnlineOrderTemplatesCount(uow, counterpartyId.Value) > _templateSettings.GetMaximumNumberActiveOnlineOrderTemplates)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplatesLimitExceeded("400"));
			}

			return Result.Success();
		}

		private Result ValidateTemplateDeliveryScheduleProperties(
			IUnitOfWork uow,
			int onlineOrderId,
			TemplateDeliveryScheduleDto schedule)
		{
			if(schedule is null)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsEmptyExternalCounterparty);
			}

			var deliveryScheduleValid = true;
			ILookup<WeekDayName, DeliveryScheduleDto> scheduleLookup = null;

			if(schedule.Type == CustomerTemplateDeliveryScheduleType.Weekly)
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

				var firstDeliveryDate = schedule.FirstDeliveryDate;

				if(!schedule.FirstDeliveryDate.HasValue)
				{
					return Result.Failure(OnlineOrderTemplateApiErrors.IsEmptyScheduleFirstDeliveryDate);
				}

				if(firstDeliveryDate <= DateTime.Today)
				{
					_logger.LogWarning("Дата первой доставки не может быть раньше или равной сегодня");
					return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateDateUnavailable);
				}

				scheduleLookup = _customerTemplateRepository.GetWeeklyDeliverySchedules(uow, onlineOrderId);
				deliveryScheduleValid = scheduleLookup[schedule.WeekDay.Value.ConvertToWeekDayName()]
					.Any(x => x.ErpId == schedule.DeliveryScheduleId);
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

				scheduleLookup = _customerTemplateRepository.GetWeeklyDeliverySchedules(uow, onlineOrderId);
				if(_commonDeliverySchedulesService.ProcessSchedules(scheduleLookup, out var commonDeliverySchedules))
				{
					deliveryScheduleValid = commonDeliverySchedules.Any(x => x.ErpId == schedule.DeliveryScheduleId);
				}
			}

			var deliverySchedule = _deliveryScheduleRepository.Get(uow, schedule.DeliveryScheduleId);

			if(deliverySchedule is null)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsDeliveryScheduleUnknown);
			}

			if(!deliveryScheduleValid)
			{
				return Result.Failure(OnlineOrderTemplateApiErrors.IsDeliveryScheduleInvalid);
			}

			if(deliverySchedule.To < new TimeSpan(11, 0, 0))
			{
				if(schedule.Type == CustomerTemplateDeliveryScheduleType.Weekly)
				{
					if(schedule.FirstDeliveryDate < DateTime.Today.AddDays(2))
					{
						return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateDateUnavailable);
					}
				}
				else
				{
					if(schedule.DayOfMonth < DateTime.Today.AddDays(2).Day)
					{
						return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateDateUnavailable);
					}
				}
			}
			else
			{
				if(schedule.Type == CustomerTemplateDeliveryScheduleType.Weekly)
				{
					if(schedule.FirstDeliveryDate < DateTime.Today.AddDays(3))
					{
						return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateDateUnavailable);
					}
				}
				else
				{
					if(schedule.DayOfMonth < DateTime.Today.AddDays(3).Day)
					{
						return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateDateUnavailable);
					}
				}
			}

			return Result.Success();
		}
	}
}
