using DriverApi.Contracts.V6.Responses;
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
using Vodovoz.Settings.Mango;

namespace DriverAPI.Library.V6.Services
{
	/// <inheritdoc/>
	public class CallsService : ICallsService
	{
		private const string _phoneNumberPattern = @"^7\d{10}$";

		private readonly ILogger<CallsService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IMangoVpbxCallsService _mangoVpbxCallsService;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IMangoSettings _mangoSettings;

		/// <inheritdoc/>
		public CallsService(
			ILogger<CallsService> logger,
			IUnitOfWork uow,
			IMangoVpbxCallsService mangoVpbxCallsService,
			IRouteListRepository routeListRepository,
			IEmployeeRepository employeeRepository,
			IMangoSettings mangoSettings
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_mangoVpbxCallsService = mangoVpbxCallsService ?? throw new ArgumentNullException(nameof(mangoVpbxCallsService));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		/// <inheritdoc/>
		public async Task<Result<GetCallResponse>> MakeCall(int routeListId, Employee driver, string toNumber, CancellationToken cancellationToken)
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
				return Result.Failure<GetCallResponse>(phoneNumberValidationResult.Errors);
			}

			var routeList =
				await _routeListRepository.GetRouteListByIdAsync(_uow, routeListId, cancellationToken);

			if(routeList is null)
			{
				_logger.LogError(
					"Маршрутный лист с номером {RouteListId} не найден",
					routeListId);

				return Result.Failure<GetCallResponse>(RouteListErrors.CreateNotFound(routeListId));
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogError(
					"Маршрутный лист с номером {RouteListId} находится в статусе {RouteListStatus}, а не в статусе EnRoute",
					routeListId,
					routeList.Status);

				return Result.Failure<GetCallResponse>(RouteListErrors.NotEnRouteState);
			}

			if(routeList.Driver is null
				|| routeList.Driver.Id != driver.Id)
			{
				_logger.LogError(
					"Водитель с id {DriverId} пытается получить доступ к маршрутному листу с номером {RouteListId}, водителем которого является {RouteListDriverId}",
					driver.Id,
					routeListId,
					routeList.Driver?.Id);

				return Result.Failure<GetCallResponse>(Errors.Security.Authorization.RouteListAccessDenied);
			}

			var extension = await _employeeRepository.GetActiveDriverMangoExtensionNumber(_uow, driver.Id, cancellationToken);

			if(extension is null || extension.ExtensionNumber is null)
			{
				_logger.LogError(
					"У водителя с id {DriverId} не найден активный добавочный номер Mango",
					driver.Id);

				return Result.Failure<GetCallResponse>(Errors.PhoneNumberErrors.CreateActiveMangoExtensionNumberNotFound(driver.Id));
			}

			await _mangoVpbxCallsService.SendCallbackCommand(
				extension.ExtensionNumber.ToString(),
				toNumber,
				cancellationToken);

			return Result.Success(new GetCallResponse
			{
				TimeOut = _mangoSettings.DriversCallTimeOut
			});
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
