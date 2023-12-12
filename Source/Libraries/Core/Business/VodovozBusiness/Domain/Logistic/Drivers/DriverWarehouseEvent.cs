﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Domain.Logistic.Drivers
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "события нахождения водителя на складе",
		Nominative = "событие нахождения водителя на складе")]
	[HistoryTrace]
	[EntityPermission]
	public class DriverWarehouseEvent : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public static char QrParametersSeparator = ';';
		public static string QrType = "EventQr";
		public static string HasCompletedEvents = "HasCompletedEvents";
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
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.ServiceContainer.GetService(
					typeof(IDriverWarehouseEventRepository)) is IDriverWarehouseEventRepository driverWarehouseEventRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(driverWarehouseEventRepository) }");
			}
			
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
				
				if(DocumentType.HasValue
					&& QrPositionOnDocument.HasValue
					&& validationContext.Items.ContainsKey(HasCompletedEvents)
					&& !IsArchive
					&& !(bool)validationContext.Items[HasCompletedEvents])
				{
					using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
					{
						var hasEvents = driverWarehouseEventRepository.HasActiveDriverWarehouseEventsForDocumentAndQrPosition(
							uow, DocumentType.Value, QrPositionOnDocument.Value);

						if(hasEvents)
						{
							yield return new ValidationResult(
								"Нельзя создавать больше одного события, размещаемого на документе на одной и той же позиции");
						}
					}
				}
			}
		}
	}
}
