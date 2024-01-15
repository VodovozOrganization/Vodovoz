using System.ComponentModel.DataAnnotations;
using System.Text;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Logistics.Drivers
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "события нахождения водителя на складе",
		Nominative = "событие нахождения водителя на складе")]
	[HistoryTrace]
	[EntityPermission]
	public class DriverWarehouseEvent : PropertyChangedBase, IDomainObject
	{
		public static char QrParametersSeparator = ';';
		public static string QrType = "EventQr";
		public const int EventNameMaxLength = 150;
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

		public virtual string GenerateQrData(int? documentId = null)
		{
			var sb = new StringBuilder();
			
			sb.Append(QrType);
			sb.Append(QrParametersSeparator);
			sb.Append($"{Id}");
			sb.Append(QrParametersSeparator);
			sb.Append($"{documentId}");
			sb.Append(QrParametersSeparator);
			sb.Append($"{Latitude}");
			sb.Append(QrParametersSeparator);
			sb.Append($"{Longitude}");

			return sb.ToString();
		}

		public override string ToString()
		{
			return EventName;
		}
	}
}
