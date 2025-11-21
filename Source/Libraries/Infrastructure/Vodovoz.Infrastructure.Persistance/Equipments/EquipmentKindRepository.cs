using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.EntityRepositories.Equipments;

namespace Vodovoz.Infrastructure.Persistance.Equipments
{
	internal sealed class EquipmentKindRepository : IEquipmentKindRepository
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
