using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Mindbox.Mapping;
using Vodovoz.Mindbox.Services;

namespace Vodovoz.Mindbox
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Регистрирует сервисы интеграции с Mindbox.
		/// Требует зарегистрированные IMindboxSettings, IUnitOfWorkFactory и IHttpClientFactory.
		/// </summary>
		public static IServiceCollection AddMindbox(this IServiceCollection services)
		{
			services.AddHttpClient(nameof(MindboxClient));

			services.AddSingleton<IMindboxCustomerRequestFactory, MindboxCustomerRequestFactory>();
			services.AddSingleton<IMindboxRetryPolicy, MindboxRetryPolicy>();
			services.AddSingleton<IMindboxSendingCircuitBreaker, MindboxSendingCircuitBreaker>();

			services.AddScoped<IMindboxClient, MindboxClient>();
			services.AddScoped<IMindboxAlertSender, LoggingMindboxAlertSender>();
			services.AddScoped<IMindboxCounterpartyEnqueuer, MindboxCounterpartyEnqueuer>();
			services.AddScoped<IMindboxCounterpartyOutboxProcessor, MindboxCounterpartyOutboxProcessor>();

			return services;
		}
	}
}
