using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using QS.DomainModel.UoW;
using TransactionalOutbox.MassTransit;
using Vodovoz.Core.Domain.Clients.Mindbox;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxOnlineOrderEnqueuer : IMindboxOnlineOrderEnqueuer
	{
		private readonly IUnitOfWorkOutbox<IBus> _outbox;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxOnlineOrderEnqueuer> _logger;

		public MindboxOnlineOrderEnqueuer(
			IUnitOfWorkOutbox<IBus> outbox,
			IMindboxSettings settings,
			ILogger<MindboxOnlineOrderEnqueuer> logger)
		{
			_outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task EnqueueAsync(
			IUnitOfWork unitOfWork,
			OnlineOrder onlineOrder,
			DateTime changeAt,
			CancellationToken cancellationToken)
		{
			if(unitOfWork is null) throw new ArgumentNullException(nameof(unitOfWork));
			if(onlineOrder is null) throw new ArgumentNullException(nameof(onlineOrder));

			await EnqueueCoreAsync(unitOfWork, onlineOrder, changeAt, cancellationToken).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task EnqueueCancelAsync(
			IUnitOfWork unitOfWork,
			int onlineOrderId,
			DateTime changeAt,
			CancellationToken cancellationToken)
		{
			var onlineOrder = unitOfWork.GetById<OnlineOrder>(onlineOrderId);
			if(onlineOrder is null)
			{
				return;
			}

			await EnqueueCoreAsync(unitOfWork, onlineOrder, changeAt, cancellationToken).ConfigureAwait(false);
		}

		private async Task EnqueueCoreAsync(
			IUnitOfWork unitOfWork,
			OnlineOrder onlineOrder,
			DateTime changeAt,
			CancellationToken cancellationToken)
		{
			if(!_settings.IsEnabled)
			{
				return;
			}

			if(onlineOrder.MindboxSnapshot is null
				&& onlineOrder.MindboxStatus != MindboxCounterpartyOutboxStatus.Queued
				&& onlineOrder.OnlineOrderStatus == OnlineOrderStatus.Canceled)
			{
				return;
			}

			var now = DateTime.UtcNow;

			if(onlineOrder.MindboxSyncRequestedAt is null)
			{
				onlineOrder.MindboxTransactionId = Guid.NewGuid();
				onlineOrder.MindboxSyncRequestedAt = now;
			}

			onlineOrder.MindboxLastChangeAt = changeAt;
			onlineOrder.MindboxStatus = MindboxCounterpartyOutboxStatus.Queued;

			unitOfWork.Save(onlineOrder);

			await _outbox.GetPublishEndpoint(unitOfWork)
				.Publish(
					new MindboxOnlineOrderSyncMessage
					{
						OnlineOrderId = onlineOrder.Id,
						TransactionId = onlineOrder.MindboxTransactionId ?? Guid.NewGuid()
					},
					cancellationToken)
				.ConfigureAwait(false);

			_logger.LogInformation(
				"Онлайн-заказ {OnlineOrderId} поставлен в очередь Mindbox, transactionId={TransactionId}",
				onlineOrder.Id,
				onlineOrder.MindboxTransactionId);
		}
	}
}
