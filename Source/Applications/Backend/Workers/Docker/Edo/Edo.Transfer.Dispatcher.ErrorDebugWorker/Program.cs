using Autofac.Extensions.DependencyInjection;
using Edo.CodesSaver;
using Edo.Common;
using Edo.Docflow.Consumers;
using Edo.Documents;
using Edo.ErrorDebugWorker.Consumers;
using Edo.ErrorDebugWorker.Consumers.Definitions;
using Edo.Problems;
using Edo.Receipt.Dispatcher;
using Edo.Receipt.Dispatcher.Consumers;
using Edo.Receipt.Dispatcher.Consumers.Definitions;
using Edo.Receipt.Dispatcher.ErrorDebug.Consumers;
using Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions;
using Edo.Receipt.Sender;
using Edo.Scheduler;
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
					builder.ClearProviders();
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

					services.Configure<CashboxesSetting>(hostContext.Configuration);

					services.AddMessageTransportSettings();

					services.AddHttpClient();

					services.AddEdo();
					services.AddEdoProblemRegistration();
					services.AddCodesPool();

					services.AddEdoTransfer();

					services
						.AddCodesSaverServices()
						.AddEdoDocflowServices()
						.AddEdoDocumentsServices()
						.AddEdoReceiptDispatcherServices()
						.AddEdoReceiptSenderServices()
						.AddEdoSchedulerServices()
						.AddEdoTransferDispatcherServices()
						.AddEdoTransferRoutineServices()
						.AddEdoTransferSenderServices()
						;

					services.AddEdo();
					services.AddEdoProblemRegistration();
					services.AddCodesPool();

					services.AddEdoMassTransit(
						configureBus: cfg =>
						{
							// Выбор какой консюмер дебажить:

							//request
							//cfg.AddConsumer<EdoRequestCreatedErrorConsumer, EdoRequestCreatedErrorConsumerDefinition>();

							//document
							//cfg.AddConsumer<DocumentTaskCreatedErrorConsumer, DocumentTaskCreatedErrorConsumerDefinition>();
							//cfg.AddConsumer<DocumentTransferCompleteErrorConsumer, DocumentTransferCompleteErrorConsumerDefinition>();
							//cfg.AddConsumer<OrderDocumentAcceptedErrorConsumer, OrderDocumentAcceptedErrorConsumerDefinition>();

							//receipt
							//cfg.AddConsumer<ReceiptTaskCreatedErrorConsumer, ReceiptTaskCreatedErrorConsumerDefinition>();
							//cfg.AddConsumer<ReceiptReadyToSendErrorConsumer, ReceiptReadyToSendErrorConsumerDefinition>();
							//cfg.AddConsumer<ReceiptTransferCompleteErrorConsumer, ReceiptTransferCompleteErrorConsumerDefinition>();

							//transfer
							//cfg.AddConsumer<TransferDocumentAcceptedErrorConsumer, TransferDocumentAcceptedErrorConsumerDefinition>();

							//docflow
							//cfg.AddConsumer<DocflowUpdatedErrorConsumer, DocflowUpdatedErrorConsumerDefinition>();
							//cfg.AddConsumer<OrderDocumentSendErrorConsumer, OrderDocumentSendErrorConsumerDefinition>();
							cfg.AddConsumer<TransferDocumentSendErrorConsumer, TransferDocumentSendErrorConsumerDefinition>();
						}
					);
				});
	}
}
