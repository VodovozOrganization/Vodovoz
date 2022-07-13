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
				/*case RoboatsRequestType.LastOrderDataBottles:
					return new LastOrderBottleCountHandler(request);
				case RoboatsRequestType.LastOrderDataReturn:
					return new LastOrderDataReturnHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrderDataAddressHouse:
					return new LastOrderDataAddressHouseHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrderDataAddressApartment:
					return new LastOrderDataAddressApartmentHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrderDataAddressCorp:
					return new LastOrderDataAddressCorpusHandler(_roboatsRepository, request);
				case RoboatsRequestType.QuantityAddress:
					return new QuantityAddressHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrderDataFirstNameCode:
					return new LastOrderDataFirstNameCodeHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrderDataPatronymicCode:
					return new LastOrderDataPatronymicCodeHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrderDataStreetCode:
					return new LastOrderDataStreetCodeHandler(_roboatsRepository, request);
				case RoboatsRequestType.Check:
					return new CheckHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrderDataFirstName:
					return new LastOrderDataClientNameHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrderDataLastName:
					return new LastOrderDataClientNameHandler(_roboatsRepository, request);
				case RoboatsRequestType.LastOrderDataPatronymic:
					return new LastOrderDataClientNameHandler(_roboatsRepository, request);*/
				default:
					return null;
			}
		}
	}

}
