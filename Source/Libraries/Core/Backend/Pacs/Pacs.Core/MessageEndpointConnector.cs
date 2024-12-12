using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QS.Utilities.Debug;
using System;
using System.Threading.Tasks;

namespace Pacs.Core
{
	public class MessageEndpointConnector
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<MessageEndpointConnector> _logger;
		private readonly IReceiveEndpointConnector _receiveEndpointConnector;

		public MessageEndpointConnector(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_logger = _serviceProvider.GetRequiredService<ILogger<MessageEndpointConnector>>();
			_receiveEndpointConnector = _serviceProvider.GetRequiredService<IReceiveEndpointConnector>();
		}

		public async Task TryConnectEndpoint<TConsumerDefinition>() 
			where TConsumerDefinition : IConsumerDefinition
		{
			TConsumerDefinition consumerDefinition;
			try
			{
				consumerDefinition = _serviceProvider.GetRequiredService<TConsumerDefinition>();
			}
			catch(Exception ex)
			{
				var pacsInitException = ex.FindExceptionTypeInInner<PacsInitException>();
				if(pacsInitException != null)
				{
					_logger.LogInformation(ex.Message);
					return;
				}

				throw;
			}

			var handle = _receiveEndpointConnector.ConnectReceiveEndpoint(consumerDefinition.EndpointDefinition, DefaultEndpointNameFormatter.Instance,
				(context, configurator) =>
				{
					configurator.ConfigureConsumer(context, consumerDefinition.ConsumerType);
				}
			);

			var result = await handle.Ready;
			if(!result.IsStarted)
			{
				throw new PacsException($"Невозможно подключить конечную точку для {typeof(TConsumerDefinition).Name}");
			}
		}
	}
}
