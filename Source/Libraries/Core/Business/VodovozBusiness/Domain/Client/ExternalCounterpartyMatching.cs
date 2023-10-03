using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Сопоставления клиента из внешнего источника",
		Nominative = "Сопоставление клиента из внешнего источника",
		Prepositional = "Сопоставлении клиента из внешнего источника",
		PrepositionalPlural = "Сопоставлениях клиента из внешнего источника"
	)]
	[HistoryTrace]
	public class ExternalCounterpartyMatching : PropertyChangedBase, IDomainObject
	{
		private ExternalCounterpartyMatchingStatus _status;
		private CounterpartyFrom _counterpartyFrom;
		private DateTime? _created;
		private ExternalCounterparty _assignedExternalCounterparty;
		private string _phoneNumber;
		private Guid _externalCounterpartyGuid;

		public virtual int Id { get; set; }
		public virtual DateTime Version { get; set; }

		public virtual DateTime? Created
		{
			get => _created;
			set => SetField(ref _created, value);
		}

		[Display(Name = "Внешний номер клиента")]
		public virtual Guid ExternalCounterpartyGuid
		{
			get => _externalCounterpartyGuid;
			set => SetField(ref _externalCounterpartyGuid, value);
		}

		public virtual ExternalCounterpartyMatchingStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Телефон клиента")]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		public virtual CounterpartyFrom CounterpartyFrom
		{
			get => _counterpartyFrom;
			set => SetField(ref _counterpartyFrom, value);
		}

		public virtual ExternalCounterparty AssignedExternalCounterparty
		{
			get => _assignedExternalCounterparty;
			set => SetField(ref _assignedExternalCounterparty, value);
		}

		public virtual void AssignCounterparty(ExternalCounterparty externalCounterparty)
		{
			AssignedExternalCounterparty = externalCounterparty;
			Status = ExternalCounterpartyMatchingStatus.Processed;
		}

		public virtual void SetLegalCounterpartyStatus()
		{
			Status = ExternalCounterpartyMatchingStatus.LegalCounterparty;
		}
	}
}
