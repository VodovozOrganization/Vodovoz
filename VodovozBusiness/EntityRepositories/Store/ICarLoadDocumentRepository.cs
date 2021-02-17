using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Store
{
    public interface ICarLoadDocumentRepository
    {
        decimal LoadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId);
    }
}