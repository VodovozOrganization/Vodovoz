using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Logistics
{
	public static class DeliveryShiftRepository
	{
		public static IList<DeliveryShift> ActiveShifts (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DeliveryShift> ().List<DeliveryShift> ();
		}
	}
}

