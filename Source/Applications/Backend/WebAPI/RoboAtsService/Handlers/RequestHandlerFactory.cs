using Autofac;
using RoboAtsService.Contracts.Requests;
using System;

namespace RoboatsService.Handlers
{
	public class RequestHandlerFactory
	{
		private readonly ILifetimeScope _scope;

		public RequestHandlerFactory(ILifetimeScope scope)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		public GetRequestHandlerBase GetHandler(RequestDto request)
		{
			var requestParameter = new TypedParameter(request.GetType(), request);

			switch(request.RequestType)
			{
				case RoboatsRequestType.Address:
					return _scope.Resolve<AddressHandler>(requestParameter);
				case RoboatsRequestType.LastOrder:
					return _scope.Resolve<LastOrderHandler>(requestParameter);
				case RoboatsRequestType.Order:
					return _scope.Resolve<OrderHandler>(requestParameter);
				case RoboatsRequestType.ClientCheck:
					return _scope.Resolve<ClientCheckHandler>(requestParameter);
				case RoboatsRequestType.DateTime:
					return _scope.Resolve<DeliveryIntervalsHandler>(requestParameter);
				case RoboatsRequestType.WaterType:
					return _scope.Resolve<WaterTypeHandler>(requestParameter);
				default:
					return null;
			}
		}
	}
}
