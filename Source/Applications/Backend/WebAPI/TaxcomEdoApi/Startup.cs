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

namespace TaxcomEdoApi
{
	public class Startup
	{
		private const string _nLogSectionName = nameof(NLog);

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			
			CreateBaseConfig();

			services.AddControllers()
				.AddXmlSerializerFormatters();

			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				});


			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaxcomEdoApi", Version = "v1" }); });
			
			var apiSection = Configuration.GetSection("Api");
			var certificateThumbprint = apiSection.GetValue<string>("CertificateThumbprint").ToUpper();
			var certificate = CertificateLogic.GetAvailableCertificates().SingleOrDefault(x => x.Thumbprint == certificateThumbprint);

			if(certificate is null)
			{
				throw new InvalidOperationException("Не найден сертификат в личном хранилище пользователя");
			}
			
			services.AddHostedService<AutoSendReceiveService>();
			services.AddHostedService<ContactsUpdaterService>();
			services.AddHostedService<DocumentFlowService>();
			services.AddSingleton(_ => new Factory().CreateApi(
				apiSection.GetValue<string>("BaseUrl"),
				true,
				apiSection.GetValue<string>("IntegratorId"),
				certificate.RawData,
				apiSection.GetValue<string>("EdxClientId")));

			services.AddSingleton<ISessionProvider, DefaultSessionProvider>();
			services.AddSingleton<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>();
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
			services.AddSingleton<IContactStateConverter, ContactStateConverter>();

			services.ConfigureHealthCheckService<TaxcomEdoApiHealthCheck>(true);
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
					.Driver<LoggedMySqlClientDriver>();

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				dbConfig,
				new[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
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
