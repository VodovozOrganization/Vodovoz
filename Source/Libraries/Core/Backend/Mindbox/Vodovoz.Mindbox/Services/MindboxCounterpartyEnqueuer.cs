using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.Mindbox;
using Vodovoz.Settings.Database.Mindbox;

namespace Vodovoz.Mindbox.Services
{
	/// <inheritdoc />
	public sealed class MindboxCounterpartyEnqueuer : IMindboxCounterpartyEnqueuer
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxCounterpartyEnqueuer> _logger;

		public MindboxCounterpartyEnqueuer(
			IUnitOfWorkFactory unitOfWorkFactory,
			IMindboxSettings settings,
			ILogger<MindboxCounterpartyEnqueuer> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public Task<int> EnqueueNewCounterpartiesAsync(CancellationToken cancellationToken)
		{
			if(!_settings.IsEnabled)
			{
				return Task.FromResult(0);
			}

			return Task.Run(() => Enqueue(cancellationToken), cancellationToken);
		}

		private int Enqueue(CancellationToken cancellationToken)
		{
			if(cancellationToken.IsCancellationRequested)
			{
				return 0;
			}

			var lastCounterpartyId = _settings.LastEnqueuedCounterpartyId;

			if(lastCounterpartyId < 0)
			{
				InitializeLastCounterpartyId();
				return 0;
			}

			var batchSize = Math.Max(1, _settings.EnqueueBatchSize);

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Постановка контрагентов в очередь Mindbox"))
			{
				var counterparties = GetCounterparties(unitOfWork, lastCounterpartyId, batchSize);

				if(counterparties.Count == 0)
				{
					return 0;
				}

				var now = DateTime.UtcNow;
				var retryUntil = now.AddHours(Math.Max(1, _settings.RetryMaxHours));

				foreach(var counterparty in counterparties)
				{
					var message = new MindboxCounterpartyOutboxMessage
					{
						Counterparty = counterparty,
						TransactionId = Guid.NewGuid(),
						Status = MindboxCounterpartyOutboxStatus.Queued,
						CreatedAt = now,
						RetryUntil = retryUntil
					};

					unitOfWork.Save(message);
				}

				unitOfWork.Commit();

				lastCounterpartyId = counterparties.Max(counterparty => counterparty.Id);

				_settings.SetLastEnqueuedCounterpartyId(lastCounterpartyId);

				_logger.LogInformation(
					"В очередь Mindbox добавлено контрагентов: {Count}, последний Id: {LastCounterpartyId}",
					counterparties.Count,
					lastCounterpartyId);

				return counterparties.Count;
			}
		}

		private void InitializeLastCounterpartyId()
		{
			_settings.SetLastEnqueuedCounterpartyId(0);

			_logger.LogInformation(
				"Инициализирован указатель очереди Mindbox: 0. Все существующие контрагенты будут добавлены в очередь.");
		}

		private static System.Collections.Generic.IList<Counterparty> GetCounterparties(
			IUnitOfWork unitOfWork,
			int lastCounterpartyId,
			int batchSize)
		{
			const string sql = @"
SELECT c.*
FROM counterparty c
LEFT JOIN mindbox_counterparty_outbox o ON o.counterparty_id = c.id
WHERE c.id > :lastCounterpartyId
  AND o.id IS NULL
ORDER BY c.id
LIMIT :batchSize";

			return unitOfWork.Session
				.CreateSQLQuery(sql)
				.AddEntity(typeof(Counterparty))
				.SetInt32("lastCounterpartyId", lastCounterpartyId)
				.SetInt32("batchSize", batchSize)
				.List<Counterparty>();
		}
	}
}
