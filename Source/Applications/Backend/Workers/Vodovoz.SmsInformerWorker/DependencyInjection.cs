using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sms.External.Interface;
using Sms.External.SmsRu;
using Vodovoz.SmsInformerWorker.Options;
using Vodovoz.SmsInformerWorker.Services;

namespace Vodovoz.SmsInformerWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddSmsInformerWorker(this IServiceCollection services, HostBuilderContext context) => services
			.ConfigureSmsInformerWorker(context)
			.AddSingleton<ILowBalanceNotificationService, LowBalanceNotificationService>()
			.AddSingleton<ISmsSender, SmsRuSendController>()
			.AddSingleton<ISmsBalanceNotifier, SmsRuSendController>()
			;

		public static IServiceCollection ConfigureSmsInformerWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<SmsInformerOptions>(context.Configuration.GetSection(nameof(SmsInformerOptions)))
			.Configure<SmsRuConfiguration>(context.Configuration.GetSection(nameof(SmsRuConfiguration)));
	}
}
