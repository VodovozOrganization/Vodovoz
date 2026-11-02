using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Extensions;
using CustomerOrdersApi.Library.V8.Factories;
using CustomerOrdersApi.Library.V8.Repositories;
using CustomerOrdersApi.Library.V8.Services.Validators;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Errors.Sale;

namespace CustomerOrdersApi.Library.V8.Services
{
	public class CustomerTemplateService
	{
		private readonly ILogger<CustomerTemplateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ITemplateControllerRequestValidator _templateControllerRequestValidator;
		private readonly ICanCreateTemplateFromOnlineOrderValidator _canCreateTemplateValidator;
		private readonly ICustomerTemplateRepository _customerTemplateRepository;
		private readonly IRepeatTemplateEveryWeeksFactory _repeatEveryWeeksFactory;
		private readonly IRepeatTemplateEveryMonthFactory _repeatEveryMonthFactory;
		private readonly ICustomerTemplateFactory _customerTemplateFactory;
		private readonly OnlineOrderTemplateCreator _templateCreator;
		private readonly IOnlineOrderTemplateStatusHandler _templateStatusHandler;

		public CustomerTemplateService(
			ILogger<CustomerTemplateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ITemplateControllerRequestValidator templateControllerRequestValidator,
			ICanCreateTemplateFromOnlineOrderValidator canCreateTemplateValidator,
			ICustomerTemplateRepository customerTemplateRepository,
			IRepeatTemplateEveryWeeksFactory repeatEveryWeeksFactory,
			IRepeatTemplateEveryMonthFactory repeatEveryMonthFactory,
			ICustomerTemplateFactory customerTemplateFactory,
			OnlineOrderTemplateCreator templateCreator,
			IOnlineOrderTemplateStatusHandler templateStatusHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_templateControllerRequestValidator = templateControllerRequestValidator ?? throw new ArgumentNullException(nameof(templateControllerRequestValidator));
			_canCreateTemplateValidator = canCreateTemplateValidator ?? throw new ArgumentNullException(nameof(canCreateTemplateValidator));
			_customerTemplateRepository = customerTemplateRepository ?? throw new ArgumentNullException(nameof(customerTemplateRepository));
			_repeatEveryWeeksFactory = repeatEveryWeeksFactory ?? throw new ArgumentNullException(nameof(repeatEveryWeeksFactory));
			_repeatEveryMonthFactory = repeatEveryMonthFactory ?? throw new ArgumentNullException(nameof(repeatEveryMonthFactory));
			_customerTemplateFactory = customerTemplateFactory ?? throw new ArgumentNullException(nameof(customerTemplateFactory));
			_templateCreator = templateCreator ?? throw new ArgumentNullException(nameof(templateCreator));
			_templateStatusHandler = templateStatusHandler ?? throw new ArgumentNullException(nameof(templateStatusHandler));
		}

		public Result<GetOrderTemplateScheduleOptionsResponse> GetOrderTemplateScheduleOptions(GetOrderTemplateScheduleOptionsRequest request)
		{
			ILookup<WeekDayName, DeliveryScheduleDto> scheduleLookup = null;
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Получение настроек доставки автозаказа"))
			{
				_logger.LogInformation("Проверяем пришедшие параметры запроса");
				var validationResult = _templateControllerRequestValidator.Validate(uow, request);

				if(validationResult.IsFailure)
				{
					return Result.Failure<GetOrderTemplateScheduleOptionsResponse>(validationResult.Errors);
				}
					
				scheduleLookup = _customerTemplateRepository.GetWeeklyDeliverySchedules(uow, request.OnlineOrderId);
			}
				
			var weekDaySchedules = (
					from day in Enum.GetValues<WeekDayName>()
					where day != WeekDayName.Today
					let schedulesValue = scheduleLookup[day]
					select TemplateWeekdaySchedule.Create(day.ToDayOfWeek(), schedulesValue)
				)
				.ToList();

			var weekly = WeeklyTemplateScheduleOptions.Create(_repeatEveryWeeksFactory.Create(), weekDaySchedules);
			var monthly = _repeatEveryMonthFactory.Create(scheduleLookup);

			return Result.Success(GetOrderTemplateScheduleOptionsResponse.Create(weekly, monthly));
		}

