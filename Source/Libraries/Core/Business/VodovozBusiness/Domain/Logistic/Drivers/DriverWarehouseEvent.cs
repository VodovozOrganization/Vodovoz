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
		private const int _eventNameMaxLength = 150;
		private decimal? _latitude;
		private decimal? _longitude;
		private bool _isArchive;
		private string _eventName;
		private DriverWarehouseEventType _type;
		private EventQrDocumentType? _documentType;
		private EventQrPositionOnDocument? _qrPositionOnDocument;

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
		
		[Display(Name = "Название события")]
		public virtual string EventName
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
		public virtual EventQrDocumentType? DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		[Display(Name = "Расположение Qr на документе")]
		public virtual EventQrPositionOnDocument? QrPositionOnDocument
		{
			get => _qrPositionOnDocument;
			set => SetField(ref _qrPositionOnDocument, value);
		}

		public virtual void WriteCoordinates(decimal? latitude, decimal? longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}
		
		public virtual void ResetEventParameters()
		{
			switch(Type)
			{
				case DriverWarehouseEventType.OnDocuments:
					Latitude = null;
					Longitude = null;
					break;
				case DriverWarehouseEventType.OnLocation:
					DocumentType = null;
					QrPositionOnDocument = null;
					break;
			}
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(EventName))
			{
				yield return new ValidationResult("Имя события должно быть заполнено");
			}
			else if(EventName.Length > _eventNameMaxLength)
			{
				yield return new ValidationResult($"Длина названия события превышена на {_eventNameMaxLength - EventName.Length}");
			}

			if(Type == DriverWarehouseEventType.OnLocation && (Latitude is null || Longitude is null))
			{
				yield return new ValidationResult("Не заполнены или неправильно заполнены координаты");
			}
			
			if(Type == DriverWarehouseEventType.OnDocuments)
			{
				if(DocumentType is null)
				{
					yield return new ValidationResult("Не заполнен документ, на котором будет размещен Qr код");
				}
				
				if(QrPositionOnDocument is null)
				{
					yield return new ValidationResult("Не указано размещение Qr кода на документе");
				}
			}
		}
	}
}
