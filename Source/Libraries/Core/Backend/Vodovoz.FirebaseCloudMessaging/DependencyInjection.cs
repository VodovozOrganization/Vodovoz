using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using FirebaseCloudMessaging.Client.Options;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Vodovoz.Application.FirebaseCloudMessaging;

namespace Vodovoz.FirebaseCloudMessaging
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddFirebaseCloudMessaging(this IServiceCollection services, IConfiguration configuration)
			=> services.AddScoped<FirebaseApp>(serviceProvider =>
				{
					var options = serviceProvider.GetRequiredService<IOptions<FirebaseCloudMessagingKeySettings>>();

					var t = JsonSerializer.Serialize(options.Value);

					return FirebaseApp.Create(new AppOptions
					{
						ProjectId = options.Value.project_id,
						Credential = GoogleCredential.FromJson(JsonSerializer.Serialize(options.Value))
					});
				})
				.AddScoped<IFirebaseCloudMessagingService, FirebaseCloudMessagingService>()
				.AddScoped<FirebaseMessaging>(sp => FirebaseMessaging.DefaultInstance)
				.Configure<FirebaseCloudMessagingKeySettings>(firebaseSettings =>
					configuration.GetSection(nameof(FirebaseCloudMessagingKeySettings)).Bind(firebaseSettings));
	}
}
