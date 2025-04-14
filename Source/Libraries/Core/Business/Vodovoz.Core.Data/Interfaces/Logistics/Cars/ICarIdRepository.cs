using QS.DomainModel.UoW;

namespace Vodovoz.Core.Data.Interfaces.Logistics.Cars
{
	public interface ICarIdRepository
	{
		int? GetCarIdByEmployeeId(IUnitOfWork uow, int employeeId);
	}
}
