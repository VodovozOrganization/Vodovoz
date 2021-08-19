using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IDeliveryShiftRepository
	{
		IList<DeliveryShift> ActiveShifts(IUnitOfWork uow);
	}
}