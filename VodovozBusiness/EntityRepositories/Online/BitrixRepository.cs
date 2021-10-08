using System.Collections.Generic;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories
{
	public class BitrixRepository : IBitrixRepository
	{
		public BitrixDealRegistration GetDealRegistration(IUnitOfWork uow, uint bitrixDealId)
		{
			return uow.Session.QueryOver<BitrixDealRegistration>()
				.Where(bdr => bdr.BitrixId == bitrixDealId)
				.SingleOrDefault();
		}

		public BitrixDealRegistration GetDealRegistrationByOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<BitrixDealRegistration>()
				.Where(bdr => bdr.Order.Id == orderId)
				.SingleOrDefault();
		}

		public IList<BitrixDealRegistration> GetDealRegistrationsToSync(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<BitrixDealRegistration>()
				.Where(bdr => bdr.NeedSync)
				.List();
		}
	}
}
