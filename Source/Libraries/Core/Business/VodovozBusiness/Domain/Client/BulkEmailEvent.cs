using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "событие рассылки",
		NominativePlural = "события рассылки"
	)]
	[EntityPermission]
	public abstract class BulkEmailEvent : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _actionTime;
		private BulkEmailEventType _eventType;
		private Counterparty _counterparty;
		private BulkEmailEventReason _reason;
		private string _reasonDetail;
		private CounterpartyEmailType? _counterpartyEmailType;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Название ")]
		public virtual DateTime ActionTime
		{
			get => _actionTime;
			set => SetField(ref _actionTime, value);
		}

		[Display(Name = "Тип события")]
		public virtual BulkEmailEventType EventType
		{
			get => _eventType;
			set => SetField(ref _eventType, value);
		}
		
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}		

		[Display(Name = "Причина отписки")]
		public virtual BulkEmailEventReason Reason
		{
			get => _reason;
			set => SetField(ref _reason, value);
		}

		[Display(Name = "Детализация причины")]
		public virtual string ReasonDetail
		{
			get => _reasonDetail;
			set => SetField(ref _reasonDetail, value);
		}

		[Display(Name = "Тип письма для клиента")]
		public virtual CounterpartyEmailType? CounterpartyEmailType
		{
			get => _counterpartyEmailType;
			set => SetField(ref _counterpartyEmailType, value);
		}
		
		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(ReasonDetail))
			{
				yield return new ValidationResult("Причина должна быть заполнена.",
					new[] { nameof(ActionTime) });
			}

			if(ReasonDetail?.Length > 500)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина причины ({ReasonDetail.Length}/500).",
					new[] { nameof(ActionTime) });
			}
		}

		#endregion
	}
}
