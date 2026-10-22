using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.Mindbox;
using Vodovoz.Settings.Mindbox;

namespace VodovozBusiness.Services.Clients
{
	/// <summary>
	/// Ставит контрагента в очередь на отправку в Mindbox
	/// </summary>
	public interface IMindboxCounterpartyOutboxService
	{
		/// <summary>
		/// Признак включенной интеграции
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Добавляет сообщение в текущий UnitOfWork.
		/// Сообщение будет сохранено вместе с контрагентом при коммите.
		/// </summary>
		void Enqueue(IUnitOfWork unitOfWork, Counterparty counterparty);
	}

	/// <inheritdoc />
	public sealed class MindboxCounterpartyOutboxService : IMindboxCounterpartyOutboxService
	{
		private readonly IMindboxSettings _settings;

		public MindboxCounterpartyOutboxService(IMindboxSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		/// <inheritdoc />
		public bool IsEnabled => _settings.IsEnabled;

		/// <inheritdoc />
		public void Enqueue(IUnitOfWork unitOfWork, Counterparty counterparty)
		{
			if(unitOfWork == null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			if(!_settings.IsEnabled)
			{
				return;
			}

			if(counterparty.Id == 0)
			{
				throw new InvalidOperationException(
					"Контрагент должен быть сохранен до постановки в очередь Mindbox.");
			}

			var message = new MindboxCounterpartyOutboxMessage
			{
				Counterparty = counterparty,
				TransactionId = Guid.NewGuid(),
				Status = MindboxCounterpartyOutboxStatus.Pending,
				CreatedAt = DateTime.UtcNow
			};

			unitOfWork.Save(message);
		}
	}
}
