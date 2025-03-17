using Edo.Common;
using Edo.Problems;
using Edo.Receipt.Dispatcher.ErrorDebug.Consumers;
using Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using TrueMark.Codes.Pool;
using TrueMark.Library;

namespace Edo.Receipt.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoReceiptDispatcherErrorDebug(this IServiceCollection services)
		{
			services
				.AddHttpClient()
				;

			services
				.AddScoped<ReceiptEdoTaskHandler>()
				.AddScoped<ResaleReceiptEdoTaskHandler>()
				.AddScoped<ForOwnNeedsReceiptEdoTaskHandler>()
				.AddScoped<Tag1260Checker>()
				;

			services.AddEdo();
			services.AddEdoProblemRegistation();
			services.AddCodesPool();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<ReceiptCompleteEventConsumer, ReceiptCompleteEventConsumerDefinition>();
				cfg.AddConsumer<ReceiptTaskCreatedEventConsumer, ReceiptTaskCreatedEventConsumerDefinition>();
				cfg.AddConsumer<TransferCompleteConsumer, TransferCompleteConsumerDefinition>();
			});

			return services;
		}
	}
}
