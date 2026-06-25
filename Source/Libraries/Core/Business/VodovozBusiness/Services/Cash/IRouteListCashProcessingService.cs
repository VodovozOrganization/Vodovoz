using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Logistic;

namespace VodovozBusiness.Services.Cash
{
	/// <summary>
	/// Сервис для обработки наличных платежей по маршуртному листу
	/// </summary>
	public interface IRouteListCashProcessingService
	{
		/// <summary>
		/// Создание приходных ордеров по вручную введенную сумму наличных платежей по маршуртному листу
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="routeList">Маршрутный лист</param>
		/// <param name="casheInput">Сумма наличных платежей</param>
		/// <returns></returns>
		Result<List<Income>> CreateManualCashIncome(IUnitOfWork uow, RouteList routeList, decimal casheInput);

		/// <summary>
		/// Пересчет суммы наличных платежей по маршуртному листу для выравнивания баланса
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="routeList">Маршрутный лист</param>
		/// <returns>Результат пересчета</returns>
		Result<IEnumerable<string>> RecalculateRouteListCahsBalance(IUnitOfWork uow, RouteList routeList);
	}
}
