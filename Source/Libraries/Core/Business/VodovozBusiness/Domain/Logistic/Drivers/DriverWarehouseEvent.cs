using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Tools;

namespace Vodovoz.Domain.Logistic.Drivers
{
	[HistoryTrace]
	[EntityPermission]
	public class DriverWarehouseEvent : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private decimal? _latitude;
		private decimal? _longitude;
		private bool _isArchive;
		private DriverWarehouseEventName _eventName;
		private DriverWarehouseEventType _type;
		private DocumentType? _documentType;
		private int? _documentId;

		public virtual int Id { get; set; }

		[Display(Name = "Широта")]
		public virtual decimal? Latitude
		{
			get => _latitude;
			set => SetField(ref _latitude, value);
		}
		
		[Display(Name = "Долгота")]
		public virtual decimal? Longitude
		{
			get => _longitude;
			set => SetField(ref _longitude, value);
		}
		
		[Display(Name = "В архиве?")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Имя события")]
		public virtual DriverWarehouseEventName EventName
		{
			get => _eventName;
			set => SetField(ref _eventName, value);
		}
		
		[Display(Name = "Тип события")]
		public virtual DriverWarehouseEventType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}
		
		[Display(Name = "Документ на котором размещен Qr")]
		public virtual DocumentType? DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}
		
		[Display(Name = "Номер документа")]
		public virtual int? DocumentId
		{
			get => _documentId;
			set => SetField(ref _documentId, value);
		}

		public virtual void WriteCoordinates(decimal? latitude, decimal? longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(EventName is null)
			{
				yield return new ValidationResult(DriverWarehouseEventName.EventNameIsNull);
			}

			if(Type == DriverWarehouseEventType.OnLocation && (Latitude is null || Longitude is null))
			{
				yield return new ValidationResult("Не заполнены или неправильно заполнены координаты");
			}
		}
	}
}
