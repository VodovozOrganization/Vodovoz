using Edo.Common;
using Edo.Documents.Consumers;
using Edo.Documents.Consumers.Definitions;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Documents
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoDocuments(this IServiceCollection services)
		{
			services
				.AddScoped<DocumentEdoTaskHandler>()
				;

			services.AddEdo();
			services.AddEdoProblemRegistation();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<TransferCompleteConsumer, TransferCompleteConsumerDefinition>();
				cfg.AddConsumer<DocumentTaskCreatedConsumer, DocumentTaskCreatedConsumerDefinition>();
				cfg.AddConsumer<OrderDocumentSentConsumer, OrderDocumentSentConsumerDefinition>();
				cfg.AddConsumer<OrderDocumentAcceptedConsumer, OrderDocumentAcceptedConsumerDefinition>();
			});

			return services;
		}
	}
}
