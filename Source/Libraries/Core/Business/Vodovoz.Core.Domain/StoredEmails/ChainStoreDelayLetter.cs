using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.StoredEmails
{
	/// <summary>
	/// История писем по заказам сетей и тендерных клиентов
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Neuter,
		Nominative = "письмо по заказу сети или тендера",
		NominativePlural = "письма по заказам сетей и тендеров")]
	public class ChainStoreDelayLetter : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationDate;
		private DateTime? _lastUpdateDate;
		private ChainStoreDelayLetterStatus _status;
		private int _orderId;
		private int _counterpartyId;
		private DateTime _estimatedPaymentDate;
		private int? _counterpartyEmailId;
		private string _lastError;

		/// <summary>
		/// Идентификатор записи
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Дата и время отбора заказа
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		/// <summary>
		/// Дата последнего изменения записи
		/// </summary>
		[Display(Name = "Дата обновления")]
		public virtual DateTime? LastUpdateDate
		{
			get => _lastUpdateDate;
			set => SetField(ref _lastUpdateDate, value);
		}

		/// <summary>
		/// Состояние письма
		/// </summary>
		[Display(Name = "Состояние")]
		public virtual ChainStoreDelayLetterStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		/// <summary>
		/// Номер заказа
		/// </summary>
		[Display(Name = "Номер заказа")]
		public virtual int OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		/// <summary>
		/// Код клиента
		/// </summary>
		[Display(Name = "Код клиента")]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		/// <summary>
		/// Расчётная дата платежа на момент отбора
		/// </summary>
		[Display(Name = "Расчётная дата платежа")]
		public virtual DateTime EstimatedPaymentDate
		{
			get => _estimatedPaymentDate;
			set => SetField(ref _estimatedPaymentDate, value);
		}

		/// <summary>
		/// Код письма в истории писем клиента
		/// </summary>
		[Display(Name = "Код письма клиента")]
		public virtual int? CounterpartyEmailId
		{
			get => _counterpartyEmailId;
			set => SetField(ref _counterpartyEmailId, value);
		}

		/// <summary>
		/// Текст последней технической ошибки формирования письма
		/// </summary>
		[Display(Name = "Ошибка")]
		public virtual string LastError
		{
			get => _lastError;
			set => SetField(ref _lastError, value);
		}
	}
}
