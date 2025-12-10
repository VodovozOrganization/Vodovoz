using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Nodes;

namespace Vodovoz.EntityRepositories.RentPackages
{
	public interface IRentPackageRepository
	{
		FreeRentPackage GetFreeRentPackage(IUnitOfWork uow, EquipmentKind equipmentKind);
		IEnumerable<FreeRentPackageWithOnlineParametersNode> GetFreeRentPackagesForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType);
		PaidRentPackage GetPaidRentPackage(IUnitOfWork uow, EquipmentKind equipmentKind);
		List<EquipmentKind> GetPaidRentEquipmentKinds(IUnitOfWork uow);
	}
}
