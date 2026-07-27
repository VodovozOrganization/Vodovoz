using System.Collections.Generic;
using CustomerAppsApi.Library.V1.Dto.Goods;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.V1.Repositories
{
	/// <summary>
	/// Контракт репозитория продаваемых позиций
	/// </summary>
	public interface ISaleItemRepository
	{
		/// <summary>
		/// Получение данных по выгружаемым номенклатурам
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <returns></returns>
		IEnumerable<OnlineNomenclatureDto> GetNomenclaturesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType);
		/// <summary>
		/// Получение параметров выгружаемых номенклатур
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <returns></returns>
		IEnumerable<NomenclatureOnlineParametersDto> GetActiveNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType);
		/// <summary>
		/// Онлайн цены выгружаемых номенклатур
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineParametersIds">Идентификаторы параметров</param>
		/// <returns></returns>
		IEnumerable<NomenclatureOnlinePriceDto> GetNomenclaturesOnlinePricesByOnlineParameters(
			IUnitOfWork uow, IEnumerable<int> onlineParametersIds);
	}
}
