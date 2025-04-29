using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Common;
using VodovozBusiness.Domain.Service;

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

		District GetAccurateDistrict(IUnitOfWork uow, decimal latitude, decimal longitude);

		FastDeliveryAvailabilityHistory GetRouteListsForFastDelivery(
			IUnitOfWork uow,
			double latitude,
			double longitude,
			bool isGetClosestByRoute,
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes,
			int? tariffZoneId,
			bool isRequestFromDesktopApp = true,
			Order fastDeliveryOrder = null);

		void UpdateFastDeliveryMaxDistanceParameter(double value);
		double GetMaxDistanceToLatestTrackPointKmFor(DateTime dateTime);
		double MaxDistanceToLatestTrackPointKm { get; }

		IList<Order> GetFastDeliveryLateOrders(IUnitOfWork uow, DateTime fromDateTime, IGeneralSettings generalSettings,
			int complaintDetalizationId);
		ServiceDistrict GetServiceDistrictByCoordinates(IUnitOfWork unitOfWork, decimal latitude, decimal longitude);
	}
}
