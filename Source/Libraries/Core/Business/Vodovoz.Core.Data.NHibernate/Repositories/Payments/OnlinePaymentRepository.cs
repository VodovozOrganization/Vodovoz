using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Payments
{
	public class OnlinePaymentRepository : IOnlinePaymentRepository
	{
		public async Task<OnlinePayment> GetByExternalIdAsync(
			IUnitOfWork uow,
			int externalId,
			CancellationToken cancellationToken)
		{
			return await uow.Session.GetAsync<OnlinePayment>(externalId, cancellationToken);
		}
	}
}
