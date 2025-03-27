using Autofac.Extensions.DependencyInjection;
using Edo.Common;
using Edo.Documents;
using Edo.Problems;
using Edo.Receipt.Dispatcher;
using Edo.Receipt.Dispatcher.ErrorDebug.Consumers;
using Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions;
using Edo.Receipt.Sender;
using Edo.Transport;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModulKassa;
using NLog.Extensions.Logging;
using QS.Project.Core;
using System;
using TrueMark.Codes.Pool;
using TrueMark.Library;
using TrueMarkApi.Client;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;

namespace Edo.Transfer.Dispatcher.ErrorDebugWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureLogging((ctx, builder) => {
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.ConfigureServices((hostContext, services) =>
				{
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

					services
						.AddTrueMarkApiClient()
					;

					services
						//document
						.AddScoped<DocumentEdoTaskHandler>()
						.AddScoped<ForOwnNeedDocumentEdoTaskHandler>()
						.AddScoped<ForResaleDocumentEdoTaskHandler>()

						//transfer.dispatcher
						.AddScoped<TransferEdoHandler>()
						.AddEdoTransfer()

						//receipt.dispatcher
						.AddScoped<ReceiptEdoTaskHandler>()
						.AddScoped<ResaleReceiptEdoTaskHandler>()
						.AddScoped<ForOwnNeedsReceiptEdoTaskHandler>()
						.AddScoped<Tag1260Checker>()

						//receipt.sender
						.AddModulKassa()
						.Configure<CashboxesSetting>(hostContext.Configuration)
						.AddScoped<FiscalDocumentFactory>()
						.AddScoped<ReceiptSender>()
						;

					services.AddEdo();
					services.AddEdoProblemRegistation();
					services.AddCodesPool();

					services.AddEdoMassTransit(
						configureBus: cfg =>
						{
							// Выбор какие ошибки дебажить:

							////cfg.AddConsumer<TransferCompleteErrorConsumer, TransferCompleteErrorConsumerDefinition>();
							//cfg.AddConsumer<ReceiptReadyToSendErrorConsumer, ReceiptReadyToSendErrorConsumerDefinition>();
							//cfg.AddConsumer<DocumentTaskCreatedErrorConsumer, DocumentTaskCreatedErrorConsumerDefinition>();
							cfg.AddConsumer<TransferDocumentAcceptedErrorConsumer, TransferDocumentAcceptedErrorConsumerDefinition>();
						}
					);
				});
	}
}
