using FastPaymentsAPI.HealthChecks;
using FastPaymentsAPI.Library.ApiClients;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Managers;
using FastPaymentsAPI.Library.Models;
using FastPaymentsAPI.Library.Notifications;
using FastPaymentsAPI.Library.Services;
using FastPaymentsAPI.Library.Validators;
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
using System;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Settings.FastPayments;
using VodovozHealthCheck;
using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI
{
	public class Startup
	{
		private const string _nLogSectionName = "NLog";

		private ILogger<Startup> _logger;

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
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				});

			_logger = new Logger<Startup>(LoggerFactory.Create(logging =>
				logging.AddConfiguration(Configuration.GetSection(_nLogSectionName))));

			services.AddHttpClient()
				.AddControllers()
				.AddXmlSerializerFormatters();

			// Подключение к БД
			services.AddScoped((provider) => provider.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("Сервис быстрых платежей"));
			
			// Конфигурация Nhibernate
			services
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
				;

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddStaticHistoryTracker();

			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "FastPaymentsAPI", Version = "v1" }); });

			services.AddHttpClient<IOrderService, OrderService>(c =>
			{
				c.BaseAddress = new Uri(Configuration.GetSection("OrderService").GetValue<string>("ApiBase"));
				c.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
			});

			services.AddHttpClient<IDriverAPIService, DriverAPIService>(c =>
			{
				c.BaseAddress = new Uri(Configuration.GetSection("DriverAPIService").GetValue<string>("ApiBase"));
				c.DefaultRequestHeaders.Add("Accept", "application/json");
			});

			services.AddScoped<ISiteSettings, SiteSettings>();
			services.AddScoped<SiteClient>();
			services.AddScoped<MobileAppClient>();
			services.AddScoped<SiteNotifier>();
			services.AddScoped<MobileAppNotifier>();
			services.AddScoped<NotificationModel>();
			

			//backgroundServices
			services.AddHostedService<FastPaymentStatusUpdater>();
			services.AddHostedService<CachePaymentManager>();

			//repositories
			services.AddSingleton<IOrderRepository, OrderRepository>();
			services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
			services.AddSingleton<IFastPaymentRepository, FastPaymentRepository>();
			services.AddSingleton<IRouteListItemRepository, RouteListItemRepository>();
			services.AddSingleton<ISelfDeliveryRepository, SelfDeliveryRepository>();
			services.AddSingleton<ICashRepository, CashRepository>();

			//factories
			services.AddSingleton<IFastPaymentFactory, FastPaymentFactory>();

			//converters
			services.AddSingleton<IOrderSumConverter, OrderSumConverter>();
			services.AddSingleton<IResponseCodeConverter, ResponseCodeConverter>();
			services.AddSingleton<IRequestFromConverter, RequestFromConverter>();

			//models
			services.AddScoped<IFastPaymentOrderModel, FastPaymentOrderModel>();
			services.AddScoped<IFastPaymentModel, FastPaymentModel>();

			//validators
			services.AddScoped<IFastPaymentValidator, FastPaymentValidator>();

			//helpers
			services.AddSingleton<IDTOManager, DTOManager>();
			services.AddScoped<ISignatureManager, SignatureManager>();
			services.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>();
			services.AddSingleton<IFastPaymentManager, FastPaymentManager>();
			services.AddSingleton<IErrorHandler, ErrorHandler>();
			services.AddSingleton(_ => new FastPaymentFileCache("/tmp/VodovozFastPaymentServiceTemp.txt"));
			services.AddScoped<IOrderRequestManager, OrderRequestManager>();

			services.ConfigureHealthCheckService<FastPaymentsHealthCheck>();
		}
		
		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseSwagger();

			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FastPaymentsAPI v1"));
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			});

			app.ConfigureHealthCheckApplicationBuilder();
		}
	}
}

