using MassTransit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TrueMark.Contracts.Requests;

namespace TrueMark.ProductInstanceInfoCheck.Worker;
public class ProductInstanceInfoRequestConsumerDefinition : ConsumerDefinition<ProductInstanceInfoRequestConsumer>
{
	private readonly IOptionsMonitor<TrueMarkProductInstanceInfoCheckOptions> _optionsMonitor;

	protected ProductInstanceInfoRequestConsumerDefinition(
		IOptionsMonitor<TrueMarkProductInstanceInfoCheckOptions> optionsMonitor)
	{
		Endpoint(x =>
		{
			x.Name = "product.instance.info.requests";
		});
		_optionsMonitor = optionsMonitor;
	}

	protected override void ConfigureConsumer(
		IReceiveEndpointConfigurator endpointConfigurator,
		IConsumerConfigurator<ProductInstanceInfoRequestConsumer> consumerConfigurator,
		IRegistrationContext context)
	{
		if(consumerConfigurator is IRabbitMqReceiveEndpointConfigurator rmqc)
		{
			rmqc.Exclusive = true;
			rmqc.Durable = true;
			rmqc.ExchangeType = ExchangeType.Fanout;

			rmqc.ConcurrentMessageLimit = _optionsMonitor.CurrentValue.CodesPerRequestLimit;
			rmqc.PrefetchCount = _optionsMonitor.CurrentValue.CodesPerRequestLimit;

			rmqc.UseTimeout(configure =>
			{
				configure.Timeout = _optionsMonitor.CurrentValue.RequestsTimeOut;
			});

			rmqc.Bind<ProductInstanceInfoRequest>();
		}
	}
}
