using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using RoboatsService.Handlers;
using RoboatsService.Monitoring;
using RoboatsService.Options;
using RoboAtsService.Contracts.Requests;
using RoboAtsService.Contracts.Responses;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Application.Contacts;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
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
		private readonly IOptions<RoboAtsOptions> _roboAtsOptions;
		private readonly RequestHandlerFactory _handlerFactory;
		private readonly RoboatsCallRegistrator _roboatsCallRegistrator;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IGenericRepository<DeliveryPoint> _deliveryPointRepository;
		private readonly IPhoneService _phoneService;

		public RoboatsController(
			ILogger<RoboatsController> logger,
			IOptions<RoboAtsOptions> roboAtsOptions,
			RequestHandlerFactory handlerFactory,
			RoboatsCallRegistrator roboatsCallRegistrator,
			IRoboatsRepository roboatsRepository,
			IGenericRepository<Order> orderRepository,
			IGenericRepository<DeliveryPoint> deliveryPointRepository,
			IPhoneService phoneService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboAtsOptions = roboAtsOptions ?? throw new ArgumentNullException(nameof(roboAtsOptions));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
			_roboatsCallRegistrator = roboatsCallRegistrator ?? throw new ArgumentNullException(nameof(roboatsCallRegistrator));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
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

		/// <summary>
		/// Получение точек доставки по номеру входящего звонка
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="counterpartyPhone">Номер входящего звонка</param>
		/// <returns></returns>
		[HttpGet(nameof(GetContactPhoneHasOrdersForDeliveryToday))]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetContactPhoneHasOrdersForDeliveryTodayResponse))]
		public IActionResult GetContactPhoneHasOrdersForDeliveryToday([FromServices]IUnitOfWork unitOfWork, string counterpartyPhone)
		{
			try
			{
				var todayOrdersDeliveryPointsIds = _orderRepository.GetValue(
					unitOfWork,
					o => o.DeliveryPoint.Id,
					o => o.ContactPhone != null
						&& o.DeliveryDate != null
						&& o.DeliveryDate.Value.Date == DateTime.Today
						&& o.ContactPhone.DigitsNumber == counterpartyPhone.Substring(1));

				return Ok(
					new GetContactPhoneHasOrdersForDeliveryTodayResponse
					{
						Status = todayOrdersDeliveryPointsIds.Any(),
						DeliveryPointIds = todayOrdersDeliveryPointsIds
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка {ExceptionMessage}", ex.Message);
				return Problem("Ошибка выполнения запроса");
			}
		}

		/// <summary>
		/// Получение информации о точке доставки
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="deliveryPointId">Идентификатор точки доставки</param>
		/// <returns></returns>
		[HttpGet(nameof(GetDeliveryPointInfo))]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeliveryPointInfoResponse))]
		public IActionResult GetDeliveryPointInfo([FromServices] IUnitOfWork unitOfWork, int deliveryPointId)
		{
			var deliveryPoint = _deliveryPointRepository
				.Get(unitOfWork, dp => dp.Id == deliveryPointId)
				.FirstOrDefault();

			if(deliveryPoint is null)
			{
				return Problem($"Точка доставки {deliveryPointId} не найдена");
			}

			var buildingNumber = _roboatsRepository.GetDeliveryPointBuilding(deliveryPointId, deliveryPoint.Counterparty.Id);

			var result = new DeliveryPointInfoResponse
			{
				StreetId = _roboatsRepository.GetRoboAtsStreetId(deliveryPoint.Counterparty.Id, deliveryPointId),
				HouseNumber = GetHouseNumber(buildingNumber),
				BuildingNumber = GetCorpusNumber(buildingNumber),
				AppartmentNumber = GetApartmentNumber(_roboatsRepository.GetDeliveryPointApartment(deliveryPointId, deliveryPoint.Counterparty.Id))
			};

			return Ok(result);
		}

		/// <summary>
		/// Получение телефонов для уточнения времени доставки по контактному телефону сегодняшних заказов
		/// </summary>
		/// <param name="counterpartyPhone">Номер входящего звонка</param>
		/// <returns></returns>
		[HttpGet(nameof(GetCourierPhonesByTodayOrderContactPhone))]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetCourierPhonesResponse))]
		public IActionResult GetCourierPhonesByTodayOrderContactPhone(string counterpartyPhone)
		{
			try
			{
				var phone = string.Empty;

				_phoneService
					.GetCourierPhonesByTodayOrderContactPhone(counterpartyPhone.Substring(1).ToString())
					.Match(
						phoneNumber => phone = "7" + phoneNumber,
						errors => _logger.LogWarning("Телефон курьера не найден: {@Errors}", errors.Select(e => e.Message)));

				var dispatcherPhone = _phoneService
					.GetCourierDispatcherPhone();

				return Ok(new GetCourierPhonesResponse
				{
					CourierPhone = phone,
					CourierDispatcher = dispatcherPhone,
					CallTimeout = (int)_roboAtsOptions.Value.CallToCourierTimeOut.TotalSeconds,
				});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка {ExceptionMessage}", ex.Message);
				return Problem("Ошибка выполнения запроса");
			}
		}

		private string GetHouseNumber(string fullBuildingNumber)
		{
			var regex = new Regex(@"\d{1,}");
			var match = regex.Match(fullBuildingNumber);
			return match.Value;
		}

		private string GetCorpusNumber(string fullBuildingNumber)
		{
			var regex = new Regex(@"((к[ ]*[.]?[ ]*\d+)|(кор[ ]*[.]?[ ]*\d+)|(корп[ ]*[.]?[ ]*\d+)|(корпус[ ]*[.]?[ ]*\d+)){1,}"); //на всякий решили добавить поиск по корп
			var match = regex.Match(fullBuildingNumber);

			var regexDigits = new Regex(@"\d{1,}");
			var corpusNumber = regexDigits.Match(match.Value);
			return corpusNumber.Value;
		}

		private string GetApartmentNumber(string fullApartmentNumber)
		{
			var regex = new Regex(@"\d{1,}");
			var match = regex.Match(fullApartmentNumber);
			return match.Value;
		}
	}
}
