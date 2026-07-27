using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.EntityRepositories.RentPackages;
using Vodovoz.Nodes;

namespace Vodovoz.Infrastructure.Persistance.RentPackages
{
	internal sealed class RentPackageRepository : IRentPackageRepository
	{
		public FreeRentPackage GetFreeRentPackage(IUnitOfWork uow, EquipmentKind equipmentKind)
		{
			var package = uow.Session.QueryOver<FreeRentPackage>()
				.Where(p => p.EquipmentKind == equipmentKind)
				.SingleOrDefault();

			return package;
		}

		public PaidRentPackage GetPaidRentPackage(IUnitOfWork uow, EquipmentKind equipmentKind)
		{
			var package = uow.Session.QueryOver<PaidRentPackage>()
				.Where(p => p.EquipmentKind == equipmentKind)
				.SingleOrDefault();

			return package;
		}

		public List<EquipmentKind> GetPaidRentEquipmentKinds(IUnitOfWork uow)
		{
			var availableTypes = uow.Session.CreateCriteria(typeof(PaidRentPackage))
				.List<PaidRentPackage>()
				.Select(p => p.EquipmentKind)
				.Distinct()
				.ToList();

			return availableTypes;
		}
	}
}

