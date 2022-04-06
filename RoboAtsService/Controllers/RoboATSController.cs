using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

		public RoboATSController(ILogger<RoboATSController> logger, RoboatsRepository roboatsRepository, RoboatsOrderModel roboatsOrderModel)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_roboatsRepository = roboatsRepository ?? throw new System.ArgumentNullException(nameof(roboatsRepository));
			_roboatsOrderModel = roboatsOrderModel ?? throw new System.ArgumentNullException(nameof(roboatsOrderModel));
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

			RequestHandlerFactory adapter = new RequestHandlerFactory(_roboatsRepository, _roboatsOrderModel);
			var roboatsRequest = adapter.GetRequest(request);
			if(roboatsRequest == null)
			{
				return "null request";
			}
			var result = roboatsRequest.Execute();
			sw.Stop();
			var query = HttpContext.Request.GetEncodedPathAndQuery();
			_logger.LogInformation($"Request: {query} | Response: {result} | Request time: {sw.Elapsed.Seconds}.{sw.Elapsed.Milliseconds} sec.");
			return result;
		}
	}
}
