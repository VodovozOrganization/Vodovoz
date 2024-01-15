using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.NHibernate.Logistics;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace Vodovoz.Core.Data.NHibernate.Validators
{
	public class DriverWarehouseEventValidator
	{
		private readonly IDriverWarehouseEventRepository _driverWarehouseEventRepository;

		public DriverWarehouseEventValidator(IDriverWarehouseEventRepository driverWarehouseEventRepository)
		{
			_driverWarehouseEventRepository =
				driverWarehouseEventRepository ?? throw new ArgumentNullException(nameof(driverWarehouseEventRepository));
		}

		public virtual IEnumerable<ValidationResult> Validate(DriverWarehouseEvent @event)
		{
			if(string.IsNullOrWhiteSpace(@event.EventName))
			{
				yield return new ValidationResult("Имя события должно быть заполнено");
			}
			else if(@event.EventName.Length > DriverWarehouseEvent.EventNameMaxLength)
			{
				yield return new ValidationResult(
					$"Длина названия события превышена на {DriverWarehouseEvent.EventNameMaxLength - @event.EventName.Length}");
			}

			if(@event.Type == DriverWarehouseEventType.OnLocation && (@event.Latitude is null || @event.Longitude is null))
			{
				yield return new ValidationResult("Не заполнены или неправильно заполнены координаты");
			}
			
			if(@event.Type == DriverWarehouseEventType.OnDocuments)
			{
				if(@event.DocumentType is null)
				{
					yield return new ValidationResult("Не заполнен документ, на котором будет размещен Qr код");
				}
				
				if(@event.QrPositionOnDocument is null)
				{
					yield return new ValidationResult("Не указано размещение Qr кода на документе");
				}
				
				if(@event.DocumentType.HasValue
					&& @event.QrPositionOnDocument.HasValue
					&& !@event.IsArchive)
				{
					using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
					{
						var hasEvents = _driverWarehouseEventRepository.HasOtherActiveDriverWarehouseEventsForDocumentAndQrPosition(
							uow, @event.Id, @event.DocumentType.Value, @event.QrPositionOnDocument.Value);

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
