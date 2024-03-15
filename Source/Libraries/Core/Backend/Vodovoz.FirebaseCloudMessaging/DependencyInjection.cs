using FirebaseCloudMessaging.Client.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vodovoz.Application.FirebaseCloudMessaging;
using GoogleClientInitializer = Google.Apis.Services.BaseClientService.Initializer;
using GoogleFirebaseCloudMessagingService = Google.Apis.FirebaseCloudMessaging.v1.FirebaseCloudMessagingService;

namespace Vodovoz.FirebaseCloudMessaging
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddFirebaseCloudMessaging(this IServiceCollection services, IConfiguration configuration)
			=> services.AddScoped<IFirebaseCloudMessagingService, FirebaseCloudMessagingService>()
				.AddScoped<GoogleClientInitializer>(serviceProvider =>
				{
					var options = serviceProvider.GetRequiredService<IOptions<FirebaseCloudMessagingSettings>>();
					return new GoogleClientInitializer
					{
						ApplicationName = "Vodovoz Firebase Cloud Messaging Service Client",
						ApiKey = options.Value.ApiKey,
					};
				})
				.AddScoped<GoogleFirebaseCloudMessagingService>(serviceProvider =>
					new GoogleFirebaseCloudMessagingService(serviceProvider.GetRequiredService<GoogleClientInitializer>()))
				.Configure<FirebaseCloudMessagingSettings>(firebaseSettings =>
					configuration.GetSection(nameof(FirebaseCloudMessagingSettings)).Bind(firebaseSettings));
	}
}
