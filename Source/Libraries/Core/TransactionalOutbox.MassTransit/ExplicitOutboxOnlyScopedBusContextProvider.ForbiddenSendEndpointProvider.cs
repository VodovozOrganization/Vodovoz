using MassTransit;
using MassTransit.Util;
using System;
using System.Threading.Tasks;

namespace TransactionalOutbox.MassTransit
{
	public sealed partial class ExplicitOutboxOnlyScopedBusContextProvider
	{
		/// <summary>
		/// Источник конечных точек отправки, который не даёт отправить сообщение.
		/// Где используется: <see cref="ExplicitOutboxOnlyScopedBusContextProvider"/> отдаёт его как ISendEndpointProvider (MassTransit) из DI
		/// на десктопе вне потребителя (диалоги, модели представлений, сервисы).
		/// Что происходит: GetSendEndpoint(адрес) бросает InvalidOperationException с подсказкой <see cref="Message"/>:
		/// отправлять через IUnitOfWorkOutbox&lt;IBus&gt;.GetSendEndpointProvider(uow). Сообщение никуда не уходит.
		/// Подписка наблюдателей (ConnectSendObserver (MassTransit)) исключение не бросает и ничего не делает: запрещена только отправка.
		/// <see cref="IUnitOfWorkOutbox{TBus}"/> этот класс не использует: у него свой ISendEndpointProvider, который пишет в outbox
		/// </summary>
		private sealed class ForbiddenSendEndpointProvider : ISendEndpointProvider
		{
			/// <summary>
			/// Переопределение штатного метода MassTransit <see cref="ISendEndpointProvider.GetSendEndpoint"/>:
			/// вместо конечной точки отправки бросает исключение с подсказкой
			/// </summary>
			public Task<ISendEndpoint> GetSendEndpoint(Uri address) =>
				throw new InvalidOperationException(Message);

			/// <summary>
			/// Переопределение штатного метода MassTransit <see cref="ISendObserverConnector.ConnectSendObserver"/>:
			/// подписку принимает и ничего не делает, отправлять всё равно нельзя
			/// </summary>
			public ConnectHandle ConnectSendObserver(ISendObserver observer) => new EmptyConnectHandle();
		}
	}
}
