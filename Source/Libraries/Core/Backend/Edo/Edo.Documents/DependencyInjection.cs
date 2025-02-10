using Edo.Common;
using Edo.Documents.Consumers;
using Edo.Documents.Consumers.Definitions;
using Edo.TaskValidation;
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
			services.AddEdoTaskValidation();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<TransferDoneConsumer, TransferDoneConsumerDefinition>();
				cfg.AddConsumer<DocumentTaskCreatedConsumer, DocumentTaskCreatedConsumerDefinition>();
				cfg.AddConsumer<OrderDocumentSentConsumer, OrderDocumentSentConsumerDefinition>();
				cfg.AddConsumer<OrderDocumentAcceptedConsumer, OrderDocumentAcceptedConsumerDefinition>();
			});

			return services;
		}
	}
}
