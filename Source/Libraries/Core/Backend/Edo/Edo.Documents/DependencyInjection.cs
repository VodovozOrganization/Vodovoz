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
				.AddScoped<TransferRequestCreator>()
				.AddScoped<EdoTaskItemTrueMarkStatusProvider>()
				.AddScoped<EdoTaskItemTrueMarkStatusProviderFactory>()
				;

			services.AddEdoTaskValidation();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<TransferDoneConsumer, TransferDoneConsumerDefinition>();
				cfg.AddConsumer<DocumentTaskCreatedConsumer, DocumentTaskCreatedConsumerDefinition>();
				cfg.AddConsumer<CustomerDocumentSentConsumer, CustomerDocumentSentConsumerDefinition>();
				cfg.AddConsumer<CustomerDocumentAcceptedConsumer, CustomerDocumentAcceptedConsumerDefinition>();
			});

			return services;
		}
	}
}
