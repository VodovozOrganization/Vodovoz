using System.Collections.Generic;
using CustomerAppsApi.Library.V1.Dto;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.V1.Repositories
{
	/// <summary>
	/// Контракт получения данных по пакетам аренды
	/// </summary>
	public interface ICustomerAppRentPackageRepository
	{
		/// <summary>
		/// Получение данных по выгружаемым пакетам бесплатной аренды
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <returns></returns>
		IEnumerable<FreeRentPackageDto> GetFreeRentPackagesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType);
	}
}
