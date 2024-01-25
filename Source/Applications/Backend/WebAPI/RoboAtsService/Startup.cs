using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.Services;
using QS.Services;
using RoboatsService.Authentication;
using RoboatsService.Monitoring;
using RoboatsService.OrderValidation;
using Sms.External.SmsRu;
using System.Linq;
using System.Reflection;
using Vodovoz;
using Vodovoz.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.DataService;
using Vodovoz.Data.NHibernate;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;

namespace RoboatsService
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
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				}
			);
			services.AddAuthentication()
				.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);
			services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme);
			services.AddMvc().AddControllersAsServices();

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

			services.AddApplication();
			services.AddBusiness();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterType<BaseParametersProvider>().AsImplementedInterfaces();
			builder.RegisterType<RoboatsCallFactory>().AsImplementedInterfaces();
			builder.RegisterType<RoboatsRepository>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<RoboatsSettings>().As<IRoboatsSettings>();
			builder.RegisterType<RoboatsCallBatchRegistrator>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<RoboatsCallRegistrator>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<ValidOrdersProvider>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<ApiKeyAuthenticationOptions>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<ApiKeyAuthenticationHandler>().AsSelf().AsImplementedInterfaces();
			builder.RegisterInstance(ServicesConfig.UserService).As<IUserService>();

			builder.RegisterModule<DatabaseSettingsModule>();
			builder.RegisterModule<SmsExternalSmsRuModule>();
			
			builder.RegisterType<CallTaskWorker>()
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterInstance(ErrorReporter.Instance).AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Handler"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly)
				.Where(t => t.Name.EndsWith("Provider"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly)
				.Where(t => t.Name.EndsWith("Model"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly)
				.Where(t => t.Name.EndsWith("Repository"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly, Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Factory"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly)
				.Where(t => t.Name.EndsWith("Controller"))
				.AsSelf()
				.AsImplementedInterfaces();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<SettingsController>().RefreshSettings();

			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

	}
}
