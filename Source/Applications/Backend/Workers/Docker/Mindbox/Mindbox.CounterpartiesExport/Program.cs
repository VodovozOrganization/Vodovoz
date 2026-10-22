using System;
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
using Vodovoz.Mindbox;
using Vodovoz.Mindbox.Export;

namespace Mindbox.CounterpartiesExport
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			if(args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
			{
				PrintUsage();
				return 0;
			}

			var options = MindboxExportOptionsParser.Parse(args, out var error);

			if(options == null)
			{
				Console.Error.WriteLine(error);
				PrintUsage();
				return 2;
			}

			var host = CreateHostBuilder(args).Build();

			using(var scope = host.Services.CreateScope())
			using(var cancellationTokenSource = new CancellationTokenSource())
			{
				Console.CancelKeyPress += (sender, eventArgs) =>
				{
					eventArgs.Cancel = true;
					cancellationTokenSource.Cancel();
				};

				var exporter = scope.ServiceProvider.GetRequiredService<IMindboxCounterpartiesExporter>();
				MindboxExportResult result;

				try
				{
					result = await exporter.ExportAsync(options, cancellationTokenSource.Token);
				}
				catch(Exception ex)
				{
					Console.Error.WriteLine($"Ошибка выгрузки: {ex.Message}");
					return 1;
				}

				Console.WriteLine();
				Console.WriteLine($"Всего контрагентов: {result.Total}");
				Console.WriteLine($"Отправлено/сохранено: {result.Sent}");
				Console.WriteLine($"Пакетов: {result.Batches}");

				if(result.IsSuccess)
				{
					Console.WriteLine("Выгрузка завершена успешно.");
					return 0;
				}

				Console.Error.WriteLine("Выгрузка завершена с ошибками:");

				foreach(var exportError in result.Errors)
				{
					Console.Error.WriteLine($"  - {exportError}");
				}

				return 1;
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
						.AddTrackedUoW()
						.AddMindbox();

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
					services.AddStaticHistoryTracker();
				});
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Выгрузка контрагентов Доставки воды в Mindbox.");
			Console.WriteLine();
			Console.WriteLine("Параметры:");
			Console.WriteLine("  --from yyyy-MM-dd   дата создания контрагента (по умолчанию 2025-01-01)");
			Console.WriteLine("  --dry-run           сформировать CSV-файлы, не отправляя в Mindbox");
			Console.WriteLine("  --output <path>     сохранить весь CSV в указанный файл, не отправляя в Mindbox");
			Console.WriteLine("  --batch-size <n>    размер пакета строк (по умолчанию из настройки Mindbox.ImportBatchSize)");
			Console.WriteLine("  --help              показать эту справку");
		}
	}
}
