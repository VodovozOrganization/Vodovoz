using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class DeliveryShiftRepository : IDeliveryShiftRepository
	{
		public IList<DeliveryShift> ActiveShifts(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DeliveryShift>().List<DeliveryShift>();
		}
	}
}

