using Autofac.Extensions.DependencyInjection;

namespace TaxcomEdo.Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services
				.AddControllers();

			builder.Host.UseServiceProviderFactory(
				new AutofacServiceProviderFactory())
					.ConfigureServices((context, services) => services
						.AddHttpClient("test", (serviceProvider, client) =>
							{
								;
							})
						);

			var app = builder.Build();

			// Configure the HTTP request pipeline.

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}
