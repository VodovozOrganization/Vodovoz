using System;
using System.Linq;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Factories;
using CustomerOrdersApi.Library.V8.Repositories;
using CustomerOrdersApi.Library.V8.Services.Validators;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Services
{
	public class CustomerTemplateService
	{
		private readonly ILogger<CustomerTemplateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ITemplateControllerRequestValidator _templateControllerRequestValidator;
		private readonly IOnlineTemplateRepository _onlineTemplateRepository;
		private readonly IRepeatTemplateEveryWeeksFactory _repeatEveryWeeksFactory;
		private readonly IRepeatTemplateEveryMonthFactory _repeatEveryMonthFactory;

		public CustomerTemplateService(
			ILogger<CustomerTemplateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ITemplateControllerRequestValidator templateControllerRequestValidator,
			IOnlineTemplateRepository onlineTemplateRepository,
			IRepeatTemplateEveryWeeksFactory repeatEveryWeeksFactory,
			IRepeatTemplateEveryMonthFactory repeatEveryMonthFactory
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_templateControllerRequestValidator = templateControllerRequestValidator ?? throw new ArgumentNullException(nameof(templateControllerRequestValidator));
			_onlineTemplateRepository = onlineTemplateRepository ?? throw new ArgumentNullException(nameof(onlineTemplateRepository));
			_repeatEveryWeeksFactory = repeatEveryWeeksFactory ?? throw new ArgumentNullException(nameof(repeatEveryWeeksFactory));
			_repeatEveryMonthFactory = repeatEveryMonthFactory ?? throw new ArgumentNullException(nameof(repeatEveryMonthFactory));
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
					
				scheduleLookup = _onlineTemplateRepository.GetWeeklyDeliverySchedules(uow, request.OnlineOrderId);
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

		public Result<CheckOrderTemplateResponse> CheckOrderTemplate(CheckOrderTemplateRequest request)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Проверка параметров автозаказа"))
			{
				_logger.LogInformation("Проверяем пришедшие параметры запроса");
				var validationResult = _templateControllerRequestValidator.Validate(uow, request);

				if(validationResult.IsFailure)
				{
					return Result.Failure<CheckOrderTemplateResponse>(validationResult.Errors);
				}
			}
		}
	}
}
