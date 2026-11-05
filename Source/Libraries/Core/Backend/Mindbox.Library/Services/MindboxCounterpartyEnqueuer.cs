using MassTransit;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.MassTransit;
using Vodovoz.Core.Domain.Clients.Mindbox;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxCounterpartyEnqueuer : IMindboxCounterpartyEnqueuer
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IUnitOfWorkOutbox<IBus> _outbox;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxCounterpartyEnqueuer> _logger;

		public MindboxCounterpartyEnqueuer(
			IUnitOfWorkFactory unitOfWorkFactory,
			IUnitOfWorkOutbox<IBus> outbox,
			IMindboxSettings settings,
			ILogger<MindboxCounterpartyEnqueuer> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task<int> EnqueueNewCounterpartiesAsync(CancellationToken cancellationToken)
		{
			if(!_settings.IsEnabled)
			{
				return 0;
			}

			if(cancellationToken.IsCancellationRequested)
			{
				return 0;
			}

			var batchSize = Math.Max(1, _settings.EnqueueBatchSize);

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Постановка контрагентов в очередь Mindbox"))
			{
				var counterparties = GetCounterparties(unitOfWork, batchSize);

				if(counterparties.Count == 0)
				{
					return 0;
				}

				var publishEndpoint = _outbox.GetPublishEndpoint(unitOfWork);

				foreach(var counterparty in counterparties)
				{
					var transactionId = counterparty.MindboxTransactionId;

					if(transactionId == Guid.Empty)
					{
						transactionId = Guid.NewGuid();
						counterparty.MindboxTransactionId = transactionId;
					}

					counterparty.MindboxStatus = MindboxCounterpartyOutboxStatus.Queued;

					unitOfWork.Save(counterparty);

					await publishEndpoint.Publish(
						new MindboxCounterpartySendMessage
						{
							CounterpartyId = counterparty.Id,
							TransactionId = transactionId
						},
						cancellationToken)
						.ConfigureAwait(false);
				}

				await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

				_logger.LogInformation(
					"Опубликовано в outbox Mindbox контрагентов: {Count}",
					counterparties.Count);

				return counterparties.Count;
			}
		}

		private static IList<Counterparty> GetCounterparties(
			IUnitOfWork unitOfWork,
			int batchSize)
		{
			const string sql = @"
SELECT c.*
FROM counterparty c
WHERE c.mindbox_status = 'Unknown'
ORDER BY c.id
LIMIT :batchSize";

			return unitOfWork.Session
				.CreateSQLQuery(sql)
				.AddEntity(typeof(Counterparty))
				.SetInt32("batchSize", batchSize)
				.List<Counterparty>();
		}
	}
}
