using System;
using EdoDocumentsConsumer.Services;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QS.Project.Core;
using TaxcomEdo.Library;
using Vodovoz;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;

namespace EdoDocumentsConsumer
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
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
						.AddTrackedUoW()
						.AddBusiness(hostContext.Configuration)
						.AddOptions()
						
						.AddMessageTransportSettings()
						.AddMassTransit(busConf => busConf.ConfigureRabbitMq())
						;
					
					services.AddHttpClient<ITaxcomService>(client =>
						client.BaseAddress = new Uri(hostContext.Configuration.GetSection("TaxcomApi")["BaseAddress"]));
				});
	}
}
