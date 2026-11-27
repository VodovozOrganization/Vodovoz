using System;
using MassTransit;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Настройка очереди синхронизации онлайн-заказов с Mindbox
	/// </summary>
	public sealed class MindboxOnlineOrderSyncConsumerDefinition
		: ConsumerDefinition<MindboxOnlineOrderSyncConsumer>
	{
		private readonly IMindboxSettings _settings;

		/// <summary>
		/// Создаёт описание очереди синхронизации
		/// </summary>
		/// <param name="settings">Настройки Mindbox</param>
		public MindboxOnlineOrderSyncConsumerDefinition(IMindboxSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			EndpointName = "mindbox-online-order-sync";
			ConcurrentMessageLimit = 1;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<MindboxOnlineOrderSyncConsumer> consumerConfigurator)
		{
			endpointConfigurator.PrefetchCount = 1;

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
