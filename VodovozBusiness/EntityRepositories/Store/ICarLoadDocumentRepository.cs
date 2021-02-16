using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Store
{
    public interface ICarLoadDocumentRepository
    {
        bool HasTerminalLoaded(IUnitOfWork uow, int routelistId, int terminalId);
    }
}