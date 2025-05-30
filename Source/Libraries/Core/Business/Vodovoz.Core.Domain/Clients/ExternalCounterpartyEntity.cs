using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Contacts;

namespace Vodovoz.Core.Domain.Clients
{
	public class ExternalCounterpartyEntity : PropertyChangedBase, IDomainObject
	{
		public const string TableName = "external_counterparties";

		private Guid _externalCounterpartyId;
		private PhoneEntity _phone;
		private EmailEntity _email;
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
		public virtual PhoneEntity Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}

		[Display(Name = "Электронная почта")]
		public virtual EmailEntity Email
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
}
