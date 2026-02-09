using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IRouteListRepository
	{
		IEnumerable<RouteList> GetDriverRouteLists(IUnitOfWork uow, int driverId, DateTime? date = null, RouteListStatus? status = null);
		IList<RouteList> GetRoutesAtDay(IUnitOfWork uow, DateTime dateForRouting, bool showCompleted, int[] onlyInGeographicGroup, int[] onlyWithDeliveryShifts);
		QueryOver<RouteList> GetRoutesAtDay(DateTime date, List<int> geographicGroupsIds, bool onlyNonPrinted);
		IList<GoodsInRouteListResult> GetGoodsAndEquipsInRL(IUnitOfWork uow, RouteList routeList, ISubdivisionRepository subdivisionRepository = null, Warehouse warehouse = null);
		IList<GoodsInRouteListResult> GetGoodsInRLWithoutEquipments(IUnitOfWork uow, RouteList routeList);
		bool HasRouteList(int driverId, DateTime date, int deliveryShiftId);
		IList<GoodsInRouteListResult> GetFastDeliveryOrdersItemsInRL(IUnitOfWork uoW, int routeListId, RouteListItemStatus[] excludeAddressStatuses = null);
		GoodsInRouteListResult GetTerminalInRL(IUnitOfWork uow, RouteList routeList, Warehouse warehouse);
		IList<GoodsInRouteListResult> GetEquipmentsInRL(IUnitOfWork uow, RouteList routeList);
		IList<GoodsInRouteListResult> AllGoodsLoaded(IUnitOfWork uow, RouteList routeList, CarLoadDocument excludeDoc = null);
		IList<GoodsInRouteListResultToDivide> AllGoodsLoadedDivided(IUnitOfWork uow, RouteList routeList, CarLoadDocument excludeDoc = null);
		IEnumerable<GoodsInRouteListResult> AllGoodsDelivered(IUnitOfWork uow, RouteList routeList, DeliveryDirection? deliveryDirection = null);
		IEnumerable<GoodsInRouteListResult> AllGoodsDelivered(IEnumerable<DeliveryDocument> deliveryDocuments);
		IEnumerable<GoodsInRouteListResult> AllGoodsReceivedFromClient(IEnumerable<DeliveryDocument> deliveryDocuments);
		IEnumerable<GoodsInRouteListResult> AllGoodsTransferredToAnotherDrivers(IUnitOfWork uow, RouteList routeList,
			NomenclatureCategory[] categories = null, AddressTransferType? addressTransferType = null);
		IEnumerable<GoodsInRouteListResult> AllGoodsTransferredFromDrivers(IUnitOfWork uow, RouteList routeList,
			NomenclatureCategory[] categories = null, AddressTransferType? addressTransferType = null);
		IEnumerable<GoodsInRouteListResult> GetActualEquipmentForShipment(IUnitOfWork uow, int routeListId, Direction direction);
		IEnumerable<GoodsInRouteListResult> GetActualGoodsForShipment(IUnitOfWork uow, int routeListId);
		List<ReturnsNode> GetReturnsToWarehouse(IUnitOfWork uow, int routeListId, NomenclatureCategory[] categories = null, int[] excludeNomenclatureIds = null);
		List<ReturnsNode> GetReturnsToWarehouse(IUnitOfWork uow, int routeListId, params int[] nomenclatureIds);
		IEnumerable<CarLoadDocument> GetCarLoadDocuments(IUnitOfWork uow, int routelistId);
		int BottlesUnloadedByCarUnloadedDocuments(IUnitOfWork uow, int emptyBottleId, int routeListId, params int[] exceptDocumentIds);
		RouteList GetActualRouteListByOrder(IUnitOfWork uow, Domain.Orders.Order order);
		RouteList GetActualRouteListByOrder(IUnitOfWork uow, int orderId);
		bool RouteListWasChanged(RouteList routeList);
		IList<GoodsInRouteListResultWithSpecialRequirements> GetGoodsAndEquipsInRLWithSpecialRequirements(IUnitOfWork uow, RouteList routeList, ISubdivisionRepository subdivisionRepository = null, Warehouse warehouse = null);
		/// <summary>
		/// Проверяет необходимость погрузки терминала в МЛ
		/// </summary>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="routeListId">Идентификатор МЛ</param>
		/// <returns></returns>
		bool IsTerminalRequired(IUnitOfWork uow, int routeListId);
		bool RouteListContainsGivenFuelLiters(IUnitOfWork uow, int routeListId);
		decimal TerminalTransferedCountToRouteList(IUnitOfWork unitOfWork, RouteList routeList);
		IList<DocumentPrintHistory> GetPrintsHistory(IUnitOfWork uow, RouteList routeList);
		IEnumerable<int> GetDriverRouteListsIds(IUnitOfWork uow, Employee driver, RouteListStatus? status = null);
		IList<RouteList> GetRouteListsByIds(IUnitOfWork uow, int[] routeListsIds);
		RouteList GetRouteListById(IUnitOfWork uow, int routeListsId);
		GoodsInRouteListResultWithSpecialRequirements GetTerminalInRLWithSpecialRequirements(IUnitOfWork uow, RouteList routeList,
			Warehouse warehouse = null);
		DriverAttachedTerminalDocumentBase GetLastTerminalDocumentForEmployee(IUnitOfWork uow, Employee employee);
		IEnumerable<KeyValuePair<string, int>> GetDeliveryItemsToReturn(IUnitOfWork unitOfWork, int routeListsId);
		SelfDriverTerminalTransferDocument GetSelfDriverTerminalTransferDocument(IUnitOfWork unitOfWork, Employee driver, RouteList routeList);
		IList<NewDriverAdvanceRouteListNode> GetOldUnclosedRouteLists(IUnitOfWork uow, DateTime routeListDate, int driverId);
		bool HasEmployeeAdvance(IUnitOfWork uow, int routeListId, int driverId);

		DateTime? GetDateByDriverWorkingDayNumber(IUnitOfWork uow, int driverId, int dayNumber, CarTypeOfUse? driverOfCarTypeOfUse = null,
			CarOwnType? driverOfCarOwnType = null);

		DateTime? GetLastRouteListDateByDriver(IUnitOfWork uow, int driverId, CarTypeOfUse? driverOfCarTypeOfUse = null,
			CarOwnType? driverOfCarOwnType = null);

		IList<RouteList> GetRouteListsForCarInPeriods(IUnitOfWork uow, int carId,
			IList<(DateTime startDate, DateTime? endDate)> periods);

		IList<Employee> GetDriversWithAdditionalLoading(IUnitOfWork uow, params int[] routeListIds);
		decimal GetRouteListTotalSalesGoodsWeight(IUnitOfWork uow, int routeListId);
		decimal GetRouteListPaidDeliveriesSum(IUnitOfWork uow, int routeListId, IEnumerable<int> paidDeliveriesNomenclaturesIds);
		decimal GetRouteListSalesSum(IUnitOfWork uow, int routeListId);
		bool HasFreeBalanceForOrder(IUnitOfWork uow, Order order, RouteList routeListTo);
		bool IsOrderNeedIndividualSetOnLoad(IUnitOfWork uow, int orderId);
		int GetUnclosedRouteListsCountHavingDebtByDriver(IUnitOfWork uow, int driverId, int excludeRouteListId = 0);
		decimal GetUnclosedRouteListsDebtsSumByDriver(IUnitOfWork uow, int driverId, int excludeRouteListId = 0);
		RouteListProfitabilitySpendings GetRouteListSpendings(IUnitOfWork uow, int routeListId, decimal routeListExpensesPerKg);
		IList<Nomenclature> GetRouteListNomenclatures(IUnitOfWork uow, int routeListId, bool isArchived = false);

		decimal GetCargoDailyNorm(CarTypeOfUse carTypeOfUse);
		void SaveCargoDailyNorms(Dictionary<CarTypeOfUse, decimal> cargoDailyNorms);
		Task<IList<RouteList>> GetCarsRouteListsForPeriod(IUnitOfWork uow, CarTypeOfUse[] carTypesOfUse, CarOwnType[] carOwnTypes, Car car,
			int[] includedCarModelIds, int[] excludedCarModelIds, DateTime startDate, DateTime endDate,
			bool isOnlyCarsWithCompletedFastDelivery, bool isOnlyCarsWithCompletedCommonDelivery, CancellationToken cancellationToken);
		IQueryable<ExploitationReportRouteListDataNode> GetExploitationReportRouteListDataNodes(IUnitOfWork unitOfWork, IEnumerable<int> routeListsIds);
		IQueryable<int> GetOrderIdsByRouteLists(IUnitOfWork unitOfWork, IEnumerable<int> routeListsIds);
		decimal GetCarsConfirmedDistanceForPeriod(IUnitOfWork unitOfWork, int carId, DateTime startDate, DateTime endDate);

		/// <summary>
		/// Возвращает список идентификаторов заказов, которые были завершены в маршрутных листах за текущую дату для указанного автомобиля
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="carId">Id автомобиля</param>
		/// <returns>Список идентификаторов заказов</returns>
		IEnumerable<int> GetCompletedOrdersInTodayRouteListsByCarId(IUnitOfWork uow, int carId);

		/// <summary>
		/// Получить идентификаторы водителей с активными МЛ
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="driverIds"></param>
		/// <returns></returns>
		HashSet<int> GetDriverIdsWithActiveRouteList(IUnitOfWork uow, int[] driverIds);
	}
}
