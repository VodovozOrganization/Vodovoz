using MassTransit;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
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

				var now = DateTime.UtcNow;
				var states = GetStates(unitOfWork, counterparties);
				var publishEndpoint = _outbox.GetPublishEndpoint(unitOfWork);

				foreach(var counterparty in counterparties)
				{
					var isNewState = !states.TryGetValue(counterparty.Id, out var state);

					if(isNewState)
					{
						state = new MindboxCounterpartyState
						{
							CounterpartyId = counterparty.Id,
							CreatedAt = now
						};

						states[counterparty.Id] = state;
					}

					if(!state.TransactionId.HasValue || state.TransactionId.Value == Guid.Empty)
					{
						state.TransactionId = Guid.NewGuid();
					}

					state.Status = MindboxCounterpartyOutboxStatus.Queued;
					state.UpdatedAt = now;

					if(isNewState)
					{
						// Id задаётся вручную (counterparty_id), поэтому нужен явный Insert.
						unitOfWork.Session.Save(state);
					}
					else
					{
						unitOfWork.Save(state);
					}

					await publishEndpoint.Publish(
						new MindboxCounterpartySendMessage
						{
							CounterpartyId = counterparty.Id,
							TransactionId = state.TransactionId.Value
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
			Counterparty counterpartyAlias = null;
			MindboxCounterpartyState stateAlias = null;

			var stateForCounterparty = QueryOver.Of<MindboxCounterpartyState>(() => stateAlias)
				.Where(state => state.CounterpartyId == counterpartyAlias.Id)
				.Select(state => state.CounterpartyId);

			var withoutState = unitOfWork.Session
				.QueryOver<Counterparty>(() => counterpartyAlias)
				.WithSubquery.WhereNotExists(stateForCounterparty)
				.OrderBy(counterparty => counterparty.Id).Asc
				.Take(batchSize)
				.List();

			Counterparty counterpartyWithUnknownStateAlias = null;
			MindboxCounterpartyState unknownStateAlias = null;

			var unknownStateForCounterparty = QueryOver.Of<MindboxCounterpartyState>(() => unknownStateAlias)
				.Where(state => state.CounterpartyId == counterpartyWithUnknownStateAlias.Id)
				.And(state => state.Status == MindboxCounterpartyOutboxStatus.Unknown)
				.Select(state => state.CounterpartyId);

			var withUnknownState = unitOfWork.Session
				.QueryOver<Counterparty>(() => counterpartyWithUnknownStateAlias)
				.WithSubquery.WhereExists(unknownStateForCounterparty)
				.OrderBy(counterparty => counterparty.Id).Asc
				.Take(batchSize)
				.List();

			return withoutState
				.Concat(withUnknownState)
				.OrderBy(counterparty => counterparty.Id)
				.Take(batchSize)
				.ToList();
		}

		private static IDictionary<int, MindboxCounterpartyState> GetStates(
			IUnitOfWork unitOfWork,
			IList<Counterparty> counterparties)
		{
			var counterpartyIds = counterparties
				.Select(counterparty => counterparty.Id)
				.Cast<object>()
				.ToArray();

			return unitOfWork.Session
				.QueryOver<MindboxCounterpartyState>()
				.WhereRestrictionOn(state => state.CounterpartyId)
				.IsIn(counterpartyIds)
				.List()
				.ToDictionary(state => state.CounterpartyId);
		}
	}
}
