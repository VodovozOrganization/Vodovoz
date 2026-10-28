using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;

namespace Mindbox.CounterpartiesAdmin
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			if(args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
			{
				PrintUsage();
				return 0;
			}

			var options = MindboxAdminOptions.Parse(args, out var error);

			if(options == null)
			{
				if(!string.IsNullOrWhiteSpace(error))
				{
					Console.Error.WriteLine(error);
				}

				PrintUsage();
				return 2;
			}

			var host = CreateHostBuilder(args).Build();

			using(var scope = host.Services.CreateScope())
			{
				var service = scope.ServiceProvider.GetRequiredService<IMindboxAdminService>();
				var count = options.Command == "report"
					? await service.ReportAsync(options, CancellationToken.None)
					: await service.ResendAsync(options, CancellationToken.None);

				Console.WriteLine(options.Command == "report"
					? $"Записей в отчете: {count}"
					: $"Поставлено на повторную отправку: {count}");

				return 0;
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services.AddLogging(logging =>
						{
							logging.ClearProviders();
							logging.AddNLog();
							logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
						})

						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(QS.Attachments.Domain.Attachment).Assembly,
							typeof(EmployeeWithLoginMap).Assembly,
							typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly)
						.AddDatabaseConnection()
						.AddCore()
						.AddInfrastructure()
						.AddTrackedUoW();

					services.AddScoped<IMindboxAdminService, MindboxAdminService>();

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
					services.AddStaticHistoryTracker();
				});
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Управление отправкой контрагентов в Mindbox.");
			Console.WriteLine();
			Console.WriteLine("Отчет:");
			Console.WriteLine("  report [--status DataError,Error] [--output report.csv]");
			Console.WriteLine();
			Console.WriteLine("Повторная отправка:");
			Console.WriteLine("  resend --id 123");
			Console.WriteLine("  resend --ids 1,2,3");
			Console.WriteLine("  resend --status DataError,Error");
			Console.WriteLine("  resend --all-errors");
			Console.WriteLine();
			Console.WriteLine("При повторной отправке генерируется новый transactionId и сбрасывается счетчик попыток.");
		}
	}
}
