using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Operations 
{
    public interface IEmployeeNomenclatureMovementRepository {
        int GetDriverTerminalBalance(IUnitOfWork uow, int driverId, int terminalId);
        IList<EmployeeBalanceNode> GetNomenclaturesFromDriverBalance(IUnitOfWork uow, int driverId);
        EmployeeBalanceNode GetTerminalFromDriverBalance(IUnitOfWork uow, int driverId, int terminalId);
    }
}