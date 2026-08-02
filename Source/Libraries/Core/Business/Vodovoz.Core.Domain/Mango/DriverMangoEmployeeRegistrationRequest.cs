using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Mango
{
	/// <summary>
	/// Заявка на регистрацию водителя как сотрудника Манго и выделение ему добавочного номера
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на регистрацию сотрудника Манго",
		NominativePlural = "заявки на регистрацию сотрудников Манго",
		Genitive = "заявки на регистрацию сотрудника Манго",
		GenitivePlural = "заявок на регистрацию сотрудников Манго",
		Accusative = "заявку на регистрацию сотрудника Манго",
		AccusativePlural = "заявки на регистрацию сотрудников Манго",
		Prepositional = "заявке на регистрацию сотрудника Манго",
		PrepositionalPlural = "заявках на регистрацию сотрудников Манго")]
	[HistoryTrace]
	public class DriverMangoEmployeeRegistrationRequest : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _driverId;
		private DriverMangoEmployeeRegistrationRequestStatus _status;
		private DateTime _createdAt;
		private DateTime? _processedAt;
		private string _errorMessage;

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
		/// Идентификатор водителя, для которого создаётся сотрудник Манго
		/// </summary>
		[Display(Name = "Идентификатор водителя")]
		public virtual int DriverId
		{
			get => _driverId;
			set => SetField(ref _driverId, value);
		}

		/// <summary>
		/// Статус обработки заявки
		/// </summary>
		[Display(Name = "Статус")]
		public virtual DriverMangoEmployeeRegistrationRequestStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		/// <summary>
		/// Дата создания заявки
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime CreatedAt
		{
			get => _createdAt;
			set => SetField(ref _createdAt, value);
		}

		/// <summary>
		/// Дата обработки заявки
		/// </summary>
		[Display(Name = "Дата обработки")]
		public virtual DateTime? ProcessedAt
		{
			get => _processedAt;
			set => SetField(ref _processedAt, value);
		}

		/// <summary>
		/// Сообщение об ошибке, если обработка заявки завершилась ошибкой
		/// </summary>
		[Display(Name = "Сообщение об ошибке")]
		public virtual string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}
	}
}
