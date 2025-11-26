using Autofac.Extensions.DependencyInjection;
using Edo.CodesSaver;
using Edo.Common;
using Edo.Documents;
using Edo.InformalOrderDocuments;
using Edo.Problems;
using Edo.Receipt.Dispatcher;
using Edo.Receipt.Sender;
using Edo.Scheduler;
using Edo.Transfer;
using Edo.Transfer.Dispatcher;
using Edo.Transport;
using Edo.Withdrawal;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ModulKassa;
using NLog.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Taxcom.Docflow.Utility;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;

namespace CustomTaskDebugExecutor
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			ServiceCollection services = new ServiceCollection();

			var projectSettingsPath = Path.Combine(GetProjectDirectory(), "appsettings.Development.json");
			var settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json");

			if(File.Exists(projectSettingsPath))
			{
				settingsPath = projectSettingsPath;
			}

			var builder = new ConfigurationBuilder();
			builder.AddJsonFile(settingsPath);
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

			services.Configure<CashboxesSetting>(configuration);

			services.AddMessageTransportSettings();
			services.AddEdoMassTransit();

			services.AddHttpClient();

			services.AddEdo();
			services.AddEdoProblemRegistration();
			services.AddCodesPool();

			services.AddEdoTransfer();

			services
				.AddCodesSaverServices()
				.AddEdoDocflowServices()
				.AddEdoDocumentsServices()
				.AddTenderEdoServices()
				.AddEdoReceiptDispatcherServices()
				.AddEdoReceiptSenderServices()
				.AddEdoSchedulerServices()
				.AddEdoTransferDispatcherServices()
				.AddEdoTransferRoutineServices()
				.AddEdoTransferSenderServices()
				.AddEdoWithdrawalService()
				.AddInformalOrderDocumentEdoServices()
				;

			services.AddScoped<EdoExecutor>();

			services.AddTaxcomRehandleService();

			// Коммит с подтверждением в консоли
			services.AddScoped<TrackedUnitOfWorkFactory>();
			services.Replace(ServiceDescriptor.Scoped(typeof(IUnitOfWorkFactory), typeof(ConsoleApprovedUnitOfWorkFactory)));
			services.Replace(ServiceDescriptor.Scoped(typeof(IUnitOfWork), typeof(ConsoleApprovedUnitOfWork)));

			var autofacFactory = new AutofacServiceProviderFactory();
			var autofacBuilder = autofacFactory.CreateBuilder(services);
			var serviceProvider = autofacFactory.CreateServiceProvider(autofacBuilder);

			using(var scope = serviceProvider.CreateScope())
			{
				var sp = scope.ServiceProvider;
				var timeout = TimeSpan.FromMinutes(30);
				var cts = new CancellationTokenSource();

				var exec = sp.GetRequiredService<EdoExecutor>();

				try
				{
					await exec.TrySendEdoEvent(cts.Token);
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		private static string GetProjectDirectory()
		{
			string current = AppContext.BaseDirectory;
			while(!Directory.GetFiles(current, "*.csproj").Any() && Directory.GetParent(current) != null)
			{
				current = Directory.GetParent(current)?.FullName ?? current;
			}
			return current;
		}
	}
}
