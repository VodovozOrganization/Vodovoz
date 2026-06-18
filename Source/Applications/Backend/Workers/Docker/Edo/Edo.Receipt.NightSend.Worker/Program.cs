using System;
using System.Text;
using Autofac.Extensions.DependencyInjection;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Banks.Domain;
using QS.BusinessCommon.HMap;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.Domain;
using QS.Project.HibernateMapping;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Zabbix.Sender;

namespace Edo.Receipt.NightSend.Worker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			CreateHostBuilder(args).Build().Run();
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((ctx, builder) =>
				{
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((_, services) =>
				{
					services
						.AddMappingAssemblies(
							typeof(UserBaseMap).Assembly,
							typeof(Bank).Assembly,
							typeof(HistoryMain).Assembly,
							typeof(TypeOfEntity).Assembly,
							typeof(AssemblyFinder).Assembly,
							typeof(MeasurementUnitsMap).Assembly
						)
						.AddDatabaseConnection()
						.AddTrackedUoW()
						.AddMessageTransportSettings()
						.AddEdoReceiptNightSend()
						;

					services
						.ConfigureZabbixSenderFromDataBase(nameof(ReceiptNightSendProblemWorker));
				});
	}
}
