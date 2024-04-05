using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoboatsService.Handlers;
using RoboatsService.Monitoring;
using RoboAtsService.Contracts.Requests;
using RoboAtsService.Contracts.Responses;
using System;
using System.Diagnostics;
using System.Linq;
using Vodovoz.Application.Contacts;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;

namespace RoboatsService.Controllers
{
	[ApiController]
	[Route("api")]
	[Authorize]
	public class RoboatsController : ControllerBase
	{
		private readonly ILogger<RoboatsController> _logger;

		private readonly RequestHandlerFactory _handlerFactory;
		private readonly RoboatsCallRegistrator _roboatsCallRegistrator;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IPhoneService _phoneService;

		public RoboatsController(
			ILogger<RoboatsController> logger,
			RequestHandlerFactory handlerFactory,
			RoboatsCallRegistrator roboatsCallRegistrator,
			IRoboatsRepository roboatsRepository,
			IPhoneService phoneService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
			_roboatsCallRegistrator = roboatsCallRegistrator ?? throw new ArgumentNullException(nameof(roboatsCallRegistrator));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_phoneService = phoneService ?? throw new ArgumentNullException(nameof(phoneService));
		}

		[HttpGet]
		public string Get(
			[FromQuery(Name = "CID")] string clientPhone,
			[FromQuery(Name = "CALL_UUID")] string callUUID,
			[FromQuery(Name = "request")] string requestType,
			[FromQuery(Name = "address_id")] string addressId,
			[FromQuery(Name = "order_id")] string orderId,
			[FromQuery(Name = "add")] string isAddOrder,
			[FromQuery(Name = "return")] string returnBottlesCount,
			[FromQuery(Name = "date")] string date,
			[FromQuery(Name = "time")] string time,
			[FromQuery(Name = "fullorder")] string fullOrder,
			[FromQuery(Name = "show")] string checkType,
			[FromQuery(Name = "waterquantity")] string waterQuantity,
			[FromQuery(Name = "bill")] string banknoteForReturn,
			[FromQuery(Name = "payment_type")] string paymentType
			)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();

			if(!Guid.TryParse(callUUID, out Guid callGuid))
			{
				return "ERROR. UUID has incorrect format";
			}

			var request = new RequestDto
			{
				ClientPhone = clientPhone,
				CallGuid = callGuid,
				RequestType = requestType,
				AddressId = addressId,
				Date = date,
				Time = time,
				IsAddOrder = isAddOrder,
				ReturnBottlesCount = returnBottlesCount,
				WaterQuantity = waterQuantity,
				BanknoteForReturn = banknoteForReturn,
				PaymentType = paymentType,
				IsFullOrder = fullOrder,
				RequestSubType = checkType,
				OrderId = orderId
			};

			_roboatsCallRegistrator.RegisterCall(request.ClientPhone, request.CallGuid);

			var handler = _handlerFactory.GetHandler(request);
			if(handler == null)
			{
				_roboatsCallRegistrator.RegisterTerminatingFail(request.ClientPhone, request.CallGuid, RoboatsCallFailType.UnknownRequest, RoboatsCallOperation.OnCreateHandler,
					"Неизвестный тип запроса. Обратитесь в отдел разработки.");
				return "ERROR. Null request";
			}

			var result = handler.Execute();
			stopWatch.Stop();
			var query = HttpContext.Request.GetEncodedPathAndQuery();
			_logger.LogInformation($"Request: {query} | Response: {result} | Request time: {stopWatch.Elapsed.Seconds}.{stopWatch.Elapsed.Milliseconds} sec.");
			return result;
		}

		[HttpGet(nameof(GetCounterpartyHasOrdersForDeliveryToday))]
		public IActionResult GetCounterpartyHasOrdersForDeliveryToday(string counterPartyPhone)
		{
			try
			{
				var counterpartyId = _roboatsRepository.GetCounterpartyIdsByPhone(counterPartyPhone).FirstOrDefault();
				return Ok(_roboatsRepository.CounterpartyHasTodayDeliveryOrders(counterpartyId));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка {ExceptionMessage}", ex.Message);
				return Problem("Ошибка выполнения запроса");
			}
		}

		[HttpGet(nameof(GetCourierPhones))]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetCourierPhonesResponse))]
		public IActionResult GetCourierPhones(int orderId)
		{
			try
			{
				var phone = string.Empty;

				_phoneService
					.GetCourierPhoneNumberForOrder(orderId)
					.Match(
						phoneNumber => phone = phoneNumber,
						errors => _logger.LogWarning("Телефон курьера не найден: {@Errors}", errors.Select(e => e.Message)));

				var dispatcherPhone = _phoneService
					.GetCourierDispatcherPhone();

				return Ok(new GetCourierPhonesResponse
				{
					CourierPhone = phone,
					CourierDispatcher = dispatcherPhone,
					CallTimeout = 60
				});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка {ExceptionMessage}", ex.Message);
				return Problem("Ошибка выполнения запроса");
			}
		}
	}
}
