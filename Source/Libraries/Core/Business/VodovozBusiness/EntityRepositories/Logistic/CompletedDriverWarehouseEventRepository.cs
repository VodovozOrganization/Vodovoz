using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class CompletedDriverWarehouseEventRepository : ICompletedDriverWarehouseEventRepository
	{
		public bool HasCompletedEventsByEventId(IUnitOfWork uow, int eventId)
		{
			return uow.Session.QueryOver<CompletedDriverWarehouseEvent>()
				.Where(x => x.DriverWarehouseEvent.Id == eventId)
				.RowCount() > 0;
		}
	}
}
