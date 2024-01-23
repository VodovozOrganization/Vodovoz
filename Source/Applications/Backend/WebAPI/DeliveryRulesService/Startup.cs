using Autofac;
using DeliveryRulesService.Cache;
using Fias.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Services;
using System.Linq;
using System.Reflection;
using DeliveryRulesService.HealthChecks;
using Vodovoz;
using Vodovoz.Core.DataService;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using VodovozHealthCheck;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Microsoft.Extensions.Logging;

namespace DeliveryRulesService
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

			services.AddMvc().AddControllersAsServices();
			services.AddControllers().AddJsonOptions(j =>
			{
				//Необходимо для сериализации свойств как PascalCase
				j.JsonSerializerOptions.PropertyNamingPolicy = null;
			});

			services
				.AddLogging(logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				})

				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()

				.ConfigureHealthCheckService<DeliveryRulesServiceHealthCheck>()
				.AddHttpClient()
				.AddFiasClient()
				;
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterType<BaseParametersProvider>().AsImplementedInterfaces();
			builder.RegisterType<DistrictCache>().AsSelf().AsImplementedInterfaces();
			
			builder.RegisterType<CallTaskWorker>()
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterType<UserService>()
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterInstance(ErrorReporter.Instance).AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Handler"))
				.AsSelf()
				.AsImplementedInterfaces();

			var vodovozBusinessAssembly = typeof(VodovozBusinessAssemblyFinder).Assembly;

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Provider"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Model"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Repository"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly, Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Factory"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Controller"))
				.AsSelf()
				.AsImplementedInterfaces();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			app.ConfigureHealthCheckApplicationBuilder();
        }
	}
}
