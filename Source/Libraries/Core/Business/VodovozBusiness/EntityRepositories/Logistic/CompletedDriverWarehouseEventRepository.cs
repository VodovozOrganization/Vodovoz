using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class CompletedDriverWarehouseEventRepository : ICompletedDriverWarehouseEventRepository
	{
		public bool HasCompletedEventsByEventId(IUnitOfWork uow, int eventId)
		{
			var query = from completedEvents in uow.Session.Query<CompletedDriverWarehouseEvent>()
				join @event in uow.Session.Query<DriverWarehouseEvent>()
					on completedEvents.DriverWarehouseEvent.Id equals @event.Id
				where @event.Id == eventId
				select completedEvents.Id;

			return query.Any();
		}

		public IEnumerable<CompletedEventDto> GetTodayCompletedEventsForEmployee(IUnitOfWork unitOfWork, int driverId)
		{
			var query = from completedEvent in unitOfWork.Session.Query<CompletedDriverWarehouseEvent>()
				join @event in unitOfWork.Session.Query<DriverWarehouseEvent>()
					on completedEvent.DriverWarehouseEvent.Id equals @event.Id
				where completedEvent.Employee.Id == driverId
					&& completedEvent.CompletedDate.Date == DateTime.Today
				select new CompletedEventDto
				{
					EventName = @event.EventName,
					CompletedDate = completedEvent.CompletedDate
				};

			return query.ToList();
		}
	}
}
