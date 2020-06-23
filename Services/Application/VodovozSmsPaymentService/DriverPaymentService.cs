using System;
using System.ServiceModel;
using Android;
using SmsPaymentService;

namespace VodovozSmsPaymentService
{
	public class DriverPaymentService : IDriverPaymentService
	{
		private readonly ChannelFactory<IAndroidDriverService> androidDriverServiceChannelFactory;

		public DriverPaymentService(ChannelFactory<IAndroidDriverService> androidDriverServiceChannelFactory)
		{
			this.androidDriverServiceChannelFactory = androidDriverServiceChannelFactory ?? throw new ArgumentNullException(nameof(androidDriverServiceChannelFactory));
		}

		public void RefreshPaymentStatus(int orderId)
		{
			IAndroidDriverService androidDriverService = androidDriverServiceChannelFactory.CreateChannel();
			androidDriverService.RefreshPaymentStatus(orderId);
		}
	}
}
