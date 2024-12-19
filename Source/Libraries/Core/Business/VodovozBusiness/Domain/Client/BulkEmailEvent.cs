using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

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
		private BulkEmailEventType _type;
		private Counterparty _counterparty;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Название ")]
		public virtual DateTime ActionTime
		{
			get => _actionTime;
			set => SetField(ref _actionTime, value);
		}

		[Display(Name = "Тип события")]
		public virtual BulkEmailEventType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}
		
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		private BulkEmailEventReason _reason;
		private string _reasonDetail;

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

		public enum BulkEmailEventType
		{
			[Display(Name = "Подписка")]
			Subscribing,
			[Display(Name = "Отписка")]
			Unsubscribing
		}
	}
}
