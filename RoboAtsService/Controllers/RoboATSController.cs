using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoboAtsService.Monitoring;
using RoboAtsService.Requests;
using System.Diagnostics;
using Vodovoz.Domain.Roboats;
using Vodovoz.Parameters;

namespace RoboAtsService.Controllers
{
	[ApiController]
	[Route("api")]
	public class RoboATSController : ControllerBase
	{
		private readonly ILogger<RoboATSController> _logger;

		private readonly RequestHandlerFactory _handlerFactory;
		private readonly RoboatsCallRegistrator _roboatsCallRegistrator;

		public RoboATSController(ILogger<RoboATSController> logger, RequestHandlerFactory handlerFactory, RoboatsCallRegistrator roboatsCallRegistrator)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_handlerFactory = handlerFactory ?? throw new System.ArgumentNullException(nameof(handlerFactory));
			_roboatsCallRegistrator = roboatsCallRegistrator ?? throw new System.ArgumentNullException(nameof(roboatsCallRegistrator));
		}

		[HttpGet]
		public string Get(
			[FromQuery(Name = "CID")] string clientPhone,
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
			[FromQuery(Name = "terminal")] string isTerminal
			)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			var request = new RequestDto
			{
				ClientPhone = clientPhone,
				RequestType = requestType,
				AddressId = addressId,
				Date = date,
				Time = time,
				IsAddOrder = isAddOrder,
				ReturnBottlesCount = returnBottlesCount,
				WaterQuantity = waterQuantity,
				BanknoteForReturn = banknoteForReturn,
				IsTerminal = isTerminal,
				IsFullOrder = fullOrder,
				RequestSubType = checkType,
				OrderId = orderId
			};

			_roboatsCallRegistrator.RegisterCall(request.ClientPhone);

			var handler = _handlerFactory.GetHandler(request);
			if(handler == null)
			{
				_roboatsCallRegistrator.RegisterTerminatingFail(request.ClientPhone, RoboatsCallFailType.UnknownRequest, RoboatsCallOperation.OnCreateHandler,
					"Неизвестный тип запроса. Обратитесь в отдел разработки.");
				return "null request";
			}

			var result = handler.Execute();
			sw.Stop();
			var query = HttpContext.Request.GetEncodedPathAndQuery();
			_logger.LogInformation($"Request: {query} | Response: {result} | Request time: {sw.Elapsed.Seconds}.{sw.Elapsed.Milliseconds} sec.");
			return result;
		}
	}
}
