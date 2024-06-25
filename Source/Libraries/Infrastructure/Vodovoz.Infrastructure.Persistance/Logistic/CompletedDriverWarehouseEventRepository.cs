using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Logistics.Drivers;
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
	}
}
