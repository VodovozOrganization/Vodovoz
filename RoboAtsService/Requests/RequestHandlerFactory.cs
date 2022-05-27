using Autofac;
using System;

namespace RoboAtsService.Requests
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
			switch(request.RequestType)
			{
				case RoboatsRequestType.Address:
					return _scope.Resolve<AddressHandler>(new TypedParameter(request.GetType(), request));
				case RoboatsRequestType.LastOrder:
					return _scope.Resolve<LastOrderHandler>(new TypedParameter(request.GetType(), request));
				case RoboatsRequestType.Order:
					return _scope.Resolve<OrderHandler>(new TypedParameter(request.GetType(), request));
				case RoboatsRequestType.ClientCheck:
					return _scope.Resolve<ClientCheckHandler>(new TypedParameter(request.GetType(), request));
				case RoboatsRequestType.DateTime:
					return _scope.Resolve<DeliveryIntervalsHandler>(new TypedParameter(request.GetType(), request));
				case RoboatsRequestType.WaterType:
					return _scope.Resolve<WaterTypeHandler>(new TypedParameter(request.GetType(), request));
				default:
					return null;
			}
		}
	}

}
