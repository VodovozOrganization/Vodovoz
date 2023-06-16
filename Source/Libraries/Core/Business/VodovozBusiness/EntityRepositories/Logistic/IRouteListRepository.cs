using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IRouteListRepository
	{
		IList<RouteList> GetDriverRouteLists(IUnitOfWork uow, Employee driver, DateTime? date = null, RouteListStatus? status = null);
		QueryOver<RouteList> GetRoutesAtDay(DateTime date);
		QueryOver<RouteList> GetRoutesAtDay(DateTime date, List<int> geographicGroupsIds);
		IList<GoodsInRouteListResult> GetGoodsAndEquipsInRL(IUnitOfWork uow, RouteList routeList, ISubdivisionRepository subdivisionRepository = null, Warehouse warehouse = null);
		IList<GoodsInRouteListResult> GetGoodsInRLWithoutEquipments(IUnitOfWork uow, RouteList routeList);
		bool HasRouteList(int driverId, DateTime date, int deliveryShiftId);
		IList<GoodsInRouteListResult> GetFastDeliveryOrdersItemsInRL(IUnitOfWork uoW, int routeListId, RouteListItemStatus [] excludeAddressStatuses = null);
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
		bool RouteListContainsGivedFuelLiters(IUnitOfWork uow, int id);
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
		decimal GetRouteListTotalWeight(IUnitOfWork uow, int routeListId);
		decimal GetRouteListPaidDeliveriesSum(IUnitOfWork uow, int routeListId, IEnumerable<int> paidDeliveriesNomenclaturesIds);
		decimal GetRouteListSalesSum(IUnitOfWork uow, int routeListId);
		bool HasFreeBalanceForOrder(IUnitOfWork uow, Order order, RouteList routeListTo);
		int GetUnclosedRouteListsCountHavingDebtByDriver(IUnitOfWork uow, int driverId);
		decimal GetRouteListsDebtSumByDriver(IUnitOfWork uow, int driverId);
	}
}
