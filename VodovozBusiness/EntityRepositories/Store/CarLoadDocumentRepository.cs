using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Domain.Documents;

namespace Vodovoz.EntityRepositories.Store
{
    public class CarLoadDocumentRepository : ICarLoadDocumentRepository
    {
        public bool HasTerminalLoaded(IUnitOfWork uow, int routelistId, int terminalId)
        {
            CarLoadDocument carLoadDocumentAlias = null;
            CarLoadDocumentItem carLoadDocumentItemAlias = null;

            var query = uow.Session.QueryOver(() => carLoadDocumentAlias)
                                    .JoinAlias(c => c.Items, () => carLoadDocumentItemAlias)
                                    .Where(() => carLoadDocumentAlias.RouteList.Id == routelistId)
                                    .And(() => carLoadDocumentItemAlias.Id == terminalId)
                                    .List();

            return query.Any();
        }
    }
}
