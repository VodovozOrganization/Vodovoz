using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;

namespace Vodovoz.EntityRepositories.Documents
{
    public interface ISelfDeliveryCashDistributionDocumentRepository
    {
        SelfDeliveryCashDistributionDocument GetSelfDeliveryCashDistributionDocument(IUnitOfWork uow, int selfDeliveryOrderId);
    }
}