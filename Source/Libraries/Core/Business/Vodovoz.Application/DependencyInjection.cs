using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sms.Internal.Client.Framework;
using Vodovoz.Application.FastPayment;
using Vodovoz.Application.Pacs;
using Vodovoz.Core.Application;
using Vodovoz.Core.Application.FastPayment;

namespace Vodovoz.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			services
				.AddCoreApplication()
				.AddSingleton<OperatorService>()
				;

			services.Replace(ServiceDescriptor.Scoped<IFastPaymentSender, FastPaymentSenderFramework>());
			services.Replace(ServiceDescriptor.Scoped<SmsClientChannelFactory, SmsClientChannelFactory>());

			return services;
		}
	}
}
