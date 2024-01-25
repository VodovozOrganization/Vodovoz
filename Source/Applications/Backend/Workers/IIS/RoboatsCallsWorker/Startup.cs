using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QS.Project.Core;
using System.Linq;
using System.Reflection;
using Vodovoz;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Parameters;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;

namespace RoboatsCallsWorker
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
			services.AddLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddNLogWeb();
				logging.AddConfiguration(Configuration.GetSection("NLog"));
			});

			services
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
				;
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterType<BaseParametersProvider>().AsImplementedInterfaces();
			builder.RegisterType<RoboatsRepository>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<SettingsController>().As<ISettingsController>();
			builder.RegisterType<RoboatsSettings>().As<IRoboatsSettings>();

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

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly, Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Controller"))
				.AsSelf()
				.AsImplementedInterfaces();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }
	}
}
