using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Android;
using Vodovoz.Services;
using SmsPaymentService;

namespace VodovozAndroidDriverService
{
	public class AndroidDriverServiceInstanceProvider : IInstanceProvider
	{
		private readonly WageCalculationServiceFactory wageCalculationServiceFactory;
		private readonly IDriverServiceParametersProvider parameters;
		private readonly ChannelFactory<ISmsPaymentService> smsPaymentChannelFactory;
		private readonly IDriverNotificator driverNotificator;

		public AndroidDriverServiceInstanceProvider(
			WageCalculationServiceFactory wageCalculationServiceFactory, 
			IDriverServiceParametersProvider parameters,
			ChannelFactory<ISmsPaymentService> smsPaymentChannelFactory,
			IDriverNotificator driverNotificator
			)
		{
			this.wageCalculationServiceFactory = wageCalculationServiceFactory ?? throw new ArgumentNullException(nameof(wageCalculationServiceFactory));
			this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
			this.smsPaymentChannelFactory = smsPaymentChannelFactory ?? throw new ArgumentNullException(nameof(smsPaymentChannelFactory));
			this.driverNotificator = driverNotificator ?? throw new ArgumentNullException(nameof(driverNotificator));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new AndroidDriverService(wageCalculationServiceFactory, parameters, smsPaymentChannelFactory, driverNotificator);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
		}

		#endregion IInstanceProvider implementation
	}
}
