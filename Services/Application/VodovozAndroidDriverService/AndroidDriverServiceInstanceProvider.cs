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
		private readonly WageParameterService wageParameterService;
		private readonly IDriverServiceParametersProvider parameters;
		private readonly ChannelFactory<ISmsPaymentService> smsPaymentChannelFactory;
		private readonly IDriverNotificator driverNotificator;

		public AndroidDriverServiceInstanceProvider(
			WageParameterService wageParameterService, 
			IDriverServiceParametersProvider parameters,
			ChannelFactory<ISmsPaymentService> smsPaymentChannelFactory,
			IDriverNotificator driverNotificator
			)
		{
			this.wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
			this.smsPaymentChannelFactory = smsPaymentChannelFactory ?? throw new ArgumentNullException(nameof(smsPaymentChannelFactory));
			this.driverNotificator = driverNotificator ?? throw new ArgumentNullException(nameof(driverNotificator));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new AndroidDriverService(wageParameterService, parameters, smsPaymentChannelFactory, driverNotificator);
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
