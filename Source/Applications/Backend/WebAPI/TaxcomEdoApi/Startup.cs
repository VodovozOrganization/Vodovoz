using EdoService.Converters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using QS.HistoryLog;
using QS.Project.Core;
using System;
using System.Linq;
using System.Text;
using Taxcom.Client.Api;
using TaxcomEdoApi.Converters;
using TaxcomEdoApi.Factories;
using TaxcomEdoApi.HealthChecks;
using TaxcomEdoApi.Services;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Tools.Orders;
using VodovozHealthCheck;

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

			services.AddMappingAssemblies(
				typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
				typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
				typeof(QS.Banks.Domain.Bank).Assembly,
				typeof(QS.HistoryLog.HistoryMain).Assembly,
				typeof(QS.Project.Domain.TypeOfEntity).Assembly,
				typeof(QS.Attachments.Domain.Attachment).Assembly,
				typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
			);
			services.AddDatabaseConnection();
			services.AddCore();
			services.AddTrackedUoW();
			services.AddServiceUser();
			services.AddStaticHistoryTracker();


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
	}
}
