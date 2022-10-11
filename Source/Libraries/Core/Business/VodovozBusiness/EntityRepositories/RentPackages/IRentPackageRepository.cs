using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories.RentPackages
{
	public interface IRentPackageRepository
	{
		FreeRentPackage GetFreeRentPackage(IUnitOfWork uow, EquipmentKind equipmentKind);
		PaidRentPackage GetPaidRentPackage(IUnitOfWork uow, EquipmentKind equipmentKind);
		List<EquipmentKind> GetPaidRentEquipmentKinds(IUnitOfWork uow);
	}
}