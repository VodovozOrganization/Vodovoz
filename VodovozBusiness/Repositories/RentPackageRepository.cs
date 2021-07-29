using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.Repositories
{
	public static class RentPackageRepository
	{
		public static FreeRentPackage GetFreeRentPackage (IUnitOfWork uow, EquipmentKind equipmentKind)
		{
			var package = uow.Session.QueryOver<FreeRentPackage>()
				.Where(p => p.EquipmentKind == equipmentKind)
				.SingleOrDefault();
			return package;
		}
		
		public static PaidRentPackage GetPaidRentPackage (IUnitOfWork uow, EquipmentKind equipmentKind)
		{
			var package = uow.Session.QueryOver<PaidRentPackage>()
				.Where(p => p.EquipmentKind == equipmentKind)
				.SingleOrDefault();
			return package;
		}

		public static List<EquipmentKind> GetPaidRentEquipmentKinds (IUnitOfWork uow)
		{
			var availableTypes = uow.Session.CreateCriteria (typeof(PaidRentPackage))
				.List<PaidRentPackage> ()
				.Select (p => p.EquipmentKind)
				.Distinct ().ToList ();
			return availableTypes;
		}
	}
}

