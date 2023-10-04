using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Logistic.Drivers
{
	[HistoryTrace]
	[EntityPermission]
	public class DriverWarehouseEvent : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private decimal? _latitude;
		private decimal? _longitude;
		private DriverWarehouseEventName _eventName;

		public virtual int Id { get; set; }

		public virtual decimal? Latitude
		{
			get => _latitude;
			set => SetField(ref _latitude, value);
		}
		
		public virtual decimal? Longitude
		{
			get => _longitude;
			set => SetField(ref _longitude, value);
		}
		
		public virtual DriverWarehouseEventName EventName
		{
			get => _eventName;
			set => SetField(ref _eventName, value);
		}
		
		public virtual DriverWarehouseEventType Type { get; set; }
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(EventName is null)
			{
				yield return new ValidationResult(DriverWarehouseEventName.EventNameIsNull);
			}
		}
	}
}
