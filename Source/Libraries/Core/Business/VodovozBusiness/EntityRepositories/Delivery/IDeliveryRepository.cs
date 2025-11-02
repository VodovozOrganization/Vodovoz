using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
		District GetDistrict(
			IUnitOfWork uow,
			decimal latitude,
			decimal longitude,
			DistrictsSet districtsSet = null
		);

		/// <summary>
		/// Возвращает район для указанных координат, 
		/// если существует наложение районов, то возвращает первый попавшийся район 
		/// </summary>
		Task<District> GetDistrictAsync(
			IUnitOfWork uow,
			decimal latitude,
			decimal longitude,
			CancellationToken cancellationToken,
			DistrictsSet districtsSet = null
		);


		/// <summary>
		/// Возвращает список районов в границы которых входят указанные координаты
		/// </summary>
		IEnumerable<District> GetDistricts(
			IUnitOfWork uow,
			decimal latitude,
			decimal longitude,
			DistrictsSet districtsSet = null
		);

		/// <summary>
		/// Возвращает список районов в границы которых входят указанные координаты
		/// </summary>
		Task<IEnumerable<District>> GetDistrictsAsync(
			IUnitOfWork uow,
			decimal latitude,
			decimal longitude,
			CancellationToken cancellationToken,
			DistrictsSet districtsSet = null
		);

		District GetAccurateDistrict(IUnitOfWork uow, decimal latitude, decimal longitude);

		FastDeliveryAvailabilityHistory GetRouteListsForFastDeliveryForOrder(
			IUnitOfWork uow,
			double latitude,
			double longitude,
			bool isGetClosestByRoute,
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes,
			int? tariffZoneId,
			Order fastDeliveryOrder
		);

		Task<FastDeliveryAvailabilityHistory> GetRouteListsForFastDeliveryAsync(
			IUnitOfWork uow,
			double latitude,
			double longitude,
			bool isGetClosestByRoute,
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes,
			int? tariffZoneId,
			CancellationToken cancellationToken
		);

		void UpdateFastDeliveryMaxDistanceParameter(double value);
		double GetMaxDistanceToLatestTrackPointKmFor(DateTime dateTime);
		double GetGetMaxDistanceToLatestTrackPointKm();
		IList<Order> GetFastDeliveryLateOrders(IUnitOfWork uow, DateTime fromDateTime, IGeneralSettings generalSettings,
			int complaintDetalizationId);
		ServiceDistrict GetServiceDistrictByCoordinates(IUnitOfWork unitOfWork, decimal latitude, decimal longitude);
	}
}
