using Microsoft.Extensions.DependencyInjection;

namespace CashReceiptApi.Client.Framework
{
	public static class DependencyInjection
	{
		public static void AddCashReceiptClientChannel(this IServiceCollection services)
		{
			services.AddScoped<CashReceiptClientChannelFactory>();
		}
	}
}
