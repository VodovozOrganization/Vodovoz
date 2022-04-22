using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Services;

namespace Vodovoz.EntityRepositories.Delivery
{
	public interface IDeliveryRepository
	{
		/// <summary>
		/// Возвращает район для указанных координат, 
		/// если существует наложение районов, то возвращает первый попавшийся район 
		/// </summary>
		District GetDistrict(IUnitOfWork uow, decimal latitude, decimal longitude, DistrictsSet districtsSet = null);

		/// <summary>
		/// Возвращает список районов в границы которых входят указанные координаты
		/// </summary>
		IEnumerable<District> GetDistricts(IUnitOfWork uow, decimal latitude, decimal longitude, DistrictsSet districtsSet = null);

		#region MyRegion

		bool FastDeliveryAvailable(IUnitOfWork uow, double latitude, double longitude,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider, IList<NomenclatureAmountNode> nomenclatureNodes);

		RouteList GetRouteListForFastDelivery(IUnitOfWork uow, double latitude, double longitude, bool getClosestByRoute,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider, IEnumerable<NomenclatureAmountNode> nomenclatureNodes);

		IEnumerable<FastDeliveryVerificationDetailsNode> GetRouteListsForFastDelivery(
			IUnitOfWork uow, double latitude, double longitude, IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes);

		#endregion
	}
}
