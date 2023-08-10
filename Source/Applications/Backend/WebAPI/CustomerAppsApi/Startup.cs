using CustomerAppsApi.Converters;
using CustomerAppsApi.Factories;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Models;
using CustomerAppsApi.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Numeric;
using System.Linq;
using System.Reflection;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Factories;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Parameters;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;
using UserRepository = QS.Project.Repositories.UserRepository;

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
			services.AddControllers();
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerAppsApi", Version = "v1" }); });
			
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
				});

			RegisterDependencies(services);
			CreateBaseConfig();
		}

		private void RegisterDependencies(IServiceCollection services)
		{
			services.AddSingleton<IPhoneRepository, PhoneRepository>();
			services.AddSingleton<IEmailRepository, EmailRepository>();
			services.AddSingleton<ISettingsController, SettingsController>();
			services.AddSingleton<ISessionProvider, DefaultSessionProvider>();
			services.AddSingleton<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>();
			services.AddSingleton<IRoboatsSettings, RoboatsSettings>();
			services.AddSingleton<IRoboatsRepository, RoboatsRepository>();
			services.AddSingleton<IExternalCounterpartyRepository, ExternalCounterpartyRepository>();
			services.AddSingleton<IExternalCounterpartyMatchingRepository, ExternalCounterpartyMatchingRepository>();
			services.AddSingleton<IRegisteredNaturalCounterpartyDtoFactory, RegisteredNaturalCounterpartyDtoFactory>();
			services.AddSingleton<IExternalCounterpartyMatchingFactory, ExternalCounterpartyMatchingFactory>();
			services.AddSingleton<IExternalCounterpartyFactory, ExternalCounterpartyFactory>();
			services.AddSingleton<CounterpartyModelFactory>();
			services.AddSingleton<ICounterpartyFactory, CounterpartyFactory>();
			services.AddSingleton<PhoneFormatter>(_ => new PhoneFormatter(PhoneFormat.DigitsTen));
			services.AddSingleton<ICounterpartySettings, CounterpartySettings>();
			services.AddSingleton<ICameFromConverter, CameFromConverter>();
			services.AddSingleton<ContactFinderForExternalCounterpartyFromOne>();
			services.AddSingleton<ContactFinderForExternalCounterpartyFromTwo>();
			services.AddSingleton<ContactFinderForExternalCounterpartyFromMany>();
			services.AddSingleton<IContactManagerForExternalCounterparty, ContactManagerForExternalCounterparty>();

			services.AddScoped<IUnitOfWork>(_ => UnitOfWorkFactory.CreateWithoutRoot("Сервис интеграции"));
			services.AddScoped<ICounterpartyModel, CounterpartyModel>();
			services.AddScoped<CounterpartyModelValidator>();
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

			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
		
		private void CreateBaseConfig()
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDbConfig = Configuration.GetSection("DomainDB");

			conStrBuilder.Server = domainDbConfig.GetValue<string>("Server");
			conStrBuilder.Port = domainDbConfig.GetValue<uint>("Port");
			conStrBuilder.Database = domainDbConfig.GetValue<string>("Database");
			conStrBuilder.UserID = domainDbConfig.GetValue<string>("UserID");
			conStrBuilder.Password = domainDbConfig.GetValue<string>("Password");
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString)
				.Driver<LoggedMySqlClientDriver>()
				.AdoNetBatchSize(100);

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				dbConfig,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);

			string userLogin = domainDbConfig.GetValue<string>("UserID");
			int serviceUserId = 0;

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение пользователя"))
			{
				var serviceUser = unitOfWork.Session.Query<Vodovoz.Domain.Employees.User>()
					.Where(u => u.Login == userLogin)
					.FirstOrDefault();

				serviceUserId = serviceUser.Id;

				ServicesConfig.UserService = new UserService(serviceUser);
			}

			UserRepository.GetCurrentUserId = () => serviceUserId;
			HistoryMain.Enable(conStrBuilder);
		}
	}
}
