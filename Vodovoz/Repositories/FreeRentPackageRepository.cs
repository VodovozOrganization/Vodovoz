using System.Collections.Generic;
using Vodovoz.Domain;
using QSOrmProject;
using System.Linq;

namespace Vodovoz.Repository
{
	public static class FreeRentPackageRepository
	{
		public static List<EquipmentType> GetPresentEquipmentTypes (IUnitOfWork uow)
		{
			var availableTypes = uow.Session.CreateCriteria (typeof(FreeRentPackage))
				.List<FreeRentPackage> ()
				.Select (p => p.EquipmentType)
				.Distinct ().ToList ();
			return availableTypes;
		}
	}
}

