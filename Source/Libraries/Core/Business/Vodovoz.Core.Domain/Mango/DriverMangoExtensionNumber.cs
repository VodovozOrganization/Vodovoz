using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Mango
{
	/// <summary>
	/// Добавочный номер Манго для водителя
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "добавочный номер Манго для водителя",
		NominativePlural = "добавочные номера Манго для водителей",
		Genitive = "добавочного номера Манго для водителя",
		GenitivePlural = "добавочных номеров Манго для водителей",
		Accusative = "добавочный номер Манго для водителя",
		AccusativePlural = "добавочные номера Манго для водителей",
		Prepositional = "добавочном номере Манго для водителя",
		PrepositionalPlural = "добавочных номерах Манго для водителей")]
	[HistoryTrace]
	public class DriverMangoExtensionNumber : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _driverId;
		private uint? _extensionNumber;
		private DriversMangoExtensionNumbersStatus _status;
		private DateTime _activatedAt;
		private DateTime? _deactivatedAt;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Идентификатор водителя
		/// </summary>
		[Display(Name = "Идентификатор водителя")]
		public virtual int DriverId
		{
			get => _driverId;
			set => SetField(ref _driverId, value);
		}

		/// <summary>
		/// Добавочный номер
		/// </summary>
		[Display(Name = "Добавочный номер")]
		public virtual uint? ExtensionNumber
		{
			get => _extensionNumber;
			set => SetField(ref _extensionNumber, value);
		}

		/// <summary>
		/// Добавочный номер
		/// </summary>
		[Display(Name = "Добавочный номер")]
		public virtual DriversMangoExtensionNumbersStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		/// <summary>
		/// Время активации добавочного номера
		/// </summary>
		[Display(Name = "Время активации добавочного номера")]
		public virtual DateTime ActivatedAt
		{
			get => _activatedAt;
			set => SetField(ref _activatedAt, value);
		}

		/// <summary>
		/// Время деактивации добавочного номера
		/// </summary>
		[Display(Name = "Время деактивации добавочного номера")]
		public virtual DateTime? DeactivatedAt
		{
			get => _deactivatedAt;
			set => SetField(ref _deactivatedAt, value);
		}
	}

	/// <summary>
	/// Статус добавочного номера Манго для водителей
	/// </summary>
	public enum DriversMangoExtensionNumbersStatus
	{
		/// <summary>
		/// Активен
		/// </summary>
		[Display(Name = "Активен")]
		Active,

		/// <summary>
		/// Деактивирован
		/// </summary>
		[Display(Name = "Деактивирован")]
		Deactivated
	}
}
