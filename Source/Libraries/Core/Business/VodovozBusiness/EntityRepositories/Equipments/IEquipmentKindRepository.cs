using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.BasicHandbooks;

namespace Vodovoz.EntityRepositories.Equipments
{
	public interface IEquipmentKindRepository
	{
		List<EquipmentKind> GetPaidRentEquipmentKinds(IUnitOfWork uow);
	}
}