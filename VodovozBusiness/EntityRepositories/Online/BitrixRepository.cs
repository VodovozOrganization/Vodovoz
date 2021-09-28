using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories
{
	public class BitrixRepository : IBitrixRepository
	{
		public BitrixDealRegistration GetDealRegistration(IUnitOfWork uow, uint bitrixDealId)
		{
			return uow.Session.QueryOver<BitrixDealRegistration>()
				.Where(dp => dp.BitrixId == bitrixDealId)
				.SingleOrDefault();
		}

		public BitrixDealRegistration GetDealRegistrationByOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<BitrixDealRegistration>()
				.Where(dp => dp.Order.Id == orderId)
				.SingleOrDefault();
		}
	}
}