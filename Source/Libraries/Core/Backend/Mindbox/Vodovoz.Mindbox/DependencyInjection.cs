using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Mindbox.Export;
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

			services.AddSingleton<IMindboxCounterpartyRequestFactory, MindboxCounterpartyRequestFactory>();
			services.AddSingleton<IMindboxCustomerCsvMapper, MindboxCustomerCsvMapper>();

			services.AddScoped<IMindboxClient, MindboxClient>();
			services.AddScoped<IMindboxCounterpartyOutboxProcessor, MindboxCounterpartyOutboxProcessor>();
			services.AddScoped<IMindboxCounterpartiesExporter, MindboxCounterpartiesExporter>();

			return services;
		}
	}
}
