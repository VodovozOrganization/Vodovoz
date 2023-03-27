using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Сопоставления клиента с контактом",
		Nominative = "Сопоставление клиента с контактом",
		Prepositional = "Сопоставлении клиента с контактом",
		PrepositionalPlural = "Сопоставлениях клиента с контактом"
	)]
	public class ExternalCounterpartyMatching : PropertyChangedBase, IDomainObject
	{
		private ExternalCounterpartyMatchingStatus _status;
		private CounterpartyFrom _counterpartyFrom;
		private DateTime? _created;
		private ExternalCounterparty _assignedExternalCounterparty;
		private ExternalCounterparty _existingExternalCounterpartyWithSameParams;
		private string _phoneNumber;
		private Guid _externalCounterpartyId;

		public virtual int Id { get; set; }
		public virtual DateTime Version { get; set; }

		public virtual DateTime? Created
		{
			get => _created;
			set => SetField(ref _created, value);
		}

		[Display(Name = "Внешний номер клиента")]
		public virtual Guid ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
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
		
		public virtual ExternalCounterparty ExistingExternalCounterpartyWithSameParams
		{
			get => _existingExternalCounterpartyWithSameParams;
			set => SetField(ref _existingExternalCounterpartyWithSameParams, value);
		}

		public virtual void AssignCounterparty(ExternalCounterparty externalCounterparty)
		{
			AssignedExternalCounterparty = externalCounterparty;
			Status = ExternalCounterpartyMatchingStatus.Processed;
		}
	}

	public enum ExternalCounterpartyMatchingStatus
	{
		[Display(Name = "Ожидает обработки")]
		AwaitingProcessing,
		[Display(Name = "Обработан")]
		Processed
	}
}
