using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Operations
{
	public interface IWagesMovementRepository
	{
		decimal GetCurrentEmployeeWageBalance(IUnitOfWork uow, int employeeId);
	}
}