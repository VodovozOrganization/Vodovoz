using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Client
{
	[HistoryTrace]
	public class ExternalCounterparty : PropertyChangedBase, IDomainObject
	{
		private Guid _externalCounterpartyId;
		private Phone _phone;
		private Email _email;
		private bool _isArchive;
		private DateTime? _creationDate;
		private IList<int> _externalCounterpartyMatchingIds = new List<int>();
		private IList<int> _externalCounterpartyAssignNotificationsIds = new List<int>();
		public const string TableName = "external_counterparties";
		public const string IdColumn = "id";
		public const string PhoneIdColumn = "phone_id";

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
		
		[Display(Name = "Коды уведомлений о присвоении клиента внешнему пользователю")]
		public virtual IList<int> ExternalCounterpartyAssignNotificationsIds
		{
			get => _externalCounterpartyAssignNotificationsIds;
			set => SetField(ref _externalCounterpartyAssignNotificationsIds, value);
		}
		
		[Display(Name = "Коды заявок на сопоставление внешних пользователей")]
		public virtual IList<int> ExternalCounterpartyMatchingIds
		{
			get => _externalCounterpartyMatchingIds;
			set => SetField(ref _externalCounterpartyMatchingIds, value);
		}

		[Display(Name = "Откуда клиент")]
		public virtual CounterpartyFrom CounterpartyFrom { get; }
	}
}
