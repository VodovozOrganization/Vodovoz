using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repository
{
	public static class WarehouseRepository
	{
		public static IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow)
		{
			return uow.Session.CreateCriteria<Warehouse> ().List<Warehouse> ();
		}
	}
}

