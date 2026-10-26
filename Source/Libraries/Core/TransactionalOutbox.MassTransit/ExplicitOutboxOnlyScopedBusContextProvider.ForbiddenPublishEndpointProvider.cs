using MassTransit;
using MassTransit.Util;
using System;
using System.Threading.Tasks;

namespace TransactionalOutbox.MassTransit
{
	public sealed partial class ExplicitOutboxOnlyScopedBusContextProvider
	{
		/// <summary>
		/// Источник конечных точек публикации, который не даёт опубликовать сообщение.
		/// Где используется: <see cref="ExplicitOutboxOnlyScopedBusContextProvider"/> оборачивает его в штатный PublishEndpoint
		/// MassTransit и отдаёт как IPublishEndpoint из DI на десктопе вне потребителя (диалоги, модели представлений, сервисы).
		/// Что происходит: Publish у такого IPublishEndpoint запрашивает у этого класса конечную точку (GetPublishSendEndpoint (MassTransit))
		/// и получает InvalidOperationException с подсказкой <see cref="Message"/>: публиковать через
		/// IUnitOfWorkOutbox&lt;IBus&gt;.GetPublishEndpoint(uow). Сообщение никуда не уходит.
		/// Подписка наблюдателей (ConnectPublishObserver (MassTransit)) исключение не бросает и ничего не делает: запрещена только публикация.
		/// <see cref="IUnitOfWorkOutbox{TBus}"/> этот класс не использует: у него свой IPublishEndpoint, который пишет в outbox
		/// </summary>
		private sealed class ForbiddenPublishEndpointProvider : IPublishEndpointProvider
		{
			/// <summary>
			/// Переопределение штатного метода MassTransit <see cref="IPublishEndpointProvider.GetPublishSendEndpoint{T}"/>:
			/// вместо конечной точки публикации бросает исключение с подсказкой
			/// </summary>
			public Task<ISendEndpoint> GetPublishSendEndpoint<T>()
				where T : class =>
				throw new InvalidOperationException(Message);

			/// <summary>
			/// Переопределение штатного метода MassTransit <see cref="IPublishObserverConnector.ConnectPublishObserver"/>:
			/// подписку принимает и ничего не делает, публиковать всё равно нельзя
			/// </summary>
			public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => new EmptyConnectHandle();
		}
	}
}
