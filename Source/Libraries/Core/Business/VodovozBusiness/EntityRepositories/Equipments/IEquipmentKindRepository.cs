using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories.Equipments
{
	public interface IEquipmentKindRepository
	{
		List<EquipmentKind> GetPaidRentEquipmentKinds(IUnitOfWork uow);
	}
}