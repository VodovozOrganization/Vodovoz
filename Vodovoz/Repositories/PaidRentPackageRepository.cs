using Vodovoz.Domain;
using QSOrmProject;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.Repository
{
	public static class PaidRentPackageRepository
	{
		public static List<EquipmentType> GetPresentEquipmentTypes (IUnitOfWork uow)
		{
			var availableTypes = uow.Session.CreateCriteria (typeof(PaidRentPackage))
				.List<PaidRentPackage> ()
				.Select (p => p.EquipmentType)
				.Distinct ().ToList ();
			return availableTypes;
		}
	}
}

