using Autofac.Extensions.DependencyInjection;
using CustomerOnlineOrdersRegistrar.Consumers;
using CustomerOnlineOrdersRegistrar.Factories;
using CustomerOrdersApi.Library;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QS.Project.Core;
using Vodovoz;
using Vodovoz.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;

namespace CustomerOnlineOrdersRegistrar
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services.AddMappingAssemblies(
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
						.AddBusiness()
						.AddDependenciesGroup()
						.AddApplicationOrderServices()
						.AddStaticScopeForEntity()
						//.AddStaticHistoryTracker()

						.AddScoped<IOnlineOrderFactory, OnlineOrderFactory>()
						
						.AddMessageTransportSettings()
						.AddMassTransit(busConf =>
						{
							busConf.AddConsumer<OnlineOrderRegisteredConsumer, OnlineOrderRegisteredConsumerDefinition>();
							busConf.AddConsumer<OnlineOrderRegisterFaultConsumer, OnlineOrderRegisterFaultConsumerDefinition>();

							busConf.ConfigureRabbitMq();
						})
						;
				});
	}
}
