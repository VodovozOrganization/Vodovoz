using QS.DomainModel.UoW;
using System.Linq;
using NHibernate.Criterion;
using Vodovoz.Domain.Documents;

namespace Vodovoz.EntityRepositories.Store
{
    public class CarLoadDocumentRepository : ICarLoadDocumentRepository
    {
        public decimal LoadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId)
        {
            CarLoadDocument carLoadDocumentAlias = null;
            CarLoadDocumentItem carLoadDocumentItemAlias = null;

            var query = uow.Session.QueryOver(() => carLoadDocumentAlias)
                                    .JoinAlias(c => c.Items, () => carLoadDocumentItemAlias)
                                    .Where(() => carLoadDocumentAlias.RouteList.Id == routelistId)
                                    .And(() => carLoadDocumentItemAlias.Nomenclature.Id == terminalId)
                                    .Select(Projections.Sum(() => carLoadDocumentItemAlias.Amount))
                                    .SingleOrDefault<decimal>();

            return query;
        }
    }
}
