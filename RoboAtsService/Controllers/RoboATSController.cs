using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoboAtsService.Monitoring;
using RoboAtsService.Requests;
using System.Diagnostics;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Models.Orders;

namespace RoboAtsService.Controllers
{
	[ApiController]
	[Route("api")]
	public class RoboATSController : ControllerBase
	{
		private readonly ILogger<RoboATSController> _logger;

		private readonly RoboatsRepository _roboatsRepository;
		private readonly RoboatsOrderModel _roboatsOrderModel;
		private readonly RequestHandlerFactory _handlerFactory;
		private readonly RoboatsCallRegistrator _callRegistrator;

		public RoboATSController(ILogger<RoboATSController> logger, RoboatsRepository repository, RoboatsOrderModel orderModel, RequestHandlerFactory handlerFactory, RoboatsCallRegistrator callRegistrator)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_roboatsRepository = repository ?? throw new System.ArgumentNullException(nameof(repository));
			_roboatsOrderModel = orderModel ?? throw new System.ArgumentNullException(nameof(orderModel));
			_handlerFactory = handlerFactory ?? throw new System.ArgumentNullException(nameof(handlerFactory));
			_callRegistrator = callRegistrator ?? throw new System.ArgumentNullException(nameof(callRegistrator));
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

			var handler = _handlerFactory.GetHandler(request);
			if(handler == null)
			{
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
