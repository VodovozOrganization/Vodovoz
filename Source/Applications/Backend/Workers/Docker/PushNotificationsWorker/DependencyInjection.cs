using Firebase.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PushNotificationsWorker.Options;
using Vodovoz.Data.NHibernate;

namespace PushNotificationsWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPushNotificationsWorker(this IServiceCollection services, HostBuilderContext hostContext) =>
			services.AddDatabase(hostContext)
					.AddFirebaseClient(hostContext.Configuration)
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
