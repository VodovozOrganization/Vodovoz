using MassTransit;
using System;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Настройка потребителя отправки контрагентов в Mindbox:
	/// очередь и повторы только для временных ошибок.
	/// </summary>
	public class MindboxCounterpartySendConsumerDefinition : ConsumerDefinition<MindboxCounterpartySendConsumer>
	{
		private readonly IMindboxSettings _settings;

		public MindboxCounterpartySendConsumerDefinition(IMindboxSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			EndpointName = "mindbox-counterparty-send";
			ConcurrentMessageLimit = 10;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<MindboxCounterpartySendConsumer> consumerConfigurator)
		{
			endpointConfigurator.UseMessageRetry(retryConfigurator =>
			{
				retryConfigurator.Intervals(
					MindboxRetrySchedule.BuildIntervals(
						_settings.RetryScheduleMinutes,
						_settings.RetryIntervalMinutes,
						_settings.RetryMaxHours));
				retryConfigurator.Handle<MindboxTransientException>();
				retryConfigurator.Ignore<OperationCanceledException>();
			});
		}
	}
}