		public Result<CheckOrderTemplateResponse> CheckOrderTemplate(
			CheckOrderTemplateRequest request
			)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Проверка параметров автозаказа");
			_logger.LogInformation("Проверяем пришедшие параметры запроса");
			var validationResult = _templateControllerRequestValidator.Validate(uow, request);

			if(validationResult.IsFailure)
			{
				return Result.Failure<CheckOrderTemplateResponse>(validationResult.Errors);
			}

			_logger.LogInformation("Проверяем доступность автозаказа");
			var canCreateTemplateDto = _customerTemplateRepository.GetCanCreateTemplateDataFromOnlineOrder(
				uow,
				request.OnlineOrderId);
				
			var canCreateTemplateResult = _canCreateTemplateValidator.Validate(uow, canCreateTemplateDto);

			if(canCreateTemplateResult.IsFailure)
			{
				return Result.Failure<CheckOrderTemplateResponse>(canCreateTemplateResult.Errors);
			}
				
			var response = _customerTemplateFactory.CreateCheckOrderTemplateResponse(uow, request);
			return Result.Success(response);
		}

		public async Task<Result<CreateOrderTemplateResponse>> CreateOrderTemplate(
			CreateOrderTemplateRequest request,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Создание шаблона автозаказа");
			
			_logger.LogInformation("Проверяем пришедшие параметры запроса");
			var validationResult = _templateControllerRequestValidator.Validate(uow, request);

			if(validationResult.IsFailure)
			{
				return Result.Failure<CreateOrderTemplateResponse>(validationResult.Errors);
			}

			_logger.LogInformation("Проверяем доступность автозаказа");
			var canCreateTemplateDto = _customerTemplateRepository.GetCanCreateTemplateDataFromOnlineOrder(
				uow,
				request.OnlineOrderId);

			var canCreateTemplateResult = _canCreateTemplateValidator.Validate(uow, canCreateTemplateDto);

			if(canCreateTemplateResult.IsFailure)
			{
				return Result.Failure<CreateOrderTemplateResponse>(canCreateTemplateResult.Errors);
			}

			var onlineOrder = uow.GetById<OnlineOrder>(request.OnlineOrderId);
			var timers = uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

			if(onlineOrder.OnlineOrderStatus != OnlineOrderStatus.PendingAutoOrderSettings
				|| onlineOrder.TemplateSetupTimeIsUp(timers))
			{
				return Result.Failure<CreateOrderTemplateResponse>(OnlineOrderTemplateApiErrors.OrderTemplateSetupExpired);
			}

			if(onlineOrder.Template != null)
			{
				return Result.Failure<CreateOrderTemplateResponse>(OnlineOrderTemplateApiErrors.OrderTemplateAlreadyExists);
			}

			var schedule = request.Schedule;

			await _templateCreator.CreateAsync(
				uow,
				onlineOrder,
				TemplateDeliveryScheduleData.Create(
					schedule.Type.ToTemplateScheduleType(),
					schedule.DeliveryScheduleId,
					schedule.WeekDay.ToWeekDayName(),
					schedule.RepeatEveryWeeks,
					schedule.FirstDeliveryDate,
					schedule.DayOfMonth),
				cancellationToken);

			await uow.CommitAsync(cancellationToken);

			var response = _customerTemplateFactory.CreateOrderTemplateResponse(uow, onlineOrder, request);
			return Result.Success(response);
		}

		public async Task<Result<OrderTemplateInfoDto>> GetOrderTemplateInfoAsync(
			GetOrderTemplateInfoDto orderTemplateInfo,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Создание детализации об автозаказе");
			//TODO-6121 какие делаем здесь проверки?
			_logger.LogInformation("Проверяем пришедшие параметры запроса");
			/*var validationResult = _templateControllerRequestValidator.Validate(uow, orderTemplateInfo);

			if(validationResult.IsFailure)
			{
				return Result.Failure<OrderTemplateInfoDto>(validationResult.Errors);
			}*/

			var templateInfo = await _customerTemplateFactory.CreateOrderTemplateInfoAsync(
				uow,
				orderTemplateInfo.Source,
				orderTemplateInfo.OrderTemplateId,
				cancellationToken);

			if(templateInfo is null)
			{
				return Result.Failure<OrderTemplateInfoDto>(OnlineOrderTemplateApiErrors.OrderTemplateNotFound);
			}

			return templateInfo;
		}
		
