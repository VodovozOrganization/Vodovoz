using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vodovoz.Data.NHibernate;
using Vodovoz.SmsInformerWorker.Options;
using Vodovoz.SmsInformerWorker.Services;

namespace Vodovoz.SmsInformerWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddSmsInformerWorker(this IServiceCollection services, HostBuilderContext context) => services
			.ConfigureSmsInformerWorker(context)
			.AddScoped<ILowBalanceNotificationService, LowBalanceNotificationService>()
			.AddDatabase(context);

		public static IServiceCollection ConfigureSmsInformerWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<SmsInformerOptions>(context.Configuration.GetSection(nameof(SmsInformerOptions)));
	}
}
