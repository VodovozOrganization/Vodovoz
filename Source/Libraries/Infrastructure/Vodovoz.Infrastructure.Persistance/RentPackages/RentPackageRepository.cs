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

		public IEnumerable<FreeRentPackageWithOnlineParametersNode> GetFreeRentPackagesForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			FreeRentPackageOnlineParameters onlineParametersAlias = null;
			FreeRentPackageWithOnlineParametersNode resultAlias = null;
			Nomenclature depositServiceAlias = null;

			var query = uow.Session.QueryOver<FreeRentPackage>()
				.JoinAlias(fp => fp.OnlineParameters, () => onlineParametersAlias)
				.JoinAlias(fp => fp.DepositService, () => depositServiceAlias)
				.Where(() => onlineParametersAlias.PackageOnlineAvailability != null)
				.And(() => onlineParametersAlias.Type == parameterType)
				.SelectList(list => list
					.Select(fp => fp.Id).WithAlias(() => resultAlias.Id)
					.Select(fp => fp.OnlineName).WithAlias(() => resultAlias.OnlineName)
					.Select(fp => fp.MinWaterAmount).WithAlias(() => resultAlias.MinWaterAmount)
					.Select(fp => fp.Deposit).WithAlias(() => resultAlias.Deposit)
					.Select(() => depositServiceAlias.Id).WithAlias(() => resultAlias.DepositServiceId)
					.Select(() => onlineParametersAlias.PackageOnlineAvailability).WithAlias(() => resultAlias.OnlineAvailability))
				.TransformUsing(Transformers.AliasToBean<FreeRentPackageWithOnlineParametersNode>());

			return query.List<FreeRentPackageWithOnlineParametersNode>();
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

