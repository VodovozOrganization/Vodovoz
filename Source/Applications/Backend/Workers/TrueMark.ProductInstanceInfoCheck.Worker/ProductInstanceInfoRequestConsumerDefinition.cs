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

			var currentCodesPerRequestLimit = _optionsMonitor.CurrentValue.CodesPerRequestLimit;
			var currentRequestsTimeOut = _optionsMonitor.CurrentValue.RequestsTimeOut;

			rmqc.PrefetchCount = currentCodesPerRequestLimit;

			rmqc.Batch<ProductInstanceInfoRequest>(batchConfig =>
			{
				batchConfig.MessageLimit = currentCodesPerRequestLimit;
				batchConfig.TimeLimit = currentRequestsTimeOut;
			});
		}
	}
}
