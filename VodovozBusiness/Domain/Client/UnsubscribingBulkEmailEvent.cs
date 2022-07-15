using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public class UnsubscribingBulkEmailEvent : BulkEmailEvent, IValidatableObject
	{
		private UnsubscribingReason _unsubscribingReason;
		private string _otherReason;

		[Display(Name = "Причина отписки")]
		public virtual UnsubscribingReason UnsubscribingReason
		{
			get => _unsubscribingReason;
			set => SetField(ref _unsubscribingReason, value);
		}

		[Display(Name = "Текст другой причины")]
		public virtual string OtherReason
		{
			get => _otherReason;
			set => SetField(ref _otherReason, value);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(OtherReason))
			{
				yield return new ValidationResult("Причина должна быть заполнена.",
					new[] { nameof(ActionTime) });
			}

			if(OtherReason?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина причины ({OtherReason.Length}/255).",
					new[] { nameof(ActionTime) });
			}
		}

		#endregion
	}
}
