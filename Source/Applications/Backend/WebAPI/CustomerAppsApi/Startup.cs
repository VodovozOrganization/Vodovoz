using CustomerAppsApi.HealthChecks;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Validators;
using CustomerAppsApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.DB;
using QS.Services;
using QS.Utilities.Numeric;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Factories;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Roboats;
using Vodovoz.Settings.Roboats;
using VodovozHealthCheck;

namespace CustomerAppsApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddSwaggerGen(c => 
				{ 
					c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerAppsApi", Version = "v1" }); 
				})

				.AddLogging(logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				})

				.AddStackExchangeRedisCache(redisOptions =>
				{
					var connection = Configuration.GetConnectionString("Redis");
					redisOptions.Configuration = connection;
				})

				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(EmployeeWithLoginMap).Assembly,
					typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()

				.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot())
				.AddSingleton<IPhoneRepository, PhoneRepository>()
				.AddSingleton<IEmailRepository, EmailRepository>()
				.AddSingleton<ISettingsController, SettingsController>()
				.AddSingleton<IRoboatsSettings, RoboatsSettings>()
				.AddSingleton<IRoboatsRepository, RoboatsRepository>()
				.AddSingleton<IBottlesRepository, BottlesRepository>()
				.AddSingleton<ICachedBottlesDebtRepository, CachedBottlesDebtRepository>()
				.AddSingleton<INomenclatureRepository, NomenclatureRepository>()
				.AddSingleton<IStockRepository, StockRepository>()
				.AddSingleton<IExternalCounterpartyRepository, ExternalCounterpartyRepository>()
				.AddSingleton<IExternalCounterpartyMatchingRepository, ExternalCounterpartyMatchingRepository>()
				.AddSingleton<IRegisteredNaturalCounterpartyDtoFactory, RegisteredNaturalCounterpartyDtoFactory>()
				.AddSingleton<IExternalCounterpartyMatchingFactory, ExternalCounterpartyMatchingFactory>()
				.AddSingleton<IExternalCounterpartyFactory, ExternalCounterpartyFactory>()
				.AddSingleton<CounterpartyModelFactory>()
				.AddSingleton<ICounterpartyFactory, CounterpartyFactory>()
				.AddSingleton<INomenclatureFactory, NomenclatureFactory>()
				.AddSingleton<PhoneFormatter>(_ => new PhoneFormatter(PhoneFormat.DigitsTen))
				.AddSingleton<ICameFromConverter, CameFromConverter>()
				.AddSingleton<ISourceConverter, SourceConverter>()
				.AddSingleton<ContactFinderForExternalCounterpartyFromOne>()
				.AddSingleton<ContactFinderForExternalCounterpartyFromTwo>()
				.AddSingleton<ContactFinderForExternalCounterpartyFromMany>()
				.AddSingleton<IContactManagerForExternalCounterparty, ContactManagerForExternalCounterparty>()
				.AddSingleton<IGoodsOnlineParametersController, GoodsOnlineParametersController>()
				.AddScoped<ICounterpartyModel, CounterpartyModel>()
				.AddScoped<INomenclatureModel, NomenclatureModel>()
				.AddScoped<CounterpartyModelValidator>()

				.ConfigureHealthCheckService<CustomerAppsApiHealthCheck>()

				.AddHttpClient()
				.AddControllers()
				;

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			Library.DependencyInjection.AddCustomerApiLibrary(services);

			services.AddStaticHistoryTracker();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerAppsApi v1"));
			}

			app.UseMiddleware<ResponseLoggingMiddleware>();
			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

			app.ConfigureHealthCheckApplicationBuilder();
		}
	}
}
