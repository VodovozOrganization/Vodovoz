using Mango.Vpbx.Client.Services;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Errors.Logistics;

namespace DriverAPI.Library.V7.Services
{
	/// <inheritdoc/>
	public class CallsService : ICallsService
	{
		private const string _phoneNumberPattern = @"^7\d{10}$";

		private readonly ILogger<CallsService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IMangoWebhookCallsService _mangoWebhookCallsService;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IEmployeeRepository _employeeRepository;

		/// <inheritdoc/>
		public CallsService(
			ILogger<CallsService> logger,
			IUnitOfWork uow,
			IMangoWebhookCallsService mangoWebhookCallsService,
			IRouteListRepository routeListRepository,
			IEmployeeRepository employeeRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_mangoWebhookCallsService = mangoWebhookCallsService ?? throw new ArgumentNullException(nameof(mangoWebhookCallsService));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		/// <inheritdoc/>
		public async Task<Result> MakeWebhookCall(int routeListId, Employee driver, string toNumber, CancellationToken cancellationToken)
		{
			if(driver is null)
			{
				throw new ArgumentNullException(nameof(driver));
			}

			if(string.IsNullOrWhiteSpace(toNumber))
			{
				throw new ArgumentException($"'{nameof(toNumber)}' cannot be null or whitespace.", nameof(toNumber));
			}

			var phoneNumberValidationResult = ValidatePhoneNumber(toNumber);

			if(phoneNumberValidationResult.IsFailure)
			{
				return Result.Failure(phoneNumberValidationResult.Errors);
			}

			var routeList =
				await _routeListRepository.GetRouteListByIdAsync(_uow, routeListId, cancellationToken);

			if(routeList is null)
			{
				_logger.LogError(
					"Маршрутный лист с номером {RouteListId} не найден",
					routeListId);

				return Result.Failure(RouteListErrors.CreateNotFound(routeListId));
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogError(
					"Маршрутный лист с номером {RouteListId} находится в статусе {RouteListStatus}, а не в статусе EnRoute",
					routeListId,
					routeList.Status);

				return Result.Failure(RouteListErrors.NotEnRouteState);
			}

			if(routeList.Driver is null
				|| routeList.Driver.Id != driver.Id)
			{
				_logger.LogError(
					"Водитель с id {DriverId} пытается получить доступ к маршрутному листу с номером {RouteListId}, водителем которого является {RouteListDriverId}",
					driver.Id,
					routeListId,
					routeList.Driver?.Id);

				return Result.Failure(Errors.Security.Authorization.RouteListAccessDenied);
			}

			var extension = await _employeeRepository.GetActiveDriverMangoExtensionNumber(_uow, driver.Id, cancellationToken);

			if(extension is null || extension.ExtensionNumber is null)
			{
				_logger.LogError(
					"У водителя с id {DriverId} не найден активный добавочный номер Mango",
					driver.Id);

				return Result.Failure(Errors.PhoneNumberErrors.CreateActiveMangoExtensionNumberNotFound(driver.Id));
			}

			await _mangoWebhookCallsService.MakeCall(
				extension.ExtensionNumber.ToString(),
				toNumber,
				cancellationToken);

			return Result.Success();
		}

		private Result ValidatePhoneNumber(string phoneNumber)
		{
			if(!Regex.IsMatch(phoneNumber, _phoneNumberPattern))
			{
				var formatMessage = "\"Начинается с 7 и содержит 11 цифр\"";

				_logger.LogError(
					"Номер телефона {PhoneNumber} не соответствует формату {FormatMessage}",
					phoneNumber,
					formatMessage);

				return Result.Failure(Errors.PhoneNumberErrors.CreateInvalidFormat(phoneNumber, formatMessage));
			}

			return Result.Success();
		}
	}
}
