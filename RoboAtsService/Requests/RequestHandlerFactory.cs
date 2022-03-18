using System;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Models.Orders;

namespace RoboAtsService.Requests
{
	public class RequestHandlerFactory
	{
		private readonly RoboatsRepository _roboatsRepository;
		private readonly RoboatsOrderModel _roboatsOrderModel;

		public RequestHandlerFactory(RoboatsRepository roboatsRepository, RoboatsOrderModel roboatsOrderModel)
		{
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_roboatsOrderModel = roboatsOrderModel ?? throw new ArgumentNullException(nameof(roboatsOrderModel));
		}

		public GetRequestHandlerBase GetRequest(RequestDto request)
		{
			switch(request.RequestType)
			{
				case RoboatsRequestType.Address:
					return new AddressHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrder:
					return new LastOrderHandler(_roboatsRepository, request);
				case RoboatsRequestType.Order:
					return new OrderHandler(_roboatsRepository, _roboatsOrderModel, request);
				case RoboatsRequestType.ClientCheck:
					return new ClientCheckHandler(_roboatsRepository, request);
				case RoboatsRequestType.DateTime:
					return new DeliveryIntervalsHandler(_roboatsRepository, request);
				case RoboatsRequestType.WaterType:
					return new WaterTypeHandler(_roboatsRepository, request);
				default:
					return null;
			}
		}
	}

}
