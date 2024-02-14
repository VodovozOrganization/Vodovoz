using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;

namespace Vodovoz.SmsInformerWorker
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
				.ConfigureLogging((hostContext, loggingBuilder) =>
				{
					loggingBuilder.ClearProviders();
					loggingBuilder.AddNLog();
					loggingBuilder.AddConfiguration(hostContext.Configuration.GetSection(_nLogSectionName));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddHostedService<UndeliveryNotApprovedSmsInformerWorker>()
						.AddHostedService<NewClientSmsInformerWorker>()
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
						.AddSmsInformerWorker(hostContext);
				});
	}
}
