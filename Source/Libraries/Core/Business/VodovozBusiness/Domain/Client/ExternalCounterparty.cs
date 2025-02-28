using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Пользователь ИПЗ(сайт, МП)
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Пользователи ИПЗ",
		Nominative = "Пользователь ИПЗ")]
	[HistoryTrace]
	public class ExternalCounterparty : PropertyChangedBase, IDomainObject
	{
		private Guid _externalCounterpartyId;
		private Phone _phone;
		private Email _email;
		private bool _isArchive;
		private DateTime? _creationDate;

		public virtual int Id { get; set; }

		/// <summary>
		/// Код пользователя в ИПЗ
		/// </summary>
		[Display(Name = "Код пользователя в ИПЗ")]
		public virtual Guid ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}
		
		/// <summary>
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		/// <summary>
		/// Телефон, по которому зарегистрирован пользователь
		/// </summary>
		[Display(Name = "Телефон, по которому зарегистрирован пользователь")]
		public virtual Phone Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}

		/// <summary>
		/// Электронная почта
		/// </summary>
		[Display(Name = "Электронная почта")]
		public virtual Email Email
		{
			get => _email;
			set => SetField(ref _email, value);
		}

		/// <summary>
		/// Архивный
		/// </summary>
		[Display(Name = "В архиве?")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// Откуда пользователь
		/// </summary>
		public virtual CounterpartyFrom CounterpartyFrom { get; }
	}
}
