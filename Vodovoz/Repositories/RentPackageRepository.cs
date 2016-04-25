using System.Collections.Generic;
using Vodovoz.Domain;
using QSOrmProject;
using System.Linq;

namespace Vodovoz.Repository
{
	public static class RentPackageRepository
	{
		public static FreeRentPackage GetFreeRentPackage (IUnitOfWork uow, EquipmentType equipmentType)
		{
			var package = uow.Session.QueryOver<FreeRentPackage>()
				.Where(p => p.EquipmentType == equipmentType)
				.SingleOrDefault();
			return package;
		}

		public static PaidRentPackage GetPaidRentPackage (IUnitOfWork uow, EquipmentType equipmentType)
		{
			var package = uow.Session.QueryOver<PaidRentPackage>()
				.Where(p => p.EquipmentType == equipmentType)
				.SingleOrDefault();
			return package;
		}

		public static List<EquipmentType> GetPaidRentEquipmentTypes (IUnitOfWork uow)
		{
			var availableTypes = uow.Session.CreateCriteria (typeof(PaidRentPackage))
				.List<PaidRentPackage> ()
				.Select (p => p.EquipmentType)
				.Distinct ().ToList ();
			return availableTypes;
		}
	}
}

