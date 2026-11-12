using Microsoft.Extensions.DependencyInjection;
using Mindbox.Library.Mapping;
using Mindbox.Library.Services;

namespace Mindbox.Library
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Регистрирует сервисы интеграции с Mindbox.
		/// Требует зарегистрированные IMindboxSettings, IUnitOfWorkFactory и IHttpClientFactory.
		/// </summary>
		public static IServiceCollection AddMindbox(this IServiceCollection services)
		{
			services.AddMemoryCache();
			services.AddHttpClient(nameof(MindboxClient));

			services.AddSingleton<IMindboxCustomerRequestFactory, MindboxCustomerRequestFactory>();
			services.AddSingleton<IMindboxRetryPolicy, MindboxRetryPolicy>();
			services.AddSingleton<IMindboxSendingCircuitBreaker, MindboxSendingCircuitBreaker>();
			services.AddSingleton<IMindboxBonusCircuitBreaker, MindboxBonusCircuitBreaker>();

			services.AddScoped<IMindboxClient, MindboxClient>();
			services.AddScoped<IMindboxAlertSender, LoggingMindboxAlertSender>();
			services.AddScoped<IMindboxCounterpartyEnqueuer, MindboxCounterpartyEnqueuer>();
			services.AddScoped<IMindboxCounterpartySendService, MindboxCounterpartySendService>();
			services.AddScoped<IMindboxBonusService, MindboxBonusService>();
			services.AddScoped<IMindboxAuthorizedCartService, MindboxAuthorizedCartService>();
			services.AddScoped<IMindboxAuthorizedLoyaltyOrderService, MindboxAuthorizedLoyaltyOrderService>();
			services.AddScoped<IMindboxOnlineOrderStateBuilder, MindboxOnlineOrderStateBuilder>();
			services.AddScoped<IMindboxOnlineOrderSyncProcessor, MindboxOnlineOrderSyncProcessor>();
			services.AddScoped<IMindboxOnlineOrderEnqueuer, MindboxOnlineOrderEnqueuer>();
			services.AddScoped<IMindboxOrderBonusesService, MindboxOrderBonusesService>();

			return services;
		}
	}
}
