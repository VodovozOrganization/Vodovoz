using System;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Clients.Mindbox;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Состояние интеграции онлайн-заказа с Mindbox
	/// </summary>
	public class OnlineOrderMindboxState : PropertyChangedBase, IDomainObject
	{
		private long? _mindboxOrderId;
		private Guid? _mindboxTransactionId;
		private int _bonusesWrittenOff;
		private int? _bonusesToEarn;
		private MindboxCounterpartyOutboxStatus _mindboxStatus = MindboxCounterpartyOutboxStatus.Unknown;
		private string _mindboxLastError;
		private DateTime? _mindboxLastAttemptAt;
		private int _mindboxAttempts;
		private int? _mindboxLastHttpStatusCode;
		private string _mindboxLastStatus;
		private string _mindboxLastErrorId;
		private string _mindboxSnapshot;
		private DateTime? _mindboxSnapshotUpdatedAt;
		private DateTime? _mindboxLastChangeAt;
		private DateTime? _mindboxSyncRequestedAt;
		private bool _bonusesAvailable;

		/// <summary>
		/// Идентификатор онлайн-заказа (он же первичный ключ)
		/// </summary>
		public virtual int OnlineOrderId { get; set; }

		int IDomainObject.Id => OnlineOrderId;

		/// <summary>
		/// Онлайн-заказ
		/// </summary>
		public virtual OnlineOrder OnlineOrder { get; set; }

		/// <summary>
		/// ID заказа в Mindbox
		/// </summary>
		public virtual long? MindboxOrderId
		{
			get => _mindboxOrderId;
			set => SetField(ref _mindboxOrderId, value);
		}

		/// <summary>
		/// Ключ идемпотентности Mindbox
		/// </summary>
		public virtual Guid? MindboxTransactionId
		{
			get => _mindboxTransactionId;
			set => SetField(ref _mindboxTransactionId, value);
		}

		/// <summary>
		/// Списано бонусов
		/// </summary>
		public virtual int BonusesWrittenOff
		{
			get => _bonusesWrittenOff;
			set => SetField(ref _bonusesWrittenOff, value);
		}

		/// <summary>
		/// Бонусов к начислению
		/// </summary>
		public virtual int? BonusesToEarn
		{
			get => _bonusesToEarn;
			set => SetField(ref _bonusesToEarn, value);
		}

		/// <summary>
		/// Статус отправки в Mindbox
		/// </summary>
		public virtual MindboxCounterpartyOutboxStatus MindboxStatus
		{
			get => _mindboxStatus;
			set => SetField(ref _mindboxStatus, value);
		}

		/// <summary>
		/// Последняя ошибка Mindbox
		/// </summary>
		public virtual string MindboxLastError
		{
			get => _mindboxLastError;
			set => SetField(ref _mindboxLastError, value);
		}

		/// <summary>
		/// Дата последней попытки Mindbox
		/// </summary>
		public virtual DateTime? MindboxLastAttemptAt
		{
			get => _mindboxLastAttemptAt;
			set => SetField(ref _mindboxLastAttemptAt, value);
		}

		/// <summary>
		/// Количество попыток Mindbox
		/// </summary>
		public virtual int MindboxAttempts
		{
			get => _mindboxAttempts;
			set => SetField(ref _mindboxAttempts, value);
		}

		/// <summary>
		/// HTTP-код последнего ответа Mindbox
		/// </summary>
		public virtual int? MindboxLastHttpStatusCode
		{
			get => _mindboxLastHttpStatusCode;
			set => SetField(ref _mindboxLastHttpStatusCode, value);
		}

		/// <summary>
		/// Статус последнего ответа Mindbox
		/// </summary>
		public virtual string MindboxLastStatus
		{
			get => _mindboxLastStatus;
			set => SetField(ref _mindboxLastStatus, value);
		}

		/// <summary>
		/// ID последней ошибки Mindbox
		/// </summary>
		public virtual string MindboxLastErrorId
		{
			get => _mindboxLastErrorId;
			set => SetField(ref _mindboxLastErrorId, value);
		}

		/// <summary>
		/// Снимок состава и статусов Mindbox
		/// </summary>
		public virtual string MindboxSnapshot
		{
			get => _mindboxSnapshot;
			set => SetField(ref _mindboxSnapshot, value);
		}

		/// <summary>
		/// Дата обновления снимка Mindbox
		/// </summary>
		public virtual DateTime? MindboxSnapshotUpdatedAt
		{
			get => _mindboxSnapshotUpdatedAt;
			set => SetField(ref _mindboxSnapshotUpdatedAt, value);
		}

		/// <summary>
		/// Дата последнего изменения для Mindbox
		/// </summary>
		public virtual DateTime? MindboxLastChangeAt
		{
			get => _mindboxLastChangeAt;
			set => SetField(ref _mindboxLastChangeAt, value);
		}

		/// <summary>
		/// Дата постановки в очередь Mindbox
		/// </summary>
		public virtual DateTime? MindboxSyncRequestedAt
		{
			get => _mindboxSyncRequestedAt;
			set => SetField(ref _mindboxSyncRequestedAt, value);
		}

		/// <summary>
		/// Бонусы доступны клиенту
		/// </summary>
		public virtual bool BonusesAvailable
		{
			get => _bonusesAvailable;
			set => SetField(ref _bonusesAvailable, value);
		}
	}
}
