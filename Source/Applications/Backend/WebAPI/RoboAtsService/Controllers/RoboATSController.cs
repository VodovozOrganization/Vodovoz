using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoboatsService.Monitoring;
using RoboatsService.Requests;
using System;
using System.Diagnostics;
using Vodovoz.Domain.Roboats;

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

		public RoboatsController(ILogger<RoboatsController> logger, RequestHandlerFactory handlerFactory, RoboatsCallRegistrator roboatsCallRegistrator)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_handlerFactory = handlerFactory ?? throw new System.ArgumentNullException(nameof(handlerFactory));
			_roboatsCallRegistrator = roboatsCallRegistrator ?? throw new System.ArgumentNullException(nameof(roboatsCallRegistrator));
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
	}
}
