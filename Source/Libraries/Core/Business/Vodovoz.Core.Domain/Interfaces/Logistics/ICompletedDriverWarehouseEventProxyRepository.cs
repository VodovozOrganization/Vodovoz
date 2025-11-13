using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Logistics;

namespace Vodovoz.Core.Domain.Interfaces.Logistics
{
	public interface ICompletedDriverWarehouseEventProxyRepository
	{
		IEnumerable<CompletedEventDto> GetTodayCompletedEventsForEmployee(IUnitOfWork unitOfWork, int employeeId);
	}
}
