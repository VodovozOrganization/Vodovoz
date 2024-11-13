using MassTransit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TrueMark.Contracts.Requests;

namespace TrueMark.ProductInstanceInfoCheck.Worker;
public class ProductInstanceInfoRequestConsumerDefinition : ConsumerDefinition<ProductInstanceInfoRequestConsumer>
{
	private readonly IOptionsMonitor<TrueMarkProductInstanceInfoCheckOptions> _optionsMonitor;

	public ProductInstanceInfoRequestConsumerDefinition(
		IOptionsMonitor<TrueMarkProductInstanceInfoCheckOptions> optionsMonitor)
	{
		_optionsMonitor = optionsMonitor;
	}

	protected override void ConfigureConsumer(
		IReceiveEndpointConfigurator endpointConfigurator,
		IConsumerConfigurator<ProductInstanceInfoRequestConsumer> consumerConfigurator,
		IRegistrationContext context)
	{
		var currentCodesPerRequestLimit = _optionsMonitor.CurrentValue.CodesPerRequestLimit;
		var currentRequestsTimeOut = _optionsMonitor.CurrentValue.RequestsTimeOut;

		consumerConfigurator.Options<BatchOptions>(options => options
			.SetMessageLimit(currentCodesPerRequestLimit)
			.SetTimeLimit(currentRequestsTimeOut)
			.SetTimeLimitStart(BatchTimeLimitStart.FromLast));

		if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmqc)
		{
			rmqc.Durable = true;
			rmqc.ExchangeType = ExchangeType.Fanout;

			rmqc.PrefetchCount = currentCodesPerRequestLimit;

			rmqc.Batch<ProductInstanceInfoRequest>(batchConfig =>
			{
				batchConfig.MessageLimit = currentCodesPerRequestLimit;
				batchConfig.TimeLimit = currentRequestsTimeOut;
				batchConfig.ConcurrencyLimit = 1;
			});
		}
	}
}
