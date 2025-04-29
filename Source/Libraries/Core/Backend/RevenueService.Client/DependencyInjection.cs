using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Settings.Counterparty;

namespace RevenueService.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddRevenueServiceClient(this IServiceCollection services)
			=> services.AddScoped<IRevenueServiceClient, RevenueServiceClient>(sp =>
			{
				var counterpartySettings = sp.GetRequiredService<ICounterpartySettings>();
				return new RevenueServiceClient(counterpartySettings.RevenueServiceClientAccessToken);
			});
	}
}
