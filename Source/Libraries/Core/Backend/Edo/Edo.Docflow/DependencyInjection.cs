using Edo.Docflow;
using Edo.Docflow.Consumers;
using Edo.Docflow.Consumers.Definitions;
using Edo.Docflow.Factories;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Documents
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoDocflow(this IServiceCollection services)
		{
			services
				.AddScoped<DocflowHandler>()
				.AddScoped<OrderUpdInfoFactory>()
				.AddScoped<TransferOrderUpdInfoFactory>()
				;

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<TransferDocumentSendConsumer, TransferDocumentSendConsumerDefinition>();
				cfg.AddConsumer<OrderDocumentSendConsumer, OrderDocumentSendConsumerDefinition>();
				cfg.AddConsumer<DocflowUpdatedConsumer, DocflowUpdatedConsumerDefinition>();
			});

			return services;
		}
	}
}
