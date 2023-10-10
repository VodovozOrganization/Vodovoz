using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods.Rent;

namespace Vodovoz.EntityRepositories.Equipments
{
	public class EquipmentKindRepository : IEquipmentKindRepository
	{
		public List<EquipmentKind> GetPaidRentEquipmentKinds(IUnitOfWork uow)
		{
			var availableTypes = uow.Session.CreateCriteria(typeof(PaidRentPackage))
				.List<PaidRentPackage>()
				.Select(p => p.EquipmentKind)
				.Distinct().ToList();
			return availableTypes;
		}
	}
}

