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
		private const int _timeOut = 120;

		private async Task ExportTablesAsync(
			MySqlConnection connectionSource,
			MySqlConnection connectionTarget,
			DateTime startDate,
			CancellationToken stoppingToken)
		{
			#region Calendar

			var calendarInsertSql = GetCalendarInsertSql();
			connectionTarget.Execute(calendarInsertSql, commandTimeout: _timeOut);

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

			var guiltyInGuiltyInUndeliveredOrdersSql = GetGuiltyInUndeliveredOrdersSelectSql();
			var guiltyInGuiltyInUndeliveredOrderList = await connectionSource.GetDataAsync<dynamic>(guiltyInGuiltyInUndeliveredOrdersSql, new { date = startDate });
			var guiltyInGuiltyInUndeliveredOrderInsertSql = GetGuiltyInUndeliveredOrdersInsertSql();

			connectionTarget.Execute(guiltyInGuiltyInUndeliveredOrderInsertSql, guiltyInGuiltyInUndeliveredOrderList, undeliveredOrderTransaction, _timeOut);
			undeliveredOrderTransaction.Commit();

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
