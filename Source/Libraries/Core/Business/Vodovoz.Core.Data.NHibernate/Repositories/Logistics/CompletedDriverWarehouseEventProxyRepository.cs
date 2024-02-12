using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Logistics;
using Vodovoz.Core.Domain.Interfaces.Logistics;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Logistics
{
	public class CompletedDriverWarehouseEventProxyRepository : ICompletedDriverWarehouseEventProxyRepository
	{
		public IEnumerable<CompletedEventDto> GetTodayCompletedEventsForEmployee(IUnitOfWork unitOfWork, int employeeId)
		{
			var query = from completedEvent in unitOfWork.Session.Query<CompletedDriverWarehouseEventProxy>()
				join @event in unitOfWork.Session.Query<DriverWarehouseEvent>()
					on completedEvent.DriverWarehouseEvent.Id equals @event.Id
				where completedEvent.Employee.Id == employeeId
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
