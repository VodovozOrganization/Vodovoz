using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	internal sealed class DeliveryShiftRepository : IDeliveryShiftRepository
	{
		public IList<DeliveryShift> ActiveShifts(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DeliveryShift>().List<DeliveryShift>();
		}
	}
}

