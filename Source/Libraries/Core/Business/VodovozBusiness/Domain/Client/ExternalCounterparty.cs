using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Client
{
	public class ExternalCounterparty : PropertyChangedBase, IDomainObject
	{
		private Guid _externalCounterpartyId;
		private Phone _phone;
		private Email _email;
		private bool _isArchive;
		private DateTime? _creationDate;

		public virtual int Id { get; set; }

		[Display(Name = "Внешний код клиента")]
		public virtual Guid ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}
		
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Телефон клиента")]
		public virtual Phone Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}

		[Display(Name = "Электронная почта")]
		public virtual Email Email
		{
			get => _email;
			set => SetField(ref _email, value);
		}

		[Display(Name = "В архиве?")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual CounterpartyFrom CounterpartyFrom { get; }
	}

	public enum CounterpartyFrom
	{
		[Display(Name = "Мобильное приложение")]
		MobileApp = 54,
		[Display(Name = "Сайт")]
		WebSite = 55
	}
}
