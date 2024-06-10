using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PushNotificationsWorker.Options;
using Vodovoz.FirebaseCloudMessaging;

namespace PushNotificationsWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPushNotificationsWorker(this IServiceCollection services, HostBuilderContext hostContext) =>
			services
				.AddFirebaseCloudMessaging(hostContext.Configuration)
				.Configure<TransferedFastDeliveryNotificationWorkerSettings>((options) =>
				{
					hostContext.Configuration.GetSection(nameof(TransferedFastDeliveryNotificationWorkerSettings)).Bind(options);
				})
				.Configure<CanceledFastDeliveryNotificationWorkerSettings>((options) =>
				{
					hostContext.Configuration.GetSection(nameof(CanceledFastDeliveryNotificationWorkerSettings)).Bind(options);
				})
				.AddHostedService<TransferedFastDeliveryNotificationWorker>()
				.AddHostedService<CanceledFastDeliveryNotificationWorker>();
	}
}
