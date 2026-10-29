using System.Collections.Generic;
using CustomerAppsApi.Library.V1.Dto.Goods;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.V1.Repositories
{
	/// <summary>
	/// Интерфейс работы с промонаборами
	/// </summary>
	public interface ICustomerAppPromotionalSetRepository
	{
		/// <summary>
		/// Получение параметров промонабора
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <returns></returns>
		IEnumerable<PromotionalSetOnlineParametersDto> GetActivePromotionalSetsOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType);
		/// <summary>
		/// Получение позиции промонабора с остатком на складе
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <param name="warehouses">Идентификаторы складов, по которым считается баланс</param>
		/// <returns></returns>
		IEnumerable<PromotionalSetItemBalanceDto> GetPromotionalSetsItemsWithBalanceForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouses);
	}
}
