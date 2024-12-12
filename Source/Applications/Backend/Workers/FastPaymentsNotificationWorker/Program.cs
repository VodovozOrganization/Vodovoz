using Autofac;
using Autofac.Extensions.DependencyInjection;
using FastPaymentsAPI.Library.ApiClients;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.FastPayments;
using Vodovoz.Tools;

namespace FastPaymentsNotificationWorker
{
	public class Program
	{
		private const string _nLogSectionName = nameof(NLog);

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory(ConfigureContainer))
				.UseWindowsService()
				.ConfigureServices((context, services) =>
				{
					services.AddLogging(
						logging =>
						{
							logging.ClearProviders();
							logging.AddNLog();
							logging.AddConfiguration(context.Configuration.GetSection(_nLogSectionName));
						});

					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(QS.Attachments.Domain.Attachment).Assembly,
							typeof(EmployeeWithLoginMap).Assembly
						)
						.AddDatabaseConnection()
						.AddCore()
						.AddInfrastructure()
						.AddTrackedUoW();

				Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
					services.AddHostedService<PaymentsNotificationWorker>();
				});

		public static void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterType<NotificationHandler>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<FastPaymentFactory>()
				.As<IFastPaymentFactory>()
				.InstancePerLifetimeScope();

			builder.RegisterType<SiteSettings>()
				.As<ISiteSettings>()
				.InstancePerLifetimeScope();

			builder.RegisterType<OrderSumConverter>()
				.As<IOrderSumConverter>()
			.InstancePerLifetimeScope();

			builder.RegisterType<SiteClient>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<SiteNotifier>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<MobileAppNotifier>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<MobileAppClient>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<NotificationModel>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterInstance(ErrorReporter.Instance)
				.As<IErrorReporter>()
				.SingleInstance();
		}
	}
}
