using System.Net.Http.Headers;

namespace TrueMark.ProductInstanceInfoCheck.Worker;
public static class DependencyInjection
{
	public static IServiceCollection AddProductInstanceInfoCheckWorker(this IServiceCollection services)
	{
		services.AddHttpClient<ProductInstanceInfoRequestConsumer>((serviceProvider, client) =>
		{
			var configuration = serviceProvider.GetRequiredService<IConfiguration>();
			var baseUri = configuration.GetValue<string>("ExternalTrueMarkBaseUrl")
				?? throw new InvalidOperationException("Не найдена настройка \"ExternalTrueMarkBaseUrl\" в конфигурации");

			client.BaseAddress = new Uri(baseUri);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		});

		return services;
	}
}