		public async Task<Result<IEnumerable<OrderTemplateCardDto>>> GetOrderTemplatesAsync(
			GetOrderTemplatesDto getOrderTemplates,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Выборка списка шаблонов автозаказов клиента");
			//TODO-6121 какие делаем здесь проверки?
			_logger.LogInformation("Проверяем пришедшие параметры запроса");
			/*var validationResult = _templateControllerRequestValidator.Validate(uow, orderTemplateInfo);

			if(validationResult.IsFailure)
			{
				return Result.Failure<OrderTemplateInfoDto>(validationResult.Errors);
			}*/

			var skipElements = (getOrderTemplates.Page - 1) * getOrderTemplates.TemplatesCountOnPage;
			var rawData =
				await _customerTemplateRepository.GetOrderTemplateCardsRawDataAsync(
					uow,
					getOrderTemplates.ErpCounterpartyId,
					skipElements,
					getOrderTemplates.TemplatesCountOnPage,
					cancellationToken
				);

			var orderTemplates = _customerTemplateFactory.CreateOrderTemplateCards(
				getOrderTemplates.Source,
				rawData
				);

			return Result.Success(orderTemplates);
		}

		public async Task<Result<PauseOrderTemplateResponse>> PauseOrderTemplate(
			PauseOrderTemplateRequest request,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Приостановка шаблона автозаказа");
			//TODO-6121 какие делаем здесь проверки?
			_logger.LogInformation("Проверяем пришедшие параметры запроса");
			/*var validationResult = _templateControllerRequestValidator.Validate(uow, orderTemplateInfo);

			if(validationResult.IsFailure)
			{
				return Result.Failure<OrderTemplateInfoDto>(validationResult.Errors);
			}*/

			var template = uow.GetById<OnlineOrderTemplate>(request.OrderTemplateId);

			if(template is null)
			{
				return Result.Failure<PauseOrderTemplateResponse>(OnlineOrderTemplateApiErrors.OrderTemplateNotFound);
			}
			
			//проверки состояний

			var result = _templateStatusHandler.PauseTemplate(uow, template, request.ErpCounterpartyId, null);

			if(result.IsFailure)
			{
				return Result.Failure<PauseOrderTemplateResponse>(result.Errors);
			}
			
			await uow.CommitAsync(cancellationToken);
			return Result.Success(PauseOrderTemplateResponse.CreateSuccess(template.Id));
		}

		public async Task<Result<GetOrderTemplateResumeDatesResponse>> GetOrderTemplateResumeDatesAsync(
			GetOrderTemplateResumeDatesRequest request,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Получение ближайших дат шаблона автозаказа");
			//TODO-6121 какие делаем здесь проверки?
			_logger.LogInformation("Проверяем пришедшие параметры запроса");
			var validationResult = _templateControllerRequestValidator.Validate(uow, request);

			if(validationResult.IsFailure)
			{
				return Result.Failure<GetOrderTemplateResumeDatesResponse>(validationResult.Errors);
			}
			
			//проверки состояний
			var template = uow.GetById<OnlineOrderTemplate>(request.OrderTemplateId);

			var dates = _customerTemplateFactory.GetResumeDates(uow, template, null);
			
			await uow.CommitAsync(cancellationToken);
			return Result.Success(GetOrderTemplateResumeDatesResponse.Create(dates));
		}

