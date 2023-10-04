using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Logistic.Drivers
{
	[HistoryTrace]
	[EntityPermission]
	public class DriverWarehouseEventName : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public const string EventNameIsNull = "Не заполнено имя события";
		private string _eventName;

		public virtual int Id { get; set; }

		public virtual string Name
		{
			get => _eventName;
			set => SetField(ref _eventName, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult(EventNameIsNull);
			}
		}
	}
}
