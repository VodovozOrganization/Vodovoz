using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Logistic;

namespace VodovozBusiness.Services.Cash
{
	/// <summary>
	/// Сервис для распределения наличных платежей по маршуртному листу
	/// </summary>
	public interface IRouteListCashDistributionService
	{
		/// <summary>
		/// Распределяются наличные платежи по организациям в зависимости от того, заказы по каким организациям есть в маршрутном листе
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="routeList">Маршрутный лист</param>
		/// <param name="casheInput">Сумма наличных платежей</param>
		/// <returns></returns>
		Result<List<Income>> ManualCashIncomeDistribution(IUnitOfWork uow, RouteList routeList, decimal casheInput);
	}
}
