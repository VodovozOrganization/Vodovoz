using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sms.External.Interface;
using Sms.External.SmsRu;
using Vodovoz.Core.DataService;
using Vodovoz.Data.NHibernate;
using Vodovoz.EntityRepositories.SmsNotifications;
using Vodovoz.Parameters;
using Vodovoz.Services;
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
			.AddSingleton<ISmsNotificationRepository, SmsNotificationRepository>()
			.AddSingleton<ISmsNotifierParametersProvider, BaseParametersProvider>()
			.AddSingleton<IParametersProvider, ParametersProvider>()
			.AddDatabase(context);

		public static IServiceCollection ConfigureSmsInformerWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<SmsInformerOptions>(context.Configuration.GetSection(nameof(SmsInformerOptions)))
			.Configure<SmsRuConfiguration>(context.Configuration.GetSection(nameof(SmsRuConfiguration)));
	}
}
