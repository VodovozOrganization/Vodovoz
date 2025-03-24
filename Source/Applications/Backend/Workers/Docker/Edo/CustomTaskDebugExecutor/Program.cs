using Autofac.Extensions.DependencyInjection;
using Edo.Common;
using Edo.Problems;
using Edo.Receipt.Dispatcher;
using Edo.Transport;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using System;
using System.Threading;
using TrueMark.Codes.Pool;
using TrueMark.Library;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;

namespace CustomTaskDebugExecutor
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			ServiceCollection services = new ServiceCollection();

			var builder = new ConfigurationBuilder();
			builder.SetBasePath(Environment.CurrentDirectory);
			builder.AddJsonFile("appsettings.Development.json");
			IConfiguration configuration = builder.Build();
			services.AddScoped<IConfiguration>(_ => configuration);

			services.AddLogging(loggingBuilder =>
			 {
				 // configure Logging with NLog
				 loggingBuilder.ClearProviders();
				 loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
				 loggingBuilder.AddNLog(configuration);
			 });

			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(Vodovoz.Core.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
				)
				.AddDatabaseConnection()
				.AddNHibernateConventions()
				.AddCoreDataRepositories()
				.AddCore()
				.AddTrackedUoW()
				.AddMessageTransportSettings()

				.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
			;

			services.AddMessageTransportSettings();
			services.AddEdoMassTransit();

			services.AddHttpClient();

			services.AddEdo();
			services.AddEdoProblemRegistation();
			services.AddCodesPool();

			services
				.AddScoped<ReceiptEdoTaskHandler>()
				.AddScoped<ResaleReceiptEdoTaskHandler>()
				.AddScoped<ForOwnNeedsReceiptEdoTaskHandler>()
				.AddScoped<Tag1260Checker>()
				;

			var autofacFactory = new AutofacServiceProviderFactory();
			var autofacBuilder = autofacFactory.CreateBuilder(services);			
			var serviceProvider = autofacFactory.CreateServiceProvider(autofacBuilder);

			var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

			logger.LogInformation("Debug запуск обработчиков задач ЭДО");

			var receiptTaskHandler = serviceProvider.GetRequiredService<ReceiptEdoTaskHandler>();
			CancellationTokenSource cts = new CancellationTokenSource();

			// Вызов обработчика
			var id = 0;
			receiptTaskHandler.HandleNew(id, cts.Token).Wait();
		}
	}
}