		public async Task<Result<ResumeOrderTemplateResponse>> ResumeOrderTemplateAsync(
			ResumeOrderTemplateRequest request,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Возобновление шаблона автозаказа");
			_logger.LogInformation("Проверяем пришедшие параметры запроса");

			var validationResult = _templateControllerRequestValidator.ValidateResumeRequest(uow, request);
			if(validationResult.IsFailure)
			{
				return Result.Failure<ResumeOrderTemplateResponse>(validationResult.Errors);
			}

			var template = uow.GetById<OnlineOrderTemplate>(request.OrderTemplateId);
			if(template is null)
			{
				return Result.Failure<ResumeOrderTemplateResponse>(OnlineOrderTemplateApiErrors.OrderTemplateNotFound);
			}

			// Если уже Active — idempotent: возвращаем текущие данные
			if(template.Status == OnlineOrderTemplateStatus.Active)
			{
				if(template.Schedule.FirstDeliveryDate == request.NextDeliveryDate)
				{
					var newNextDeliveries = _customerTemplateFactory.GetResumeDates(uow, template, null);
					
					return Result.Success(
						ResumeOrderTemplateResponse.Create(
							template.Id,
							TemplateStatusDto.Active,
							template.Schedule.FirstDeliveryDate.Value,
							newNextDeliveries
						)
					);
				}
				
				return Result.Failure<ResumeOrderTemplateResponse>(OnlineOrderTemplateApiErrors.OrderTemplateActionUnavailable);
			}

			// Смена статуса на Active
			var statusResult = _templateStatusHandler.ResumeTemplate(uow, template, request.ErpCounterpartyId, null);
			
			if(statusResult.IsFailure)
			{
				return Result.Failure<ResumeOrderTemplateResponse>(statusResult.Errors);
			}

			// Weekly: перезаписываем FirstDeliveryDate
			if(template.Schedule.ScheduleType == TemplateDeliveryScheduleType.Weekly)
			{
				template.Schedule = TemplateDeliveryScheduleData.Create(
					template.Schedule.ScheduleType,
					template.Schedule.DeliveryScheduleId,
					template.Schedule.Weekday,
					template.Schedule.RepeatEveryWeeks,
					request.NextDeliveryDate,
					template.Schedule.DayOfMonth);
			}
			// Monthly: проверяем что dayOfMonth совпадает
			else if(template.Schedule.ScheduleType == TemplateDeliveryScheduleType.Monthly)
			{
				if(request.NextDeliveryDate.Day != template.Schedule.DayOfMonth.Value)
				{
					return Result.Failure<ResumeOrderTemplateResponse>(
						new Error("400", $"Выбранная дата {request.NextDeliveryDate:dd.MM.yyyy} не соответствует числу месяца {template.Schedule.DayOfMonth}"));
				}
			}

			await uow.CommitAsync(cancellationToken);

			var nextDeliveries = _customerTemplateFactory.GetResumeDates(uow, template, null);
			return Result.Success(ResumeOrderTemplateResponse.Create(
				template.Id,
				TemplateStatusDto.Active,
				request.NextDeliveryDate,
				nextDeliveries));
		}

		public async Task<Result<ArchiveOrderTemplateResponse>> ArchiveOrderTemplateAsync(
			ArchiveOrderTemplateRequest request,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Архивирование шаблона автозаказа");
			_logger.LogInformation("Проверяем пришедшие параметры запроса");

			// Валидация
			var validationResult = _templateControllerRequestValidator.ValidateArchiveRequest(uow, request);
			if(validationResult.IsFailure)
			{
				return Result.Failure<ArchiveOrderTemplateResponse>(validationResult.Errors);
			}

			var template = uow.GetById<OnlineOrderTemplate>(request.OrderTemplateId);
			if(template is null)
			{
				return Result.Failure<ArchiveOrderTemplateResponse>(OnlineOrderTemplateApiErrors.OrderTemplateNotFound);
			}

			// Если уже архивирован — idempotent: возвращаем текущие данные
			if(template.Status == OnlineOrderTemplateStatus.Archived)
			{
				return Result.Success(ArchiveOrderTemplateResponse.Create(template.Id, TemplateStatusDto.Archived));
			}

			// Удаление токена рекуррентного платежа
			if(template.RecurrentPayment != null)
			{
				var payment = uow.GetById<OnlineRecurrentPayment>(template.RecurrentPayment.Id);
				if(payment != null)
				{
					uow.Delete(payment);
				}
				template.RecurrentPayment = null;
			}

			// Архивация
			template.Archive();
			
			// Запись события
			OnlineOrderTemplateStatusChanged.Create(DateTime.Now, null, request.ErpCounterpartyId, OnlineOrderTemplateStatusChangedType.ToArchive);

			await uow.CommitAsync(cancellationToken);

			return Result.Success(ArchiveOrderTemplateResponse.Create(template.Id, TemplateStatusDto.Archived));
		}
	}
}
