using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.V1.Dto;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;

namespace CustomerAppsApi.Library.V1.Repositories
{
	/// <summary>
	/// Репозиторий получения данных по пакетам аренды
	/// </summary>
	public class CustomerAppRentPackageRepository : ICustomerAppRentPackageRepository
	{
		/// <inheritdoc/>
		public IEnumerable<FreeRentPackageDto> GetFreeRentPackagesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			var query =
				from rentPackage in uow.Session.Query<FreeRentPackage>()
				join onlineParameters in uow.Session.Query<FreeRentPackageOnlineParameters>()
					on rentPackage.Id equals onlineParameters.FreeRentPackage.Id
				join depositNomenclature in uow.Session.Query<Nomenclature>()
					on rentPackage.DepositService.Id equals depositNomenclature.Id
				where onlineParameters.PackageOnlineAvailability != null
					&& onlineParameters.Type == parameterType
				select new FreeRentPackageDto
				{
					ErpId = rentPackage.Id,
					OnlineName = rentPackage.OnlineName,
					MinWaterAmount = rentPackage.MinWaterAmount,
					Deposit = rentPackage.Deposit,
					DepositServiceId = depositNomenclature.Id,
					OnlineAvailability = onlineParameters.PackageOnlineAvailability
				};

			return query.ToList();
		}
	}
}
