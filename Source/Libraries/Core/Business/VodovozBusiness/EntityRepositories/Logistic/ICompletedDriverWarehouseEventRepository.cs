using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface ICompletedDriverWarehouseEventRepository
	{
		bool HasCompletedEventsByEventId(IUnitOfWork uow, int eventId);
		IEnumerable<CompletedEventDto> GetTodayCompletedEventsForEmployee(IUnitOfWork unitOfWork, int driverId);
	}
}
