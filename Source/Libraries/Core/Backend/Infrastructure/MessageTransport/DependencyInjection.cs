using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Settings.Pacs;

namespace MessageTransport
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMessageTransportSettings(this IServiceCollection services)
		{
			services.AddSingleton<IMessageTransportSettings>((provider) =>
			{
				var configuration = provider.GetRequiredService<IConfiguration>();
				var transportSettings = new ConfigTransportSettings();
				configuration.Bind("MessageTransport", transportSettings);
				return transportSettings;
			});
			return services;
		}
	}
}
