using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.EntityRepositories.Documents;

namespace Vodovoz.Infrastructure.Persistance.Documents
{
	internal sealed class SelfDeliveryCashDistributionDocumentRepository : ISelfDeliveryCashDistributionDocumentRepository
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
