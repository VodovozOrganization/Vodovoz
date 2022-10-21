using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;

namespace Vodovoz.EntityRepositories.Documents
{
    public class SelfDeliveryCashDistributionDocumentRepository : ISelfDeliveryCashDistributionDocumentRepository
    {
        public SelfDeliveryCashDistributionDocument GetSelfDeliveryCashDistributionDocument(IUnitOfWork uow,
            int selfDeliveryOrderId)
        {
            return uow.Session.QueryOver<SelfDeliveryCashDistributionDocument>()
                .Where(x => x.Order.Id == selfDeliveryOrderId)
                .SingleOrDefault();
        }
    }
}