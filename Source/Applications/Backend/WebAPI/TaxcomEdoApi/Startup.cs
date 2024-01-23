using EdoService.Converters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Repositories;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Taxcom.Client.Api;
using TaxcomEdoApi.Converters;
using TaxcomEdoApi.Factories;
using TaxcomEdoApi.Services;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Parameters;
using Vodovoz.Tools.Orders;
using QS.Services;
using QS.Project.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using VodovozHealthCheck;
using TaxcomEdoApi.HealthChecks;
using Vodovoz.EntityRepositories;
using Vodovoz.Services;
using QS.Project.Core;

namespace TaxcomEdoApi
{
	public class Startup
	{
		private const string _nLogSectionName = nameof(NLog);
		private Logger<Startup> _logger;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				});

			_logger = new Logger<Startup>(LoggerFactory.Create(logging =>
				logging.AddConfiguration(Configuration.GetSection(_nLogSectionName))));

			_logger.LogInformation("Логирование Startup начато");

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			services.AddControllers()
				.AddXmlSerializerFormatters();

			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaxcomEdoApi", Version = "v1" }); });
			
			var apiSection = Configuration.GetSection("Api");
			var certificateThumbprint = apiSection.GetValue<string>("CertificateThumbprint").ToUpper();
			var certificate = CertificateLogic.GetAvailableCertificates().SingleOrDefault(x => x.Thumbprint == certificateThumbprint);

			if(certificate is null)
			{
				_logger.LogCritical("Не найден сертификат в личном хранилище пользователя");
				throw new InvalidOperationException("Не найден сертификат в личном хранилище пользователя");
			}

			services.AddCore();
			services.AddTrackedUoW();
			services.AddHostedService<AutoSendReceiveService>();
			services.AddHostedService<ContactsUpdaterService>();
			services.AddHostedService<DocumentFlowService>();
			services.AddSingleton(_ => new Factory().CreateApi(
				apiSection.GetValue<string>("BaseUrl"),
				true,
				apiSection.GetValue<string>("IntegratorId"),
				certificate.RawData,
				apiSection.GetValue<string>("EdxClientId")));

			services.AddSingleton<IOrderRepository, OrderRepository>();
			services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
			services.AddSingleton<ICounterpartyRepository, CounterpartyRepository>();

			services.AddSingleton(_ => certificate);
			services.AddSingleton<EdoUpdFactory>();
			services.AddSingleton<EdoBillFactory>();
			services.AddSingleton<PrintableDocumentSaver>();
			services.AddSingleton<ParticipantDocFlowConverter>();
			services.AddSingleton<EdoContainerMainDocumentIdParser>();
			services.AddSingleton<UpdProductConverter>();
			services.AddSingleton<IParametersProvider, ParametersProvider>();
			services.AddSingleton<IOrganizationParametersProvider, OrganizationParametersProvider>();
			services.AddSingleton<IContactStateConverter, ContactStateConverter>();

			services.AddSingleton(typeof(IGenericRepository<>), typeof(GenericRepository<>));

			services.ConfigureHealthCheckService<TaxcomEdoApiHealthCheck>(true);

			CreateBaseConfig(services);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaxcomEdoApi v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

			app.ConfigureHealthCheckApplicationBuilder();
		}

		private void CreateBaseConfig(IServiceCollection services)
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
					.Driver<LoggedMySqlClientDriver>();

			var provider = services.BuildServiceProvider();
			var ormConfig = provider.GetRequiredService<IOrmConfig>();
			ormConfig.ConfigureOrm(
				dbConfig,
				new[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(AssemblyFinder))
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

			QS.Project.Repositories.UserRepository.GetCurrentUserId = () => serviceUserId;
			HistoryMain.Enable(conStrBuilder);
		}
	}
}
