using Dapper;
using DatabaseServiceWorker.Helpers;
using MySqlConnector;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseServiceWorker.PowerBiWorker
{
	internal partial class PowerBiExportWorker
	{
		private const int _timeOut = 600;

		private async Task ExportTablesAsync(
			MySqlConnection connectionSource,
			MySqlConnection connectionTarget,
			DateTime startDate,
			CancellationToken stoppingToken)
		{
			#region Calendar

			var maxCalendarDateSql = GetMaxCalendarDateSelectSql();
			var maxCalendarDateInTarget = (await connectionTarget.GetDataAsync<DateTime>(maxCalendarDateSql)).Single();
			
			if(maxCalendarDateInTarget < DateTime.Today.AddDays(-1))
			{
				var calendarInsertSql = GetCalendarInsertSql();
				connectionTarget.Execute(calendarInsertSql, commandTimeout: _timeOut); 
			}

			#endregion Calendar

			#region planPerDay

			// Пока уточнается, нужно ли
			//var planPerDaysSql = GetPlanPerDateSelectSql();
			//var planPerDayList = await connectionSource.GetDataAsync<dynamic>(planPerDaysSql, new { date = startDate });
			//var planPerDayInsertSql = GetPlanPerDateInsertSql();

			//var planPerDayTransaction = connectionTarget.BeginTransaction();
			//connectionTarget.Execute(planPerDayInsertSql, planPerDayList, planPerDayTransaction);
			//planPerDayTransaction.Commit();

			#endregion

			#region nomenclature

			var maxNomenclatureIdSql = GetMaxNomenclatureIdSelectSql();
			var maxNomenclatureInTarget = (await connectionTarget.GetDataAsync<int>(maxNomenclatureIdSql)).Single();
			var maxNomenclatureInSource = (await connectionSource.GetDataAsync<int>(maxNomenclatureIdSql)).Single();

			if(maxNomenclatureInSource > maxNomenclatureInTarget)
			{
				var nomenclaturesSql = GetNomenclatureSelectSql();
				var nomenclatureList = await connectionSource.GetDataAsync<dynamic>(nomenclaturesSql, new { id = maxNomenclatureInTarget });

				var nomenclatureInsertSql = GetInsertNomenclatureSql();
				var nomenclatureTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(nomenclatureInsertSql, nomenclatureList, nomenclatureTransaction, _timeOut);
				nomenclatureTransaction.Commit();
			}
			#endregion

			#region subdivision

			var maxSubdivisionIdSql = GetMaxSubdivisionIdSelectSql();
			var maxSubdivisionInTarget = (await connectionTarget.GetDataAsync<int>(maxSubdivisionIdSql)).Single();
			var maxSubdivisionInSource = (await connectionSource.GetDataAsync<int>(maxSubdivisionIdSql)).Single();

			if(maxSubdivisionInSource > maxSubdivisionInTarget)
			{
				var subdivisionsSql = GetSubdivisionsSelectSql();
				var subdivisionList = await connectionSource.GetDataAsync<dynamic>(subdivisionsSql, new { id = maxSubdivisionInTarget });

				var subdivisionInsertSql = GetSubdivisionsInsertSql();
				var subdivisionTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(subdivisionInsertSql, subdivisionList, subdivisionTransaction, _timeOut);
				subdivisionTransaction.Commit();
			}

			#endregion


			#region counterparty

			var maxCounterpartyIdSql = GetMaxCounterpartyIdSelectSql();
			var maxCounterpartyInTarget = (await connectionTarget.GetDataAsync<int>(maxCounterpartyIdSql)).Single();
			var maxCounterpartyInSource = (await connectionSource.GetDataAsync<int>(maxCounterpartyIdSql)).Single();

			if(maxCounterpartyInSource > maxCounterpartyInTarget)
			{
				var counterpartysSql = GetCounterpartysSelectSql();
				var counterpartyList = await connectionSource.GetDataAsync<dynamic>(counterpartysSql, new { id = maxCounterpartyInTarget });

				var counterpartyInsertSql = GetCounterpartysInsertSql();
				var counterpartyTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(counterpartyInsertSql, counterpartyList, counterpartyTransaction, _timeOut);
				counterpartyTransaction.Commit();
			}

			#endregion
			#region DeliveryPoint

			var maxDeliveryPointIdSql = GetMaxDeliveryPointIdSelectSql();
			var maxDeliveryPointInTarget = (await connectionTarget.GetDataAsync<int>(maxDeliveryPointIdSql)).Single();
			var maxDeliveryPointInSource = (await connectionSource.GetDataAsync<int>(maxDeliveryPointIdSql)).Single();

			if(maxDeliveryPointInSource > maxDeliveryPointInTarget)
			{
				var deliveryPointsSql = GetDeliveryPointSelectSql();
				var deliveryPointList = await connectionSource.GetDataAsync<dynamic>(deliveryPointsSql, new { id = maxDeliveryPointInTarget });

				var deliveryPoinInsertSql = GetInsertDeliveryPointSql();
				var deliveryPointTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(deliveryPoinInsertSql, deliveryPointList, deliveryPointTransaction, _timeOut);
				deliveryPointTransaction.Commit();
			}
			#endregion DeliveryPoint
			
			#region DeliverySchedule

			var maxDeliveryScheduleIdSql = GetMaxDeliveryScheduleIdSelectSql();
			var maxDeliveryScheduleInTarget = (await connectionTarget.GetDataAsync<int>(maxDeliveryScheduleIdSql)).Single();
			var maxDeliveryScheduleInSource = (await connectionSource.GetDataAsync<int>(maxDeliveryScheduleIdSql)).Single();

			if(maxDeliveryScheduleInSource > maxDeliveryScheduleInTarget)
			{
				var deliverySchedulesSql = GetDeliveryScheduleSelectSql();
				var deliveryScheduleList = await connectionSource.GetDataAsync<dynamic>(deliverySchedulesSql, new { id = maxDeliveryScheduleInTarget });

				var deliverySchedulesInsertSql = GetInsertDeliveryScheduleSql();
				var deliverySchedulesTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(deliverySchedulesInsertSql, deliveryScheduleList, deliverySchedulesTransaction, _timeOut);
				deliverySchedulesTransaction.Commit();
			}
			
			#endregion DeliverySchedule
			
			#region RouteListAddress

			var maxRouteListAddressIdSql = GetMaxRouteListAddressIdSelectSql();
			var maxRouteListAddressInTarget = (await connectionTarget.GetDataAsync<int>(maxRouteListAddressIdSql)).Single();
			var maxRouteListAddressInSource = (await connectionSource.GetDataAsync<int>(maxRouteListAddressIdSql)).Single();

			if(maxRouteListAddressInSource > maxRouteListAddressInTarget)
			{
				var routeListAddressSelectSql = GetRouteListAddressSelectSql();
				var routeListAddressList = await connectionSource.GetDataAsync<dynamic>(routeListAddressSelectSql, new { id = maxRouteListAddressInTarget });

				var routeListAddressInsertSql = GetInsertRouteListAddressSql();
				var routeListAddressTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(routeListAddressInsertSql, routeListAddressList, routeListAddressTransaction, _timeOut);
				routeListAddressTransaction.Commit();
			}
			
			#endregion RouteListAddress

			#region RouteList

			var maxRouteListIdSql = GetMaxRouteListIdSelectSql();
			var maxRouteListInTarget = (await connectionTarget.GetDataAsync<int>(maxRouteListIdSql)).Single();
			var maxRouteListInSource = (await connectionSource.GetDataAsync<int>(maxRouteListIdSql)).Single();

			if(maxRouteListInSource > maxRouteListInTarget)
			{
				var routeListSelectSql = GetRouteListSelectSql();
				var routeListList = await connectionSource.GetDataAsync<dynamic>(routeListSelectSql, new { id = maxRouteListInTarget });

				var routeListInsertSql = GetInsertRouteListSql();
				var routeListTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(routeListInsertSql, routeListList, routeListTransaction, _timeOut);
				routeListTransaction.Commit();
			}
			
			#endregion RouteList
			
			#region Car

			var maxCarIdSql = GetMaxCarIdSelectSql();
			var maxCarInTarget = (await connectionTarget.GetDataAsync<int>(maxCarIdSql)).Single();
			var maxCarInSource = (await connectionSource.GetDataAsync<int>(maxCarIdSql)).Single();

			if(maxCarInSource > maxCarInTarget)
			{
				var carSelectSql = GetCarSelectSql();
				var carList = await connectionSource.GetDataAsync<dynamic>(carSelectSql, new { id = maxCarInTarget });

				var carInsertSql = GetInsertCarSql();
				var carTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(carInsertSql, carList, carTransaction, _timeOut);
				carTransaction.Commit();
			}
			
			#endregion Car
			
			#region CarModel

			var maxCarModelIdSql = GetMaxCarModelIdSelectSql();
			var maxCarModelInTarget = (await connectionTarget.GetDataAsync<int>(maxCarModelIdSql)).Single();
			var maxCarModelInSource = (await connectionSource.GetDataAsync<int>(maxCarModelIdSql)).Single();

			if(maxCarModelInSource > maxCarModelInTarget)
			{
				var carModelSelectSql = GetCarModelSelectSql();
				var carModelList = await connectionSource.GetDataAsync<dynamic>(carModelSelectSql, new { id = maxCarModelInTarget });

				var carModelInsertSql = GetInsertCarModelSql();
				var carModelTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(carModelInsertSql, carModelList, carModelTransaction, _timeOut);
				carModelTransaction.Commit();
			}
			
			#endregion CarModel

			TruncateTables(connectionTarget, startDate);

			#region Orders

			var ordersSql = GetOrdersSelectSql();
			var orderList = await connectionSource.GetDataAsync<dynamic>(ordersSql, new { date = startDate });
			var orderInsertSql = GetInsertOrderSql();

			var orderTransaction = connectionTarget.BeginTransaction();
			connectionTarget.Execute(orderInsertSql, orderList, orderTransaction, _timeOut);

			var orderItemsSql = GetOrderItemsSelectSql();
			var orderItemsList = await connectionSource.GetDataAsync<dynamic>(orderItemsSql, new { date = startDate });
			var orderItemsInsertSql = GetInsertOrderItemsSql();
			connectionTarget.Execute(orderItemsInsertSql, orderItemsList, orderTransaction, _timeOut);
			orderTransaction.Commit();

			#endregion


			#region undelivered_orders

			var undeliveredOrdersSql = GetUndeliveredOrdersSelectSql();
			var undeliveredOrderList = await connectionSource.GetDataAsync<dynamic>(undeliveredOrdersSql, new { date = startDate });
			var undeliveredOrderInsertSql = GetUndeliveredOrdersInsertSql();

			var undeliveredOrderTransaction = connectionTarget.BeginTransaction();
			connectionTarget.Execute(undeliveredOrderInsertSql, undeliveredOrderList, undeliveredOrderTransaction, _timeOut);
			undeliveredOrderTransaction.Commit();
			
			var guiltyInUndeliveredOrdersSql = GetGuiltyInUndeliveredOrdersSelectSql();
			var guiltyInGuiltyInUndeliveredOrderList = await connectionSource.GetDataAsync<dynamic>(guiltyInUndeliveredOrdersSql, new { date = startDate });
			var guiltyInGuiltyInUndeliveredOrderInsertSql = GetGuiltyInUndeliveredOrdersInsertSql();

			var guiltyInUndeliveredOrdersTransaction = connectionTarget.BeginTransaction();
			connectionTarget.Execute(guiltyInGuiltyInUndeliveredOrderInsertSql, guiltyInGuiltyInUndeliveredOrderList, guiltyInUndeliveredOrdersTransaction, _timeOut);
			guiltyInUndeliveredOrdersTransaction.Commit();

			#endregion
		}


		private void TruncateTables(MySqlConnection connectionTarget, DateTime truncateFromdate, int? maxNomenclatureId = null)
		{
			var truncateTransaction = connectionTarget.BeginTransaction();

			connectionTarget.Execute(@"delete g.* from guilty_in_undelivered_orders g left join undelivered_orders u on u.id = g.undelivery_id
									 where u.creation_date > @date or g.undelivery_id is null or u.id is null;",
									 new { date = truncateFromdate }, truncateTransaction, _timeOut);

			connectionTarget.Execute("delete from undelivered_orders where creation_date > @date;", new { date = truncateFromdate }, truncateTransaction, _timeOut);

			connectionTarget.Execute("delete oi.* from order_items oi left join orders o on o.id = oi.order_id" +
				" where o.create_date > @date or oi.order_id is null or o.id is null;",
				new { date = truncateFromdate }, truncateTransaction, _timeOut);

			connectionTarget.Execute("delete from orders where create_date > @date;", new { date = truncateFromdate }, truncateTransaction, _timeOut);			

			truncateTransaction.Commit();
		}

	}
}
